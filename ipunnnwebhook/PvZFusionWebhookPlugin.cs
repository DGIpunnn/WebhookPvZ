using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Reflection;
using System.Text.Json;

namespace WebhookPvZFusion
{
    // Simplified plugin class for compilation without BepInEx dependencies
    public class PvZFusionWebhookPlugin
    {
        private Thread httpThread;
        private bool isRunning;
        private System.Action<string> logger;

        public void Initialize()
        {
            logger = (msg) => System.Console.WriteLine($"[WebhookPvZ] {msg}");
            logger("Webhook PvZ Fusion plugin loaded");

            // Start the HTTP server in a separate thread
            httpThread = new Thread(StartHttpServer)
            {
                IsBackground = true,
                Name = "WebhookHttpServer"
            };
            isRunning = true;
            httpThread.Start();
        }

        private void StartHttpServer()
        {
            try
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:6969/");
                listener.Start();
                logger($"Webhook server started on port 6969");

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
                            logger($"Error processing request: {ex.Message}");
                        }
                    }
                }

                listener.Close();
                logger($"Webhook server stopped");
            }
            catch (Exception ex)
            {
                logger($"Failed to start HTTP server: {ex.Message}");
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            string method = context.Request.HttpMethod;
            string url = context.Request.Url.AbsolutePath;

            logger($"Received {method} request to {url}");

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
                logger($"Error processing request: {ex.Message}");
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
            logger($"Spawn request body: {requestBody}");

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

                // For now, use string-based spawn logic since we don't have the actual game enums
                if (type.ToLower() == "plant")
                {
                    // Execute the spawn in the main thread
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        // Since we don't have access to PlantType enum, we'll just log for now
                        logger($"Attempting to spawn plant {id} at row {row}, col {col}, amount {amount}, duration {duration}");
                        // Actual spawn logic would go here based on the game's implementation
                    });
                }
                else if (type.ToLower() == "zombie")
                {
                    // Execute the spawn in the main thread
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        // Since we don't have access to ZombieType enum, we'll just log for now
                        logger($"Attempting to spawn zombie {id} at row {row}, col {col}, amount {amount}, duration {duration}");
                        // Actual spawn logic would go here based on the game's implementation
                    });
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
                logger($"Error handling spawn request: {ex.Message}");
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
                // Simulate getting all available plant types
                string[] allPlants = { "Peashooter", "Sunflower", "CherryBomb", "WallNut", "PotatoMine" };
                
                foreach (var plantType in allPlants)
                {
                    // Execute the spawn in the main thread
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        // Simulate plant spawning
                        logger($"Simulated spawning plant {plantType}");
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
                logger($"Error handling spawn all plants: {ex.Message}");
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
                // Simulate getting all available zombie types
                string[] allZombies = { "BasicZombie", "ConeheadZombie", "BucketheadZombie", "FlagZombie", "PoleVaultingZombie" };
                
                foreach (var zombieType in allZombies)
                {
                    // Execute the spawn in the main thread
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        // Simulate zombie spawning
                        logger($"Simulated spawning zombie {zombieType}");
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
                logger($"Error handling spawn all zombies: {ex.Message}");
                context.Response.StatusCode = 500;
                string responseString = "{\"error\":\"Internal server error\"}";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
        }

        // Method to spawn plants with delay between each spawn
        // Simulated spawn function for plants
        private void SpawnPlantsWithDelay(string plantName, int row, int col, int amount, int duration)
        {
            if (amount <= 0) return;
            
            // Validate row and column are within bounds (1-indexed: rows 1-5, cols 1-9)
            if (row < 1 || row > 5 || col < 1 || col > 9)
            {
                logger($"Row {row} or column {col} out of bounds for plant spawn");
                return;
            }
            
            logger($"Simulated spawning {amount} of plant {plantName} at row {row}, col {col} with {duration}ms delay");
        }

        // Simulated spawn function for zombies
        private void SpawnZombiesWithDelay(string zombieName, int row, int col, int amount, int duration)
        {
            if (amount <= 0) return;
            
            // Validate row is within bounds (1-indexed: rows 1-5)
            if (row < 1 || row > 5)
            {
                logger($"Row {row} out of bounds for zombie spawn");
                return;
            }
            
            logger($"Simulated spawning {amount} of zombie {zombieName} at row {row}, col {col} with {duration}ms delay");
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

        // Component to handle delayed spawning using Unity coroutines
        public class SpawnDelayComponent : MonoBehaviour
        {
            public IEnumerator SpawnPlantAfterDelay(GameObject delayObject, PlantType plantType, int row, int col, int spawnIndex, int duration)
            {
                yield return new WaitForSeconds(duration / 1000.0f * spawnIndex); // WaitForSeconds takes seconds, not milliseconds
                
                // Call the game's spawn function
                if (CreatePlant.Instance != null)
                {
                    CreatePlant.Instance.SetPlant(col, row, plantType);
                    var plugin = BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<PvZFusionWebhookPlugin>();
                    if (plugin != null)
                    {
                        plugin.logger($"Spawned plant {plantType} at row {row + 1}, col {col + 1}");
                    }
                }
                else
                {
                    var plugin = BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<PvZFusionWebhookPlugin>();
                    if (plugin != null)
                    {
                        plugin.logger("CreatePlant.Instance is null");
                    }
                }
                
                // Clean up the GameObject after spawning
                GameObject.Destroy(delayObject);
            }
            
            public IEnumerator SpawnZombieAfterDelay(GameObject delayObject, ZombieType zombieType, int row, float xPosition, int spawnIndex, int duration)
            {
                yield return new WaitForSeconds(duration / 1000.0f * spawnIndex); // WaitForSeconds takes seconds, not milliseconds
                
                // Call the game's spawn function
                if (CreateZombie.Instance != null)
                {
                    CreateZombie.Instance.SetZombie(row, zombieType, xPosition);
                    var plugin = BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<PvZFusionWebhookPlugin>();
                    if (plugin != null)
                    {
                        plugin.logger($"Spawned zombie {zombieType} at row {row + 1}, x={xPosition}");
                    }
                }
                else
                {
                    var plugin = BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<PvZFusionWebhookPlugin>();
                    if (plugin != null)
                    {
                        plugin.logger("CreateZombie.Instance is null");
                    }
                }
                
                // Clean up the GameObject after spawning
                GameObject.Destroy(delayObject);
            }
        }

        public override bool Unload()
        {
            isRunning = false;
            logger("Webhook PvZ Fusion plugin unloaded");
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