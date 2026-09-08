# Sonarr Fork: Per-Root-Folder Shared Recycle Bin

This fork adds a shared recycle bin that each *Sonarr* root folder can enable independently. Deleted or replaced media files are moved to a `.bin` folder on the same filesystem. The fork stays automatically synchronized with Radarr upstream, within a few hours; except failed tests require manual intervention.

## Changes

| Before | After |
| --- | --- |
| One global recycle-bin setting | A global switch plus a switch for each root folder |
| One fixed behavior | A global mode for upgrades, deletes, or both |
| Files removed permanently | Files moved to a shared `.bin` folder on their filesystem |

## Example

Given these Sonarr root folders:

- `/media/library/tv/anime`
- `/media/library/tv/kids`
- `/requests/library/tv`

A deleted file is moved as follows:

| Original file | Recycle-bin destination |
| --- | --- |
| `/media/library/tv/anime/Show/Season 1/episode.mkv` | `/media/.bin/library/tv/anime/Show/Season 1/episode.mkv` |
| `/media/library/tv/kids/Show/Season 1/episode.mkv` | `/media/.bin/library/tv/kids/Show/Season 1/episode.mkv` |
| `/requests/library/tv/Show/Season 1/episode.mkv` | `/requests/.bin/library/tv/Show/Season 1/episode.mkv` |

The path below the top-level folder is preserved.

## Behavior

The recycle bin is used only when all applicable settings allow the operation:

| Global switch | Root-folder switch | Mode | Result |
| --- | --- | --- | --- |
| Off | Any | Any | Permanent delete |
| On | Off | Any | Permanent delete |
| On | On | `Both` | Upgrades and deletes go to `.bin` |
| On | On | `UpgradesOnly` | Only upgrades go to `.bin` |
| On | On | `DeletesOnly` | Only deletes go to `.bin` |

## Defaults

- On a new installation, the recycle-bin is disabled by default;
- During migration, a non-empty legacy recycle-bin path enables the new global switch.
- The default recycle bin mode is `Both`.
- New root folders have the recycle bin enabled by default.
- Automatic cleanup is set to 7 days by default.

## Scope and support

This is a personal Sonarr fork. I build only the Docker image. Native releases are not provided, but the code remains intended to work on Sonarr's other supported platforms and architectures. Feel free to build those versions yourself.

## Contributions

Only PRs directly related to this fork's purpose will be considered. Unrelated changes should be submitted upstream.

Please discuss any proposed feature expansion before implementation begins. Bug fixes and maintenance changes must remain consistent with Sonarr upstream's code, structure, and established mechanisms.
