using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UniCortex.Core.Domains.CodeGraph;

namespace UniCortex.Core.Infrastructures.CodeGraph;

/// <summary>
/// Builds a code graph from Unity project sources using Roslyn. A syntax pass collects
/// declarations, a semantic pass resolves inheritance, calls and usages. Calls that cannot
/// be resolved semantically (e.g. missing Unity references) degrade to name-matched
/// CallsUnresolved edges instead of being dropped.
/// </summary>
internal sealed class RoslynCodeGraphBuilder
{
    private const int MaxUnresolvedCallCandidates = 10;

    private static readonly SymbolDisplayFormat s_idFormat = new(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        memberOptions: SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType
                       | SymbolDisplayMemberOptions.IncludeExplicitInterface,
        parameterOptions: SymbolDisplayParameterOptions.IncludeType,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    private static readonly CSharpParseOptions s_parseOptions = new(
        LanguageVersion.Latest,
        preprocessorSymbols: ["UNITY_EDITOR", "UNITY_5_3_OR_NEWER", "UNITY_2022_3_OR_NEWER"]);

    private sealed class BuildState
    {
        public readonly Dictionary<string, CodeSymbolNode> Nodes = new(StringComparer.Ordinal);
        public readonly Dictionary<ISymbol, string> IdsBySymbol = new(SymbolEqualityComparer.Default);
        public readonly List<(INamedTypeSymbol Symbol, CodeSymbolNode Node)> Types = [];
        public readonly List<(IMethodSymbol Symbol, CodeSymbolNode Node)> Methods = [];
        public readonly HashSet<string> SerializationCandidates = new(StringComparer.Ordinal);
        public readonly List<CodeEdge> Edges = [];
        public readonly HashSet<string> EdgeKeys = new(StringComparer.Ordinal);
        public readonly Dictionary<string, List<CodeSymbolNode>> CallablesByName = new(StringComparer.Ordinal);
    }

    public CodeGraphSnapshot Build(
        string projectPath, UnityProjectSource source, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // Files that fail to read (e.g. transiently locked) are excluded from the snapshot's
        // file list, so the store notices the mismatch on the next query and re-indexes.
        var trees = new List<(SyntaxTree Tree, SourceFileRecord File)>(source.Files.Count);
        var parsedFiles = new List<SourceFileRecord>(source.Files.Count);
        foreach (var file in source.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string text;
            try
            {
                text = File.ReadAllText(file.FullPath);
            }
            catch (Exception)
            {
                continue;
            }

            trees.Add((CSharpSyntaxTree.ParseText(text, s_parseOptions, path: file.FullPath), file));
            parsedFiles.Add(file);
        }

        var (references, unityReferencesResolved) =
            UnityReferenceResolver.Resolve(projectPath, source.AssemblyNames);
        var compilation = CSharpCompilation.Create(
            "UniCortex.CodeGraph",
            trees.Select(t => t.Tree),
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        var state = new BuildState();

        foreach (var (tree, file) in trees)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CollectDeclarations(compilation.GetSemanticModel(tree), tree, file, state);
        }

        foreach (var node in state.Nodes.Values)
        {
            if (node.Kind is CodeSymbolKind.Method or CodeSymbolKind.Constructor)
            {
                if (!state.CallablesByName.TryGetValue(node.Name, out var list))
                {
                    list = [];
                    state.CallablesByName[node.Name] = list;
                }

                list.Add(node);
            }
        }

        foreach (var (tree, file) in trees)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CollectBodyEdges(compilation.GetSemanticModel(tree), tree, file, state);
        }

        CollectRelationEdges(state);
        ClassifyUnitySymbols(state);

        return new CodeGraphSnapshot(
            projectPath,
            state.Nodes,
            state.Edges,
            parsedFiles,
            DateTimeOffset.UtcNow,
            stopwatch.Elapsed,
            unityReferencesResolved);
    }

    // ----- Declaration pass -----

    private static void CollectDeclarations(
        SemanticModel model, SyntaxTree tree, SourceFileRecord file, BuildState state)
    {
        foreach (var syntaxNode in tree.GetRoot().DescendantNodes())
        {
            switch (syntaxNode)
            {
                case BaseTypeDeclarationSyntax typeDeclaration:
                    AddTypeDeclaration(model, typeDeclaration, file, state);
                    break;
                case DelegateDeclarationSyntax delegateDeclaration:
                    if (model.GetDeclaredSymbol(delegateDeclaration) is { } delegateSymbol)
                    {
                        GetOrAddNode(delegateSymbol, CodeSymbolKind.Delegate, file,
                            delegateDeclaration.GetLocation(), Modifiers(delegateDeclaration.Modifiers),
                            null, state, out _);
                    }

                    break;
            }
        }
    }

    private static void AddTypeDeclaration(
        SemanticModel model, BaseTypeDeclarationSyntax typeDeclaration, SourceFileRecord file, BuildState state)
    {
        if (model.GetDeclaredSymbol(typeDeclaration) is not { } symbol)
        {
            return;
        }

        var node = GetOrAddNode(symbol, ToKind(symbol.TypeKind), file, typeDeclaration.GetLocation(),
            Modifiers(typeDeclaration.Modifiers), null, state, out var isNew);

        if (isNew)
        {
            state.Types.Add((symbol, node));

            if (symbol.TypeKind is TypeKind.Class or TypeKind.Struct
                && symbol.BaseType is { } baseType
                && baseType.SpecialType is not (SpecialType.System_Object or SpecialType.System_ValueType))
            {
                node.BaseClassName = DisplayBaseType(baseType);
            }

            foreach (var interfaceType in symbol.Interfaces)
            {
                node.Interfaces.Add(DisplayBaseType(interfaceType));
            }

            node.UnityRole = ResolveUnityRoleFromBaseChain(symbol);
        }

        if (typeDeclaration is not TypeDeclarationSyntax typeWithMembers)
        {
            return;
        }

        foreach (var member in typeWithMembers.Members)
        {
            switch (member)
            {
                case BaseTypeDeclarationSyntax or DelegateDeclarationSyntax:
                    // Nested types are handled by the outer DescendantNodes loop.
                    break;
                case MethodDeclarationSyntax method:
                    AddMember(model, method, CodeSymbolKind.Method, Modifiers(method.Modifiers), node.Id, file, state);
                    break;
                case ConstructorDeclarationSyntax constructor:
                    AddMember(model, constructor, CodeSymbolKind.Constructor, Modifiers(constructor.Modifiers),
                        node.Id, file, state);
                    break;
                case OperatorDeclarationSyntax op:
                    AddMember(model, op, CodeSymbolKind.Method, Modifiers(op.Modifiers), node.Id, file, state);
                    break;
                case ConversionOperatorDeclarationSyntax conversion:
                    AddMember(model, conversion, CodeSymbolKind.Method, Modifiers(conversion.Modifiers),
                        node.Id, file, state);
                    break;
                case DestructorDeclarationSyntax destructor:
                    AddMember(model, destructor, CodeSymbolKind.Method, Modifiers(destructor.Modifiers),
                        node.Id, file, state);
                    break;
                case PropertyDeclarationSyntax property:
                    AddMember(model, property, CodeSymbolKind.Property, Modifiers(property.Modifiers),
                        node.Id, file, state);
                    break;
                case IndexerDeclarationSyntax indexer:
                    AddMember(model, indexer, CodeSymbolKind.Property, Modifiers(indexer.Modifiers),
                        node.Id, file, state);
                    break;
                case EventDeclarationSyntax eventDeclaration:
                    AddMember(model, eventDeclaration, CodeSymbolKind.Event, Modifiers(eventDeclaration.Modifiers),
                        node.Id, file, state);
                    break;
                case FieldDeclarationSyntax fieldDeclaration:
                    AddFieldDeclaration(model, fieldDeclaration, node.Id, file, state);
                    break;
                case EventFieldDeclarationSyntax eventField:
                    foreach (var variable in eventField.Declaration.Variables)
                    {
                        if (model.GetDeclaredSymbol(variable) is { } eventSymbol)
                        {
                            GetOrAddNode(eventSymbol, CodeSymbolKind.Event, file, variable.GetLocation(),
                                Modifiers(eventField.Modifiers), node.Id, state, out _);
                        }
                    }

                    break;
            }
        }
    }

    private static void AddMember(
        SemanticModel model, MemberDeclarationSyntax member, CodeSymbolKind kind, string? modifiers,
        string parentId, SourceFileRecord file, BuildState state)
    {
        if (model.GetDeclaredSymbol(member) is not { } symbol)
        {
            return;
        }

        var node = GetOrAddNode(symbol, kind, file, member.GetLocation(), modifiers, parentId, state, out var isNew);
        if (isNew && symbol is IMethodSymbol methodSymbol)
        {
            state.Methods.Add((methodSymbol, node));
        }
    }

    private static void AddFieldDeclaration(
        SemanticModel model, FieldDeclarationSyntax fieldDeclaration, string parentId,
        SourceFileRecord file, BuildState state)
    {
        var attributeNames = fieldDeclaration.AttributeLists
            .SelectMany(list => list.Attributes)
            .Select(attribute => NormalizeAttributeName(attribute.Name.ToString()))
            .ToList();

        var isPublic = fieldDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword);
        var isExcluded = fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword)
                         || fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword)
                         || fieldDeclaration.Modifiers.Any(SyntaxKind.ReadOnlyKeyword)
                         || attributeNames.Contains("NonSerialized");
        var isSerializationCandidate = !isExcluded
                                       && (attributeNames.Contains("SerializeField")
                                           || attributeNames.Contains("SerializeReference")
                                           || isPublic);

        foreach (var variable in fieldDeclaration.Declaration.Variables)
        {
            if (model.GetDeclaredSymbol(variable) is not { } fieldSymbol)
            {
                continue;
            }

            var node = GetOrAddNode(fieldSymbol, CodeSymbolKind.Field, file, variable.GetLocation(),
                Modifiers(fieldDeclaration.Modifiers), parentId, state, out _);
            if (isSerializationCandidate)
            {
                state.SerializationCandidates.Add(node.Id);
            }
        }
    }

    private static string NormalizeAttributeName(string attributeName)
    {
        var simple = CodeGraphSnapshot.SimpleName(attributeName);
        return simple.EndsWith("Attribute", StringComparison.Ordinal)
            ? simple[..^"Attribute".Length]
            : simple;
    }

    private static CodeSymbolNode GetOrAddNode(
        ISymbol symbol, CodeSymbolKind kind, SourceFileRecord file, Location location,
        string? modifiers, string? parentId, BuildState state, out bool isNew)
    {
        var id = symbol.ToDisplayString(s_idFormat);
        if (symbol is IMethodSymbol { MethodKind: MethodKind.StaticConstructor })
        {
            // "Type.Type()" would collide with the instance constructor's id.
            id = symbol.ContainingType.ToDisplayString(s_idFormat) + ".cctor()";
        }

        state.IdsBySymbol[symbol] = id;

        var span = location.GetLineSpan();
        var symbolLocation = new SymbolLocation(
            file.RelativePath, span.StartLinePosition.Line + 1, span.EndLinePosition.Line + 1);

        if (state.Nodes.TryGetValue(id, out var existing))
        {
            // Partial type/method: keep one node and record every declaration location.
            existing.Locations.Add(symbolLocation);
            isNew = false;
            return existing;
        }

        var name = symbol switch
        {
            IMethodSymbol { MethodKind: MethodKind.Constructor } constructor => constructor.ContainingType.Name,
            IMethodSymbol { MethodKind: MethodKind.StaticConstructor } => "cctor",
            _ => symbol.Name
        };

        var node = new CodeSymbolNode
        {
            Id = id,
            Kind = kind,
            Name = name,
            Assembly = file.AssemblyName,
            Namespace = symbol.ContainingNamespace is { IsGlobalNamespace: false } containingNamespace
                ? containingNamespace.ToDisplayString()
                : null,
            ParentId = parentId,
            Modifiers = string.IsNullOrEmpty(modifiers) ? null : modifiers
        };
        node.Locations.Add(symbolLocation);
        state.Nodes[id] = node;
        isNew = true;
        return node;
    }

    private static string? Modifiers(SyntaxTokenList modifiers)
        => modifiers.Count == 0 ? null : string.Join(" ", modifiers.Select(m => m.Text));

    private static CodeSymbolKind ToKind(TypeKind typeKind) => typeKind switch
    {
        TypeKind.Struct => CodeSymbolKind.Struct,
        TypeKind.Interface => CodeSymbolKind.Interface,
        TypeKind.Enum => CodeSymbolKind.Enum,
        TypeKind.Delegate => CodeSymbolKind.Delegate,
        _ => CodeSymbolKind.Class
    };

    private static string DisplayBaseType(INamedTypeSymbol type)
        => type is IErrorTypeSymbol ? type.Name : type.OriginalDefinition.ToDisplayString(s_idFormat);

    private static string? ResolveUnityRoleFromBaseChain(INamedTypeSymbol symbol)
    {
        var current = symbol.BaseType;
        for (var depth = 0; current is not null && depth < 32; depth++)
        {
            if (current is IErrorTypeSymbol)
            {
                // Unresolved base: the name-based fallback in ClassifyUnitySymbols takes over.
                return null;
            }

            var fullName = current.OriginalDefinition.ToDisplayString();
            if (UnityCodeConventions.RolesByBaseClass.TryGetValue(fullName, out var role))
            {
                return role;
            }

            current = current.BaseType;
        }

        return null;
    }

    // ----- Edge pass -----

    private static void CollectBodyEdges(
        SemanticModel model, SyntaxTree tree, SourceFileRecord file, BuildState state)
    {
        foreach (var syntaxNode in tree.GetRoot().DescendantNodes())
        {
            switch (syntaxNode)
            {
                case InvocationExpressionSyntax invocation:
                    HandleInvocation(model, invocation, file, state);
                    break;
                case BaseObjectCreationExpressionSyntax creation:
                    HandleObjectCreation(model, creation, file, state);
                    break;
                case IdentifierNameSyntax or GenericNameSyntax:
                    HandleNameReference(model, (SimpleNameSyntax)syntaxNode, file, state);
                    break;
            }
        }
    }

    private static void HandleInvocation(
        SemanticModel model, InvocationExpressionSyntax invocation, SourceFileRecord file, BuildState state)
    {
        if (GetEnclosingId(model, invocation, state) is not { } fromId)
        {
            return;
        }

        var line = Line(invocation);
        var symbolInfo = model.GetSymbolInfo(invocation);

        if (symbolInfo.Symbol is IMethodSymbol target)
        {
            var resolved = (target.ReducedFrom ?? target).OriginalDefinition;
            if (state.IdsBySymbol.TryGetValue(resolved, out var toId))
            {
                var isBaseCall = invocation.Expression
                    is MemberAccessExpressionSyntax { Expression: BaseExpressionSyntax };
                AddEdge(state, fromId, toId,
                    isBaseCall ? CodeEdgeKind.CallsBase : CodeEdgeKind.Calls, file.RelativePath, line);
            }

            // Resolved to an external symbol: nothing to record, and no unresolved fallback.
            return;
        }

        var addedCandidate = false;
        foreach (var candidate in symbolInfo.CandidateSymbols)
        {
            if (candidate is IMethodSymbol candidateMethod
                && state.IdsBySymbol.TryGetValue(
                    (candidateMethod.ReducedFrom ?? candidateMethod).OriginalDefinition, out var candidateId))
            {
                AddEdge(state, fromId, candidateId, CodeEdgeKind.CallsUnresolved, file.RelativePath, line);
                addedCandidate = true;
            }
        }

        if (addedCandidate)
        {
            return;
        }

        // Fall back to matching indexed methods by the invoked name.
        if (InvokedName(invocation.Expression) is { } name
            && state.CallablesByName.TryGetValue(name, out var callables))
        {
            foreach (var callable in callables.Take(MaxUnresolvedCallCandidates))
            {
                AddEdge(state, fromId, callable.Id, CodeEdgeKind.CallsUnresolved, file.RelativePath, line);
            }
        }
    }

    private static void HandleObjectCreation(
        SemanticModel model, BaseObjectCreationExpressionSyntax creation, SourceFileRecord file, BuildState state)
    {
        if (GetEnclosingId(model, creation, state) is not { } fromId)
        {
            return;
        }

        var line = Line(creation);

        if (model.GetSymbolInfo(creation).Symbol is IMethodSymbol constructor)
        {
            if (state.IdsBySymbol.TryGetValue(constructor.OriginalDefinition, out var constructorId))
            {
                AddEdge(state, fromId, constructorId, CodeEdgeKind.Calls, file.RelativePath, line);
            }
            else if (state.IdsBySymbol.TryGetValue(
                         constructor.ContainingType.OriginalDefinition, out var typeId))
            {
                // Compiler-generated default constructor of an indexed type.
                AddEdge(state, fromId, typeId, CodeEdgeKind.Uses, file.RelativePath, line);
            }

            return;
        }

        if (model.GetTypeInfo(creation).Type is { } createdType
            && state.IdsBySymbol.TryGetValue(createdType.OriginalDefinition, out var createdTypeId))
        {
            AddEdge(state, fromId, createdTypeId, CodeEdgeKind.Uses, file.RelativePath, line);
        }
    }

    private static void HandleNameReference(
        SemanticModel model, SimpleNameSyntax name, SourceFileRecord file, BuildState state)
    {
        if (name is IdentifierNameSyntax { IsVar: true })
        {
            return;
        }

        // Base lists are covered by Inherits/Implements edges.
        if (name.FirstAncestorOrSelf<BaseListSyntax>() is not null)
        {
            return;
        }

        var accessRoot = name.Parent switch
        {
            MemberAccessExpressionSyntax memberAccess when memberAccess.Name == name => (SyntaxNode)memberAccess,
            MemberBindingExpressionSyntax memberBinding when memberBinding.Name == name => memberBinding,
            _ => name
        };
        var isInvocationTarget = accessRoot.Parent is InvocationExpressionSyntax invocation
                                 && invocation.Expression == accessRoot;

        // The created type name is covered by constructor Calls edges.
        var typeRoot = name.Parent is QualifiedNameSyntax qualified && qualified.Right == name
            ? (SyntaxNode)qualified
            : name;
        if (typeRoot.Parent is ObjectCreationExpressionSyntax objectCreation && objectCreation.Type == typeRoot)
        {
            return;
        }

        if (GetEnclosingId(model, name, state) is not { } fromId)
        {
            return;
        }

        var symbol = model.GetSymbolInfo(name).Symbol;

        // The invoked method name itself is covered by Calls edges, but invoking a
        // delegate-typed field/property/event is still a usage of that member.
        if (isInvocationTarget && symbol is IMethodSymbol or null)
        {
            return;
        }

        var targetSymbol = symbol switch
        {
            INamedTypeSymbol namedType => namedType.OriginalDefinition,
            IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } enumMember
                => enumMember.ContainingType.OriginalDefinition,
            IFieldSymbol field => field.OriginalDefinition,
            IPropertySymbol property => (ISymbol)property.OriginalDefinition,
            IEventSymbol eventSymbol => eventSymbol.OriginalDefinition,
            IMethodSymbol method => (method.ReducedFrom ?? method).OriginalDefinition,
            _ => null
        };

        if (targetSymbol is not null
            && state.IdsBySymbol.TryGetValue(targetSymbol, out var toId)
            && toId != fromId)
        {
            AddEdge(state, fromId, toId, CodeEdgeKind.Uses, file.RelativePath, Line(name));
        }
    }

    private static string? GetEnclosingId(SemanticModel model, SyntaxNode node, BuildState state)
    {
        foreach (var ancestor in node.Ancestors())
        {
            ISymbol? symbol = ancestor switch
            {
                VariableDeclaratorSyntax variable
                    when variable.Parent?.Parent is FieldDeclarationSyntax or EventFieldDeclarationSyntax
                    => model.GetDeclaredSymbol(variable),
                // References in the field's type are attributed to the (first) declared field.
                BaseFieldDeclarationSyntax { Declaration.Variables.Count: > 0 } field
                    => model.GetDeclaredSymbol(field.Declaration.Variables[0]),
                BaseMethodDeclarationSyntax or PropertyDeclarationSyntax or IndexerDeclarationSyntax
                    or EventDeclarationSyntax or BaseTypeDeclarationSyntax
                    => model.GetDeclaredSymbol(ancestor),
                _ => null
            };

            if (symbol is not null && state.IdsBySymbol.TryGetValue(symbol, out var id))
            {
                return id;
            }
        }

        return null;
    }

    private static string? InvokedName(ExpressionSyntax expression) => expression switch
    {
        MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
        MemberBindingExpressionSyntax memberBinding => memberBinding.Name.Identifier.Text,
        IdentifierNameSyntax identifier => identifier.Identifier.Text,
        GenericNameSyntax generic => generic.Identifier.Text,
        AliasQualifiedNameSyntax alias => alias.Name.Identifier.Text,
        _ => null
    };

    private static int Line(SyntaxNode node)
        => node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

    private static void AddEdge(
        BuildState state, string fromId, string toId, CodeEdgeKind kind, string file, int line)
    {
        var key = $"{fromId}|{toId}|{(int)kind}|{file}|{line}";
        if (state.EdgeKeys.Add(key))
        {
            state.Edges.Add(new CodeEdge(fromId, toId, kind, file, line));
        }
    }

    // ----- Relation pass (inheritance, interface implementations, overrides) -----

    private static void CollectRelationEdges(BuildState state)
    {
        foreach (var (symbol, node) in state.Types)
        {
            var location = node.Locations[0];

            if (symbol.BaseType is { } baseType
                && state.IdsBySymbol.TryGetValue(baseType.OriginalDefinition, out var baseId))
            {
                AddEdge(state, node.Id, baseId, CodeEdgeKind.Inherits, location.File, location.StartLine);
            }

            foreach (var interfaceType in symbol.Interfaces)
            {
                if (state.IdsBySymbol.TryGetValue(interfaceType.OriginalDefinition, out var interfaceId))
                {
                    AddEdge(state, node.Id, interfaceId, CodeEdgeKind.Implements, location.File, location.StartLine);
                }
            }

            if (symbol.TypeKind is not (TypeKind.Class or TypeKind.Struct))
            {
                continue;
            }

            foreach (var interfaceType in symbol.AllInterfaces)
            {
                if (!state.IdsBySymbol.ContainsKey(interfaceType.OriginalDefinition))
                {
                    continue;
                }

                foreach (var member in interfaceType.GetMembers())
                {
                    if (member is IMethodSymbol { MethodKind: not MethodKind.Ordinary })
                    {
                        continue;
                    }

                    if (member is not (IMethodSymbol or IPropertySymbol or IEventSymbol)
                        || !state.IdsBySymbol.TryGetValue(member.OriginalDefinition, out var memberId))
                    {
                        continue;
                    }

                    // The implementation may live on a base class (class Base { public void M() {} }
                    // class Derived : Base, IFoo {}); duplicate edges from multiple derived types
                    // are collapsed by AddEdge's dedup key.
                    var implementation = symbol.FindImplementationForInterfaceMember(member);
                    if (implementation is null
                        || !state.IdsBySymbol.TryGetValue(implementation.OriginalDefinition, out var implementationId))
                    {
                        continue;
                    }

                    var implementationNode = state.Nodes[implementationId];
                    var implementationLocation = implementationNode.Locations[0];
                    AddEdge(state, implementationId, memberId, CodeEdgeKind.ImplementsMember,
                        implementationLocation.File, implementationLocation.StartLine);
                }
            }
        }

        foreach (var (symbol, node) in state.Methods)
        {
            if (symbol is { IsOverride: true, OverriddenMethod: { } overridden }
                && state.IdsBySymbol.TryGetValue(overridden.OriginalDefinition, out var overriddenId))
            {
                var location = node.Locations[0];
                AddEdge(state, node.Id, overriddenId, CodeEdgeKind.Overrides, location.File, location.StartLine);
            }
        }
    }

    // ----- Unity classification pass -----

    private static void ClassifyUnitySymbols(BuildState state)
    {
        foreach (var (_, node) in state.Types)
        {
            ResolveUnityRole(node, state, new HashSet<string>(StringComparer.Ordinal));
        }

        foreach (var node in state.Nodes.Values)
        {
            if (node.ParentId is null || !state.Nodes.TryGetValue(node.ParentId, out var parent)
                || parent.UnityRole is not { } role)
            {
                continue;
            }

            switch (node.Kind)
            {
                case CodeSymbolKind.Method
                    when UnityCodeConventions.IsUnityMessage(role, node.Name)
                         && node.Modifiers?.Contains("static") is not true:
                    node.IsUnityMessage = true;
                    break;
                case CodeSymbolKind.Field when state.SerializationCandidates.Contains(node.Id):
                    node.IsSerialized = true;
                    break;
            }
        }
    }

    private static string? ResolveUnityRole(CodeSymbolNode type, BuildState state, HashSet<string> visiting)
    {
        if (type.UnityRole is not null)
        {
            return type.UnityRole;
        }

        if (type.BaseClassName is null || !visiting.Add(type.Id))
        {
            return null;
        }

        string? role = null;
        if (UnityCodeConventions.RolesByBaseClass.TryGetValue(type.BaseClassName, out var exactRole))
        {
            role = exactRole;
        }
        else if (state.Nodes.TryGetValue(type.BaseClassName, out var baseNode)
                 && baseNode.Kind == CodeSymbolKind.Class)
        {
            role = ResolveUnityRole(baseNode, state, visiting);
        }
        else if (UnityCodeConventions.RolesByBaseClassSimpleName.TryGetValue(
                     CodeGraphSnapshot.SimpleName(type.BaseClassName), out var simpleNameRole))
        {
            // Name-based fallback for bases that could not be resolved semantically.
            role = simpleNameRole;
        }

        type.UnityRole = role;
        return role;
    }
}
