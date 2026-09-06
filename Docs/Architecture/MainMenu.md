# Main menu architecture

## Implemented slice

Spelljammer starts in `MainMenuWindow`. Its current actions are Game Settings
and Quit Game; it intentionally does not advertise Continue, New Game, or the
older expedition prototype while those flows are not connected.

The authored background is:

```text
Content/Packs/base/Assets/UI/MainMenu/Background.png
```

The WPF project links that exact base-pack file as a compiled application
resource. `SpriteForgeMainMenuView` loads it without filesystem discovery and
draws it edge-to-edge with aspect-preserving cover scaling. The main window
opens maximized. The background contains no interactive or localized text.

## UI ownership

SpriteForge UI interop version 1 owns the retained main-menu grouping, fixed
logical layout, button state, focus, modal trapping, hit testing,
pointer/keyboard navigation, and copied stable actions. The managed WPF host
realizes the authored image, copied solid presentation commands, focus outline,
and game-localized text. The menu grouping and button fills are transparent so
the labels appear directly over the authored background; the focus outline
remains visible for keyboard navigation. This mirrors the settings-dialog boundary
until SpriteForge exposes direct managed-host texture presentation.

The logical menu canvas is 1280 by 720 pixels. It is uniformly scaled and
centered inside the client area, while the background independently uses cover
scaling so resizing does not distort the artwork or controls. The dark right
side of the composition holds the transparent menu labels and preserves the
ship silhouette on the left.

A localized version label is right-aligned at the bottom-right of the logical
canvas. Its value comes from the WPF application's informational-version
metadata with build metadata removed; `VersionPrefix` is currently `0.1.0`.
This keeps the visible version aligned with the executable rather than with an
independent presentation constant.

## Localization and lifecycle

Player-visible strings are authored in the `en-US` and `fr-FR` menu/settings
catalogs under `Content/Packs/base/Localization`. The offline compiler builds
all four catalogs before WPF compilation and embeds the artifacts. The
application stages and publishes both complete namespaces in the selected
locale on the UI thread before constructing the menu. Applying a language
change republishes the catalogs and rebuilds the retained menu document.

Game Settings opens the existing owner-modal settings dialog. Quit Game emits
a copied stable action and requests ordinary application shutdown. Closing the
operating-system window has the same shutdown result. Native UI documents are
destroyed on their owner thread when the window unloads.

The base content manifest does not yet declare a general runtime asset root.
This one background has an explicit build link; generalized pack asset loading,
hot reload, and mod-provided presentation assets remain planned.
