using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using ShootingMODGeneralLib;
using UI;
using UltimateSunChomper.BepInEx;
using UnityEngine;

// 解决 HashSet 与 Il2CppSystem.Collections.Generic 的命名冲突
using HashSetInt = System.Collections.Generic.HashSet<int>;

namespace RogueShooting_UltimateSunChomper;

[BepInDependency("wuxuanmengxi.ultimatesunchomper", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("CustomizeShooting", BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin("RogueShooting-UltimateSunChomper", "Rogue Shooting - Ultimate Sun Chomper", "1.0.0")]
public sealed class Core : BasePlugin
{
    public static ManualLogSource TraceLog { get; private set; }

    public const int PlantId = 5247;
    public const int CherryWarGodId = 903;
    public const int SunNutEmperorId = 963;
    public const int SunNutId = 905;

    public const string FinalWillBuff = "质变：终焉意志·圣樱";
    public const string BigStomachBuff = "质变：大胃袋";

    public const float ShootingRangeBonus = 2f;
    public const float ShootingAttackMultiplier = 2f;

    public static readonly PlantType Type = (PlantType)PlantId;
    public static readonly PlantType CherryWarGod = (PlantType)CherryWarGodId;
    public static readonly PlantType SunNutEmperor = (PlantType)SunNutEmperorId;
    public static readonly PlantType SunNut = (PlantType)SunNutId;

    private static readonly HashSetInt ShootingComponents = new();
    private static readonly HashSetInt RangeBoostedComponents = new();
    private static readonly HashSetInt StarUpComponents = new();

    public override void Load()
    {
        TraceLog = Log;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

        // 圣樱的原生融合配方是 903+963、903+905；诸神进化模式用路线表达同一终点。
        CherryWarGod.AddEvolution(Type);
        SunNutEmperor.AddEvolution(Type);
        SunNut.AddEvolution(Type);

        Type.AddNewRogueShootingPlant(
            hasDamageBuff: true,
            hasSpeedBuff: true,
            hasStarUpBuff: true,
            role: "输出/防御",
            reinforce: Reinforce);

        RegisterDiamondBuff(FinalWillBuff, "诸神进化模式下索敌范围提升2格。");
        RegisterDiamondBuff(BigStomachBuff, "吞食消化时间降为5秒，回血量×3。");

        TraceLog.LogInfo($"[圣樱诸神进化附属] 诸神进化模式词条注册完成：{FinalWillBuff} / {BigStomachBuff}");
    }

    private static void RegisterDiamondBuff(string name, string desc)
    {
        Type.AddBuff(
            () => Type,
            name,
            () => desc,
            () => 1,
            () => 0.05f,
            () => Quality.diamond,
            RefreshTraits);
    }

    private static void Reinforce(Plant plant)
    {
        if (plant == null) return;

        // 保留圣樱自身的攻击逻辑，只叠加诸神进化模式专用的独立增幅。
        plant.ModifyDamage(PlantDamageAdder.Shooting, 3f, add: true, new Il2CppSystem.Nullable<float>(float.MaxValue));
        plant.ModifySpeed(PlantSpeedAdder.Shooting, 2f);
    }

    private static void RefreshTraits()
    {
        try
        {
            List<Plant> plants = Lawnf.GetAllPlants();
            if (plants == null) return;

            for (int i = 0; i < plants.Count; i++)
            {
                Plant plant = plants[i];
                if (plant != null && plant.thePlantType == Type)
                {
                    plant.GetComponent<UltimateSunChomperComponent>()?.RefreshTraits();
                }
            }
        }
        catch
        {
            // 诸神进化模式词条可能在图鉴外获得，不能让刷新失败影响原插件。
        }
    }

    public static bool HasBuff(string name)
    {
        try { return Type.GetBuffCount(name) > 0; }
        catch { return false; }
    }

    public static bool IsShootingMode()
    {
        try { return Board.Instance != null && Board.Instance.boardTag.rogueShooting; }
        catch { return false; }
    }

    public static bool IsShootingComponent(UltimateSunChomperComponent component) =>
        component != null && ShootingComponents.Contains(component.GetInstanceID());

    public static bool HasRangeBoost(UltimateSunChomperComponent component) =>
        component != null && RangeBoostedComponents.Contains(component.GetInstanceID());

    public static void SyncComponentMode(UltimateSunChomperComponent component)
    {
        if (component == null) return;

        int id = component.GetInstanceID();
        bool shooting = IsShootingMode();
        bool starUp = component.plant != null && component.plant.starUp;
        //TraceLog?.LogInfo($"[圣樱诸神进化附属] 同步组件：ID={id}，诸神进化模式={shooting}，星辉={starUp}");

        if (shooting)
        {
            ShootingComponents.Add(id);
            if (HasBuff(FinalWillBuff))
                RangeBoostedComponents.Add(id);
            else
                RangeBoostedComponents.Remove(id);
        }
        else
        {
            ShootingComponents.Remove(id);
            RangeBoostedComponents.Remove(id);
        }
    }

    public static void ForgetComponent(UltimateSunChomperComponent component)
    {
        if (component == null) return;

        int id = component.GetInstanceID();
        ShootingComponents.Remove(id);
        RangeBoostedComponents.Remove(id);
        StarUpComponents.Remove(id);
    }

    public static void ApplyStarUpDigestOverride(UltimateSunChomperComponent component, Plant plant)
    {
        if (component == null || plant == null || !IsShootingComponent(component) || !plant.starUp)
            return;

        try
        {
            plant.attributeCountdown = 5f;
            StarUpComponents.Add(component.GetInstanceID());
            TraceLog?.LogInfo($"[圣樱诸神进化附属] 星辉吞食消化覆盖为5秒：ID={component.GetInstanceID()}");
        }
        catch
        {
        }
    }

    public static void SpawnBigCherrySunNutsFromLeftEdge(UltimateSunChomperComponent component, Plant plant)
    {
        if (component == null || plant == null || !IsShootingComponent(component) || !plant.starUp || Board.Instance == null)
        {
            TraceLog?.LogWarning("[圣樱诸神进化附属] 跳过坚果生成：组件/模式/星辉/棋盘条件不满足");
            return;
        }

        try
        {
            FieldInfo prefabField = typeof(UltimateSunChomperComponent).Assembly
                .GetType("UltimateSunChomper.BepInEx.Core")?
                .GetField("BigCherrySunNutPrefab", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (prefabField?.GetValue(null) is not GameObject prefab)
            {
                TraceLog?.LogError("[圣樱诸神进化附属] 无法通过反射取得 BigCherrySunNutPrefab");
                return;
            }

            Mouse mouse = UnityEngine.Object.FindObjectOfType<Mouse>();
            float plantX = plant.transform.position.x;
            float plantY = plant.transform.position.y;
            float x = plantX - plant.thePlantColumn;
            int totalDamage = Mathf.RoundToInt(plant.attackDamage * component.AttackMultiplier * ShootingAttackMultiplier);

            for (int row = 0; row < Board.Instance.rowNum; row++)
            {
                float y = mouse != null ? mouse.GetLandY(x, row) : plantY;

                GameObject nut = UnityEngine.Object.Instantiate(prefab);
                nut.transform.position = new Vector3(x, y, 0f);

                Component roller = nut.GetComponent("BigCherrySunNutRollComponent")
                    ?? nut.AddComponent<BigCherrySunNutRollComponent>();

                Type rollerType = roller.GetType();
                rollerType.GetField("CreatorAttackDamage")?.SetValue(roller, totalDamage);
                rollerType.GetField("InitialRow")?.SetValue(roller, row);
                rollerType.GetMethod("StartRolling")?.Invoke(roller, null);
            }

            TraceLog?.LogInfo($"[圣樱诸神进化附属] 点击/吞食触发每行坚果生成：行数={Board.Instance.rowNum}");
        }
        catch (Exception ex)
        {
            TraceLog?.LogError($"[圣樱诸神进化附属] 每行坚果生成异常：{ex}");
        }
    }
}



