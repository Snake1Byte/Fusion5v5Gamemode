using System;
using System.Collections.Generic;
using System.Linq;
using BoneLib;
using Fusion5vs5Gamemode.Utilities.HarmonyPatches;
using LabFusion.NativeStructs;
using MelonLoader;
using SLZ.AI;
using SLZ.Combat;
using SLZ.Marrow.Data;
using SLZ.Props.Weapons;
using UnityEngine;

namespace Fusion5vs5Gamemode.Utilities;

public static class ProjectileTrace
{
    private static Action<Projectile, TriggerRefProxy, Gun, ImpactProperties, Attack_>? _OnProjectileImpactedSurface;
    private static Action<Projectile, TriggerRefProxy, ImpactProperties, Attack_>? _OnProjectileDamagedPlayer;

    public static event Action<Projectile, TriggerRefProxy, Gun, ImpactProperties, Attack_>? OnProjectileImpactedSurface
    {
        add
        {
            if (_OnProjectileImpactedSurface == null)
            {
                Hooking.OnLevelInitialized += EmptyDictionaries;
                GunPatches.OnGunFired += GunFired;
                ProjectilePatches.OnSetBulletObject += ProjectileDispatched;
                ImpactPropertiesPatches.OnAttackReceived += ProjectileImpactedSurface;
            }

            _OnProjectileImpactedSurface += value;
        }
        remove
        {
            _OnProjectileImpactedSurface -= value;
            if (_OnProjectileImpactedSurface == null)
            {
                EmptyDictionaries();
                Hooking.OnLevelInitialized -= EmptyDictionaries;
                GunPatches.OnGunFired -= GunFired;
                ProjectilePatches.OnSetBulletObject -= ProjectileDispatched;
                ImpactPropertiesPatches.OnAttackReceived -= ProjectileImpactedSurface;
            }
        }
    }
/*
    public static event Action<Projectile, TriggerRefProxy, ImpactProperties, Attack_>? OnProjectileDamagedPlayer
    {
        add { OnProjectileDamagedPlayer += value; }
        remove { OnProjectileDamagedPlayer -= value; }
    }
*/
    
    private static readonly Dictionary<int, Gun> FirePointOrigin = new();
    private static readonly Dictionary<Projectile, Gun> ProjectileOrigin = new();

    private static readonly Dictionary<int, TriggerRefProxy> TriggerRefProxys = new();

    private static readonly object DictionariesLock = new();

    private static void EmptyDictionaries(LevelInfo obj)
    {
        EmptyDictionaries();
    }

    private static void EmptyDictionaries()
    {
        FirePointOrigin.Clear();
        ProjectileOrigin.Clear();
        TriggerRefProxys.Clear();
    }

    private static void GunFired(Gun gun)
    {
        try
        {
            lock (DictionariesLock)
            {
                FirePointOrigin[gun.firePointTransform.GetInstanceID()] = gun;
            }
        }
        catch (Exception e)
        {
#if DEBUG
            MelonLogger.Msg(
                $"Exception {e} in ProjectileTrace.GunFired(...). Aborting.");
#endif
        }
    }

    private static void ProjectileDispatched(Projectile projectile, ProjectileData data, Transform startTransform,
        TriggerRefProxy proxy)
    {
        try
        {
            lock (DictionariesLock)
            {
                if (FirePointOrigin.TryGetValue(startTransform.GetInstanceID(), out Gun gun))
                {
                    FirePointOrigin.Remove(startTransform.GetInstanceID());
                    ProjectileOrigin[projectile] = gun;
                    TriggerRefProxys[projectile.GetInstanceID()] = proxy;
                }
                else
                {
#if DEBUG
                    MelonLogger.Msg(
                        $"Could not find Gun that fired at Firepoint {startTransform.GetInstanceID()} with name {startTransform.gameObject.name}. Aborting.");
#endif
                }
            }
        }
        catch (Exception e)
        {
#if DEBUG
            MelonLogger.Msg(
                $"Exception {e} in ProjectileTrace.ProjectileDispatched(...). Aborting.");
#endif
        }
    }

    /*
    private static void ProjectileDamagedPlayer(Attack_ attack)
    {
    }
    */

    private static void ProjectileImpactedSurface(ImpactProperties receiver, Attack_ attack)
    {
        Projectile impactOrigin;
        Gun projectileOrigin;
        TriggerRefProxy proxy;
        lock (DictionariesLock)
        {
            try
            {
                impactOrigin = ProjectileOrigin.Keys.First(e => e._direction.Equals(attack.direction));
#if DEBUG
                // MelonLogger.Msg(
                //     $"Projectile that fits the impacted surface's impact direction is {impactOrigin.GetInstanceID()} with direction {impactOrigin._direction}");
#endif
                if (ProjectileOrigin.TryGetValue(impactOrigin, out Gun gun))
                {
                    ProjectileOrigin.Remove(impactOrigin);
                    projectileOrigin = gun;
                }
                else
                {
#if DEBUG
                    // MelonLogger.Msg(
                    //     $"Could not find Gun that fired Projectile {impactOrigin.GetInstanceID()} with name {impactOrigin.gameObject.name}. Aborting.");
#endif
                    return;
                }

                if (TriggerRefProxys.TryGetValue(impactOrigin.GetInstanceID(), out TriggerRefProxy triggerRefProxy))
                {
                    if (triggerRefProxy == null)
                    {
#if DEBUG
                        // MelonLogger.Msg(
                        //     $"TriggerRefProxy component for Prejectile with GO name \"{impactOrigin.gameObject.name}\" was found to be null. Aborting.");
#endif
                        TriggerRefProxys.Remove(impactOrigin.GetInstanceID());
                        return;
                    }

                    proxy = triggerRefProxy;
                    TriggerRefProxys.Remove(impactOrigin.GetInstanceID());
                }
                else
                {
#if DEBUG
                    // MelonLogger.Msg(
                    //     $"No TriggerRefProxy component found for Prejectile with GO name \"{impactOrigin.gameObject.name}\". Aborting.");
#endif
                    return;
                }
            }
            catch (InvalidOperationException)
            {
#if DEBUG
                // MelonLogger.Msg(
                //     $"No Projectile component found that impacted on a surface with direction {attack.direction}. Aborting.");
#endif
                return;
            }
            catch (Exception e)
            {
#if DEBUG
                // MelonLogger.Msg(
                //     $"Exception {e} in ProjectileTrace.ProjectileImpactedSurface(...). Aborting.");
#endif
                return;
            }
        }


        SafeActions.InvokeActionSafe(_OnProjectileImpactedSurface, impactOrigin, proxy, projectileOrigin, receiver,
            attack);
    }
}