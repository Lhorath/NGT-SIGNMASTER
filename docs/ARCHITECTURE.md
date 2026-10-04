# Architecture

## Scope

NGT SignMaster patches **only** `Sign.SetText`. It does not patch `TeleportWorld.SetText`, so portal connection tags remain untouched.

When a player commits text to a normal Valheim sign:

1. The Harmony prefix receives the text before vanilla persistence.
2. `SignStyleEngine` checks whether the text is already a known styled string.
3. `RAW:` bypasses styling and stores the remainder literally.
4. An explicit role prefix (`P:`, `H:`, `T:`, `S:`) is resolved first.
5. A bare label is automatically styled only if exactly one style exists for that label.
6. Ambiguous bare labels are left unchanged instead of guessing.
7. The generated text is rejected if it exceeds the configured maximum length.

## Roles

| Prefix | Role | Typical use |
|---|---|---|
| `S:` | Storage | Resource, Cookhouse, prepared food, boss, treasure, armor, weapon, utility boxes |
| `P:` | Portal | Signs placed beside portal destinations or hub transfers |
| `H:` | Header | Decorative room, biome, and sub-hub headers |
| `T:` | Trophy | Biome trophy storage |

Prefixes are input helpers only; they are not stored after a successful match.

## Why ambiguous labels are not guessed

Several labels intentionally have more than one valid style. `MEADOWS`, for example, has distinct header, trophy, and portal forms. The same issue exists for biome names, mining labels such as `COPPER`, and hub names such as `BIOMES`.

A wrong automatic choice would overwrite the semantic formatting from the master specification. Explicit role prefixes solve that with minimal typing.

## Data

`src/NGT.SignMaster/Resources/default-signs.tsv` contains the embedded runtime catalog extracted from the supplied Valheim 1.0.16 master sign document. `docs/SOURCE.md` records the source-file fingerprint and extraction counts.

The catalog is intentionally separate from the resolver code so future Valheim updates can revise sign data without rewriting the Harmony integration.
