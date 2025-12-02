# Installation Guide for PvZFusionWebhookPlugin

This guide will help you install and set up the PvZFusionWebhookPlugin for Plants vs Zombies Fusion.

## Prerequisites

Before installing this plugin, make sure you have:

1. **BepInEx 6.x** installed for your Plants vs Zombies Fusion game
2. **.NET 6.0** runtime (included with BepInEx 6.x)
3. **Plants vs Zombies Fusion** game (version 3.1.1 or compatible)
4. **ToolModData.dll** (from PvZWebhook4 project or game installation)

## Installation Steps

### Step 1: Prepare Your Game Directory

1. Navigate to your Plants vs Zombies Fusion installation directory
2. Ensure BepInEx is properly installed (you should see a `BepInEx` folder)

### Step 2: Install the Plugin

1. Build the plugin by running:
   - On Windows: `build.bat`
   - On Linux/Mac: `chmod +x build.sh && ./build.sh`

2. Copy the compiled DLL file from `bin/Debug/` to your game's `BepInEx/plugins/` folder:
   ```
   [Game Directory]/
   └── BepInEx/
       ├── core/
       ├── plugins/          ← Place the plugin DLL here
       │   └── PvZFusionWebhookPlugin.dll
       └── config/
   ```

3. If you have the `ToolModData.dll` from the PvZWebhook4 project, place it in the `BepInEx/` root directory or ensure it's accessible to the game.

### Step 3: Configure the Plugin (Optional)

The plugin comes with a default configuration file. If you need to change settings:

1. Navigate to `BepInEx/config/`
2. Edit `webhook.pvzfusion.cfg` with a text editor
3. Modify settings as needed (port number, server address, etc.)

### Step 4: Launch the Game

1. Start your Plants vs Zombies Fusion game
2. The plugin should load automatically if BepInEx is working correctly
3. Check the BepInEx console/log for any errors or confirmation messages
4. The webhook server should start on port 6969

## Verification

To verify the plugin is working:

1. Open your web browser or use curl to test the endpoint:
   ```bash
   curl http://localhost:6969/
   ```
   
2. You should see a response indicating the server is running

3. Check the BepInEx logs for any messages from the webhook plugin

## Troubleshooting

### Plugin Not Loading
- Ensure BepInEx is properly installed
- Check that the DLL file is placed in the correct `BepInEx/plugins/` directory
- Verify all dependencies (ToolModData.dll) are available
- Check BepInEx logs for error messages

### Port Already in Use
- Modify the port in `BepInEx/config/webhook.pvzfusion.cfg`
- Default port is 6969

### Access Denied
- Make sure you're running the game with proper permissions
- Check Windows Firewall settings if accessing from another machine

### HTTP Server Not Starting
- Ensure no other application is using port 6969
- Check if antivirus software is blocking the connection

## Usage

Once installed, you can send HTTP requests to control the game:

### Spawn a Plant
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

### Spawn a Zombie
```bash
curl -X POST http://localhost:6969/spawn \
  -H "Content-Type: application/json" \
  -d '{
    "action": "spawn",
    "type": "zombie",
    "id": "NormalZombie",
    "row": 1,
    "col": 8
  }'
```

## Uninstallation

To remove the plugin:

1. Delete `PvZFusionWebhookPlugin.dll` from `BepInEx/plugins/`
2. Optionally delete `webhook.pvzfusion.cfg` from `BepInEx/config/`
3. Restart the game

## Support

If you encounter issues:

1. Check the BepInEx logs in `BepInEx/LogOutput.log`
2. Verify all dependencies are properly installed
3. Ensure your game version is compatible
4. Consult the API documentation in `API_DOCUMENTATION.md`