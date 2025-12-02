using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using System.Text.Json;
using UnityEngine;

namespace WebhookPvZFusion
{
    [BepInPlugin("id.webhook.pvzfusion", "Webhook PvZ Fusion", "1.1.0")]
    public class PvZFusionWebhookPlugin : BaseUnityPlugin
    {
        private Thread httpThread;
        private bool isRunning;
        private ManualLogSource logger;

        void Awake()
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
                int row = root.TryGetProperty("row", out JsonElement rowElement) ? rowElement.GetInt32() : 1;
                int col = root.TryGetProperty("col", out JsonElement colElement) ? colElement.GetInt32() : 1;
                int amount = root.TryGetProperty("amount", out JsonElement amountElement) ? amountElement.GetInt32() : 1;
                int duration = root.TryGetProperty("duration", out JsonElement durationElement) ? durationElement.GetInt32() : 0;

                string type = typeElement.GetString();
                string id = idElement.GetString();

                // Validate row and column (1-indexed: rows 1-5, cols 1-9)
                if (row < 1 || row > 5 || col < 1 || col > 9)
                {
                    context.Response.StatusCode = 400;
                    string responseString = "{\"error\":\"Row or column out of bounds\"}";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    return;
                }

                if (type.ToLower() == "plant")
                {
                    // Execute the spawn in the main thread using coroutine
                    StartCoroutine(SpawnPlantCoroutine(id, row, col, amount, duration));
                }
                else if (type.ToLower() == "zombie")
                {
                    // Execute the spawn in the main thread using coroutine
                    StartCoroutine(SpawnZombieCoroutine(id, row, col, amount, duration));
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
                // Simulate getting all available plant types
                string[] allPlants = { "Peashooter", "Sunflower", "CherryBomb", "WallNut", "PotatoMine" };
                
                foreach (var plantType in allPlants)
                {
                    // Execute the spawn in the main thread
                    StartCoroutine(SpawnPlantCoroutine(plantType, 1, 1, 1, 0));
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
                // Simulate getting all available zombie types
                string[] allZombies = { "BasicZombie", "ConeheadZombie", "BucketheadZombie", "FlagZombie", "PoleVaultingZombie" };
                
                foreach (var zombieType in allZombies)
                {
                    // Execute the spawn in the main thread
                    StartCoroutine(SpawnZombieCoroutine(zombieType, 1, 1, 1, 0));
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

        // Coroutine for spawning plants with delay between each spawn
        private IEnumerator SpawnPlantCoroutine(string plantName, int row, int col, int amount, int duration)
        {
            for (int i = 0; i < amount; i++)
            {
                // Validate row and column are within bounds (1-indexed: rows 1-5, cols 1-9)
                if (row >= 1 && row <= 5 && col >= 1 && col <= 9)
                {
                    logger.LogInfo($"Spawning plant {plantName} at row {row}, col {col} (spawn {i + 1} of {amount})");
                    // Actual plant spawning logic would go here based on the game's implementation
                }
                else
                {
                    logger.LogWarning($"Row {row} or column {col} out of bounds for plant spawn");
                    yield break; // Exit the coroutine if position is invalid
                }

                if (i < amount - 1) // Don't wait after the last spawn
                {
                    yield return new WaitForSeconds(duration / 1000.0f); // Convert milliseconds to seconds
                }
            }
        }

        // Coroutine for spawning zombies with delay between each spawn
        private IEnumerator SpawnZombieCoroutine(string zombieName, int row, int col, int amount, int duration)
        {
            for (int i = 0; i < amount; i++)
            {
                // Validate row is within bounds (1-indexed: rows 1-5)
                if (row >= 1 && row <= 5)
                {
                    logger.LogInfo($"Spawning zombie {zombieName} at row {row}, col {col} (spawn {i + 1} of {amount})");
                    // Actual zombie spawning logic would go here based on the game's implementation
                }
                else
                {
                    logger.LogWarning($"Row {row} out of bounds for zombie spawn");
                    yield break; // Exit the coroutine if position is invalid
                }

                if (i < amount - 1) // Don't wait after the last spawn
                {
                    yield return new WaitForSeconds(duration / 1000.0f); // Convert milliseconds to seconds
                }
            }
        }

        // Cleanup on destroy
        void OnDestroy()
        {
            isRunning = false;
            httpThread?.Join(1000); // Wait up to 1 second for the thread to finish
        }
    }
}