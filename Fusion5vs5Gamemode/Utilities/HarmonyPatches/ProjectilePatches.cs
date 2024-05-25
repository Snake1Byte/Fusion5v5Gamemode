using System;
using HarmonyLib;
using MelonLoader;
using SLZ.AI;
using SLZ.Combat;
using SLZ.Marrow.Data;
using UnityEngine;

namespace Fusion5vs5Gamemode.Utilities.HarmonyPatches;

[HarmonyPatch(typeof(Projectile))]
public static class ProjectilePatches
{
    public static Action<Projectile, ProjectileData, Transform, TriggerRefProxy>? OnSetBulletObject;
#if DEBUG
    private static int _Counter;
#endif

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Projectile.SetBulletObject))]
    public static void SetBulletObject(Projectile __instance, ProjectileData data, Transform startTransform,
        Vector3 locPos,
        Quaternion locRot, Rigidbody EmittingRigidbody, TriggerRefProxy proxy)
    {
        try
        {
#if DEBUG
            // _Counter++;
            // MelonLogger.Msg(
            //     $"{_Counter}: Called Projectile.SetBulletObject(instance = {__instance.GetInstanceID()}, data = {data.GetInstanceID()} {data}, startTransform = {startTransform.GetInstanceID()} {startTransform.position} {startTransform.rotation} {startTransform}, locPos = {locPos}, locRot = {locRot}, EmittingRigidbody = {EmittingRigidbody}, proxy = {proxy.GetInstanceID()} {proxy})");
#endif
            SafeActions.InvokeActionSafe(OnSetBulletObject, __instance, data, startTransform, proxy);
        }
        catch (Exception e)
        {
#if DEBUG
            MelonLogger.Msg(
                $"Exception {e} fired in Projectile.SetBulletObject() HarmonyPatch.");
#endif
        }
    }
/*

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Projectile), MethodType.Constructor)]
        public static void Constructor()
        {
            Counter++;
            MelonLogger.Msg($"{Counter}: Projectile's constructor called.");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Projectile.Awake))]
        public static void Awake()
        {
            Counter++;
            MelonLogger.Msg($"{Counter}: Projectile.Awake() called");
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Projectile.EstablishVariables))]
        public static void EstablishVariables(string name, Transform EmittingTransform, TriggerRefProxy proxy)
        {
            Counter++;
            MelonLogger.Msg(
                $"{Counter}: Projectile.EstablishVariables(name = {name}, EmittingTransform = {EmittingTransform}, proxy = {proxy.GetInstanceID()} {proxy}) called");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Projectile.OnDestroy))]
        public static void OnDestroy()
        {
            Counter++;
            MelonLogger.Msg($"{Counter}: Projectile.OnDestroy() called");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Projectile.OnEnable))]
        public static void OnEnable()
        {
            Counter++;
            MelonLogger.Msg($"{Counter}: Projectile.OnEnable() called");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Projectile.OnDisable))]
        public static void OnDisable()
        {
            Counter++;
            MelonLogger.Msg($"{Counter}: Projectile.OnDisable() called");
        }

        // [HarmonyPrefix]
        // [HarmonyPatch(nameof(Projectile.Update))]
        // public static void Update(Projectile __instance)
        // {
        //     Counter++;
        //     if (__instance != null && __instance._direction != null)
        //     {
        //         MelonLogger.Msg($"{Counter}: Projectile.Update() called. Current speed = {__instance.currentSpeed}, direction = {__instance._direction}. Calculated direction: {__instance.transform.forward}");
        //     }
        //     else
        //     {
        //         MelonLogger.Msg($"{Counter}: Projectile.Update() called.");
        //     }
        // }
        */
}
