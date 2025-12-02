下载请到：https://github.com/CarefreeSongs712/PVZRHTools/releases

# 纪念伟大的infinite75

自己比较菜，但试图去做吧。

## 修改器更新日志

### · 3.1.1-4.0.1
1. 适配3.1.1
2. 摆烂

### 往前的更新日志请去“修改器更新日志.txt”查看

# PVZRHTools

植物大战僵尸融合版修改器 by [@Infinite75](https://space.bilibili.com/672619350)  [@听雨夜荷](https://space.bilibili.com/3537110030092294)    
适配游戏版本3.1.1
已构建版本的链接在b站视频简介中

模组部分基于[MelonLoader](https://github.com/LavaGang/MelonLoader)与[BepInEx](https://github.com/BepInEx/BepInEx)
开发      
内置了听雨夜荷的花园修改器[pvzRH-GardenEditor](https://github.com/CarefreeSongs712/pvzRH-GardenEditor)
，并为其适配了动态id和贴图加载     
修改窗口使用了[HandyControl](https://github.com/HandyOrg/HandyControl), [FastHotKeyForWPF](https://github.com/Axvser/FastHotKeyForWPF)

融合版制作组：    
[@蓝飘飘fly](https://space.bilibili.com/3546619314178489) 请在此处下载游戏本体  
[@机鱼吐司](https://space.bilibili.com/85881762)   
[@梦珞呀](https://space.bilibili.com/270840380)    
[@蓝蝶蝶Starryfly](https://space.bilibili.com/27033629)

感谢[@MC屑鱼](https://space.bilibili.com/3493077316536784)(Github:[@SalmonCN-RH](https://github.com/SalmonCN-RH/))的技术支持

感谢[@高数带我飞](https://space.bilibili.com/1117414477)(Github:[@LibraHp](https://github.com/LibraHp/))的技术支持    

# PvZFusionWebhookPlugin

A BepInEx plugin for Plants vs Zombies Fusion that provides webhook functionality to spawn plants and zombies via HTTP requests.

## Features

- HTTP server running on port 6969
- Spawn individual plants and zombies via webhook calls
- Spawn all available plants or zombies at once
- Support for multiple spawns with delay intervals

## Endpoints

### POST /spawn
Spawn a specific plant or zombie.

Request body:
```json
{
  "action": "spawn",
  "type": "plant|zombie",
  "id": "plant_or_zombie_name|id",
  "row": 0-4,
  "col": 0-8,
  "amount": 1,
  "duration": 0 // delay in milliseconds between spawns
}
```

Example:
```json
{
  "action": "spawn",
  "type": "plant",
  "id": "Peashooter",
  "row": 2,
  "col": 5,
  "amount": 3,
  "duration": 1000
}
```

### GET /spawn/all/plants
Spawn all available plants at random positions.

### GET /spawn/all/zombies
Spawn all available zombies at random positions.

## Installation

1. Build the plugin using the provided .csproj file
2. Copy the compiled DLL to the BepInEx/Plugins folder of your Plants vs Zombies Fusion installation
3. Ensure you have the required dependencies (ToolModData.dll and other BepInEx libraries)

## Dependencies

- BepInEx 5.x or 6.x
- IL2CPP BepInEx framework
- ToolModData (from PvZWebhook4 project)
- HarmonyX

## Usage Example

You can use curl or any HTTP client to send requests to the plugin:

```bash
# Spawn a Peashooter at row 2, column 5
curl -X POST http://localhost:6969/spawn \n  -H "Content-Type: application/json" \n  -d '{
    "action": "spawn",
    "type": "plant",
    "id": "Peashooter",
    "row": 2,
    "col": 5
  }'

# Spawn 5 zombies with 500ms delay between each
curl -X POST http://localhost:6969/spawn \n  -H "Content-Type: application/json" \n  -d '{
    "action": "spawn",
    "type": "zombie",
    "id": "NormalZombie",
    "row": 1,
    "col": 8,
    "amount": 5,
    "duration": 500
  }'
```

## Notes

- The plugin integrates with the existing PvZ Fusion game functions for spawning
- Plant and zombie names are case-sensitive and must match the game's internal names
- Row values range from 0-4 (5 rows in the game)
- Column values range from 0-8 (9 columns in the game)
- All spawn operations are thread-safe and execute on the main Unity thread
