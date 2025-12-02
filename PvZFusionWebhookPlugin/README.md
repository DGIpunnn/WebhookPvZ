# PvZ Fusion Webhook Plugin

A BepInEx plugin for Plants vs Zombies Fusion that provides HTTP webhook functionality to control the game.

## Features

- HTTP server running on port 6969
- Spawn plants and zombies via HTTP requests
- Support for row, column, amount, and duration parameters
- Endpoints for spawning all plants or zombies

## Endpoints

### POST /spawn
Spawn a specific plant or zombie with parameters.

**Request Body:**
```json
{
  "action": "spawn",
  "type": "plant" or "zombie",
  "id": "Peashooter", // Plant or zombie ID
  "row": 2,           // Row (0-4)
  "col": 5,           // Column (0-8) for plants, ignored for zombies
  "amount": 3,        // Number of spawns
  "duration": 200     // Delay between spawns in ms
}
```

**Example:**
```bash
curl -X POST http://localhost:6969/spawn \
  -H "Content-Type: application/json" \
  -d '{
    "action": "spawn",
    "type": "plant",
    "id": "Peashooter",
    "row": 2,
    "col": 5,
    "amount": 1,
    "duration": 0
  }'
```

### GET /spawn/all/plants
Spawn all available plants at random positions.

**Example:**
```bash
curl http://localhost:6969/spawn/all/plants
```

### GET /spawn/all/zombies
Spawn all available zombies at random positions.

**Example:**
```bash
curl http://localhost:6969/spawn/all/zombies
```

## Installation

1. Make sure you have BepInEx installed for Plants vs Zombies Fusion
2. Copy `PvZFusionWebhookPlugin.dll` to your game's BepInEx plugins folder:
   `[PvZ Fusion Game Directory]/BepInEx/plugins/`
3. Launch the game - the webhook server will start automatically on port 6969
4. Send HTTP requests to control the game

## Supported Plant Types

- Peashooter, Sunflower, CherryBomb, Wallnut, PotatoMine, SnowPea, Chomper, Repeater
- Puffshroom, Sunshroom, Fumeshroom, GraveBuster, Hypnoshroom, Scaredyshroom, Iceberg
- Doomshroom, LilyPad, Squash, Threepeater, TangleKelp, Jalapeno, Spikeweed, Torchwood
- Tallnut, Seashroom, Plantern, Cactus, Blover, SplitPea, Starfruit, Pumpkin, Magnetshroom
- Cabbagepult, FlowerPot, KernelPult, CoffeeBean, Garlic, Umbrella, Marigold, MelonPult
- GatlingPea, TwinSunflower, Gloomshroom, Cattail, WinterMelon, GoldMagnet, Spikerock
- CobCannon, Imitater

## Supported Zombie Types

- NormalZombie, ConeheadZombie, PoleVaultingZombie, BucketheadZombie, NewspaperZombie
- ScreenDoorZombie, FootballZombie, DancingZombie, BackupDancer, DuckyTubeZombie, SnorkelZombie
- ZombieBobsledTeam, DolphinRiderZombie, JackintheBoxZombie, BalloonZombie, DiggerZombie
- PogoZombie, ZombieYeti, BungeeZombie, LadderZombie, CatapultZombie, Gargantuar, Imp

## Parameters

- `row`: Integer 0-4 (valid game rows)
- `col`: Integer 0-8 for plants (valid game columns), ignored for zombies
- `amount`: Number of spawns (1 for single, >1 for repeated spawns)
- `duration`: Delay between repeated spawns in milliseconds

## Notes

- The plugin starts automatically when the game launches
- The HTTP server runs on localhost:6969
- Invalid row/column positions will be ignored
- Spawn functions are called on the Unity main thread for thread safety