using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Extensibility;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class CustomExtensionRegistry
    {
        private static readonly HashSet<string> ReservedCliTopLevelCommands = new(StringComparer.Ordinal)
        {
            "asset",
            "component",
            "console",
            "custom",
            "editor",
            "game-view",
            "gameobject",
            "input",
            "menu",
            "prefab",
            "recorder",
            "scene",
            "scene-view",
            "screenshot",
            "test",
            "timeline"
        };

        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IReadOnlyDictionary<string, IUniCortexCustomTool> _tools;
        private readonly IReadOnlyDictionary<string, CustomToolManifestEntry> _manifestEntries;
        private readonly IReadOnlyList<IUniCortexCustomRoute> _routes;

        private CustomExtensionRegistry(
            IMainThreadDispatcher dispatcher,
            IReadOnlyDictionary<string, IUniCortexCustomTool> tools,
            IReadOnlyDictionary<string, CustomToolManifestEntry> manifestEntries,
            IReadOnlyList<IUniCortexCustomRoute> routes)
        {
            _dispatcher = dispatcher;
            _tools = tools;
            _manifestEntries = manifestEntries;
            _routes = routes;
        }

        internal static CustomExtensionRegistry Discover(IMainThreadDispatcher dispatcher, IEnumerable<Type> candidateTypes = null)
        {
            var tools = new Dictionary<string, IUniCortexCustomTool>(StringComparer.Ordinal);
            var manifestEntries = new Dictionary<string, CustomToolManifestEntry>(StringComparer.Ordinal);
            var cliCommands = new HashSet<string>(StringComparer.Ordinal);
            var routes = new Dictionary<(HttpMethodType method, string path), IUniCortexCustomRoute>();

            foreach (var candidateType in (candidateTypes ?? GetCandidateTypes()).Distinct())
            {
                if (candidateType.IsAbstract || candidateType.IsInterface)
                {
                    continue;
                }

                var isTool = typeof(IUniCortexCustomTool).IsAssignableFrom(candidateType);
                var isRoute = typeof(IUniCortexCustomRoute).IsAssignableFrom(candidateType);
                if (!isTool && !isRoute)
                {
                    continue;
                }

                if (!TryCreateInstance(candidateType, out var instance, out var instantiationError))
                {
                    Debug.LogError($"[UniCortex] Skipped custom extension '{candidateType.FullName}': {instantiationError}");
                    continue;
                }

                if (instance is IUniCortexCustomTool tool)
                {
                    CustomToolManifestEntry manifestEntry;
                    string toolError;
                    if (!TryCreateManifestEntry(candidateType, tool, out manifestEntry, out toolError))
                    {
                        Debug.LogError(
                            $"[UniCortex] Skipped custom tool '{candidateType.FullName}': {toolError}");
                        continue;
                    }

                    if (tools.ContainsKey(manifestEntry.name))
                    {
                        Debug.LogError(
                            $"[UniCortex] Skipped custom tool '{candidateType.FullName}': Tool name '{manifestEntry.name}' is already registered by another custom tool.");
                        continue;
                    }

                    tools.Add(manifestEntry.name, tool);

                    if (manifestEntry.exposeToCli && !cliCommands.Add(manifestEntry.cliCommand))
                    {
                        tools.Remove(manifestEntry.name);
                        Debug.LogError(
                            $"[UniCortex] Skipped custom tool '{candidateType.FullName}': CLI command '{manifestEntry.cliCommand}' is already registered by another custom tool.");
                        continue;
                    }

                    manifestEntries[manifestEntry.name] = manifestEntry;
                }

                if (instance is IUniCortexCustomRoute route)
                {
                    string normalizedPath;
                    string routeError;
                    if (!TryValidateRouteDefinition(route.Definition, out normalizedPath, out routeError))
                    {
                        Debug.LogError(
                            $"[UniCortex] Skipped custom route '{candidateType.FullName}': {routeError}");
                        continue;
                    }

                    var routeKey = (route.Definition.method, normalizedPath);
                    if (routes.ContainsKey(routeKey))
                    {
                        Debug.LogError(
                            $"[UniCortex] Skipped custom route '{candidateType.FullName}': Route already registered for {route.Definition.method} {normalizedPath}.");
                        continue;
                    }

                    routes.Add(routeKey, route);
                }
            }

            Debug.Log($"[UniCortex] Discovered {manifestEntries.Count} custom tools and {routes.Count} custom routes.");

            return new CustomExtensionRegistry(dispatcher, tools, manifestEntries, routes.Values.ToArray());
        }

        internal GetCustomToolsManifestResponse GetManifest()
        {
            var tools = _manifestEntries.Values
                .OrderBy(tool => tool.name, StringComparer.Ordinal)
                .ToArray();
            return new GetCustomToolsManifestResponse(tools);
        }

        internal bool HasTool(string toolName)
        {
            return _tools.ContainsKey(toolName);
        }

        internal Task<string> InvokeToolAsync(
            string toolName,
            string argumentsJson,
            CancellationToken cancellationToken = default)
        {
            if (!_tools.TryGetValue(toolName, out var tool))
            {
                throw new ArgumentException($"Custom tool '{toolName}' was not found.");
            }

            return _dispatcher.RunOnMainThreadAsync(
                () => tool.Invoke(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson),
                cancellationToken);
        }

        internal void RegisterRoutes(IRequestRouter router)
        {
            foreach (var route in _routes)
            {
                var definition = route.Definition;
                try
                {
                    router.Register(
                        definition.method,
                        NormalizePath(definition.path),
                        (context, cancellationToken) => HandleRouteAsync(route, context, cancellationToken));
                }
                catch (ArgumentException ex)
                {
                    Debug.LogError(
                        $"[UniCortex] Skipped custom route '{route.GetType().FullName}': {ex.Message}");
                }
            }
        }

        private async Task HandleRouteAsync(
            IUniCortexCustomRoute route,
            IRequestContext context,
            CancellationToken cancellationToken)
        {
            try
            {
                var request = new UniCortexCustomRouteRequest(
                    context.Path,
                    context.HttpMethod,
                    await context.ReadBodyAsync(),
                    context.GetQueryParameters()
                        .Select(parameter => new UniCortexQueryParameter(parameter.Key, parameter.Value))
                        .ToArray());

                var response = await _dispatcher.RunOnMainThreadAsync(
                    () => route.Handle(request),
                    cancellationToken);

                if (response == null)
                {
                    throw new InvalidOperationException("Custom route returned null.");
                }

                if (string.IsNullOrWhiteSpace(response.bodyJson))
                {
                    throw new InvalidOperationException("Custom route returned an empty JSON response body.");
                }

                await context.WriteResponseAsync(response.statusCode, response.bodyJson);
            }
            catch (ArgumentException ex)
            {
                await context.WriteResponseAsync(
                    HttpStatusCodes.BadRequest,
                    JsonUtility.ToJson(new ErrorResponse(ex.Message)));
            }
            catch (InvalidOperationException ex)
            {
                await context.WriteResponseAsync(
                    HttpStatusCodes.BadRequest,
                    JsonUtility.ToJson(new ErrorResponse(ex.Message)));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UniCortex] Custom route '{route.GetType().FullName}' failed: {ex}");
                await context.WriteResponseAsync(
                    HttpStatusCodes.InternalServerError,
                    JsonUtility.ToJson(new ErrorResponse(ex.Message)));
            }
        }

        private static bool TryCreateInstance(Type candidateType, out object instance, out string error)
        {
            try
            {
                instance = Activator.CreateInstance(candidateType, true);
                if (instance == null)
                {
                    throw new InvalidOperationException("Activator.CreateInstance returned null.");
                }

                error = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                instance = null;
                error = ex.Message;
                return false;
            }
        }

        private static bool TryCreateManifestEntry(
            Type candidateType,
            IUniCortexCustomTool tool,
            out CustomToolManifestEntry manifestEntry,
            out string error)
        {
            var definition = tool.Definition;
            if (definition == null)
            {
                manifestEntry = null;
                error = "Definition is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(definition.name))
            {
                manifestEntry = null;
                error = "Tool name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(definition.description))
            {
                manifestEntry = null;
                error = "Tool description is required.";
                return false;
            }

            if (!definition.exposeToCli && !definition.exposeToMcp)
            {
                manifestEntry = null;
                error = "At least one of exposeToCli or exposeToMcp must be true.";
                return false;
            }

            if (!TryBuildParameterDefinitions(candidateType, tool.ArgumentsType, out var parameters, out error))
            {
                manifestEntry = null;
                return false;
            }

            var cliCommand = string.Empty;
            if (definition.exposeToCli)
            {
                cliCommand = NormalizeCliCommand(definition.name, definition.cliCommand);
                if (!TryValidateCliCommand(cliCommand, out error))
                {
                    manifestEntry = null;
                    return false;
                }
            }

            manifestEntry = new CustomToolManifestEntry(
                definition.name,
                definition.description,
                cliCommand,
                definition.exposeToMcp,
                definition.exposeToCli,
                parameters);
            error = string.Empty;
            return true;
        }

        private static bool TryValidateRouteDefinition(
            UniCortexCustomRouteDefinition definition,
            out string normalizedPath,
            out string error)
        {
            if (definition == null)
            {
                normalizedPath = string.Empty;
                error = "Definition is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(definition.path))
            {
                normalizedPath = string.Empty;
                error = "Route path is required.";
                return false;
            }

            if (!definition.path.StartsWith("/", StringComparison.Ordinal))
            {
                normalizedPath = string.Empty;
                error = $"Route path '{definition.path}' must start with '/'.";
                return false;
            }

            normalizedPath = NormalizePath(definition.path);
            error = string.Empty;
            return true;
        }

        private static IEnumerable<Type> GetCandidateTypes()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(SafeGetTypes);
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                var types = new List<Type>();
                foreach (var type in ex.Types)
                {
                    if (type != null)
                    {
                        types.Add(type);
                    }
                }

                return types;
            }
        }

        private static bool TryBuildParameterDefinitions(
            Type candidateType,
            Type argumentsType,
            out CustomToolParameterDefinition[] parameters,
            out string error)
        {
            try
            {
                parameters = argumentsType
                    .GetFields(BindingFlags.Instance | BindingFlags.Public)
                    .Select(field => CreateParameterDefinition(field))
                    .ToArray();
                error = string.Empty;
                return true;
            }
            catch (NotSupportedException ex)
            {
                parameters = Array.Empty<CustomToolParameterDefinition>();
                error = ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                parameters = Array.Empty<CustomToolParameterDefinition>();
                error = $"Failed to inspect arguments for '{candidateType.FullName}': {ex.Message}";
                return false;
            }
        }

        private static CustomToolParameterDefinition CreateParameterDefinition(FieldInfo field)
        {
            if (TryMapScalarType(field.FieldType, out var parameterType))
            {
                return new CustomToolParameterDefinition(
                    field.Name,
                    parameterType,
                    field.IsDefined(typeof(UniCortexRequiredAttribute), inherit: true),
                    field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty);
            }

            if (TryGetCollectionItemType(field.FieldType, out var itemType) &&
                TryMapScalarType(itemType, out var itemTypeName))
            {
                return new CustomToolParameterDefinition(
                    field.Name,
                    "array",
                    field.IsDefined(typeof(UniCortexRequiredAttribute), inherit: true),
                    field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty,
                    itemTypeName);
            }

            throw new NotSupportedException(
                $"Field '{field.Name}' uses unsupported type '{field.FieldType.FullName}'. " +
                "Supported argument field types are string, bool, int, float, arrays, and List<T> of those primitives.");
        }

        private static bool TryMapScalarType(Type type, out string parameterType)
        {
            if (type == typeof(string))
            {
                parameterType = "string";
                return true;
            }

            if (type == typeof(bool))
            {
                parameterType = "boolean";
                return true;
            }

            if (type == typeof(int))
            {
                parameterType = "integer";
                return true;
            }

            if (type == typeof(float))
            {
                parameterType = "number";
                return true;
            }

            parameterType = string.Empty;
            return false;
        }

        private static bool TryGetCollectionItemType(Type collectionType, out Type itemType)
        {
            if (collectionType.IsArray)
            {
                var elementType = collectionType.GetElementType();
                if (elementType == null)
                {
                    itemType = typeof(object);
                    return false;
                }

                itemType = elementType;
                return true;
            }

            if (collectionType.IsGenericType &&
                collectionType.GetGenericTypeDefinition() == typeof(List<>))
            {
                itemType = collectionType.GetGenericArguments()[0];
                return true;
            }

            itemType = typeof(object);
            return false;
        }

        private static string NormalizeCliCommand(string toolName, string cliCommand)
        {
            var candidate = string.IsNullOrWhiteSpace(cliCommand)
                ? toolName.Replace('_', '-').ToLowerInvariant()
                : cliCommand.Trim();

            var tokens = candidate
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", tokens);
        }

        private static bool TryValidateCliCommand(string cliCommand, out string error)
        {
            if (string.IsNullOrWhiteSpace(cliCommand))
            {
                error = "CLI command is required when exposeToCli is true.";
                return false;
            }

            var tokens = cliCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                error = "CLI command must contain at least one command token.";
                return false;
            }

            if (ReservedCliTopLevelCommands.Contains(tokens[0]))
            {
                error = $"CLI command '{cliCommand}' uses reserved top-level command '{tokens[0]}'.";
                return false;
            }

            foreach (var token in tokens)
            {
                if (!IsLowerKebabCase(token))
                {
                    error = $"CLI command token '{token}' must use lower-kebab-case.";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        private static bool IsLowerKebabCase(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            if (!char.IsLower(token[0]) && !char.IsDigit(token[0]))
            {
                return false;
            }

            foreach (var character in token)
            {
                if (char.IsLower(character) || char.IsDigit(character) || character == '-')
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private static string NormalizePath(string path)
        {
            var trimmed = path.TrimEnd('/');
            return string.IsNullOrEmpty(trimmed) ? "/" : trimmed;
        }
    }
}
