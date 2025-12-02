# Changelog

All notable changes to the PvZFusionWebhookPlugin will be documented in this file.

## [1.1.0] - 2023-XX-XX

### Added
- Initial release of PvZFusionWebhookPlugin
- HTTP server functionality on port 6969
- POST /spawn endpoint for spawning plants and zombies
- GET /spawn/all/plants endpoint to spawn all available plants
- GET /spawn/all/zombies endpoint to spawn all available zombies
- Support for delayed spawning with duration parameter
- UnityMainThreadDispatcher for thread-safe game interactions
- Integration with ToolMod system for game compatibility
- JSON request/response handling
- Comprehensive API documentation
- Installation guide and build scripts

### Changed
- Designed specifically for Plants vs Zombies Fusion compatibility
- Uses CreatePlant and CreateZombie instances from ToolMod system
- Implements proper error handling and validation
- Follows BepInEx 6.x plugin architecture

### Fixed
- Proper thread safety for Unity game engine interactions
- Correct coordinate mapping for plant/zombie spawning
- Proper game resource management through GameAPP

## Features

### HTTP Endpoints
- POST /spawn: Spawn individual plants/zombies with configurable parameters
- GET /spawn/all/plants: Spawn all available plant types
- GET /spawn/all/zombies: Spawn all available zombie types

### Parameters Support
- Type: plant or zombie
- ID: Internal game name or ID
- Row: 0-4 (5 rows in game)
- Col: 0-8 (9 columns in game)
- Amount: Number of spawns with optional delay
- Duration: Delay between multiple spawns in milliseconds

### Compatibility
- Designed for Plants vs Zombies Fusion
- Compatible with BepInEx 5.x and 6.x
- Requires ToolModData for game integration
- Supports IL2CPP Unity engine