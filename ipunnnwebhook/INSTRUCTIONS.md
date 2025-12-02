# PvZFusionWebhookPlugin - Installation and Usage Instructions

## Overview
This BepInEx plugin for Plants vs Zombies Fusion provides webhook functionality to remotely control plant and zombie spawning via HTTP requests.

## Features
- HTTP webhook server running on port 6969
- Spawn plants and zombies with custom parameters
- Support for delayed/repeated spawning
- Endpoints for spawning all plants or zombies at once

## Plugin Files
1. `PvZFusionWebhookPlugin.cs` - Main plugin source code
2. `PvZFusionWebhookPlugin.csproj` - Project configuration file
3. `build.sh` - Build script

## Compilation Instructions

### Prerequisites
- .NET SDK 4.6 or higher
- BepInEx framework
- Plants vs Zombies Fusion game files

### Steps
1. Install .NET SDK (version 4.6+)
2. Place required game DLLs in the `lib/` folder:
   - `Assembly-CSharp.dll`
   - `UnityEngine.dll`
   - `BepInEx.dll`
   - `ToolModData.dll`
3. Run the build script:
   ```bash
   chmod +x build.sh
   ./build.sh
   ```

### Alternative Build Method
If the build script fails, you can build manually:
```bash
dotnet restore
dotnet build --configuration Debug
```

## Endpoints

### POST /spawn
Spawn a plant or zombie with custom parameters

Request body:
```json
{
  "action": "spawn",
  "type": "plant" or "zombie",
  "id": "Peashooter",
  "row": 2,
  "col": 5,
  "amount": 3,
  "duration": 200
}
```

Parameters:
- `type`: "plant" or "zombie"
- `id`: Name or ID of the plant/zombie type
- `row`: Grid row (1-5)
- `col`: Grid column (1-9)
- `amount`: Number of spawns (default: 1)
- `duration`: Delay between spawns in milliseconds (default: 0)

### GET /spawn/all/plants
Spawn all available plants at default positions

### GET /spawn/all/zombies
Spawn all available zombies at default positions

## Installation

1. Compile the plugin using the build script
2. Locate the compiled DLL at `bin/Debug/net46/PvZFusionWebhookPlugin.dll`
3. Copy the DLL to your PvZ Fusion game's BepInEx plugins folder:
   ```
   [PvZ Fusion Game Directory]/BepInEx/plugins/PvZFusionWebhookPlugin.dll
   ```
4. Launch the game - the webhook server will start automatically

## Usage Examples

### Spawn a single Peashooter at row 2, column 5:
```bash
curl -X POST http://localhost:6969/spawn \
  -H "Content-Type: application/json" \
  -d '{
    "action": "spawn",
    "type": "plant",
    "id": "Peashooter",
    "row": 2,
    "col": 5
  }'
```

### Spawn 3 Zombies with 500ms delay between each:
```bash
curl -X POST http://localhost:6969/spawn \
  -H "Content-Type: application/json" \
  -d '{
    "action": "spawn",
    "type": "zombie",
    "id": "NormalZombie",
    "row": 3,
    "col": 9,
    "amount": 3,
    "duration": 500
  }'
```

### Spawn all plants:
```bash
curl http://localhost:6969/spawn/all/plants
```

## Troubleshooting

1. **Port already in use**: Make sure no other application is using port 6969
2. **Plugin not loading**: Check BepInEx logs for errors
3. **Invalid plant/zombie types**: Use exact names from the game's enums
4. **Grid bounds**: Rows must be 1-5, columns must be 1-9

## Notes
- The plugin runs automatically when the game starts
- All spawn operations are properly synchronized with Unity's main thread
- Out-of-bounds coordinates will be rejected
- The server runs in a background thread to avoid blocking game execution