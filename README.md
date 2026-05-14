# Rimworld Arm-Wrestling

A RimWorld 1.6 mod by Goldzar that adds an arm wrestling table to the game.

## Features

- Build an **Arm Wrestling Table** from the Joy construction category
- Two colonists will use it together during recreation time
- Grants social joy and encourages interaction between pawns
- Fully rotatable — place it facing any direction
- Custom pixel art textures for all 4 rotations

## Cost

| Material | Amount |
|----------|--------|
| Steel    | 80     |
| Cloth    | 30     |

## How It Works

When a colonist needs recreation, the joy system can direct them to an arm wrestling table. The game automatically finds a free partner colonist, seats both pawns on opposite sides of the table, and runs a social joy session. Both pawns face each other and gain social interaction bonuses throughout.

## Compatibility

- RimWorld 1.6
- No known conflicts with other mods
- Safe to add to existing saves

## Building & Development

Source code is in `Source/`. Requires .NET 4.7.2 and the RimWorld managed DLLs.

```bash
cd Source
dotnet build RimworldArmWrestling.csproj
```

Output DLL is written to `Assemblies/` automatically.

## Author

**Goldzar** — [GitHub](https://github.com/Goldzar35)
