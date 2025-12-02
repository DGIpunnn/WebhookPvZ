# PvZFusionWebhookPlugin Architecture

This document describes the architecture and design decisions of the PvZFusionWebhookPlugin.

## Overview

The PvZFusionWebhookPlugin is a BepInEx plugin that provides HTTP webhook functionality for Plants vs Zombies Fusion. It allows external applications to control the game by sending HTTP requests to spawn plants and zombies.

## Architecture Components

### 1. HTTP Server Layer
- Runs on port 6969 by default
- Uses System.Net.HttpListener for HTTP communication
- Handles multiple endpoints (POST /spawn, GET /spawn/all/plants, GET /spawn/all/zombies)
- Processes JSON requests and returns JSON responses
- Runs in a separate thread to avoid blocking the game

### 2. Request Processing Layer
- Validates incoming requests
- Parses JSON data
- Validates parameters (row, column, type, ID)
- Maps string IDs to game-specific PlantType/ZombieType enums
- Handles error cases and returns appropriate responses

### 3. Game Integration Layer
- Uses CreatePlant.Instance and CreateZombie.Instance from ToolMod system
- Properly maps coordinates from webhook format to game format
- Ensures thread safety by executing game functions on the main Unity thread
- Uses UnityMainThreadDispatcher pattern for safe Unity interactions

### 4. Threading and Concurrency
- HTTP server runs on a background thread
- Game interactions execute on the main Unity thread via UnityMainThreadDispatcher
- Thread-safe queue for passing actions between threads
- Proper synchronization to prevent race conditions

## Design Patterns Used

### 1. Plugin Pattern (BepInEx)
- Inherits from BasePlugin
- Uses BepInPlugin attribute for identification
- Implements proper Load/Unload lifecycle

### 2. Singleton Pattern
- UnityMainThreadDispatcher singleton for centralized thread management
- Ensures only one instance of the dispatcher exists

### 3. Thread-Safe Queue Pattern
- Queue<System.Action> for passing actions between HTTP thread and Unity main thread
- Proper locking mechanisms to prevent race conditions

### 4. Observer Pattern
- HTTP listener observes incoming requests
- Processes and dispatches them accordingly

## Integration with PvZWebhook4

The plugin is designed to be compatible with the PvZWebhook4 reference project architecture:

- Uses similar data structures and naming conventions
- Integrates with ToolModData and ToolModBepInEx.PatchMgr
- Follows the same game resource access patterns
- Compatible with CreatePlant and CreateZombie instances

## Thread Safety Considerations

Unity games require all engine interactions to occur on the main thread. The plugin handles this by:

1. HTTP server runs on a background thread
2. When a spawn request is received, it's wrapped in an Action
3. The Action is queued to the UnityMainThreadDispatcher
4. The dispatcher executes the Action on the main Unity thread
5. Game functions (CreatePlant.Instance.SetPlant, CreateZombie.Instance.SetZombie) are called safely

## Error Handling

The plugin implements comprehensive error handling:

- HTTP error responses (400, 404, 500)
- Input validation (row/column bounds, valid types)
- JSON parsing error handling
- Graceful degradation when game instances are unavailable
- Detailed logging for debugging

## Configuration

The plugin supports configuration through BepInEx's configuration system:

- Port number configuration
- Server address binding
- Debug logging options
- Enable/disable functionality

## Dependencies

The plugin relies on several external components:

- BepInEx 5.x/6.x for plugin infrastructure
- Harmony for IL2CPP compatibility
- UnityEngine for Unity integration
- ToolModData for game-specific data structures
- System.Net.HttpListener for HTTP functionality
- System.Text.Json for JSON processing

## Future Enhancements

Potential areas for future development:

- Authentication and security features
- Additional game control endpoints
- Configuration via in-game UI
- Support for more game actions beyond spawning
- WebSocket support for real-time communication
- More detailed game state information endpoints