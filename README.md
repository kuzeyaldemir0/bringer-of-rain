# Bringer of Rain

Bringer of Rain is a 2D Unity action-platformer set in a drought-stricken desert aqueduct. Read old signs, restore sealed valves, survive guarded vaults, and defeat the demon warden to bring rain back to the village.

## Features

- Runtime-built 2D platforming level with three chapter-style areas
- Story prompts, readable signs, and a village elder NPC
- Water-themed player abilities, mana, projectiles, enemies, hazards, and checkpoints
- Desert tiles, villager sprites, full-screen story panels, rain effects, and a trophy moment
- Boss fight with recovery-window damage timing

## Controls

- Move: `A/D` or arrow keys
- Jump: `Space`
- Read/interact: `W`
- Water whip: left mouse / enter / `F`
- Burst: `Shift`
- Ice spear: right mouse / `E`
- Ice shard: middle mouse / `LMB2`

## Setup

This project uses Unity `6000.4.3f1`.

1. Clone the repository.
2. Open the project folder in Unity Hub.
3. Use Unity `6000.4.3f1` or a compatible Unity 6 version.
4. Open `SampleScene`.
5. Press Play.

## Project Notes

The level is constructed mostly through runtime scripts instead of a hand-authored tilemap scene. Most gameplay setup lives in `Assets/Scripts/DesertAqueductBootstrap.cs`, with supporting scripts for player movement, combat, enemies, story UI, audio, and progression.

## Credits

Created as a game jam project. Development included gameplay programming, level scripting, story integration, UI polish, and asset integration.
