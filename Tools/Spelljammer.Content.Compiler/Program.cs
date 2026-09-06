using System.Text.Json;
using Spelljammer.Content.Compilation;
using Spelljammer.Content.Diagnostics;
using Spelljammer.Content.Manifests;
using Spelljammer.Content.Sources;

const int InvalidContentExitCode = 2;
const int IoFailureExitCode = 3;
const int UsageExitCode = 64;

if (args.Length < 2 || args[0] != "validate")
{
    Console.Error.WriteLine("Usage: Spelljammer.Content.Compiler validate <pack-root> [--game-version X.Y.Z] [--json]");
    return UsageExitCode;
}

SemanticVersion gameVersion = new(0, 1, 0);
bool json = false;
for (int index = 2; index < args.Length; index++)
{
    if (args[index] == "--json")
    {
        json = true;
    }
    else if (args[index] == "--game-version" && index + 1 < args.Length &&
             SemanticVersion.TryParse(args[++index], out SemanticVersion parsed))
    {
        gameVersion = parsed;
    }
    else
    {
        Console.Error.WriteLine("Invalid command option.");
        return UsageExitCode;
    }
}

GameContentCompiler compiler = new();
ContentCompilationResult result;
try
{
    result = compiler.Compile([new DirectoryContentPackSource(args[1])], gameVersion);
}
catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException or NotSupportedException)
{
    result = new ContentCompilationResult(null, [], new ContentIoFailure(ContentIoFailureKind.ReadFailed, null));
}
if (json)
{
    WriteJson(result);
}
else
{
    WriteText(result);
}

return result.Succeeded ? 0 : result.IoFailure is null ? InvalidContentExitCode : IoFailureExitCode;

static void WriteText(ContentCompilationResult result)
{
    if (result.Succeeded)
    {
        Console.WriteLine($"valid {result.Snapshot!.Fingerprint}");
        return;
    }

    if (result.IoFailure is not null)
    {
        Console.Error.WriteLine($"io-error {ToToken(result.IoFailure.Kind)} {result.IoFailure.RelativePath ?? "-"}");
    }

    foreach (ContentDiagnostic diagnostic in result.Diagnostics)
    {
        Console.Error.WriteLine(string.Join(' ', new[]
        {
            diagnostic.Code,
            diagnostic.PackId ?? "-",
            diagnostic.RelativePath ?? "-",
            diagnostic.DefinitionId ?? "-",
            diagnostic.PropertyPath ?? "-",
        }));
    }
}

static void WriteJson(ContentCompilationResult result)
{
    using Utf8JsonWriter writer = new(Console.OpenStandardOutput(), new JsonWriterOptions { Indented = false });
    writer.WriteStartObject();
    writer.WriteBoolean("valid", result.Succeeded);
    if (result.Snapshot is not null)
    {
        writer.WriteString("fingerprint", result.Snapshot.Fingerprint.ToString());
    }

    if (result.IoFailure is not null)
    {
        writer.WriteStartObject("ioFailure");
        writer.WriteString("kind", ToToken(result.IoFailure.Kind));
        if (result.IoFailure.RelativePath is not null)
        {
            writer.WriteString("relativePath", result.IoFailure.RelativePath);
        }

        writer.WriteEndObject();
    }

    writer.WriteStartArray("diagnostics");
    foreach (ContentDiagnostic diagnostic in result.Diagnostics)
    {
        writer.WriteStartObject();
        writer.WriteString("code", diagnostic.Code);
        writer.WriteString("severity", "error");
        WriteOptional(writer, "packId", diagnostic.PackId);
        WriteOptional(writer, "relativePath", diagnostic.RelativePath);
        WriteOptional(writer, "definitionId", diagnostic.DefinitionId);
        WriteOptional(writer, "propertyPath", diagnostic.PropertyPath);
        writer.WriteStartArray("arguments");
        foreach (ContentDiagnosticArgument argument in diagnostic.Arguments)
        {
            writer.WriteStartObject();
            writer.WriteString("kind", argument.Kind.ToString());
            writer.WriteString("value", argument.Value);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    writer.WriteEndArray();
    writer.WriteEndObject();
    writer.Flush();
    Console.WriteLine();
}

static void WriteOptional(Utf8JsonWriter writer, string property, string? value)
{
    if (value is not null)
    {
        writer.WriteString(property, value);
    }
}

static string ToToken(ContentIoFailureKind kind) => kind switch
{
    ContentIoFailureKind.NotFound => "not-found",
    ContentIoFailureKind.AccessDenied => "access-denied",
    ContentIoFailureKind.ChangedDuringRead => "changed-during-read",
    ContentIoFailureKind.ReadFailed => "read-failed",
    _ => "read-failed",
};
