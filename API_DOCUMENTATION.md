# PvZFusionWebhookPlugin API Documentation

## Overview
This plugin provides an HTTP server that allows external applications to control the Plants vs Zombies Fusion game by spawning plants and zombies via webhook calls. The server runs on port 6969 by default.

## Endpoints

### POST /spawn
Spawn a specific plant or zombie.

#### Request
- Method: `POST`
- URL: `http://localhost:6969/spawn`
- Content-Type: `application/json`

#### Request Body
```json
{
  "action": "spawn",
  "type": "plant|zombie",
  "id": "plant_or_zombie_name|id",
  "row": 0-4,
  "col": 0-8,
  "amount": 1,
  "duration": 0
}
```

#### Parameters
- `action`: Must be "spawn"
- `type`: Either "plant" or "zombie"
- `id`: The internal name or ID of the plant/zombie (case-sensitive)
- `row`: Row position (0-4, 5 rows total)
- `col`: Column position (0-8, 9 columns total)
- `amount`: Number of plants/zombies to spawn (default: 1)
- `duration`: Delay in milliseconds between spawns when amount > 1 (default: 0)

#### Response
- Success: `{"success":true,"message":"Spawn request processed"}`
- Error: `{"error":"error message"}`

#### Example Request
```bash
curl -X POST http://localhost:6969/spawn \
  -H "Content-Type: application/json" \
  -d '{
    "action": "spawn",
    "type": "plant",
    "id": "Peashooter",
    "row": 2,
    "col": 5,
    "amount": 3,
    "duration": 1000
  }'
```

### GET /spawn/all/plants
Spawn all available plants at default positions.

#### Request
- Method: `GET`
- URL: `http://localhost:6969/spawn/all/plants`

#### Response
- Success: `{"success":true,"message":"All plants spawned"}`
- Error: `{"error":"error message"}`

#### Example Request
```bash
curl http://localhost:6969/spawn/all/plants
```

### GET /spawn/all/zombies
Spawn all available zombies at default positions.

#### Request
- Method: `GET`
- URL: `http://localhost:6969/spawn/all/zombies`

#### Response
- Success: `{"success":true,"message":"All zombies spawned"}`
- Error: `{"error":"error message"}`

#### Example Request
```bash
curl http://localhost:6969/spawn/all/zombies
```

## Error Responses

The API returns the following error codes:
- `400 Bad Request`: Invalid request parameters
- `404 Not Found`: Endpoint does not exist
- `500 Internal Server Error`: Internal error occurred

## Valid Plant Types

The plugin supports all plant types available in Plants vs Zombies Fusion:
- Peashooter, Sunflower, CherryBomb, Wallnut, PotatoMine, SnowPea, Chomper, Repeater
- Puffshroom, Sunshroom, Fumeshroom, GraveBuster, Hypnoshroom, Scaredyshroom
- Iceberg, Doomshroom, LilyPad, Squash, Threepeater, TangleKelp, Jalapeno
- Spikeweed, Torchwood, Tallnut, Seashroom, Plantern, Cactus, Blover
- SplitPea, Starfruit, Pumpkin, Magnetshroom, Cabbagepult, FlowerPot
- KernelPult, CoffeeBean, Garlic, Umbrella, Marigold, MelonPult
- GatlingPea, TwinSunflower, Gloomshroom, Cattail, WinterMelon
- GoldMagnet, Spikerock, CobCannon, Imitater, and more

## Valid Zombie Types

The plugin supports all zombie types available in Plants vs Zombies Fusion:
- NormalZombie, ConeheadZombie, PoleVaultingZombie, BucketheadZombie
- NewspaperZombie, ScreenDoorZombie, FootballZombie, DancingZombie
- BackupDancer, DuckyTubeZombie, SnorkelZombie, ZombieBobsledTeam
- DolphinRiderZombie, JackintheBoxZombie, BalloonZombie, DiggerZombie
- PogoZombie, ZombieYeti, BungeeZombie, LadderZombie, CatapultZombie
- Gargantuar, Imp, and more

## Implementation Details

The plugin integrates with the existing PvZ Fusion game functions using the CreatePlant and CreateZombie instances from the ToolMod system. All spawn operations are thread-safe and execute on the main Unity thread through a custom UnityMainThreadDispatcher.

The plugin uses the same data structures and communication patterns as the PvZWebhook4 project, ensuring compatibility with the game's internal systems.