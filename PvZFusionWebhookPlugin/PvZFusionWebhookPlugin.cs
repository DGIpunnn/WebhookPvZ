using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading;

namespace WebhookPvZFusion
{
    // Mock enums for PlantType and ZombieType - these would be defined in the actual game
    public enum PlantType
    {
        Peashooter, Sunflower, CherryBomb, Wallnut, PotatoMine, SnowPea, Chomper, Repeater,
        Puffshroom, Sunshroom, Fumeshroom, GraveBuster, Hypnoshroom, Scaredyshroom, Iceberg,
        Doomshroom, LilyPad, Squash, Threepeater, TangleKelp, Jalapeno, Spikeweed, Torchwood,
        Tallnut, Seashroom, Plantern, Cactus, Blover, SplitPea, Starfruit, Pumpkin, Magnetshroom,
        Cabbagepult, FlowerPot, KernelPult, CoffeeBean, Garlic, Umbrella, Marigold, MelonPult,
        GatlingPea, TwinSunflower, Gloomshroom, Cattail, WinterMelon, GoldMagnet, Spikerock,
        CobCannon, Imitater
    }

    public enum ZombieType
    {
        Nothing, NormalZombie, ConeheadZombie, PoleVaultingZombie, BucketheadZombie, NewspaperZombie,
        ScreenDoorZombie, FootballZombie, DancingZombie, BackupDancer, DuckyTubeZombie, SnorkelZombie,
        ZombieBobsledTeam, DolphinRiderZombie, JackintheBoxZombie, BalloonZombie, DiggerZombie,
        PogoZombie, ZombieYeti, BungeeZombie, LadderZombie, CatapultZombie, Gargantuar, Imp
    }

    // Mock classes for CreatePlant and CreateZombie - these would be defined in the actual game
    public class CreatePlant
    {
        public static CreatePlant Instance { get; private set; }

        static CreatePlant()
        {
            Instance = new CreatePlant();
        }

        public GameObject SetPlant(int column, int row, PlantType plantType)
        {
            // Mock implementation - in the real game, this would create the actual plant
            Debug.Log($"Creating plant {plantType} at column {column}, row {row}");
            return new GameObject($"Plant_{plantType}");
        }
    }

    public class CreateZombie
    {
        public static CreateZombie Instance { get; private set; }

        static CreateZombie()
        {
            Instance = new CreateZombie();
        }

        public GameObject SetZombie(int row, ZombieType zombieType, float x)
        {
            // Mock implementation - in the real game, this would create the actual zombie
            Debug.Log($"Creating zombie {zombieType} at row {row}, x {x}");
            return new GameObject($"Zombie_{zombieType}");
        }

        public GameObject SetZombieWithMindControl(int row, ZombieType zombieType, float x)
        {
            // Mock implementation - in the real game, this would create the actual zombie with mind control
            Debug.Log($"Creating mind-controlled zombie {zombieType} at row {row}, x {x}");
            return new GameObject($"MindControlZombie_{zombieType}");
        }
    }

    public class Mouse
    {
        public static Mouse Instance { get; private set; }

        static Mouse()
        {
            Instance = new Mouse();
        }

        public float GetBoxXFromColumn(int column)
        {
            // Mock implementation - returns a position based on the column
            return -2.5f + (column * 1.0f); // Assuming each column is 1.0f units wide
        }

        public float GetLandY(float x, int row)
        {
            // Mock implementation - returns a position based on the row
            return 1.0f - (row * 0.5f); // Assuming each row is 0.5f units tall
        }
    }
    [BepInPlugin("id.webhook.pvzfusion", "Webhook PvZ Fusion", "1.1.0")]
    public class PvZFusionWebhookPlugin : BaseUnityPlugin
    {
        private const int PORT = 6969;
        private HttpListener httpListener;
        private Thread listenerThread;
        private bool isRunning = false;
        private ManualLogSource logSource;

        // Plant and zombie spawn functions
        private Dictionary<string, System.Action<int, int>> plantSpawners = new Dictionary<string, System.Action<int, int>>();
        private Dictionary<string, System.Action<int, int>> zombieSpawners = new Dictionary<string, System.Action<int, int>>();

        void Awake()
        {
            logSource = Logger;
            InitializeSpawners();
            StartHttpListener();
        }

        void OnDestroy()
        {
            StopHttpListener();
        }

        private void InitializeSpawners()
        {
            // Initialize plant spawners - these would be replaced with actual game functions
            plantSpawners.Add("Peashooter", (row, col) => SpawnPlant("Peashooter", row, col));
            plantSpawners.Add("Sunflower", (row, col) => SpawnPlant("Sunflower", row, col));
            plantSpawners.Add("CherryBomb", (row, col) => SpawnPlant("CherryBomb", row, col));
            plantSpawners.Add("Wallnut", (row, col) => SpawnPlant("Wallnut", row, col));
            plantSpawners.Add("PotatoMine", (row, col) => SpawnPlant("PotatoMine", row, col));
            plantSpawners.Add("SnowPea", (row, col) => SpawnPlant("SnowPea", row, col));
            plantSpawners.Add("Chomper", (row, col) => SpawnPlant("Chomper", row, col));
            plantSpawners.Add("Repeater", (row, col) => SpawnPlant("Repeater", row, col));
            plantSpawners.Add("Puffshroom", (row, col) => SpawnPlant("Puffshroom", row, col));
            plantSpawners.Add("Sunshroom", (row, col) => SpawnPlant("Sunshroom", row, col));
            plantSpawners.Add("Fumeshroom", (row, col) => SpawnPlant("Fumeshroom", row, col));
            plantSpawners.Add("GraveBuster", (row, col) => SpawnPlant("GraveBuster", row, col));
            plantSpawners.Add("Hypnoshroom", (row, col) => SpawnPlant("Hypnoshroom", row, col));
            plantSpawners.Add("Scaredyshroom", (row, col) => SpawnPlant("Scaredyshroom", row, col));
            plantSpawners.Add("Iceberg", (row, col) => SpawnPlant("Iceberg", row, col));
            plantSpawners.Add("Doomshroom", (row, col) => SpawnPlant("Doomshroom", row, col));
            plantSpawners.Add("LilyPad", (row, col) => SpawnPlant("LilyPad", row, col));
            plantSpawners.Add("Squash", (row, col) => SpawnPlant("Squash", row, col));
            plantSpawners.Add("Threepeater", (row, col) => SpawnPlant("Threepeater", row, col));
            plantSpawners.Add("TangleKelp", (row, col) => SpawnPlant("TangleKelp", row, col));
            plantSpawners.Add("Jalapeno", (row, col) => SpawnPlant("Jalapeno", row, col));
            plantSpawners.Add("Spikeweed", (row, col) => SpawnPlant("Spikeweed", row, col));
            plantSpawners.Add("Torchwood", (row, col) => SpawnPlant("Torchwood", row, col));
            plantSpawners.Add("Tallnut", (row, col) => SpawnPlant("Tallnut", row, col));
            plantSpawners.Add("Seashroom", (row, col) => SpawnPlant("Seashroom", row, col));
            plantSpawners.Add("Plantern", (row, col) => SpawnPlant("Plantern", row, col));
            plantSpawners.Add("Cactus", (row, col) => SpawnPlant("Cactus", row, col));
            plantSpawners.Add("Blover", (row, col) => SpawnPlant("Blover", row, col));
            plantSpawners.Add("SplitPea", (row, col) => SpawnPlant("SplitPea", row, col));
            plantSpawners.Add("Starfruit", (row, col) => SpawnPlant("Starfruit", row, col));
            plantSpawners.Add("Pumpkin", (row, col) => SpawnPlant("Pumpkin", row, col));
            plantSpawners.Add("Magnetshroom", (row, col) => SpawnPlant("Magnetshroom", row, col));
            plantSpawners.Add("Cabbagepult", (row, col) => SpawnPlant("Cabbagepult", row, col));
            plantSpawners.Add("FlowerPot", (row, col) => SpawnPlant("FlowerPot", row, col));
            plantSpawners.Add("KernelPult", (row, col) => SpawnPlant("KernelPult", row, col));
            plantSpawners.Add("CoffeeBean", (row, col) => SpawnPlant("CoffeeBean", row, col));
            plantSpawners.Add("Garlic", (row, col) => SpawnPlant("Garlic", row, col));
            plantSpawners.Add("Umbrella", (row, col) => SpawnPlant("Umbrella", row, col));
            plantSpawners.Add("Marigold", (row, col) => SpawnPlant("Marigold", row, col));
            plantSpawners.Add("MelonPult", (row, col) => SpawnPlant("MelonPult", row, col));
            plantSpawners.Add("GatlingPea", (row, col) => SpawnPlant("GatlingPea", row, col));
            plantSpawners.Add("TwinSunflower", (row, col) => SpawnPlant("TwinSunflower", row, col));
            plantSpawners.Add("Gloomshroom", (row, col) => SpawnPlant("Gloomshroom", row, col));
            plantSpawners.Add("Cattail", (row, col) => SpawnPlant("Cattail", row, col));
            plantSpawners.Add("WinterMelon", (row, col) => SpawnPlant("WinterMelon", row, col));
            plantSpawners.Add("GoldMagnet", (row, col) => SpawnPlant("GoldMagnet", row, col));
            plantSpawners.Add("Spikerock", (row, col) => SpawnPlant("Spikerock", row, col));
            plantSpawners.Add("CobCannon", (row, col) => SpawnPlant("CobCannon", row, col));
            plantSpawners.Add("Imitater", (row, col) => SpawnPlant("Imitater", row, col));

            // Initialize zombie spawners - these would be replaced with actual game functions
            zombieSpawners.Add("NormalZombie", (row, col) => SpawnZombie("NormalZombie", row, col));
            zombieSpawners.Add("ConeheadZombie", (row, col) => SpawnZombie("ConeheadZombie", row, col));
            zombieSpawners.Add("PoleVaultingZombie", (row, col) => SpawnZombie("PoleVaultingZombie", row, col));
            zombieSpawners.Add("BucketheadZombie", (row, col) => SpawnZombie("BucketheadZombie", row, col));
            zombieSpawners.Add("NewspaperZombie", (row, col) => SpawnZombie("NewspaperZombie", row, col));
            zombieSpawners.Add("ScreenDoorZombie", (row, col) => SpawnZombie("ScreenDoorZombie", row, col));
            zombieSpawners.Add("FootballZombie", (row, col) => SpawnZombie("FootballZombie", row, col));
            zombieSpawners.Add("DancingZombie", (row, col) => SpawnZombie("DancingZombie", row, col));
            zombieSpawners.Add("BackupDancer", (row, col) => SpawnZombie("BackupDancer", row, col));
            zombieSpawners.Add("DuckyTubeZombie", (row, col) => SpawnZombie("DuckyTubeZombie", row, col));
            zombieSpawners.Add("SnorkelZombie", (row, col) => SpawnZombie("SnorkelZombie", row, col));
            zombieSpawners.Add("ZombieBobsledTeam", (row, col) => SpawnZombie("ZombieBobsledTeam", row, col));
            zombieSpawners.Add("DolphinRiderZombie", (row, col) => SpawnZombie("DolphinRiderZombie", row, col));
            zombieSpawners.Add("JackintheBoxZombie", (row, col) => SpawnZombie("JackintheBoxZombie", row, col));
            zombieSpawners.Add("BalloonZombie", (row, col) => SpawnZombie("BalloonZombie", row, col));
            zombieSpawners.Add("DiggerZombie", (row, col) => SpawnZombie("DiggerZombie", row, col));
            zombieSpawners.Add("PogoZombie", (row, col) => SpawnZombie("PogoZombie", row, col));
            zombieSpawners.Add("ZombieYeti", (row, col) => SpawnZombie("ZombieYeti", row, col));
            zombieSpawners.Add("BungeeZombie", (row, col) => SpawnZombie("BungeeZombie", row, col));
            zombieSpawners.Add("LadderZombie", (row, col) => SpawnZombie("LadderZombie", row, col));
            zombieSpawners.Add("CatapultZombie", (row, col) => SpawnZombie("CatapultZombie", row, col));
            zombieSpawners.Add("Gargantuar", (row, col) => SpawnZombie("Gargantuar", row, col));
            zombieSpawners.Add("Imp", (row, col) => SpawnZombie("Imp", row, col));
        }

        private void StartHttpListener()
        {
            try
            {
                httpListener = new HttpListener();
                httpListener.Prefixes.Add($"http://localhost:{PORT}/");
                httpListener.Prefixes.Add($"http://127.0.0.1:{PORT}/");
                
                httpListener.Start();
                isRunning = true;
                
                listenerThread = new Thread(HandleRequests);
                listenerThread.Start();
                
                Logger.LogInfo($"Webhook server started on port {PORT}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to start HTTP listener: {ex.Message}");
            }
        }

        private void StopHttpListener()
        {
            isRunning = false;
            
            if (httpListener != null && httpListener.IsListening)
            {
                httpListener.Stop();
                httpListener.Close();
            }
            
            if (listenerThread != null)
            {
                listenerThread.Join(1000); // Wait up to 1 second for thread to finish
            }
            
            Logger.LogInfo("Webhook server stopped");
        }

        private void HandleRequests()
        {
            while (isRunning)
            {
                try
                {
                    HttpListenerContext context = httpListener.GetContext();
                    ProcessRequest(context);
                }
                catch (HttpListenerException)
                {
                    // This exception is thrown when the listener is stopped
                    if (isRunning)
                    {
                        Logger.LogError("HTTP listener exception occurred");
                    }
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error handling request: {ex.Message}");
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string responseString = "";
            int statusCode = 200;

            try
            {
                if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/spawn")
                {
                    responseString = HandleSpawnRequest(request);
                }
                else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/spawn/all/plants")
                {
                    responseString = HandleSpawnAllPlantsRequest();
                }
                else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/spawn/all/zombies")
                {
                    responseString = HandleSpawnAllZombiesRequest();
                }
                else
                {
                    statusCode = 404;
                    responseString = "{\"error\":\"Endpoint not found\"}";
                }
            }
            catch (Exception ex)
            {
                statusCode = 500;
                responseString = $"{{\"error\":\"{ex.Message}\"}}";
            }

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.StatusCode = statusCode;
            response.ContentType = "application/json";
            
            using (Stream output = response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }
        }

        private string HandleSpawnRequest(HttpListenerRequest request)
        {
            string requestBody = new StreamReader(request.InputStream).ReadToEnd();
            SpawnRequest spawnRequest = JsonConvert.DeserializeObject<SpawnRequest>(requestBody);

            if (spawnRequest == null)
            {
                return "{\"error\":\"Invalid JSON\"}";
            }

            if (spawnRequest.row < 0 || spawnRequest.row > 4 || spawnRequest.col < 0 || spawnRequest.col > 8)
            {
                return "{\"error\":\"Invalid row or column position\"}";
            }

            if (spawnRequest.amount <= 0)
            {
                return "{\"error\":\"Amount must be greater than 0\"}";
            }

            if (spawnRequest.type == "plant")
            {
                if (!plantSpawners.ContainsKey(spawnRequest.id))
                {
                    return "{\"error\":\"Invalid plant ID\"}";
                }

                if (spawnRequest.amount == 1)
                {
                    // Spawn once
                    SpawnPlantOnMainThread(spawnRequest.id, spawnRequest.row, spawnRequest.col);
                }
                else
                {
                    // Spawn multiple times with delay
                    StartCoroutine(SpawnPlantRepeatedly(spawnRequest.id, spawnRequest.row, spawnRequest.col, spawnRequest.amount, spawnRequest.duration));
                }
            }
            else if (spawnRequest.type == "zombie")
            {
                if (!zombieSpawners.ContainsKey(spawnRequest.id))
                {
                    return "{\"error\":\"Invalid zombie ID\"}";
                }

                if (spawnRequest.amount == 1)
                {
                    // Spawn once
                    SpawnZombieOnMainThread(spawnRequest.id, spawnRequest.row, spawnRequest.col);
                }
                else
                {
                    // Spawn multiple times with delay
                    StartCoroutine(SpawnZombieRepeatedly(spawnRequest.id, spawnRequest.row, spawnRequest.col, spawnRequest.amount, spawnRequest.duration));
                }
            }
            else
            {
                return "{\"error\":\"Invalid type, must be 'plant' or 'zombie'\"}";
            }

            return "{\"success\":true}";
        }

        private string HandleSpawnAllPlantsRequest()
        {
            foreach (var plant in plantSpawners)
            {
                // Spawn in different positions to avoid overlap
                int row = UnityEngine.Random.Range(0, 5);
                int col = UnityEngine.Random.Range(0, 9);
                
                if (row < 5 && col < 9)
                {
                    SpawnPlantOnMainThread(plant.Key, row, col);
                }
            }
            
            return "{\"success\":true,\"message\":\"All plants spawned\"}";
        }

        private string HandleSpawnAllZombiesRequest()
        {
            foreach (var zombie in zombieSpawners)
            {
                // Spawn in different positions to avoid overlap
                int row = UnityEngine.Random.Range(0, 5);
                int col = UnityEngine.Random.Range(0, 9);
                
                if (row < 5 && col < 9)
                {
                    SpawnZombieOnMainThread(zombie.Key, row, col);
                }
            }
            
            return "{\"success\":true,\"message\":\"All zombies spawned\"}";
        }

        private IEnumerator SpawnPlantRepeatedly(string plantId, int row, int col, int amount, int duration)
        {
            for (int i = 0; i < amount; i++)
            {
                if (i > 0) yield return new WaitForSeconds(duration / 1000f);
                SpawnPlantOnMainThread(plantId, row, col);
            }
        }

        private IEnumerator SpawnZombieRepeatedly(string zombieId, int row, int col, int amount, int duration)
        {
            for (int i = 0; i < amount; i++)
            {
                if (i > 0) yield return new WaitForSeconds(duration / 1000f);
                SpawnZombieOnMainThread(zombieId, row, col);
            }
        }

        private void SpawnPlantOnMainThread(string plantId, int row, int col)
        {
            // Execute spawn on main thread to ensure Unity compatibility
            StartCoroutine(ExecuteOnMainThread(() => {
                if (plantSpawners.ContainsKey(plantId))
                {
                    plantSpawners[plantId](row, col);
                }
            }));
        }

        private void SpawnZombieOnMainThread(string zombieId, int row, int col)
        {
            // Execute spawn on main thread to ensure Unity compatibility
            StartCoroutine(ExecuteOnMainThread(() => {
                if (zombieSpawners.ContainsKey(zombieId))
                {
                    zombieSpawners[zombieId](row, col);
                }
            }));
        }

        private IEnumerator ExecuteOnMainThread(System.Action action)
        {
            yield return new WaitForEndOfFrame();
            action?.Invoke();
        }

        // Placeholder functions - these would be replaced with actual game functions
        private void SpawnPlant(string plantId, int row, int col)
        {
            Logger.LogInfo($"Spawning plant {plantId} at row {row}, col {col}");
            // Convert plant ID string to PlantType enum
            if (Enum.TryParse<PlantType>(plantId, out PlantType plantType))
            {
                // Calculate the X and Y positions based on the row and column
                float x = Mouse.Instance.GetBoxXFromColumn(col);
                float y = Mouse.Instance.GetLandY(x, row);
                
                // Spawn the plant using the actual game function
                GameObject plantObject = CreatePlant.Instance.SetPlant(col, row, plantType);
                if (plantObject != null)
                {
                    Logger.LogInfo($"Successfully spawned plant {plantId} at row {row}, col {col}");
                }
                else
                {
                    Logger.LogWarning($"Failed to spawn plant {plantId} at row {row}, col {col}");
                }
            }
            else
            {
                Logger.LogError($"Invalid plant type: {plantId}");
            }
        }

        private void SpawnZombie(string zombieId, int row, int col)
        {
            Logger.LogInfo($"Spawning zombie {zombieId} at row {row}, col {col}");
            // Convert zombie ID string to ZombieType enum
            if (Enum.TryParse<ZombieType>(zombieId, out ZombieType zombieType))
            {
                // Calculate the X position for the zombie spawn
                // Zombies typically spawn from the right side of the screen
                float x = 10f; // Right side of the board
                
                // Spawn the zombie using the actual game function
                GameObject zombieObject = CreateZombie.Instance.SetZombie(row, zombieType, x);
                if (zombieObject != null)
                {
                    Logger.LogInfo($"Successfully spawned zombie {zombieType} at row {row}");
                }
                else
                {
                    Logger.LogWarning($"Failed to spawn zombie {zombieType} at row {row}");
                }
            }
            else
            {
                Logger.LogError($"Invalid zombie type: {zombieId}");
            }
        }
    }

    public class SpawnRequest
    {
        public string action { get; set; }
        public string type { get; set; }
        public string id { get; set; }
        public int row { get; set; }
        public int col { get; set; }
        public int amount { get; set; }
        public int duration { get; set; }
    }
}