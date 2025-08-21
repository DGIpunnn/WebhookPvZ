using Il2Cpp;
using MelonLoader;

namespace ToolMod;

public static class Tool
{
    public static void log(object a)
    {
        MelonLogger.Msg(a);
    }
    
    public static void print(Plant plant)
    {
        log($"type:{plant.thePlantType} col:{plant.thePlantColumn} row:{plant.thePlantRow} health:{plant.thePlantHealth}/{plant.thePlantMaxHealth}");
    }
}