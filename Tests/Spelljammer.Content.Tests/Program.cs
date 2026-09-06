using System.Text;
using System.Text.Json;
using Spelljammer.Content;
using Spelljammer.Content.Compilation;
using Spelljammer.Content.Manifests;
using Spelljammer.Content.Sources;
using Spelljammer.Simulation.Content;

return ContentContracts.Run();

internal static class ContentContracts
{
    private static readonly SemanticVersion GameVersion = new(0, 1, 0);
    private static readonly string FixtureRoot = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Milestone0");

    public static int Run()
    {
        StableIdsAreValidatedAndOrdinal();
        ValidFixtureIsCanonicalAndDeterministic();
        EveryFrozenDiagnosticCaseIsRecognized();
        FailedReplacementPreservesPublishedSnapshot();
        Console.WriteLine("Content foundation contracts passed.");
        return 0;
    }

    private static void StableIdsAreValidatedAndOrdinal()
    {
        True(ContentId.TryParse("attribute.strength", out ContentId valid), "A valid ID was rejected.");
        False(ContentId.TryParse("Attribute.Strength", out _), "A culture-sensitive ID was accepted.");
        False(default(ContentId).IsValid, "The default ID became valid.");
        string maximum = "domain." + new string('a', 120);
        Equal(ContentId.MaximumLength, maximum.Length, "Maximum-length fixture is wrong.");
        True(ContentId.TryParse(maximum, out _), "A maximum-length ID was rejected.");
        False(ContentId.TryParse(maximum + "a", out _), "An oversized ID was accepted.");
        True(valid.CompareTo(new ContentId("attribute.toughness")) < 0, "ID comparison was not ordinal.");
    }

    private static void ValidFixtureIsCanonicalAndDeterministic()
    {
        string baseRoot = Path.Combine(FixtureRoot, "valid", "base");
        GameContentCompiler compiler = new();
        ContentCompilationResult first = compiler.Compile([new DirectoryContentPackSource(baseRoot)], GameVersion);
        ContentCompilationResult second = compiler.Compile([new DirectoryContentPackSource(baseRoot)], GameVersion);
        True(first.Succeeded, Primary(first));
        True(second.Succeeded, Primary(second));

        using JsonDocument expectedFingerprint = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(FixtureRoot, "expected", "fingerprints.json")));
        string expectedHash = expectedFingerprint.RootElement.GetProperty("sha256").GetString()!;
        Equal(expectedHash, first.Snapshot!.Fingerprint.ToString(), "Canonical fingerprint changed.");
        Equal(first.Snapshot.Fingerprint, second.Snapshot!.Fingerprint, "Repeated compilation was not deterministic.");
        byte[] expectedCanonical = File.ReadAllBytes(Path.Combine(FixtureRoot, "expected", "canonical-semantic.json"));
        True(expectedCanonical.AsSpan().SequenceEqual(first.Snapshot.CanonicalSemanticContent.AsSpan()), "Canonical semantic bytes changed.");
    }

    private static void EveryFrozenDiagnosticCaseIsRecognized()
    {
        using JsonDocument casesDocument = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(FixtureRoot, "invalid", "cases.json")));
        Dictionary<string, byte[]> baseFiles = ReadFiles(Path.Combine(FixtureRoot, "valid", "base"));
        foreach (JsonElement testCase in casesDocument.RootElement.GetProperty("cases").EnumerateArray())
        {
            Dictionary<string, byte[]> files = baseFiles.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray(), StringComparer.Ordinal);
            List<string> duplicateEntries = [];
            ApplyMutations(testCase, files, duplicateEntries);
            List<IContentPackSource> sources = [new MemoryPackSource(files, duplicateEntries)];
            if (testCase.TryGetProperty("additionalPacks", out JsonElement additionalPacks))
            {
                foreach (JsonElement pack in additionalPacks.EnumerateArray())
                {
                    Dictionary<string, byte[]> packFiles = [];
                    foreach (JsonElement file in pack.GetProperty("files").EnumerateArray())
                    {
                        packFiles.Add(file.GetProperty("path").GetString()!, Encoding.UTF8.GetBytes(file.GetProperty("text").GetString()!));
                    }

                    sources.Add(new MemoryPackSource(packFiles, []));
                }
            }

            SemanticVersion gameVersion = testCase.TryGetProperty("gameVersion", out JsonElement versionElement)
                ? ParseVersion(versionElement.GetString()!)
                : GameVersion;
            ContentLimits limits = testCase.TryGetProperty("limitOverrides", out JsonElement overrides)
                ? ContentLimits.Version1 with { ManifestBytes = overrides.GetProperty("manifestBytes").GetInt32() }
                : ContentLimits.Version1;
            ContentCompilationResult result = new GameContentCompiler(limits).Compile(sources, gameVersion);
            string expected = testCase.GetProperty("expectedPrimaryDiagnostic").GetString()!;
            Equal(expected, result.Diagnostics.FirstOrDefault()?.Code ?? "<none>",
                $"Wrong primary diagnostic for '{testCase.GetProperty("id").GetString()}'.");
        }
    }

    private static void FailedReplacementPreservesPublishedSnapshot()
    {
        GameContentRegistry registry = new();
        string baseRoot = Path.Combine(FixtureRoot, "valid", "base");
        ContentCompilationResult valid = registry.CompileAndPublish([new DirectoryContentPackSource(baseRoot)], GameVersion);
        True(valid.Succeeded, Primary(valid));
        GameContentSnapshot published = registry.Current!;
        ContentCompilationResult invalid = registry.CompileAndPublish([new MemoryPackSource([], [])], GameVersion);
        False(invalid.Succeeded, "An invalid replacement was published.");
        True(ReferenceEquals(published, registry.Current), "Failed replacement changed the registry owner.");
    }

    private static void ApplyMutations(JsonElement testCase, Dictionary<string, byte[]> files, List<string> duplicateEntries)
    {
        if (testCase.TryGetProperty("removeFiles", out JsonElement removals))
        {
            foreach (JsonElement removal in removals.EnumerateArray())
            {
                files.Remove(removal.GetString()!);
            }
        }

        if (testCase.TryGetProperty("writeText", out JsonElement writes))
        {
            foreach (JsonElement write in writes.EnumerateArray())
            {
                files[write.GetProperty("path").GetString()!] = Encoding.UTF8.GetBytes(write.GetProperty("text").GetString()!);
            }
        }

        if (testCase.TryGetProperty("writeHex", out JsonElement hexWrites))
        {
            foreach (JsonElement write in hexWrites.EnumerateArray())
            {
                files[write.GetProperty("path").GetString()!] = Convert.FromHexString(write.GetProperty("bytes").GetString()!);
            }
        }

        if (testCase.TryGetProperty("duplicateFiles", out JsonElement duplicates))
        {
            duplicateEntries.AddRange(duplicates.EnumerateArray().Select(item => item.GetString()!));
        }
    }

    private static Dictionary<string, byte[]> ReadFiles(string root) =>
        Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).ToDictionary(
            path => Path.GetRelativePath(root, path).Replace('\\', '/'), File.ReadAllBytes, StringComparer.Ordinal);

    private static SemanticVersion ParseVersion(string value)
    {
        True(SemanticVersion.TryParse(value, out SemanticVersion version), "Fixture game version is invalid.");
        return version;
    }

    private static string Primary(ContentCompilationResult result) =>
        result.Diagnostics.FirstOrDefault()?.Code ?? result.IoFailure?.Kind.ToString() ?? "Compilation failed without a diagnostic.";

    private static void True(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void False(bool condition, string message) => True(!condition, message);

    private static void Equal<T>(T expected, T actual, string message) where T : notnull
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{message} Expected '{expected}', got '{actual}'.");
        }
    }

    private sealed class MemoryPackSource(Dictionary<string, byte[]> files, IReadOnlyList<string> duplicates) : IContentPackSource
    {
        public IReadOnlyList<string> EnumerateFiles() => [.. files.Keys, .. duplicates];

        public byte[] ReadFile(string relativePath, int maximumBytes)
        {
            byte[] bytes = files[relativePath];
            if (bytes.Length > maximumBytes)
            {
                throw new ContentSourceLimitException(relativePath);
            }

            return bytes.ToArray();
        }
    }
}
