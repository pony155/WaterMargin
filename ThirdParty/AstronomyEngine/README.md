# Astronomy Engine

This directory vendors the C# implementation of
[Astronomy Engine](https://github.com/cosinekitty/astronomy) for deterministic,
offline WaterMargin builds.

- Upstream commit: `865d3da7d8112bbc7911238052c6af4aaf877181`
- Upstream package version: `2.1.19`
- License: MIT; see [`LICENSE`](LICENSE)
- Vendored files: unmodified upstream `source/csharp/astronomy.cs` and license

`AstronomyEngine.csproj` is WaterMargin-owned build integration. It targets the
repository's .NET baseline and disables nullable analysis for the unmodified
upstream source. Game code must consume Astronomy Engine through
`WaterMargin.Calendar` instead of exposing `CosineKitty` types as public game
contracts.

The WPF application project copies the upstream license into its build and
publish output under `ThirdPartyNotices` so distributed builds retain the
required notice.
