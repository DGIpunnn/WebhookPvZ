using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using ToolModData;
using static ToolModBepInEx.PatchMgr;

namespace WebhookPvZFusion
{
    [BepInPlugin("id.webhook.pvzfusion", "Webhook PvZ Fusion", "1.1.0")]
    public class PvZFusionWebhookPlugin : BasePlugin
    {
        private Thread httpThread;
        private bool isRunning;
        private ManualLogSource logger;

        public override void Load()
        {
            logger = Logger;
            logger.LogInfo("Webhook PvZ Fusion plugin loaded");

            // Start the HTTP server in a separate thread
            httpThread = new Thread(StartHttpServer)
            {
                IsBackground = true,
                Name = "WebhookHttpServer"
            };
            isRunning = true;
            httpThread.Start();

            // Ensure plugin cleanup on shutdown
            Harmony harmony = new Harmony("id.webhook.pvzfusion");
            harmony.PatchAll();
        }

        private void StartHttpServer()
        {
            try
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:6969/");
                listener.Start();
                logger.LogInfo("Webhook server started on port 6969");

                while (isRunning)
                {
                    try
                    {
                        HttpListenerContext context = listener.GetContext();
                        ProcessRequest(context);
                    }
                    catch (Exception ex)
                    {
                        if (isRunning) // Only log if we're not shutting down
                        {
                            logger.LogError($"Error processing request: {ex.Message}");
                        }
                    }
                }

                listener.Close();
                logger.LogInfo("Webhook server stopped");
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to start HTTP server: {ex.Message}");
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            string method = context.Request.HttpMethod;
            string url = context.Request.Url.AbsolutePath;

            logger.LogInfo($"Received {method} request to {url}");

            try
            {
                if (method == "POST" && url == "/spawn")
                {
                    HandleSpawnRequest(context);
                }
                else if (method == "GET" && url == "/spawn/all/plants")
                {
                    HandleSpawnAllPlants(context);
                }
                else if (method == "GET" && url == "/spawn/all/zombies")
                {
                    HandleSpawnAllZombies(context);
                }
                else
                {
                    // Return 404 for unsupported endpoints
                    context.Response.StatusCode = 404;
                    string responseString = "{\"error\":\"Endpoint not found\"}";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error processing request: {ex.Message}");
                context.Response.StatusCode = 500;
                string responseString = "{\"error\":\"Internal server error\"}";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }

            context.Response.Close();
        }

        private void HandleSpawnRequest(HttpListenerContext context)
        {
            // Read the request body
            string requestBody = new StreamReader(context.Request.InputStream).ReadToEnd();
            logger.LogInfo($"Spawn request body: {requestBody}");

            try
            {
                // Parse the JSON request
                using JsonDocument doc = JsonDocument.Parse(requestBody);
                JsonElement root = doc.RootElement;

                if (!root.TryGetProperty("action", out JsonElement actionElement) || actionElement.GetString() != "spawn")
                {
                    context.Response.StatusCode = 400;
                    string responseString = "{\"error\":\"Invalid action\"}";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    return;
                }

                if (!root.TryGetProperty("type", out JsonElement typeElement))
                {
                    context.Response.StatusCode = 400;
                    string responseString = "{\"error\":\"Type is required\"}";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    return;
                }

                if (!root.TryGetProperty("id", out JsonElement idElement))
                {
                    context.Response.StatusCode = 400;
                    string responseString = "{\"error\":\"ID is required\"}";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    return;
                }

                // Get optional parameters with defaults
                int row = root.TryGetProperty("row", out JsonElement rowElement) ? rowElement.GetInt32() : 0;
                int col = root.TryGetProperty("col", out JsonElement colElement) ? colElement.GetInt32() : 0;
                int amount = root.TryGetProperty("amount", out JsonElement amountElement) ? amountElement.GetInt32() : 1;
                int duration = root.TryGetProperty("duration", out JsonElement durationElement) ? durationElement.GetInt32() : 0;

                string type = typeElement.GetString();
                string id = idElement.GetString();

                // Validate row and column
                if (row < 0 || row > 4 || col < 0 || col > 8)
                {
                    context.Response.StatusCode = 400;
                    string responseString = "{\"error\":\"Row or column out of bounds\"}";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    return;
                }

                // Convert id to PlantType or ZombieType based on type
                if (type.ToLower() == "plant")
                {
                    // Try to find the plant type by name or ID
                    PlantType? plantType = GetPlantTypeByName(id);
                    if (plantType.HasValue)
                    {
                        // Execute the spawn in the main thread
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            SpawnPlantsWithDelay(plantType.Value, row, col, amount, duration);
                        });
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        string responseString = "{\"error\":\"Invalid plant type\"}";
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        return;
                    }
                }
                else if (type.ToLower() == "zombie")
                {
                    // Try to find the zombie type by name or ID
                    ZombieType? zombieType = GetZombieTypeByName(id);
                    if (zombieType.HasValue)
                    {
                        // Execute the spawn in the main thread
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            SpawnZombiesWithDelay(zombieType.Value, row, col, amount, duration);
                        });
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        string responseString = "{\"error\":\"Invalid zombie type\"}";
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        return;
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                    string responseString = "{\"error\":\"Type must be plant or zombie\"}";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    return;
                }

                // Success response
                context.Response.StatusCode = 200;
                string responseStringSuccess = "{\"success\":true,\"message\":\"Spawn request processed\"}";
                byte[] bufferSuccess = System.Text.Encoding.UTF8.GetBytes(responseStringSuccess);
                context.Response.ContentLength64 = bufferSuccess.Length;
                context.Response.OutputStream.Write(bufferSuccess, 0, bufferSuccess.Length);
            }
            catch (JsonException)
            {
                context.Response.StatusCode = 400;
                string responseString = "{\"error\":\"Invalid JSON\"}";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error handling spawn request: {ex.Message}");
                context.Response.StatusCode = 500;
                string responseString = "{\"error\":\"Internal server error\"}";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
        }

        private void HandleSpawnAllPlants(HttpListenerContext context)
        {
            try
            {
                // Get all available plant types from the game
                var allPlants = GameAPP.resourcesManager.allPlants;
                
                foreach (var plantType in allPlants)
                {
                    // Execute the spawn in the main thread
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        // Spawn at a default position
                        if (CreatePlant.Instance != null)
                        {
                            CreatePlant.Instance.SetPlant(4, 1, plantType); // 0-indexed: col 4 (5th column), row 1 (2nd row)
                            logger.LogInfo($"Spawned plant {plantType}");
                        }
                        else
                        {
                            logger.LogError("CreatePlant.Instance is null");
                        }
                    });
                }

                context.Response.StatusCode = 200;
                string responseString = "{\"success\":true,\"message\":\"All plants spawned\"}";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error handling spawn all plants: {ex.Message}");
                context.Response.StatusCode = 500;
                string responseString = "{\"error\":\"Internal server error\"}";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
        }

        private void HandleSpawnAllZombies(HttpListenerContext context)
        {
            try
            {
                // Get all available zombie types from the game
                var allZombies = GameAPP.resourcesManager.allZombieTypes;
                
                foreach (var zombieType in allZombies)
                {
                    // Execute the spawn in the main thread
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        // Spawn at a default position
                        if (CreateZombie.Instance != null)
                        {
                            CreateZombie.Instance.SetZombie(1, zombieType, -5f + 4 * 1.37f); // row 1 (2nd row), x position for col 5
                            logger.LogInfo($"Spawned zombie {zombieType}");
                        }
                        else
                        {
                            logger.LogError("CreateZombie.Instance is null");
                        }
                    });
                }

                context.Response.StatusCode = 200;
                string responseString = "{\"success\":true,\"message\":\"All zombies spawned\"}";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error handling spawn all zombies: {ex.Message}");
                context.Response.StatusCode = 500;
                string responseString = "{\"error\":\"Internal server error\"}";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
        }

        // Method to spawn plants with delay between each spawn
        private void SpawnPlantsWithDelay(PlantType plantType, int row, int col, int amount, int duration)
        {
            for (int i = 0; i < amount; i++)
            {
                int delay = i * duration;
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    System.Threading.Tasks.Task.Delay(delay).ContinueWith(_ =>
                    {
                        // Ensure we're back on the main thread for the actual spawn
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            // Call the game's spawn function
                            if (CreatePlant.Instance != null)
                            {
                                // Adjust coordinates to match game's system (0-indexed)
                                CreatePlant.Instance.SetPlant(col - 1, row - 1, plantType);
                                logger.LogInfo($"Spawned plant {plantType} at row {row}, col {col}");
                            }
                            else
                            {
                                logger.LogError("CreatePlant.Instance is null");
                            }
                        });
                    });
                });
            }
        }

        // Method to spawn zombies with delay between each spawn
        private void SpawnZombiesWithDelay(ZombieType zombieType, int row, int col, int amount, int duration)
        {
            for (int i = 0; i < amount; i++)
            {
                int delay = i * duration;
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    System.Threading.Tasks.Task.Delay(delay).ContinueWith(_ =>
                    {
                        // Ensure we're back on the main thread for the actual spawn
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            // Call the game's spawn function
                            if (CreateZombie.Instance != null)
                            {
                                // Calculate X position based on column (game-specific calculation)
                                float xPosition = -5f + (col - 1) * 1.37f;
                                CreateZombie.Instance.SetZombie(row - 1, zombieType, xPosition);
                                logger.LogInfo($"Spawned zombie {zombieType} at row {row}, col {col} (x={xPosition})");
                            }
                            else
                            {
                                logger.LogError("CreateZombie.Instance is null");
                            }
                        });
                    });
                });
            }
        }

        // Helper method to get PlantType by name
        private PlantType? GetPlantTypeByName(string name)
        {
            // Try to parse as integer first (for ID)
            if (int.TryParse(name, out int id))
            {
                try
                {
                    return (PlantType)id;
                }
                catch
                {
                    return null;
                }
            }

            // Try to match by name (case-insensitive)
            foreach (PlantType plantType in Enum.GetValues(typeof(PlantType)))
            {
                if (string.Equals(plantType.ToString(), name, StringComparison.OrdinalIgnoreCase))
                {
                    return plantType;
                }
            }

            return null;
        }

        // Helper method to get ZombieType by name
        private ZombieType? GetZombieTypeByName(string name)
        {
            // Try to parse as integer first (for ID)
            if (int.TryParse(name, out int id))
            {
                try
                {
                    return (ZombieType)id;
                }
                catch
                {
                    return null;
                }
            }

            // Try to match by name (case-insensitive)
            foreach (ZombieType zombieType in Enum.GetValues(typeof(ZombieType)))
            {
                if (string.Equals(zombieType.ToString(), name, StringComparison.OrdinalIgnoreCase))
                {
                    return zombieType;
                }
            }

            return null;
        }

        public override bool Unload()
        {
            isRunning = false;
            logger.LogInfo("Webhook PvZ Fusion plugin unloaded");
            return true;
        }
    }

    // Unity Main Thread Dispatcher to ensure game functions are called from main thread
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher _instance;
        private readonly Queue<System.Action> _executionQueue = new Queue<System.Action>();

        public static UnityMainThreadDispatcher Instance()
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("UnityMainThreadDispatcher");
                _instance = obj.AddComponent<UnityMainThreadDispatcher>();
            }
            return _instance;
        }

        void Update()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }

        public void Enqueue(System.Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }
    }
}