using System.Collections;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTMPro;
using MelonLoader;
using Newtonsoft.Json;
using ToolModData;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static ToolMod.PatchMgr;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


namespace ToolMod;

[HarmonyPatch(typeof(AlmanacCardZombie), "OnMouseDown")]
public static class AlmanacCardZombiePatch
{
    public static void Postfix(AlmanacCardZombie __instance)
    {
        AlmanacZombieType = __instance.theZombieType;
    }
}

[HarmonyPatch(typeof(AlmanacPlantCtrl), "GetSeedType")]
public static class AlmanacPlantCtrlPatch
{
    public static void Postfix(AlmanacPlantCtrl __instance)
    {
        AlmanacSeedType = __instance.plantSelected;
    }
}

[HarmonyPatch(typeof(Board), "Awake")]
public static class BoardPatchA
{
    public static void Postfix()
    {
        var t = Board.Instance.boardTag;
        originalTravel = t.enableTravelPlant;
        t.isScaredyDream |= PatchMgr.GameModes.ScaredyDream;
        t.isColumn |= PatchMgr.GameModes.ColumnPlanting;
        t.isSeedRain |= PatchMgr.GameModes.SeedRain;
        t.enableAllTravelPlant |= UnlockAllFusions;
        Board.Instance.boardTag = t;
    }
}

[HarmonyPatch(typeof(Board), "NewZombieUpdate")]
public static class BoardPatchB
{
    public static void Postfix()
    {
        if (NewZombieUpdateCD > 0 && NewZombieUpdateCD < 30 &&
            Board.Instance.newZombieWaveCountDown > NewZombieUpdateCD)
            Board.Instance.newZombieWaveCountDown = NewZombieUpdateCD;
    }
}

[HarmonyPatch(typeof(Bucket), "Update")]
public static class BucketPatch
{
    public static void Postfix(Bucket __instance)
    {
        if (ItemExistForever) __instance.existTime = 0.1f;
    }
}

[HarmonyPatch(typeof(Bullet), "Update")]
public static class BulletPatchA
{
    public static void Postfix(Bullet __instance)
    {
        try
        {
            if (BulletDamage[__instance.theBulletType] >= 0 &&
                __instance.Damage != BulletDamage[__instance.theBulletType])
                __instance.Damage = BulletDamage[__instance.theBulletType];
        }
        catch
        {
        }
    }
}

[HarmonyPatch(typeof(Bullet), "Die")]
public static class BulletPatchB
{
    public static bool Prefix(Bullet __instance)
    {
        if (UndeadBullet && !__instance.isZombieBullet)
        {
            __instance.hit = false;
            __instance.penetrationTimes = int.MaxValue;
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(CardUI))]
public static class CardUIPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    public static void Postfix(CardUI __instance)
    {
        GameObject obj = new("ModifierCardCD");
        var text = obj.AddComponent<TextMeshProUGUI>();
        text.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
        text.color = new Color(228f / 256f, 155f / 256f, 38f / 256f);
        obj.transform.SetParent(__instance.transform);
        obj.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        obj.transform.localPosition = new Vector3(39f, 0, 0);
    }

    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    public static void PostUpdate(CardUI __instance)
    {
        var child = __instance.transform.FindChild("ModifierCardCD");
        if (child is null) return;
        if (__instance.isAvailable || !ShowGameInfo)
        {
            child.GameObject().active = false;
        }
        else
        {
            child.GameObject().active = true;
            child.GameObject().GetComponent<TextMeshProUGUI>().text = $"{__instance.CD:N1}/{__instance.fullCD}";
        }
    }
}

[HarmonyPatch(typeof(Chomper), "Update")]
public static class ChomperPatch
{
    public static void Prefix(Chomper __instance)
    {
        if (ChomperNoCD && __instance.attributeCountdown > 0.05f) __instance.attributeCountdown = 0.05f;
    }
}

[HarmonyPatch(typeof(ConveyBeltMgr))]
public static class ConveyBeltMgrPatch
{
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void PostAwake(ConveyBeltMgr __instance)
    {
        if (ConveyBeltTypes.Count > 0)
        {
            __instance.plants = new Il2CppSystem.Collections.Generic.List<PlantType>();
            foreach (var p in ConveyBeltTypes) __instance.plants.Add((PlantType)p);
        }
    }

    [HarmonyPatch("GetCardPool")]
    [HarmonyPostfix]
    public static void PostGetCardPool(ref Il2CppSystem.Collections.Generic.List<PlantType> __result)
    {
        if (ConveyBeltTypes.Count > 0)
        {
            Il2CppSystem.Collections.Generic.List<PlantType> list = new();
            foreach (var p in ConveyBeltTypes) list.Add((PlantType)p);
            __result = list;
        }
    }
}

[HarmonyPatch(typeof(CreateBullet), "SetBullet", typeof(float), typeof(float), typeof(int), typeof(BulletType),
    typeof(int), typeof(bool))]
[HarmonyPatch(typeof(CreateBullet), "SetBullet", typeof(float), typeof(float), typeof(int), typeof(BulletType),
    typeof(BulletMoveWay), typeof(bool))]
public static class CreateBulletPatch
{
    public static void Prefix(ref BulletType theBulletType)
    {
        if (LockBulletType == -1)
            theBulletType = Enum.GetValues<BulletType>()[Random.Range(0, Enum.GetValues<BulletType>().Length)];
        if (LockBulletType >= 0) theBulletType = (BulletType)LockBulletType;
    }
}

[HarmonyPatch(typeof(CreatePlant), "SetPlant")]
public static class CreatePlantPatchC
{
    public static void Prefix(ref bool isFreeSet)
    {
        isFreeSet = FreePlanting || isFreeSet;
    }
}

[HarmonyPatch(typeof(DriverZombie), "PositionUpdate")]
public static class DriverZombiePatch
{
    public static void Postfix(DriverZombie __instance)
    {
        if (NoIceRoad) Board.Instance.iceRoadX[__instance.theZombieRow] = 35f;
    }
}

[HarmonyPatch(typeof(DroppedCard), "Update")]
public static class DroppedCardPatch
{
    public static void Postfix(DroppedCard __instance)
    {
        if (ItemExistForever) __instance.existTime = 0;
    }
}

[HarmonyPatch(typeof(Fertilize), "Update")]
public static class FertilizePatch
{
    public static void Postfix(Fertilize __instance)
    {
        if (ItemExistForever) __instance.existTime = 0.1f;
    }
}

[HarmonyPatch(typeof(GameAPP))]
public static class GameAppPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    public static void PostStart()
    {
        GameObject obj = new("Modifier");
        Object.DontDestroyOnLoad(obj);
        obj.AddComponent<DataProcessor>();
        obj.AddComponent<PatchMgr>();
    }
}

[HarmonyPatch(typeof(Glove), "Update")]
public static class GlovePatchA
{
    public static void Postfix(Glove __instance)
    {
        __instance.gameObject.transform.GetChild(0).gameObject.SetActive(!GloveNoCD);
        if (GloveFullCD > 0) __instance.fullCD = (float)GloveFullCD;
        if (GloveNoCD) __instance.CD = __instance.fullCD;
        if (__instance.avaliable || !ShowGameInfo)
        {
            __instance.transform.FindChild("ModifierGloveCD").GameObject().active = false;
        }
        else
        {
            __instance.transform.FindChild("ModifierGloveCD").GameObject().active = true;
            __instance.transform.FindChild("ModifierGloveCD").GameObject().GetComponent<TextMeshProUGUI>().text =
                $"{__instance.CD:N1}/{__instance.fullCD}";
        }
    }
}

[HarmonyPatch(typeof(Glove), "Start")]
public static class GlovePatchB
{
    public static void Postfix(Glove __instance)
    {
        GameObject obj = new("ModifierGloveCD");
        var text = obj.AddComponent<TextMeshProUGUI>();
        text.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
        text.color = new Color(228f / 256f, 155f / 256f, 38f / 256f);
        obj.transform.SetParent(__instance.GameObject().transform);
        obj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        obj.transform.localPosition = new Vector3(27.653f, 0, 0);
    }
}

[HarmonyPatch(typeof(GridItem), "SetGridItem")]
public static class GridItemPatch
{
    public static bool Prefix(ref GridItemType theType)
    {
        return (int)theType >= 3 || !NoHole;
    }
}

[HarmonyPatch(typeof(HammerMgr), "Update")]
public static class HammerMgrPatchA
{
    public static float OriginalFullCD { get; set; }

    public static void Postfix(HammerMgr __instance)
    {
        __instance.gameObject.transform.GetChild(0).GetChild(0).gameObject.SetActive(!HammerNoCD);
        if (HammerFullCD > 0)
            __instance.fullCD = (float)HammerFullCD;
        else
            __instance.fullCD = OriginalFullCD;
        if (HammerNoCD) __instance.CD = __instance.fullCD;
        if (__instance.avaliable || !ShowGameInfo)
        {
            __instance.transform.FindChild("ModifierHammerCD").GameObject().active = false;
        }
        else
        {
            __instance.transform.FindChild("ModifierHammerCD").GameObject().active = true;
            __instance.transform.FindChild("ModifierHammerCD").GameObject().GetComponent<TextMeshProUGUI>().text =
                $"{__instance.CD:N1}/{__instance.fullCD}";
        }
    }
}

[HarmonyPatch(typeof(HammerMgr), "Start")]
public static class HammerMgrPatchB
{
    public static void Postfix(HammerMgr __instance)
    {
        GameObject obj = new("ModifierHammerCD");
        var text = obj.AddComponent<TextMeshProUGUI>();
        text.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
        text.color = new Color(228f / 256f, 155f / 256f, 38f / 256f);
        obj.transform.SetParent(__instance.GameObject().transform);
        obj.transform.localScale = new Vector3(2f, 2f, 2f);
        obj.transform.localPosition = new Vector3(107, 0, 0);
    }
}

[HarmonyPatch(typeof(HyponoEmperor), "Update")]
public static class HyponoEmperorPatch
{
    public static void Postfix(HyponoEmperor __instance)
    {
        if (HyponoEmperorNoCD && __instance.summonZombieTime > 2f) __instance.summonZombieTime = 2f;
    }
}

[HarmonyPatch(typeof(InGameBtn), "OnMouseUpAsButton")]
public static class InGameBtnPatch
{
    public static bool BottomEnabled { get; set; }

    public static void Postfix(InGameBtn __instance)
    {
        if (__instance.buttonNumber == 3)
        {
            TimeSlow = !TimeSlow;
            TimeStop = false;
            Time.timeScale = TimeSlow ? 0.2f : SyncSpeed;
        }

        if (__instance.buttonNumber == 13) BottomEnabled = GameObject.Find("Bottom") is not null;
    }
}

[HarmonyPatch(typeof(InGameText), "ShowText")]
public static class InGameTextPatch
{
    public static void Postfix()
    {
        for (var i = 0; i < InGameAdvBuffs.Length; i++)
            if (InGameAdvBuffs[i] != GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().advancedUpgrades[i])
            {
                SyncInGameBuffs();
                return;
            }

        for (var i = 0; i < InGameUltiBuffs.Length; i++)
            if (InGameUltiBuffs[i] != GetBoolArray(GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().ultimateUpgrades)[i])
            {
                SyncInGameBuffs();
                return;
            }
    }
}

[HarmonyPatch(typeof(InitBoard))]
public static class InitBoardPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("ReadySetPlant")]
    public static void PreReadySetPlant()
    {
        if (CardNoInit)
            if (SeedGroup is not null)
                for (var i = SeedGroup!.transform.childCount - 1; i >= 0; i--)
                {
                    var card = SeedGroup.transform.GetChild(i);
                    if (card is null || card.childCount is 0) continue;
                    card.GetChild(0).gameObject.GetComponent<CardUI>().CD =
                        card.GetChild(0).gameObject.GetComponent<CardUI>().fullCD;
                }

        HammerMgrPatchA.OriginalFullCD =
            Object.FindObjectsOfTypeAll(Il2CppType.Of<HammerMgr>())[0].Cast<HammerMgr>().fullCD;
    }

    [HarmonyPrefix]
    [HarmonyPatch("RightMoveCamera")]
    public static void PreRightMoveCamera()
    {
        MelonCoroutines.Start(PostInitBoard());
    }
}

[HarmonyPatch(typeof(JackboxZombie), "Update")]
public static class JackboxZombiePatch
{
    public static void Postfix(JackboxZombie __instance)
    {
        if (JackboxNotExplode) __instance.popCountDown = __instance.originalCountDown;
    }
}

[HarmonyPatch(typeof(Plant), "PlantShootUpdate")]
public static class PlantPatch
{
    public static void Prefix(Plant __instance)
    {
        var s = __instance.TryCast<Shooter>();
        if (FastShooting && s is not null) s.AnimShoot();
    }
}

[HarmonyPatch(typeof(Plant), nameof(Plant.GetDamage))]
public static class PlantGetDamagePatch
{
    [HarmonyPostfix]
    public static void Postfix(Plant __instance, ref int __result)
    {
        if (HardPlant)
        {
            __result = 0;
        }
    }
}

[HarmonyPatch(typeof(Plant), nameof(Plant.Crashed))]
public static class PlantCrashedPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Plant __instance)
    {
        if (HardPlant)
        {
            return false;
        }
        return true;
    }
}


[HarmonyPatch(typeof(PotatoMine), "Update")]
public static class PotatoMinePatch
{
    [HarmonyPrefix]
    public static void Prefix(PotatoMine __instance)
    {
        if (MineNoCD && __instance.attributeCountdown > 0.05f) __instance.attributeCountdown = 0.05f;
    }
}

[HarmonyPatch(typeof(Board), nameof(Board.SetEvePlants))]
public static class BoardPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Board __instance, ref int theColumn, ref int theRow, ref bool fromWheat,ref GameObject __result)
    {
        if (fromWheat && LockWheat >= 0)
        {
            GameObject plantObject = CreatePlant.Instance.SetPlant(
                theColumn, 
                theRow, 
                (PlantType)LockWheat
            );

            plantObject.TryGetComponent<Plant>(out var component);
            if (component is not null)
            {
                component.wheatType = 1;
            }
            
            if (!plantObject)
            {
                float boxX = Mouse.Instance.GetBoxXFromColumn(theColumn);
                float landY = Mouse.Instance.GetLandY(boxX, theRow);
                Lawnf.SetDroppedCard(new Vector2(boxX, landY), (PlantType)LockWheat);
            }
            else
            {
                __result = plantObject;
            }
            return false;
        }

        return true;
    }
}



[HarmonyPatch(typeof(Present), "RandomPlant")]
public static class PresentPatchA
{
    public static bool Prefix(Present __instance)
    {
#if false
        foreach (var plant in __instance.board.plantArray)
            try
            {
                if (plant.thePlantRow == __instance.thePlantRow && plant.thePlantColumn == __instance.thePlantColumn)
                {
                    if(plant.thePlantType==__instance.thePlantType)
                        MelonLogger.Msg("TRUE");
                    var array = MixData.data.Cast<Array>();
                    for (int i = 0; i < array.Length; i++)
                    {
                        var element = array.GetValue(i);
                        MelonLogger.Msg($"{i}: {element}");
                    }

                    MelonLogger.Msg($"{plant.thePlantRow},{plant.thePlantColumn},{plant.thePlantType}");
                    return true;
                }
            }
            catch
            {
            }
#endif
        if (LockPresent >= 0)
        {
            CreatePlant.Instance.SetPlant(__instance.thePlantColumn, __instance.thePlantRow, (PlantType)LockPresent);
            if (CreatePlant.Instance.IsPuff((PlantType)LockPresent))
            {
                CreatePlant.Instance.SetPlant(__instance.thePlantColumn, __instance.thePlantRow,
                    (PlantType)LockPresent);
                CreatePlant.Instance.SetPlant(__instance.thePlantColumn, __instance.thePlantRow,
                    (PlantType)LockPresent);
            }

            return false;
        }

        if (SuperPresent)
        {
            __instance.SuperRandomPlant();
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Present), "Start")]
public static class PresentPatchB
{
    public static void Postfix(Present __instance)
    {
        if (PresentFastOpen && (int)__instance.thePlantType != 245) __instance.AnimEvent();
    }
}

[HarmonyPatch(typeof(ProgressMgr), "Awake")]
public static class ProgressMgrPatchA
{
    public static void Postfix(ProgressMgr __instance)
    {
        GameObject obj = new("ModifierGameInfo");
        var text = obj.AddComponent<TextMeshProUGUI>();
        text.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
        text.color = new Color(0, 1, 1);
        obj.transform.SetParent(__instance.GameObject().transform);
        obj.transform.localScale = new Vector3(0.4f, 0.2f, 0.2f);
        obj.transform.localPosition = new Vector3(100f, 2.2f, 0);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 50);
    }
}

[HarmonyPatch(typeof(ProgressMgr), "Update")]
public static class ProgressMgrPatchB
{
    public static void Postfix(ProgressMgr __instance)
    {
        if (ShowGameInfo)
        {
            __instance.transform.FindChild("ModifierGameInfo").GameObject().active = true;
            __instance.transform.FindChild("ModifierGameInfo").GameObject().GetComponent<TextMeshProUGUI>().text =
                $"波数: {Board.Instance.theWave}/{Board.Instance.theMaxWave} 刷新CD: {Board.Instance.newZombieWaveCountDown:N1}";
        }
        else
        {
            __instance.transform.FindChild("ModifierGameInfo").GameObject().active = false;
        }
    }
}

[HarmonyPatch(typeof(RandomZombie), "SetRandomZombie")]
public static class RamdomZombiePatch
{
    public static bool Prefix(RandomZombie __instance, ref GameObject __result)
    {
        if (!UltimateRamdomZombie) return true;
        if (Board.Instance is not null && Board.Instance.isEveStarted) return true;
        var id = Random.RandomRangeInt(200, 223);
        if (Random.RandomRangeInt(0, 5) == 1)
        {
            if (!__instance.isMindControlled)
                __result = CreateZombie.Instance.SetZombie(__instance.theZombieRow, (ZombieType)id,
                    __instance.GameObject().transform.position.x);
            else
                __result = CreateZombie.Instance.SetZombieWithMindControl(__instance.theZombieRow, (ZombieType)id,
                    __instance.GameObject().transform.position.x);
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Squalour), "LourDie")]
public static class SqualourPatch
{
    public static bool OriginalDevMode { get; set; }

    public static void Postfix()
    {
        GameAPP.developerMode = OriginalDevMode;
    }

    public static void Prefix()
    {
        OriginalDevMode = GameAPP.developerMode;
        GameAPP.developerMode |= DevLour;
    }
}

[HarmonyPatch(typeof(SuperSnowGatling), "Update")]
public static class SuperSnowGatlingPatchA
{
    public static void Postfix(SuperSnowGatling __instance)
    {
        if (UltimateSuperGatling) __instance.timer = 0.1f;
    }
}

[HarmonyPatch(typeof(SuperSnowGatling), "Shoot1")]
public static class SuperSnowGatlingPatchB
{
    public static void Postfix(SuperSnowGatling __instance)
    {
        if (UltimateSuperGatling) __instance.AttributeEvent();
    }
}

[HarmonyPatch(typeof(TravelRefresh), "OnMouseUpAsButton")]
public static class TravelRefreshPatch
{
    public static void Postfix(TravelRefresh __instance)
    {
    if (BuffRefreshNoLimit) __instance.refreshTimes = 2147483647;
    }
}

[HarmonyPatch(typeof(TravelStore), "RefreshBuff")]
public static class TravelStorePatch
{
    public static void Postfix(TravelStore __instance)
    {
        if (BuffRefreshNoLimit) __instance.count = 0;
    }
}

[HarmonyPatch(typeof(ShootingMenu), nameof(ShootingMenu.Refresh))]
public static class ShootingMenuPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {;
        if (BuffRefreshNoLimit) ShootingManager.Instance.refreshCount = 2147483647;
    }
}

[HarmonyPatch(typeof(FruitNinjaManager),nameof(FruitNinjaManager.LoseScore))]
public static class FruitNinjaManagerPatch
{
    [HarmonyPrefix]
    public static void Postfix(ref float value)
    {
        if (BuffRefreshNoLimit) value = -1e-10f;
    }
}

[HarmonyPatch(typeof(FruitObject), nameof(FruitObject.FixedUpdate))]
public static class FrFruitObjectPatch
{
    [HarmonyPostfix]
    public static void Postfix(FruitObject __instance)
    {
        if(!AutoCutFruit) return;
        __instance.gameObject.TryGetComponent<Rigidbody2D>(out var rb);
        if (rb is not null)
        {
            float screenHeight = Camera.main.orthographicSize;
            if (__instance.transform.position.y < -screenHeight && rb.velocity.y < -.0f)
            {
                __instance.Slice();
            }
        }
    }
}
/*
[HarmonyPatch(typeof(CreatePlant), "Lim")]
public static class CreatePlantPatchA
{
    public static void Postfix(ref bool __result) => __result = !UnlockAllFusions && __result;
}

[HarmonyPatch(typeof(CreatePlant), "LimTravel")]
public static class CreatePlantPatchB
{
    public static void Postfix(ref bool __result) => __result = !UnlockAllFusions && __result;
}
*/

[HarmonyPatch(typeof(UIMgr), "EnterMainMenu")]
public static class UIMgrPatch
{
    public static void Postfix()
    {
        GameObject obj1 = new("ModifierInfo");
        var text1 = obj1.AddComponent<TextMeshProUGUI>();
        text1.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
        text1.color = new Color(1, 1, 0, 1);
        text1.text = "修改器作者为b站@Infinite75\n若存在任何付费/要求三连+关注/私信发链接的情况\n说明你被盗版骗了，请注意隐私和财产安全！！！\n此信息仅在游戏主菜单和修改窗口显示";
        obj1.transform.SetParent(GameObject.Find("Leaves").transform);
        obj1.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        obj1.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 50);
        obj1.transform.localPosition = new Vector3(-345.5f, -96.1f, 0);

        GameObject obj2 = new("UpgradeInfo");
        var text2 = obj2.AddComponent<TextMeshProUGUI>();
        text2.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
        text2.color = new Color(0, 1, 0, 1);
        text2.text = "原作者@Infinite75已停更，这是@听雨夜荷的一个fork。\n" +
                     "项目地址: https://github.com/CarefreeSongs712/PVZRHTools\n" +
                     "\n" +
                     "修改器2.8.2-3.29.1更新日志:\n" +
                     "1. 适配2.8.2\n"+
                     "2. 修复旅行商店的bug";
        obj2.transform.SetParent(GameObject.Find("Leaves").transform);
        obj2.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        obj2.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 50);
        obj2.transform.localPosition = new Vector3(-345.5f, 55f, 0);
    }
}

public class CustomIZData
{
    public List<ZombieData>? Zombies { get; set; }
    public List<GridItemData>? GridItems { get; set; }
}

public class ZombieData
{
    public int Type { get; set; }
    public int Row { get; set; }
    public float PositionX { get; set; }
    public bool IsMindControlled { get; set; }
}

public class GridItemData
{
    public int Type { get; set; }
    public int Column { get; set; }
    public int Row { get; set; }
    public int PlantType { get; set; }
}

[HarmonyPatch(typeof(Zombie), "Start")]
public static class ZombiePatch
{
    public static void Postfix(Zombie __instance)
    {
        try
        {
            if (HealthZombies[__instance.theZombieType] >= 0)
            {
                __instance.theMaxHealth = HealthZombies[__instance.theZombieType];
                __instance.theHealth = __instance.theMaxHealth;
            }

            if (Health1st[__instance.theFirstArmorType] >= 0 &&
                __instance.theMaxHealth != Health1st[__instance.theFirstArmorType])
            {
                __instance.theFirstArmorMaxHealth = Health1st[__instance.theFirstArmorType];
                __instance.theFirstArmorHealth = __instance.theFirstArmorMaxHealth;
            }

            if (Health2nd[__instance.theSecondArmorType] >= 0 &&
                __instance.theMaxHealth != Health2nd[__instance.theSecondArmorType])
            {
                __instance.theSecondArmorMaxHealth = Health2nd[__instance.theSecondArmorType];
                __instance.theSecondArmorHealth = __instance.theSecondArmorMaxHealth;
            }

            __instance.UpdateHealthText();
        }
        catch
        {
        }
    }
}

[HarmonyPatch(typeof(Mouse), nameof(Mouse.TryToSetPlantByGlove))]
public static class MousePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Mouse __instance)
    {

        if (true)
        {
            int vcol = __instance.theMouseColumn - __instance.thePlantOnGlove.thePlantColumn;
            int newCol = __instance.theMouseColumn;//(0-9)
            List<Plant> plants = new List<Plant>();
            foreach (var plant in Board.Instance.plantArray)
            {
                if(plant == null || plant.gameObject == null)continue;
                if (plant.thePlantColumn == __instance.thePlantOnGlove.thePlantColumn)
                {
                    MelonLogger.Msg($"{plant.thePlantType} x:{plant.thePlantColumn} y:{plant.thePlantRow}");
                    if(plant == __instance.thePlantOnGlove){}
                    else
                    {
                        plants.Add(plant);
                    }
                }
            }
            foreach (var plant in plants)
            {
                GameObject gameObject =
                    CreatePlant.Instance.SetPlant(newCol, plant.thePlantRow, plant.thePlantType);
                if (gameObject != null && gameObject.TryGetComponent<Plant>(out var component) && component != null)
                {
                    Board.Instance.plantArray.Remove(plant);
                    plant.Die(Plant.DieReason.ByMix);
                }
            }
        }
        MelonLogger.Msg($"x:{__instance.theMouseColumn} y:{__instance.theMouseRow}  oy:{__instance.thePlantOnGlove.thePlantRow}");
        return true;
    }
}

#if false
[HarmonyPatch(typeof(Plant))]
public class Plant_HealthTextPatch
{
    [HarmonyPatch(nameof(Plant.Update))]
    [HarmonyPostfix]
    public static void Postfix_Update(Plant __instance)
    {
        if (!__instance)
            return;
        if (DataProcessor.BetterShowEnabled)
        {
            var produceText = "生产冷却:";
            var armingTimeText = "出土:";
            var chompingCoolDownText = "咀嚼冷却:";
            var reloadCooldownText = "装填冷却:";
            var summonCooldownText = "召唤冷却:";
            var charmLeftText = "魅惑次数:";
            var purgeCooldownText = "消化冷却:";
            var impactCooldownText = "普通陨石:";
            var ultimateCooldownText = "究极陨石:";
            var goldRushCooldownText = "大招冷却:";
            var snipeCooldownText = "狙击冷却:";
            var depletionCooldownText = "衰减冷却:";
            var spawnCooldownText = "生成冷却:";
            var transformCooldown = "变身冷却:";
            var attackCooldown = "攻击冷却:";
            var lightLevelText = "光照等级:";
            var solarCooldownText = "太阳CD:";
            var magnetLevelText = "磁力等级:";
            var fireTimesText = "过火次数:";
            var starCountText = "星星数:";
            var storedDamageText = "存储伤害:";

            var DisplayedString = $"{__instance.thePlantHealth}/{__instance.thePlantMaxHealth}";

            if (__instance != null)
            {
                switch (__instance.thePlantType)
                {
                    case PlantType.SunMine:
                        DisplayedString += '\n' + produceText + __instance.thePlantProduceCountDown.ToString("0.0") +
                                           "s\n" + armingTimeText + __instance.attributeCountdown.ToString("0.0") + "s";
                        break;
                    case PlantType.SilverSunflower:
                    case PlantType.SunFlower:
                    case PlantType.PeaSunFlower:
                    case PlantType.TwinFlower:
                    case PlantType.SunNut:
                    case PlantType.SunShroom:
                    case PlantType.SunPot:
                    case PlantType.SeaSunShroom:
                        DisplayedString += '\n' + produceText + __instance.thePlantProduceCountDown.ToString("0.0") +
                                           "s";
                        break;
                    case PlantType.SunMagnet:
                        DisplayedString += '\n' + produceText + __instance.attributeCountdown.ToString("0.0") + "s";
                        break;
                    case PlantType.PotatoMine:
                        DisplayedString += '\n' + armingTimeText + __instance.attributeCountdown.ToString("0.0") + "s";
                        break;
                    case PlantType.PeaMine:
                        DisplayedString += '\n' + armingTimeText + (__instance.attributeCountdown / 2).ToString("0.0") +
                                           "s";
                        break;
                    case PlantType.Chomper:
                    case PlantType.PeaChomper:
                    case PlantType.SunChomper:
                    case PlantType.CherryChomper:
                    case PlantType.NutChomper:
                    case PlantType.PotatoChomper:
                    case PlantType.DoomChomper:
                        DisplayedString += '\n' + chompingCoolDownText + __instance.attributeCountdown.ToString("0.0") +
                                           "s";
                        break;
                    case PlantType.Marigold:
                    case PlantType.TwinMarigold:
                        DisplayedString += '\n' + produceText + __instance.thePlantProduceCountDown.ToString("0.0") +
                                           "s";
                        break;
                    case PlantType.CobCannon:
                    case PlantType.FireCannon:
                    case PlantType.IceCannon:
                    case PlantType.MelonCannon:
                    case PlantType.UltimateCannon:
                        DisplayedString += '\n' + reloadCooldownText + __instance.attributeCountdown.ToString("0.0") +
                                           "s";
                        break;
                    case PlantType.SuperPumpkin:
                    case PlantType.UltimatePumpkin:
                    case PlantType.BlowerPumpkin:
                        DisplayedString += '\n' + produceText + __instance.attributeCountdown.ToString("0.0") + "s";
                        break;
                    case PlantType.HypnoEmperor:
                        DisplayedString += '\n' + summonCooldownText +
                                           __instance.GetComponent<HyponoEmperor>().summonZombieTime.ToString("0.0") +
                                           "s " + '\n' + charmLeftText +
                                           __instance.GetComponent<HyponoEmperor>().restHealth;
                        break;
                    case PlantType.HypnoQueen:
                        DisplayedString += '\n' + summonCooldownText +
                                           __instance.GetComponent<HypnoQueen>().summonZombieTime.ToString("0.0") +
                                           "s " + '\n' + charmLeftText +
                                           __instance.GetComponent<HypnoQueen>().restHealth;
                        break;
                    case PlantType.LanternMagnet:
                    case PlantType.CherryMagnet:
                    case PlantType.Magnetshroom:
                        DisplayedString += '\n' + purgeCooldownText + __instance.attributeCountdown.ToString("0.0") +
                                           "s";
                        break;
                    case PlantType.SuperStar:
                        DisplayedString += '\n' + impactCooldownText + Board.Instance.bigStarPassiveCountDown.ToString("0.0") +
                                           "s";
                        break;
                    case PlantType.UltimateStar:
                        DisplayedString += '\n' + ultimateCooldownText + Board.Instance.ultimateStarCountDown.ToString("0.0") +
                                           "s";
                        break;
                    case PlantType.GoldCabbage:
                    case PlantType.GoldCorn:
                    case PlantType.GoldGarlic:
                    case PlantType.GoldUmbrella:
                    case PlantType.GoldMelon:
                    case PlantType.SuperUmbrella:
                    case PlantType.EmeraldUmbrella:
                    case PlantType.RedEmeraldUmbrella:
                        DisplayedString += '\n' + goldRushCooldownText + __instance.flashCountDown.ToString("0.0") +
                                           "s";
                        break;
                    case PlantType.UltimateCabbage:
                        DisplayedString += '\n' + goldRushCooldownText + __instance.flashCountDown.ToString("0.0") +
                                           "s\n" + solarCooldownText + Board.Instance.solarCountDown.ToString("0.0") + "s";
                        break;
                    case PlantType.GoldSunflower:
                        DisplayedString += '\n' + goldRushCooldownText + __instance.flashCountDown.ToString("0.0") +
                                           "s\n" + produceText + __instance.attributeCountdown.ToString("0.0") + "s";
                        break;
                    case PlantType.SniperPea:
                        DisplayedString += '\n' + snipeCooldownText +
                                           __instance.thePlantAttackCountDown.ToString("0.0") + "s";
                        break;
                    case PlantType.FireSniper:
                        DisplayedString += '\n' + snipeCooldownText +
                                           __instance.thePlantAttackCountDown.ToString("0.0") + "s";
                        break;
                    case PlantType.UltimateHypno:
                        DisplayedString += '\n' + depletionCooldownText +
                                           __instance.attributeCountdown.ToString("0.0") + "s";
                        break;
                    case PlantType.SquashTorch:
                        __instance.TryGetComponent(out SquashTorch squashTorch);
                        DisplayedString += '\n' + fireTimesText + squashTorch.fireTimes;
                        break;
                    case PlantType.UltimateTorch:
                        __instance.TryGetComponent(out UltimateTorch ultimateTorch);
                        DisplayedString += '\n' + fireTimesText + ultimateTorch.fireTimes;
                        break;
                    case PlantType.CherryTorch:
                        __instance.TryGetComponent(out CherryTorch cherryTorch);
                        DisplayedString += '\n' + fireTimesText + cherryTorch.fireTimes;
                        break;
                    case PlantType.TorchSpike:
                        __instance.TryGetComponent(out CaltropTorch torchSpike);
                        DisplayedString += '\n' + fireTimesText + torchSpike.count;
                        break;
                    case PlantType.KelpTorch:
                        __instance.TryGetComponent(out KelpTorch kelpTorch);
                        DisplayedString += '\n' + fireTimesText + kelpTorch.count;
                        break;
                    case PlantType.Wheat:
                    case PlantType.ObsidianWheat:
                        DisplayedString += '\n' + transformCooldown + (30 - __instance.wheatTime).ToString("0.0") + "s";
                        break;
                    case PlantType.DoomFume:
                        DisplayedString += '\n' + attackCooldown + __instance.thePlantAttackCountDown.ToString("0.0") +
                                           "s";
                        break;
                    case PlantType.PotatoPuff:
                        DisplayedString += '\n' + spawnCooldownText + __instance.attributeCountdown.ToString("0.0") +
                                           "s";
                        break;
                    case PlantType.StarBlover:
                        __instance.TryGetComponent(out StarBlover starBlover);
                        var count = 0;
                        for (var i = 0; i < starBlover.starBullets.Count; i++)
                            if (starBlover.starBullets[i] != null)
                                count++;
                        DisplayedString += '\n' + starCountText + count + '/' + starBlover.maxBullets;
                        break;
                    case PlantType.UltimateBlover:
                        __instance.TryGetComponent(out UltimateStarBlover ultimateStarBlover);
                        var count_ultimate = 0;
                        for (var i = 0; i < ultimateStarBlover.starBullets.Count; i++)
                            if (ultimateStarBlover.starBullets[i] != null)
                                count_ultimate++;
                        DisplayedString += '\n' + starCountText + count_ultimate + '/' + ultimateStarBlover.maxBullets;
                        break;
                    case PlantType.MelonUmbrella:
                        __instance.TryGetComponent(out MelonUmbrella melonUmbrella);
                        DisplayedString += '\n' + storedDamageText + melonUmbrella.storgedDamage;
                        break;
                }

                if (__instance.wheatType != 0 && __instance.thePlantType != PlantType.Wheat && __instance.thePlantType != PlantType.ObsidianWheat)
                    DisplayedString += '\n' + transformCooldown + (30 - __instance.wheatTime).ToString("0.0") + "s";

                if (__instance.currentLightLevel != 0)
                    DisplayedString += '\n' + lightLevelText + __instance.currentLightLevel;

                if (__instance.magnetCount > 0) DisplayedString += '\n' + magnetLevelText + __instance.magnetCount;
                __instance.healthSlider.healthText.text = DisplayedString;
                __instance.healthSlider.healthTextShadow.text = DisplayedString;
                switch (GetCharInStringCount(DisplayedString, '\n'))
                {
                    case 1:
                        __instance.healthSlider.healthText.fontSize = 2.35f;
                        __instance.healthSlider.healthTextShadow.fontSize = 2.35f;
                        break;
                    case 2:
                        __instance.healthSlider.healthText.fontSize = 2.25f;
                        __instance.healthSlider.healthTextShadow.fontSize = 2.25f;
                        break;
                    case 3:
                        __instance.healthSlider.healthText.fontSize = 2.10f;
                        __instance.healthSlider.healthTextShadow.fontSize = 2.10f;
                        break;
                    case 4:
                        __instance.healthSlider.healthText.fontSize = 2.0f;
                        __instance.healthSlider.healthTextShadow.fontSize = 2.0f;
                        break;
                    default:
                        __instance.healthSlider.healthText.fontSize = 1.85f;
                        __instance.healthSlider.healthTextShadow.fontSize = 1.85f;
                        break;
                }

                object GetCharInStringCount(string str, char target)
                {
                    var count = 0;

                    for (var i = 0; i < str.Length; i++)
                        if (str[i] == target)
                            count++;

                    return count;
                }

                __instance.healthSlider.Update();
            }
        }
    }
}

[HarmonyPatch(typeof(Zombie), nameof(Zombie.UpdateHealthText))]
public class Zombie_HealthTextPatch
{
    [HarmonyPatch(nameof(Zombie.UpdateHealthText))]
    [HarmonyPostfix]
    public static void Postfix(Zombie __instance)
    {
        if (!__instance)
            return;
        if (DataProcessor.BetterShowEnabled)
        {
            var count = 0;
            foreach (var p in Board.Instance.plantArray)
                if (p != null)
                {
                    if (p.TryGetComponent(out SniperPea sniperPea) && sniperPea.targetZombie == __instance)
                    {
                        count += sniperPea.attackCount % 6;
                        break;
                    }

                    if (p.TryGetComponent(out FireSniper fireSniper) && fireSniper.targetZombie == __instance)
                    {
                        count += fireSniper.attackCount % 6;
                        break;
                    }
                }

            var poisonLevelText = "狙击秒杀:";
            var snipeExecute = "蒜值:";
            var freezeLevelText = "冻结值:";
            var snowZombieBackText = "回头:";
            var jumpTimeText = "起跳:";

            if (count > 0)
            {
                __instance.healthText.text += '\n' + snipeExecute + (6 - count % 6);
                __instance.healthTextShadow.text += '\n' + snipeExecute + (6 - count % 6);
            }

            if (__instance.freezeLevel > 0)
            {
                __instance.healthText.text +=
                    '\n' + freezeLevelText + __instance.freezeLevel + '/' + __instance.freezeMaxLevel;
                __instance.healthTextShadow.text +=
                    '\n' + freezeLevelText + __instance.freezeLevel + '/' + __instance.freezeMaxLevel;
            }

            if (__instance.poisonLevel > 0)
            {
                __instance.healthText.text += '\n' + poisonLevelText + __instance.poisonLevel;
                __instance.healthTextShadow.text += '\n' + poisonLevelText + __instance.poisonLevel;
            }

            if (__instance != null)
                switch (__instance.theZombieType)
                {
                    case ZombieType.SnowZombie:
                        if (__instance.attributeCountDown > 0)
                        {
                            __instance.healthText.text += '\n' + snowZombieBackText +
                                                          __instance.attributeCountDown.ToString("0.0") + "s";
                            __instance.healthTextShadow.text += '\n' + snowZombieBackText +
                                                                __instance.attributeCountDown.ToString("0.0") + "s";
                        }

                        break;
                    case ZombieType.SuperPogoZombie:
                        __instance.TryGetComponent(out SuperPogoZombie pogo);
                        if (pogo.waitTime > 0 && pogo.waitTime < 5)
                        {
                            __instance.healthText.text +=
                                '\n' + jumpTimeText + (5 - pogo.waitTime).ToString("0.0") + "s";
                            __instance.healthTextShadow.text +=
                                '\n' + jumpTimeText + (5 - pogo.waitTime).ToString("0.0") + "s";
                        }

                        break;
                    case ZombieType.JackboxJumpZombie:
                        __instance.TryGetComponent(out JackboxJumpZombie jackbox);
                        if (jackbox.waitTime > 0 && jackbox.waitTime < 5)
                        {
                            __instance.healthText.text +=
                                '\n' + jumpTimeText + (5 - jackbox.waitTime).ToString("0.0") + "s";
                            __instance.healthTextShadow.text +=
                                '\n' + jumpTimeText + (5 - jackbox.waitTime).ToString("0.0") + "s";
                        }

                        break;
                }

            switch (GetCharInStringCount(__instance!.healthText.text, '\n'))
            {
                case 1:
                    __instance.healthText.fontSize = 2.15f;
                    __instance.healthTextShadow.fontSize = 2.15f;
                    break;
                case 2:
                    __instance.healthText.fontSize = 2.1f;
                    __instance.healthTextShadow.fontSize = 2.1f;
                    break;
                default:
                    __instance.healthText.fontSize = 1.95f;
                    __instance.healthTextShadow.fontSize = 1.95f;
                    break;
            }
        }

        object GetCharInStringCount(string str, char target)
        {
            var count = 0;

            for (var i = 0; i < str.Length; i++)
                if (str[i] == target)
                    count++;

            return count;
        }
    }
}
#endif
[RegisterTypeInIl2Cpp]
public class PatchMgr : MonoBehaviour
{
    internal static bool originalTravel;
    private static int garlicDayTime;
    private static int seaTime;

    static PatchMgr()
    {
        foreach (var f in Enum.GetValues<Zombie.FirstArmorType>()) Health1st.Add(f, -1);
        foreach (var s in Enum.GetValues<Zombie.SecondArmorType>()) Health2nd.Add(s, -1);
    }

    public PatchMgr() : base(ClassInjector.DerivedConstructorPointer<PatchMgr>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public PatchMgr(IntPtr i) : base(i)
    {
    }

    public static bool[] AdvBuffs { get; set; } = [];
    public static bool AlmanacCreate { get; set; } = false;
    public static int AlmanacSeedType { get; set; } = -1;
    public static ZombieType AlmanacZombieType { get; set; } = ZombieType.Nothing;
    public static bool BuffRefreshNoLimit { get; set; } = false;
    public static Dictionary<BulletType, int> BulletDamage { get; set; } = [];
    public static bool CardNoInit { get; set; } = false;
    public static bool ChomperNoCD { get; set; } = false;
    public static bool SuperStarNoCD { get; set; } = false;
    public static bool AutoCutFruit { get; set; } = false;
    public static bool RandomCard { get; set; } = false;
    public static bool CobCannonNoCD { get; set; } = false;
    public static List<int> ConveyBeltTypes { get; set; } = [];
    public static bool[] Debuffs { get; set; } = [];
    public static bool DevLour { get; set; } = false;
    public static bool FastShooting { get; set; } = false;
    public static bool FreeCD { get; set; } = false;
    public static bool FreePlanting { get; set; } = false;
    public static GameModes GameModes { get; set; }
    public static bool GarlicDay { get; set; } = false;
    public static double GloveFullCD { get; set; } = 0;
    public static bool GloveNoCD { get; set; } = false;
    public static double HammerFullCD { get; set; } = 0;
    public static bool HammerNoCD { get; set; } = false;
    public static bool HardPlant { get; set; } = false;
    public static Dictionary<Zombie.FirstArmorType, int> Health1st { get; set; } = [];
    public static Dictionary<Zombie.SecondArmorType, int> Health2nd { get; set; } = [];
    public static Dictionary<PlantType, int> HealthPlants { get; set; } = [];
    public static Dictionary<ZombieType, int> HealthZombies { get; set; } = [];
    public static bool HyponoEmperorNoCD { get; set; } = false;
    public static int ImpToBeThrown { get; set; } = 37;
    public static bool[] InGameAdvBuffs { get; set; } = [];
    public static bool[] InGameDebuffs { get; set; } = [];
    public static bool[] InGameUltiBuffs { get; set; } = [];
    public static bool ItemExistForever { get; set; } = false;
    public static int JachsonSummonType { get; set; } = 7;
    public static bool JackboxNotExplode { get; set; } = false;
    public static int LockBulletType { get; set; } = -2;
    public static bool LockMoney { get; set; } = false;
    public static int LockMoneyCount { get; set; } = 3000;
    public static int LockPresent { get; set; } = -1;
    public static int LockWheat { get; set; } = -1;
    public static bool LockSun { get; set; } = false;
    public static int LockSunCount { get; set; } = 500;
    public static bool MineNoCD { get; set; } = false;
    public static MelonLogger.Instance MLogger => Core.Instance.Value.LoggerInstance;
    public static float NewZombieUpdateCD { get; set; } = 30;
    public static bool NoHole { get; set; } = false;
    public static bool NoIceRoad { get; set; } = false;
    public static bool PlantUpgrade { get; set; } = false;
    public static bool PvPPotRange { get; set; } = false;
    public static bool PresentFastOpen { get; set; } = false;
    public static List<int> SeaTypes { get; set; } = [];

    public static GameObject? SeedGroup
    {
        get
        {
            try
            {
                return InGame() ? GameObject.Find("SeedGroup") : null;
            }
            catch
            {
                return null;
            }
        }
    }

    public static bool ShowGameInfo { get; set; }
    public static bool StopSummon { get; set; } = false;
    public static bool SuperPresent { get; set; } = false;
    public static float SyncSpeed { get; set; } = -1;
    public static bool TimeSlow { get; set; }
    public static bool TimeStop { get; set; }
    public static bool[] UltiBuffs { get; set; } = [];
    public static bool UltimateRamdomZombie { get; set; } = false;
    public static bool UltimateSuperGatling { get; set; } = false;
    public static bool UndeadBullet { get; set; } = false;
    public static bool UnlockAllFusions { get; set; } = false;
    public static bool ZombieSea { get; set; } = false;
    public static int ZombieSeaCD { get; set; } = 40;
    public static bool ZombieSeaLow { get; set; } = false;

    public void Update()
    {
        if (GameAPP.theGameStatus is GameStatus.InGame or GameStatus.InInterlude or GameStatus.Selecting)
        {
            if (Input.GetKeyDown(Core.KeyTimeStop.Value.Value))
            {
                TimeStop = !TimeStop;
                TimeSlow = false;
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TimeStop = false;
                TimeSlow = !TimeSlow;
            }

            if (Input.GetKeyDown(Core.KeyShowGameInfo.Value.Value)) ShowGameInfo = !ShowGameInfo;
            if (!TimeStop && !TimeSlow) Time.timeScale = SyncSpeed;

            if (!TimeStop && TimeSlow) Time.timeScale = 0.2f;
            if (InGameBtnPatch.BottomEnabled || (TimeStop && !TimeSlow)) Time.timeScale = 0;
            try
            {
                try
                {
                    var slow = GameObject.Find("SlowTrigger").transform;
                    slow.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = $"时停(x{Time.timeScale})";
                    slow.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = $"时停(x{Time.timeScale})";
                    if (Input.GetKeyDown(Core.KeyTopMostCardBank.Value.Value))
                    {
                        if (GameAPP.canvas.GetComponent<Canvas>().sortingLayerName == "Default")
                            GameAPP.canvas.GetComponent<Canvas>().sortingLayerName = "UI";
                        else
                            GameAPP.canvas.GetComponent<Canvas>().sortingLayerName = "Default";
                    }
                }
                catch
                {
                }

                if (Input.GetKeyDown(Core.KeyAlmanacCreatePlant.Value.Value) && AlmanacSeedType != -1)
                    CreatePlant.Instance.SetPlant(Mouse.Instance.theMouseColumn, Mouse.Instance.theMouseRow,
                        (PlantType)AlmanacSeedType);
                if (Input.GetKeyDown(Core.KeyAlmanacZombieMindCtrl.Value.Value))
                    Core.AlmanacZombieMindCtrl.Value.Value = !Core.AlmanacZombieMindCtrl.Value.Value;
                if (Input.GetKeyDown(Core.KeyAlmanacCreateZombie.Value.Value) &&
                    AlmanacZombieType is not ZombieType.Nothing)
                {
                    if (Core.AlmanacZombieMindCtrl.Value.Value)
                        CreateZombie.Instance.SetZombieWithMindControl(Mouse.Instance.theMouseRow, AlmanacZombieType,
                            Mouse.Instance.mouseX);
                    else
                        CreateZombie.Instance.SetZombie(Mouse.Instance.theMouseRow, AlmanacZombieType,
                            Mouse.Instance.mouseX);
                }

                if (Input.GetKeyDown(Core.KeyAlmanacCreatePlantVase.Value.Value) && AlmanacSeedType != -1)
                    GridItem.SetGridItem(Mouse.Instance.theMouseColumn, Mouse.Instance.theMouseRow,
                        GridItemType.ScaryPot).Cast<ScaryPot>().thePlantType = (PlantType)AlmanacSeedType;
                if (Input.GetKeyDown(Core.KeyAlmanacCreateZombieVase.Value.Value) &&
                    AlmanacZombieType is not ZombieType.Nothing)
                    GridItem.SetGridItem(Mouse.Instance.theMouseColumn, Mouse.Instance.theMouseRow,
                        GridItemType.ScaryPot).Cast<ScaryPot>().theZombieType = AlmanacZombieType;
                if (Input.GetKeyDown(Core.KeyRandomCard.Value.Value))
                    RandomCard = !RandomCard;
                if (Board.Instance is not null)
                {
                    var t = Board.Instance.boardTag;
                    t.enableTravelPlant = t.enableTravelPlant || UnlockAllFusions;
                    Board.Instance.boardTag = t;
                }
                else
                {
                    return;
                }
            }
            catch (NullReferenceException e)
            {
                MLogger.Error(e);
            }

            if (LockSun) Board.Instance.theSun = LockSunCount;
            if (LockMoney) Board.Instance.theMoney = LockMoneyCount;
            if (StopSummon) Board.Instance.iceDoomFreezeTime = 1;
            if (ZombieSea)
                if (++seaTime >= ZombieSeaCD &&
                    Board.Instance.theWave is not 0 && Board.Instance.theWave < Board.Instance.theMaxWave &&
                    GameAPP.theGameStatus == (int)GameStatus.InGame)
                {
                    foreach (var j in SeaTypes)
                    {
                        if (j < 0) continue;
                        for (var i = 0; i < Board.Instance.rowNum; i++)
                            CreateZombie.Instance.SetZombie(i, (ZombieType)j);
                    }

                    seaTime = 0;
                }

            if (GarlicDay && ++garlicDayTime >= 500 && GameAPP.theGameStatus == (int)GameStatus.InGame)
            {
                garlicDayTime = 0;
                _ = FindObjectsOfTypeAll(Il2CppType.Of<Zombie>()).All(b =>
                {
                    b?.TryCast<Zombie>()?.StartCoroutine_Auto(b?.TryCast<Zombie>()?.DeLayGarliced(0.1f, false, false));
                    return true;
                });
            }
            
            if (SuperStarNoCD)
            {
                if (Board.Instance.bigStarActiveCountDown > 0.5f)
                {
                    Board.Instance.bigStarActiveCountDown = 0.5f;
                }
            }

            if (RandomCard)
            {
                Il2CppSystem.Collections.Generic.List<PlantType> randomPlant = GameAPP.resourcesManager.allPlants;
                if (InGameUI.Instance && randomPlant != null && randomPlant.Count != 0)
                {
                    for (int i = 0; i < InGameUI.Instance.cardOnBank.Length; i++)
                    {
                        try
                        {
                            var index = Random.RandomRangeInt(0, randomPlant.Count);
                            var card = InGameUI.Instance.cardOnBank[i];
                            card.thePlantType = randomPlant[index];
                            card.ChangeCardSprite();
                            card.theSeedCost = 0;
                            card.fullCD = 0;
                        }
                        catch (Exception e) { }
                    }
                }
            }
        }
    }

    //from Gaoshu
    public static string CompressString(string text)
    {
        var buffer = Encoding.UTF8.GetBytes(text);
        using var memoryStream = new MemoryStream();
        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
        {
            gZipStream.Write(buffer, 0, buffer.Length);
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    //from Gaoshu
    public static string DecompressString(string compressedText)
    {
        var gZipBuffer = Convert.FromBase64String(compressedText);
        using var memoryStream = new MemoryStream(gZipBuffer);
        using var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        gZipStream.CopyTo(resultStream);
        var buffer = resultStream.ToArray();
        return Encoding.UTF8.GetString(buffer);
    }

    public static bool[] GetBoolArray(Il2CppStructArray<int> list)
    {
        return [.. from i in list select i > 0];
    }

    public static Il2CppStructArray<int> GetIntArray(bool[] array)
    {
        return new Il2CppStructArray<int>([.. from i in array select i ? 1 : 0]);
    }

    public static bool InGame()
    {
        return Board.Instance is not null &&
               GameAPP.theGameStatus is not GameStatus.OpenOptions or GameStatus.OutGame or GameStatus.Almanac;
    }

    public static IEnumerator PostInitBoard()
    {
        var travelMgr = GameAPP.gameAPP.GetOrAddComponent<TravelMgr>();
        Board.Instance.freeCD = FreeCD;
        yield return null;
        if (!(GameAPP.theBoardType == (LevelType)3 && Board.Instance.theCurrentSurvivalRound != 1))
        {
            yield return null;

            var advs = travelMgr.advancedUpgrades;

            for (var i = 0; i < advs.Count; i++)
            {
                advs[i] = AdvBuffs[i] || advs[i];
                yield return null;
            }

            var ultis = travelMgr.ultimateUpgrades;
            for (var i = 0; i < ultis.Count; i++)
            {
                ultis[i] = UltiBuffs[i] || ultis[i] is 1 ? 1 : 0;
                yield return null;
            }

            var deb = travelMgr.debuff;
            for (var i = 0; i < deb.Count; i++)
            {
                deb[i] = Debuffs[i] || deb[i];
                yield return null;
            }
        }

        InGameAdvBuffs = new bool[TravelMgr.advancedBuffs.Count];
        InGameUltiBuffs = new bool[TravelMgr.ultimateBuffs.Count];
        InGameDebuffs = new bool[TravelMgr.debuffs.Count];
        yield return null;

        InGameAdvBuffs = travelMgr.advancedUpgrades;
        InGameUltiBuffs = GetBoolArray(travelMgr.ultimateUpgrades);
        InGameDebuffs = travelMgr.debuff;
        yield return null;
        Task.Run(SyncInGameBuffs);

        yield return null;
        if (ZombieSeaLow && SeaTypes.Count > 0)
        {
            var i = 0;
            for (var wave = 0; wave < Board.Instance.theMaxWave; wave++)
            for (var index = 0; index < 100; index++)
            {
                SetZombieList(index, wave, (ZombieType)SeaTypes[i]);
                if (++i >= SeaTypes.Count) i = 0;
            }
        }
    }

    //感谢@高数带我飞(Github:https://github.com/LibraHp/)的在出怪表修改上的技术支持
    public static unsafe void SetZombieList(int index, int wave, ZombieType value)
    {
        var fieldInfo = typeof(InitZombieList).GetField("NativeFieldInfoPtr_zombieList",
            BindingFlags.NonPublic | BindingFlags.Static);

        if (fieldInfo is not null)
        {
            var nativeFieldInfoPtr = (IntPtr)fieldInfo.GetValue(null)!;
            Unsafe.SkipInit(out IntPtr intPtr);
            IL2CPP.il2cpp_field_static_get_value(nativeFieldInfoPtr, &intPtr);
            if (intPtr == IntPtr.Zero) return;
            var arrayData = (ZombieType*)intPtr.ToPointer();
            arrayData[index * 101 + wave + 9] = value;
        }
    }

    public static void SyncInGameBuffs()
    {
        if (!InGame()) return;
        DataSync.Instance.Value.SendData(new SyncTravelBuff
        {
            AdvInGame = [.. GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().advancedUpgrades!],
            UltiInGame = [.. GetBoolArray(GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().ultimateUpgrades!)],
            DebuffsInGame = [.. GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().debuff!]
        });
    }

    public static void UpdateInGameBuffs()
    {
        for (var i = 0; i < GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().advancedUpgrades.Count; i++)
            GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().advancedUpgrades![i] = InGameAdvBuffs[i];
        for (var i = 0; i < GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().ultimateUpgrades.Count; i++)
            GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().ultimateUpgrades![i] = GetIntArray(InGameUltiBuffs)[i];
        for (var i = 0; i < GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().debuff.Count; i++)
            GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().debuff![i] = InGameDebuffs[i];
    }
}