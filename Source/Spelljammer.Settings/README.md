# Spelljammer.Settings

This headless project owns the versioned local player-preference profile,
validation, bounded JSON codec, transactional publication, and durable atomic
replacement. It contains no WPF, localization, SpriteForge, native pointers,
or authoritative campaign state.

The first profile contains bounded audio levels, subtitles, reduced motion,
screen shake, and UI scale. Campaign-affecting settings remain planned and must
enter the content-locked campaign save through an explicit schema migration.
