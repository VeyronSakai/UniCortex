using System.Text.Json;
using NUnit.Framework;
using UniCortex.Core.Test.Fixtures;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public sealed class CodeGraphUseCaseTest
{
    private CodeGraphProjectFixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new CodeGraphProjectFixture();
        _fixture.WriteSampleGame();
    }

    [TearDown]
    public void TearDown()
    {
        _fixture.Dispose();
    }

    [Test]
    public async ValueTask GetCodeMap_ReturnsAssembliesNamespacesAndUnitySummary()
    {
        var json = await _fixture.UseCase.GetCodeMapAsync();
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.That(root.GetProperty("fileCount").GetInt32(), Is.EqualTo(6));
        Assert.That(root.GetProperty("symbolCount").GetInt32(), Is.GreaterThan(10));

        var assemblies = root.GetProperty("assemblies").EnumerateArray()
            .Select(a => a.GetProperty("name").GetString()).ToList();
        Assert.That(assemblies, Does.Contain("Assembly-CSharp"));
        Assert.That(assemblies, Does.Contain("Assembly-CSharp-Editor"));
        Assert.That(assemblies, Does.Contain("Game.Sub"));

        var unity = root.GetProperty("unity");
        Assert.That(unity.GetProperty("monoBehaviourCount").GetInt32(), Is.EqualTo(1));
        Assert.That(unity.GetProperty("unityMessageMethodCount").GetInt32(), Is.EqualTo(2));
        Assert.That(unity.GetProperty("serializedFieldCount").GetInt32(), Is.EqualTo(2));
    }

    [Test]
    public async ValueTask SearchSymbols_FiltersByBaseType()
    {
        var json = await _fixture.UseCase.SearchSymbolsAsync(
            "*", kinds: "Class", filePattern: null, baseType: "MonoBehaviour", limit: 50);
        using var document = JsonDocument.Parse(json);
        var symbols = document.RootElement.GetProperty("symbols").EnumerateArray().ToList();

        Assert.That(symbols, Has.Count.EqualTo(1));
        Assert.That(symbols[0].GetProperty("id").GetString(), Is.EqualTo("Game.Player"));
        Assert.That(symbols[0].GetProperty("unityRole").GetString(), Is.EqualTo("MonoBehaviour"));
    }

    [Test]
    public async ValueTask SearchSymbols_FiltersByKindAndFilePattern()
    {
        var json = await _fixture.UseCase.SearchSymbolsAsync(
            "*", kinds: "Class", filePattern: "Assets/Editor/*", baseType: null, limit: 50);

        Assert.That(json, Does.Contain("Game.EditorTools.GameBuildTools"));
        Assert.That(json, Does.Not.Contain("Game.Player"));
    }

    [Test]
    public async ValueTask SearchSymbols_MarksUnityMessagesAndSerializedFields()
    {
        var json = await _fixture.UseCase.SearchSymbolsAsync(
            "Game.Player.*", kinds: null, filePattern: null, baseType: null, limit: 50);
        using var document = JsonDocument.Parse(json);
        var symbols = document.RootElement.GetProperty("symbols").EnumerateArray()
            .ToDictionary(s => s.GetProperty("id").GetString()!);

        Assert.That(symbols["Game.Player.Update()"].GetProperty("isUnityMessage").GetBoolean(), Is.True);
        Assert.That(symbols["Game.Player.maxHealth"].GetProperty("isSerialized").GetBoolean(), Is.True);
        Assert.That(symbols["Game.Player.speed"].GetProperty("isSerialized").GetBoolean(), Is.True);
        Assert.That(symbols["Game.Player.Move()"].TryGetProperty("isUnityMessage", out _), Is.False);
    }

    [Test]
    public async ValueTask GetCodeSnippet_ReturnsDeclarationSource()
    {
        var json = await _fixture.UseCase.GetCodeSnippetAsync("Game.Player.Move");
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.That(root.GetProperty("id").GetString(), Is.EqualTo("Game.Player.Move()"));
        var code = root.GetProperty("parts")[0].GetProperty("code").GetString();
        Assert.That(code, Does.Contain("weapon.Attack()"));
        Assert.That(code, Does.Not.Contain("private void Update"));
    }

    [Test]
    public void GetCodeSnippet_ThrowsForUnknownSymbol()
    {
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _fixture.UseCase.GetCodeSnippetAsync("Game.DoesNotExist"));
        Assert.That(exception!.Message, Does.Contain("Symbol not found"));
    }

    [Test]
    public void GetCodeSnippet_ThrowsForAmbiguousSymbol()
    {
        // Both IWeapon.Attack() and Sword.Attack() match.
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _fixture.UseCase.GetCodeSnippetAsync("Attack"));
        Assert.That(exception!.Message, Does.Contain("ambiguous"));
    }

    [Test]
    public async ValueTask FindReferences_GroupsCallsAndImplementations()
    {
        var json = await _fixture.UseCase.FindReferencesAsync("Game.IWeapon.Attack", limit: 100);
        using var document = JsonDocument.Parse(json);
        var references = document.RootElement.GetProperty("references");

        var calls = references.GetProperty("calls").EnumerateArray().ToList();
        Assert.That(calls.Select(c => c.GetProperty("from").GetString()),
            Does.Contain("Game.Player.Move()"));

        var implementedBy = references.GetProperty("implementedBy").EnumerateArray().ToList();
        Assert.That(implementedBy.Select(c => c.GetProperty("from").GetString()),
            Does.Contain("Game.Sword.Attack()"));
    }

    [Test]
    public async ValueTask FindReferences_FindsTypeUsages()
    {
        var json = await _fixture.UseCase.FindReferencesAsync("Game.IWeapon", limit: 100);
        using var document = JsonDocument.Parse(json);
        var references = document.RootElement.GetProperty("references");

        var implementedBy = references.GetProperty("implementedBy").EnumerateArray()
            .Select(c => c.GetProperty("from").GetString()).ToList();
        Assert.That(implementedBy, Does.Contain("Game.Sword"));

        var uses = references.GetProperty("uses").EnumerateArray()
            .Select(c => c.GetProperty("from").GetString()).ToList();
        Assert.That(uses, Does.Contain("Game.Player.weapon"));
    }

    [Test]
    public async ValueTask TraceCallGraph_Callers_FollowsInterfaceDispatch()
    {
        var json = await _fixture.UseCase.TraceCallGraphAsync(
            "Game.Sword.OnAttack", direction: "callers", maxDepth: 3);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement.GetProperty("root");

        // OnAttack <- Sword.Attack <- (via IWeapon.Attack) Player.Move <- Player.Update
        var attack = root.GetProperty("children")[0];
        Assert.That(attack.GetProperty("id").GetString(), Is.EqualTo("Game.Sword.Attack()"));

        var move = attack.GetProperty("children").EnumerateArray()
            .Single(c => c.GetProperty("id").GetString() == "Game.Player.Move()");
        Assert.That(move.GetProperty("viaBase").GetBoolean(), Is.True);

        var update = move.GetProperty("children").EnumerateArray()
            .Single(c => c.GetProperty("id").GetString() == "Game.Player.Update()");
        Assert.That(update.GetProperty("id").GetString(), Is.EqualTo("Game.Player.Update()"));
    }

    [Test]
    public async ValueTask TraceCallGraph_Callees_ListsInvokedMethods()
    {
        var json = await _fixture.UseCase.TraceCallGraphAsync(
            "Game.Player.Move", direction: "callees", maxDepth: 2);

        Assert.That(json, Does.Contain("Game.IWeapon.Attack()"));
        Assert.That(json, Does.Contain("UnityEngine.Debug.Log(object)"));
    }

    [Test]
    public void TraceCallGraph_ThrowsForInvalidDirection()
    {
        var exception = Assert.ThrowsAsync<ArgumentException>(
            async () => await _fixture.UseCase.TraceCallGraphAsync(
                "Game.Player.Move", direction: "sideways", maxDepth: 3));
        Assert.That(exception!.Message, Does.Contain("callers"));
    }

    [Test]
    public async ValueTask GetCodeSnippet_ResolvesGenericSymbolsWithoutTypeParameters()
    {
        _fixture.WriteScript("Assets/Scripts/Repository.cs",
            """
            namespace Game
            {
                public class Repository<T>
                {
                    public T2 Map<T2>(T2 value) => value;
                }
            }
            """);

        var typeJson = await _fixture.UseCase.GetCodeSnippetAsync("Game.Repository");
        Assert.That(typeJson, Does.Contain("Game.Repository\\u003CT\\u003E").Or.Contain("Game.Repository<T>"));

        var methodJson = await _fixture.UseCase.GetCodeSnippetAsync("Repository.Map");
        using var document = JsonDocument.Parse(methodJson);
        Assert.That(document.RootElement.GetProperty("id").GetString(),
            Is.EqualTo("Game.Repository<T>.Map<T2>(T2)"));
    }

    [Test]
    public async ValueTask Index_KeepsExplicitInterfaceImplementationsAndStaticConstructorsDistinct()
    {
        _fixture.WriteScript("Assets/Scripts/Collisions.cs",
            """
            namespace Game.Collisions
            {
                public interface IThing
                {
                    void Run();
                }

                public class Thing : IThing
                {
                    public Thing() { }
                    static Thing() { }

                    public void Run() { }
                    void IThing.Run() { }
                }
            }
            """);

        var json = await _fixture.UseCase.SearchSymbolsAsync(
            "*", kinds: null, filePattern: "Assets/Scripts/Collisions.cs", baseType: null, limit: 50);
        using var document = JsonDocument.Parse(json);
        var ids = document.RootElement.GetProperty("symbols").EnumerateArray()
            .Select(s => s.GetProperty("id").GetString()).ToList();

        Assert.That(ids, Does.Contain("Game.Collisions.Thing.Run()"));
        // Explicit interface implementations qualify the interface with its namespace.
        Assert.That(ids, Does.Contain("Game.Collisions.Thing.Game.Collisions.IThing.Run()"));
        Assert.That(ids, Does.Contain("Game.Collisions.Thing.Thing()"));
        Assert.That(ids, Does.Contain("Game.Collisions.Thing.cctor()"));
    }

    [Test]
    public async ValueTask TraceCallGraph_Callers_DoesNotTreatBaseQualifiedCallsAsDispatch()
    {
        _fixture.WriteScript("Assets/Scripts/BaseCalls.cs",
            """
            namespace Game.BaseCalls
            {
                public class Animal
                {
                    public virtual void Speak() { }
                }

                public class Dog : Animal
                {
                    public override void Speak()
                    {
                        base.Speak();
                    }
                }
            }
            """);

        var json = await _fixture.UseCase.TraceCallGraphAsync(
            "Game.BaseCalls.Dog.Speak", direction: "callers", maxDepth: 3);
        using var document = JsonDocument.Parse(json);

        // base.Speak() must not surface Dog.Speak as its own caller.
        Assert.That(document.RootElement.GetProperty("root").TryGetProperty("children", out _), Is.False);

        // The base-qualified call still appears as a regular call to Animal.Speak.
        var refsJson = await _fixture.UseCase.FindReferencesAsync("Game.BaseCalls.Animal.Speak", limit: 100);
        Assert.That(refsJson, Does.Contain("\"calls\""));
        Assert.That(refsJson, Does.Contain("Game.BaseCalls.Dog.Speak()"));
    }

    [Test]
    public async ValueTask FindReferences_FindsInterfaceImplementationOnBaseClass()
    {
        _fixture.WriteScript("Assets/Scripts/InheritedImpl.cs",
            """
            namespace Game.Inherited
            {
                public interface IRunner
                {
                    void Run();
                }

                public class RunnerBase
                {
                    public void Run() { }
                }

                public class Runner : RunnerBase, IRunner
                {
                }
            }
            """);

        var json = await _fixture.UseCase.FindReferencesAsync("Game.Inherited.IRunner.Run", limit: 100);
        Assert.That(json, Does.Contain("Game.Inherited.RunnerBase.Run()"));
    }

    [Test]
    public async ValueTask FindReferences_TracksDelegateFieldInvocationAsUsage()
    {
        _fixture.WriteScript("Assets/Scripts/DelegateHolder.cs",
            """
            namespace Game.Delegates
            {
                public class DelegateHolder
                {
                    public System.Action onDone = () => { };

                    public void Fire()
                    {
                        onDone();
                    }
                }
            }
            """);

        var json = await _fixture.UseCase.FindReferencesAsync("DelegateHolder.onDone", limit: 100);
        using var document = JsonDocument.Parse(json);
        var uses = document.RootElement.GetProperty("references").GetProperty("uses").EnumerateArray()
            .Select(u => u.GetProperty("from").GetString()).ToList();
        Assert.That(uses, Does.Contain("Game.Delegates.DelegateHolder.Fire()"));
    }

    [Test]
    public async ValueTask FindReferences_RecordsUnresolvedCallsByName()
    {
        _fixture.WriteScript("Assets/Scripts/Unresolved.cs",
            """
            namespace Game.Unresolved
            {
                public class Local
                {
                    public void DoThing() { }
                }

                public class Caller
                {
                    public void Go()
                    {
                        UnknownLib.Helper.DoThing();
                    }
                }
            }
            """);

        var json = await _fixture.UseCase.FindReferencesAsync("Game.Unresolved.Local.DoThing", limit: 100);
        using var document = JsonDocument.Parse(json);
        var unresolved = document.RootElement.GetProperty("references").GetProperty("unresolvedCalls")
            .EnumerateArray().Select(u => u.GetProperty("from").GetString()).ToList();
        Assert.That(unresolved, Does.Contain("Game.Unresolved.Caller.Go()"));
    }

    [Test]
    public async ValueTask TraceCallGraph_MarksCycles()
    {
        _fixture.WriteScript("Assets/Scripts/PingPong.cs",
            """
            namespace Game.Cycles
            {
                public class PingPong
                {
                    public void Ping() { Pong(); }
                    public void Pong() { Ping(); }
                }
            }
            """);

        var json = await _fixture.UseCase.TraceCallGraphAsync(
            "Game.Cycles.PingPong.Ping", direction: "callees", maxDepth: 5);
        Assert.That(json, Does.Contain("\"cycle\":true"));
    }

    [Test]
    public async ValueTask Limits_ReportTruncationWithoutDroppingSmallGroups()
    {
        var searchJson = await _fixture.UseCase.SearchSymbolsAsync(
            "*", kinds: null, filePattern: null, baseType: null, limit: 1);
        using (var document = JsonDocument.Parse(searchJson))
        {
            Assert.That(document.RootElement.GetProperty("truncated").GetBoolean(), Is.True);
            Assert.That(document.RootElement.GetProperty("totalMatches").GetInt32(), Is.GreaterThan(1));
            Assert.That(document.RootElement.GetProperty("symbols").GetArrayLength(), Is.EqualTo(1));
        }

        // IWeapon.Attack has both calls and implementedBy references; limit=1 must still
        // report the full total and the truncated flag.
        var refsJson = await _fixture.UseCase.FindReferencesAsync("Game.IWeapon.Attack", limit: 1);
        using (var document = JsonDocument.Parse(refsJson))
        {
            Assert.That(document.RootElement.GetProperty("truncated").GetBoolean(), Is.True);
            Assert.That(document.RootElement.GetProperty("totalReferences").GetInt32(), Is.GreaterThan(1));
        }
    }

    [Test]
    public async ValueTask Index_RefreshesWhenSourceFilesChange()
    {
        var before = await _fixture.UseCase.SearchSymbolsAsync(
            "LateJoiner", kinds: null, filePattern: null, baseType: null, limit: 10);
        Assert.That(before, Does.Contain("\"totalMatches\":0"));

        _fixture.WriteScript("Assets/Scripts/LateJoiner.cs",
            """
            namespace Game
            {
                public class LateJoiner { }
            }
            """);

        var after = await _fixture.UseCase.SearchSymbolsAsync(
            "LateJoiner", kinds: null, filePattern: null, baseType: null, limit: 10);
        Assert.That(after, Does.Contain("Game.LateJoiner"));
    }
}
