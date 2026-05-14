# Rimworld Arm-Wrestling

A RimWorld 1.6 mod by Goldzar that adds an arm wrestling table to the game.

## Features

- Build an **Arm Wrestling Table** from the Joy construction category
- Two colonists use it together during recreation time for social joy
- Fully rotatable — place it facing any of the 4 cardinal directions with unique pixel art for each
- **Win/loss tracking** — each colonist's all-time record is shown in their Health tab
- **Mood effects** — winner gets a mood boost, loser takes a small hit
- **Over the Top trait** — rare trait that gives a colonist faster joy gain and a win advantage at the table
- **Risk of injury** — small chance (~1%) to fracture an arm during a match

## Cost

| Material | Amount |
|----------|--------|
| Steel    | 80     |
| Cloth    | 30     |

## How It Works

When a colonist needs recreation, the joy system can direct them to an arm wrestling table. The game automatically finds a free adult partner, seats both pawns at opposite ends of the table, and runs a social joy session.

At the end of a match a winner is determined based on each pawn's **Manipulation** capacity, their **win streak bonus** (capped at +25%), and whether they have the **Over the Top** trait (+20%). A random roll is applied so upsets are always possible.

- Winner gets a **+6 mood** moodlet for half a day
- Loser gets a **-3 mood** moodlet for half a day
- Both pawns gain a persistent **Arm Wrestling Record** entry in their Health tab (e.g. `5W / 2L`)

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
