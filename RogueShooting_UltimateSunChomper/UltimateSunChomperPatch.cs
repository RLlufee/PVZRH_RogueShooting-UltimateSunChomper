using System.Collections.Generic;
using HarmonyLib;
using UltimateSunChomper.BepInEx;
using UnityEngine;

namespace RogueShooting_UltimateSunChomper;

[HarmonyPatch(typeof(UltimateSunChomperComponent), "RefreshTraits")]
public static class UltimateSunChomperPatch
{
    [HarmonyPostfix]
    public static void RefreshTraitsPostfix(UltimateSunChomperComponent __instance)
    {
        if (__instance == null) return;

        Core.SyncComponentMode(__instance);
        if (Core.HasBuff(Core.BigStomachBuff))
        {
            __instance.HasTraitBigStomach = true;
        }
    }
}

[HarmonyPatch(typeof(UltimateSunChomperComponent), "get_AttackMultiplier")]
public static class UltimateSunChomperAttackPatch
{
    [HarmonyPostfix]
    public static void AttackMultiplierPostfix(UltimateSunChomperComponent __instance, ref float __result)
    {
        if (Core.IsShootingComponent(__instance))
        {
            __result *= Core.ShootingAttackMultiplier;
        }
    }
}

[HarmonyPatch(typeof(UltimateSunChomperComponent), "CollectFrontZombies")]
public static class UltimateSunChomperTargetPatch
{
    [HarmonyPostfix]
    public static void CollectFrontZombiesPostfix(
        UltimateSunChomperComponent __instance,
        Plant p,
        float range,
        ref List<Zombie> __result)
    {
        if (!Core.HasRangeBoost(__instance) || p == null || __result == null) return;

        try
        {
            var allZombies = Lawnf.GetAllZombies();
            if (allZombies == null) return;

            float maxRange = range + Core.ShootingRangeBonus;
            float plantX = p.transform.position.x;

            for (int i = 0; i < allZombies.Count; i++)
            {
                Zombie zombie = allZombies[i];
                if (!IsUsable(zombie) || zombie.theZombieRow != p.thePlantRow || __result.Contains(zombie))
                {
                    continue;
                }

                float zombieX = zombie.axis != null ? zombie.axis.position.x : zombie.transform.position.x;
                float distance = zombieX - plantX;
                if (distance >= -0.5f && distance <= maxRange)
                {
                    __result.Add(zombie);
                }
            }
        }
        catch
        {
            // 目标列表扩展失败时保留原版索敌结果。
        }
    }

    private static bool IsUsable(Zombie zombie)
    {
        if (zombie == null) return false;
        try { return !zombie.beforeDying && !zombie.isMindControlled; }
        catch { return false; }
    }
}

[HarmonyPatch(typeof(UltimateSunChomperComponent), "OnDestroy")]
public static class UltimateSunChomperDestroyPatch
{
    [HarmonyPostfix]
    public static void OnDestroyPostfix(UltimateSunChomperComponent __instance) => Core.ForgetComponent(__instance);
}

[HarmonyPatch(typeof(UltimateSunChomperComponent), "DoSwallow")]
public static class UltimateSunChomperStarUpSwallowPatch
{
    [HarmonyPostfix]
    public static void DoSwallowPostfix(UltimateSunChomperComponent __instance, bool __result)
    {
        if (!__result || __instance == null) return;

        try
        {
            Core.SyncComponentMode(__instance);
            if (!Core.IsShootingComponent(__instance)) return;

            Plant plant = __instance.plant;
            if (plant != null && plant.starUp)
            {
                Core.ApplyStarUpDigestOverride(__instance, plant);
                Core.SpawnBigCherrySunNutsFromLeftEdge(__instance, plant);
            }
        }
        catch
        {
            // 不影响原本吞食结算。
        }
    }
}

[HarmonyPatch(typeof(UltimateSunChomperComponent), "DoClickEffect")]
public static class UltimateSunChomperStarUpClickPatch
{
    [HarmonyPostfix]
    public static void DoClickEffectPostfix(UltimateSunChomperComponent __instance)
    {
        if (__instance == null) return;

        try
        {
            Core.SyncComponentMode(__instance);
            Plant plant = __instance.plant;
            bool isShooting = Core.IsShootingComponent(__instance);
            bool isStarUp = plant != null && plant.starUp;

            Core.TraceLog?.LogInfo($"[圣樱诸神进化附属] DoClickEffect完成：诸神进化={isShooting}，星辉={isStarUp}");
            if (isShooting && isStarUp)
            {
                Core.SpawnBigCherrySunNutsFromLeftEdge(__instance, plant);
            }
        }
        catch
        {
            // 不影响原本点击效果。
        }
    }
}
