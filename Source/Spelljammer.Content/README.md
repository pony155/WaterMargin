# Spelljammer.Content

This project owns bounded filesystem discovery, strict JSON parsing, pack
ordering, source validation and linking, immutable snapshot compilation,
semantic fingerprinting, diagnostics, and transactional publication. It
references `Spelljammer.Simulation`, which owns stable gameplay ID wrappers and
immutable runtime definition types. Simulation never references this project
and performs no filesystem work.

Public namespaces are:

- `Spelljammer.Content` for versioned limits;
- `Spelljammer.Content.Compilation` for compilation results, snapshots,
  fingerprints, and registry publication;
- `Spelljammer.Content.Diagnostics` for bounded structured failures;
- `Spelljammer.Content.Manifests` for semantic versions and manifest contracts;
  and
- `Spelljammer.Content.Sources` for explicitly configured pack roots.

The related `Spelljammer.Simulation.Content` namespace contains the validated
stable-ID wrappers and immutable definitions that may cross into authoritative
gameplay. It has no loader or filesystem dependency.

`GameContentCompiler` builds an entire candidate before returning a snapshot.
`GameContentRegistry` publishes only successful candidates, so callers can
retain the prior snapshot after any content or I/O failure.
