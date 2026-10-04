# NGT SignMaster styling rules

This document captures the sign-formatting contract used by NGT SignMaster for the Valheim 1.0.16 master layout.

## Normal sign format

```text
<#RGB><size=3>LABEL\n<size=5>EMOJI
```

## Decorative header format

```text
<#RGB><u><cspace=6><size=5>HEADER\n<size=3>EMOJI
```

## Biome palette

| Biome | Color |
|---|---|
| Meadows | `<#bf8>` |
| Black Forest | `<#8d8>` |
| Ocean | `<#8df>` |
| Swamp | `<#bc8>` |
| Mountain | `<#cdf>` |
| Plains | `<#fd8>` |
| Mistlands | `<#caf>` |
| Ashlands | `<#f86>` |
| Deep North | `<#9ef>` |

## Portal-hub palette

| Hub | Color |
|---|---|
| Merchants & Forge | `<#fd8>` |
| Boss Altars | `<#f86>` |
| Mining | `<#ccc>` |
| Dungeons & Quests | `<#c9f>` |
| Farms & Harvest | `<#afa>` |
| Settlements | `<#8df>` |
| Main Hub utility | `<#fff>` |

## Semantic tags

- `<u>` on a normal-size portal label means a hub-transfer / return portal.
- Every `MAIN HUB` return sign is white and underlined.
- `<i>` means a reserved future portal bay.
- `<s>` is reserved for a disabled/decommissioned destination.
- `<u><cspace=6><size=5>` is reserved for large decorative section headers.

## Hard constraints

- Use compact three-digit color tags.
- Use literal `\n`, not `<br>`.
- Do not emit closing tags.
- Do not emit `<b>`.
- Keep every generated stored sign string at or below 50 characters.
- Unknown or manually formatted signs are left alone by default.
- Portal connection names are never modified; SignMaster patches normal `Sign` text only.
