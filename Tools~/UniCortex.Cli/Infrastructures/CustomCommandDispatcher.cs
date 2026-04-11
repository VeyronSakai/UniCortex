using System.Globalization;
using System.Text.Json;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Cli.Infrastructures;

internal sealed class CustomCommandDispatcher(CustomToolUseCase customToolUseCase)
{
    internal async ValueTask<bool> TryRunAsync(string[] args, CancellationToken cancellationToken)
    {
        GetCustomToolsManifestResponse manifest;
        try
        {
            manifest = await customToolUseCase.GetManifestAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Environment.ExitCode = 1;
            return true;
        }

        var command = FindMatchingCommand(args, manifest.tools);
        if (command == null)
        {
            return false;
        }

        try
        {
            var commandArguments = args.Skip(GetCommandDepth(command.cliCommand)).ToArray();
            if (commandArguments.Any(static argument => argument is "-h" or "--help"))
            {
                WriteHelp(command);
                return true;
            }

            var argumentsJson = SerializeArguments(command, commandArguments);
            var result = await customToolUseCase.InvokeAsync(command.name, argumentsJson, cancellationToken);
            if (!string.IsNullOrEmpty(result))
            {
                Console.WriteLine(result);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Environment.ExitCode = 1;
        }

        return true;
    }

    private static CustomToolManifestEntry? FindMatchingCommand(string[] args, IEnumerable<CustomToolManifestEntry> tools)
    {
        CustomToolManifestEntry? bestMatch = null;
        var bestDepth = -1;

        foreach (var tool in tools.Where(static tool => tool.exposeToCli && !string.IsNullOrWhiteSpace(tool.cliCommand)))
        {
            var depth = GetCommandDepth(tool.cliCommand);
            if (depth <= bestDepth || args.Length < depth)
            {
                continue;
            }

            var commandTokens = tool.cliCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var isMatch = true;
            for (var index = 0; index < commandTokens.Length; index++)
            {
                if (!string.Equals(args[index], commandTokens[index], StringComparison.Ordinal))
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch)
            {
                bestMatch = tool;
                bestDepth = depth;
            }
        }

        return bestMatch;
    }

    private static string SerializeArguments(CustomToolManifestEntry command, string[] args)
    {
        var values = new Dictionary<string, object?>(StringComparer.Ordinal);
        var parametersByOption = command.parameters.ToDictionary(
            static parameter => ToCliOptionName(parameter.name),
            StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            if (!argument.StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"Unexpected argument '{argument}'. Custom CLI commands only support named options in the form --option value.");
            }

            string optionName;
            string? optionValue = null;
            var equalsIndex = argument.IndexOf('=');
            if (equalsIndex >= 0)
            {
                optionName = argument[2..equalsIndex];
                optionValue = argument[(equalsIndex + 1)..];
            }
            else
            {
                optionName = argument[2..];
            }

            if (!parametersByOption.TryGetValue(optionName, out var parameter))
            {
                throw new ArgumentException($"Unknown option '--{optionName}' for command '{command.cliCommand}'.");
            }

            if (parameter.type == "array")
            {
                optionValue ??= GetNextOptionValue(args, ref index, optionName);
                AddArrayValue(values, parameter, optionValue);
                continue;
            }

            if (parameter.type == "boolean")
            {
                if (optionValue == null)
                {
                    var nextIndex = index + 1;
                    if (nextIndex < args.Length && !args[nextIndex].StartsWith("--", StringComparison.Ordinal))
                    {
                        optionValue = args[nextIndex];
                        index++;
                    }
                    else
                    {
                        optionValue = "true";
                    }
                }
            }
            else
            {
                optionValue ??= GetNextOptionValue(args, ref index, optionName);
            }

            values[parameter.name] = ParseScalarValue(parameter.type, optionValue!, optionName);
        }

        foreach (var parameter in command.parameters.Where(static parameter => parameter.required))
        {
            if (!values.ContainsKey(parameter.name))
            {
                throw new ArgumentException(
                    $"Missing required option '--{ToCliOptionName(parameter.name)}' for command '{command.cliCommand}'.");
            }
        }

        return JsonSerializer.Serialize(values);
    }

    private static void AddArrayValue(
        IDictionary<string, object?> values,
        CustomToolParameterDefinition parameter,
        string value)
    {
        if (!values.TryGetValue(parameter.name, out var existing) || existing is not List<object?> list)
        {
            list = new List<object?>();
            values[parameter.name] = list;
        }

        list.Add(ParseScalarValue(parameter.itemType, value, ToCliOptionName(parameter.name)));
    }

    private static object ParseScalarValue(string type, string value, string optionName)
    {
        return type switch
        {
            "string" => value,
            "boolean" when bool.TryParse(value, out var booleanValue) => booleanValue,
            "integer" when int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integerValue) =>
                integerValue,
            "number" when double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture, out var numberValue) => numberValue,
            "boolean" => throw new ArgumentException(
                $"Option '--{optionName}' expects a boolean value ('true' or 'false')."),
            "integer" => throw new ArgumentException(
                $"Option '--{optionName}' expects an integer value."),
            "number" => throw new ArgumentException(
                $"Option '--{optionName}' expects a numeric value."),
            _ => throw new ArgumentException(
                $"Option '--{optionName}' uses unsupported manifest type '{type}'.")
        };
    }

    private static string GetNextOptionValue(string[] args, ref int index, string optionName)
    {
        var nextIndex = index + 1;
        if (nextIndex >= args.Length || args[nextIndex].StartsWith("--", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Option '--{optionName}' requires a value.");
        }

        index = nextIndex;
        return args[nextIndex];
    }

    private static void WriteHelp(CustomToolManifestEntry command)
    {
        Console.WriteLine($"Usage: {command.cliCommand}{FormatUsageSuffix(command.parameters)}");
        Console.WriteLine();
        Console.WriteLine(command.description);

        if (command.parameters.Length == 0)
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Options:");
        foreach (var parameter in command.parameters)
        {
            var optionName = $"--{ToCliOptionName(parameter.name)}";
            var typeDisplay = parameter.type == "array"
                ? $"<{parameter.itemType}>..."
                : parameter.type == "boolean"
                    ? "[true|false]"
                    : $"<{parameter.type}>";

            var requiredSuffix = parameter.required ? " (Required)" : string.Empty;
            var description = string.IsNullOrWhiteSpace(parameter.description)
                ? string.Empty
                : $"  {parameter.description}";

            Console.WriteLine($"  {optionName} {typeDisplay}{description}{requiredSuffix}");
        }
    }

    private static string FormatUsageSuffix(IEnumerable<CustomToolParameterDefinition> parameters)
    {
        var suffix = parameters.Select(static parameter =>
        {
            var option = $"--{ToCliOptionName(parameter.name)}";
            var value = parameter.type == "boolean"
                ? string.Empty
                : parameter.type == "array"
                    ? $" <{parameter.itemType}>"
                    : $" <{parameter.type}>";

            return parameter.required
                ? $" {option}{value}"
                : $" [{option}{value}]";
        });

        return string.Concat(suffix);
    }

    private static int GetCommandDepth(string cliCommand)
    {
        return cliCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private static string ToCliOptionName(string parameterName)
    {
        if (string.IsNullOrEmpty(parameterName))
        {
            return parameterName;
        }

        var characters = new List<char>(parameterName.Length + 4);
        for (var index = 0; index < parameterName.Length; index++)
        {
            var character = parameterName[index];
            if (char.IsUpper(character))
            {
                if (index > 0)
                {
                    characters.Add('-');
                }

                characters.Add(char.ToLowerInvariant(character));
            }
            else
            {
                characters.Add(character);
            }
        }

        return new string(characters.ToArray());
    }
}
