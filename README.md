# NGT SignMaster

NGT SignMaster is a client-side Valheim sign helper for the **1.0.16** sign layout used by NerdyGamerTools. Instead of copy/pasting rich-text strings, type a normal label and SignMaster applies the audited color, size, emoji, underline/italic state, and compact `\n` formatting automatically.

The mod targets normal world **signs only**. It deliberately does **not** modify portal connection tags.

## Current status

`0.1.0` is the first implementation pass. The repository contains the full extracted sign-style catalog and automated validation. In-game verification against Valheim 1.0.16 is still required before publishing a release package.

## Usage

For a label that has one unique style, type it normally:

```text
WOOD
HALDOR
HEALTH MEALS
DACKS LANDING
```

SignMaster converts the text to the exact master string when the sign is saved.

Some labels have more than one legitimate style. Use a short role prefix:

```text
P:MEADOWS
T:MEADOWS
H:MEADOWS
S:COPPER
P:COPPER
```

| Prefix | Meaning |
|---|---|
| `S:` or `STORAGE:` | Storage sign |
| `P:` or `PORTAL:` | Portal-adjacent sign |
| `H:` or `HEADER:` | Decorative header |
| `T:` or `TROPHY:` | Trophy box |

Prefixes are input helpers only; they are not stored after a successful match.

Need a known label to remain plain for one edit? Use:

```text
RAW:WOOD
```

The stored sign text becomes `WOOD` without applying a style.

## Why role prefixes exist

The master intentionally reuses some visible labels with different styles. `MEADOWS`, for example, has separate header, trophy, and portal versions. Mining labels such as `COPPER` also differ between storage and portal signs.

SignMaster automatically styles a bare label only when the answer is unambiguous. It will not guess and overwrite the wrong semantic style.

## Styling contract

The default catalog follows the supplied Valheim 1.0.16 master rules:

- compact three-digit color tags such as `<#bf8>`
- normal labels use `<size=3>` and emoji use `<size=5>`
- decorative headers reverse that hierarchy
- `\n` is used instead of `<br>`
- no closing tags
- no `<b>` markup
- portal return labels preserve underline semantics
- reserved future portal bays preserve italics
- every generated default string remains within the 50-character sign limit

See `docs/SIGN_RULES.md` for the compact styling specification and `docs/SOURCE.md` for the source master fingerprint used to generate the initial catalog.

## Safety behavior

SignMaster is conservative by design:

- unknown labels are left unchanged
- ambiguous bare labels are left unchanged
- known styled strings are idempotent and are not rewritten
- manually formatted rich text is preserved by default
- generated strings longer than the configured limit are rejected
- `TeleportWorld.SetText` is not patched, so portal pairing names are not styled

## Configuration

BepInEx creates the normal config file for plugin GUID `com.nerdygamertools.signmaster` after first launch.

| Setting | Default | Purpose |
|---|---:|---|
| `Enabled` | `true` | Master switch |
| `AutoStyleBareLabels` | `true` | Style unambiguous plain labels |
| `EnableRolePrefixes` | `true` | Allow `P:`, `H:`, `T:`, `S:` |
| `RestyleExistingRichText` | `false` | Allow replacement of manually formatted matching labels |
| `MaxStyledLength` | `50` | Maximum generated sign-string length |
| `VerboseLogging` | `false` | Log resolution decisions |

## Building

Requirements:

- Valheim 1.0.16 installation
- BepInExPack Valheim `5.4.2351`
- .NET SDK / Visual Studio capable of building `net48`

Copy the example build properties file:

```powershell
Copy-Item Directory.Build.props.example Directory.Build.props
```

Edit `VALHEIM_INSTALL` to the local Valheim directory, then build:

```powershell
dotnet build .\NGT-SIGNMASTER.sln -c Release
```

The project references BepInEx, Harmony, and Valheim/Unity assemblies directly from the local game installation. Those game assemblies are intentionally not committed.

## Runtime dependencies

- `denikson-BepInExPack_Valheim-5.4.2351`

Jötunn is **not required** by SignMaster itself.

## Validation

The sign catalog can be validated without proprietary game assemblies:

```bash
python tools/validate_sign_data.py
```

The current catalog contains:

- **285** unique role/style rules
- **256** unique visible labels
- **22** intentionally ambiguous labels
- maximum generated length: **50** characters

## Next verification pass

- Compile against a local Valheim 1.0.16 install.
- Verify `Sign.SetText` behavior in-game.
- Test multiplayer persistence and viewing from a client without SignMaster.
- Package the verified DLL for Thunderstore/release distribution.
