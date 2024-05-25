using System;
using System.Collections;
using BoneLib.Nullables;
using Fusion5vs5Gamemode.Utilities;
using LabFusion.NativeStructs;
using MelonLoader;
using SLZ.AI;
using SLZ.Combat;
using SLZ.Marrow.Data;
using SLZ.Marrow.Pool;
using SLZ.Props.Weapons;
using UnityEngine;

namespace Fusion5vs5Gamemode.Client.Combat;

public static class ProjectileRicochet
{
    private static bool _Enabled;
    private static readonly object EnabledLock = new();

    public static void Enable()
    {
        lock (EnabledLock)
        {
            if (!_Enabled)
            {
                ProjectileTrace.OnProjectileImpactedSurface += OnProjectileImpactedSurface;
                _Enabled = true;
            }
        }
    }

    public static void Disable()
    {
        lock (EnabledLock)
        {
            if (_Enabled)
            {
                ProjectileTrace.OnProjectileImpactedSurface -= OnProjectileImpactedSurface;
                _Enabled = false;
            }
        }
    }

    private static void OnProjectileImpactedSurface(Projectile projectile, TriggerRefProxy proxy,
        Gun projectileOrigin, ImpactProperties receiver, Attack_ attack)
    {
        try
        {
#if DEBUG
            // MelonLogger.Msg($"Firing ricochet from Projectile impact of instance {projectile.GetInstanceID()}");
#endif
            Spawnable spawnable = projectileOrigin.defaultCartridge.projectile.spawnable;
            Transform transform = projectileOrigin.transform;
            AssetSpawner.Register(spawnable);
            AssetSpawner.Spawn(spawnable, attack.origin, transform.rotation,
                new BoxedNullable<Vector3>(Vector3.one), false,
                new BoxedNullable<int>(null), (Action<GameObject>)(go => MelonCoroutines.Start(DispatchRicochetProjectile(go.GetComponent<Projectile>(), projectile, proxy,
                    projectileOrigin, receiver, attack))));
        }
        catch (Exception e)
        {
#if DEBUG
            MelonLogger.Msg(
                $"Exception {e} in ProjectileRicochet.OnProjectileImpactedSurface(...). Aborting.");
#endif
        }
    }

    private static IEnumerator DispatchRicochetProjectile(Projectile newProjectile, Projectile originProjectile, TriggerRefProxy proxy,
        Gun projectileOrigin, ImpactProperties receiver, Attack_ attack)
    {
        try
        {
            Vector3 reflectDirection = Vector3.Reflect(attack.direction, attack.normal);
            Transform newProjectileTransform = newProjectile.transform;
            newProjectileTransform.SetPositionAndRotation(attack.origin, Quaternion.LookRotation(reflectDirection));

            newProjectile.SetBulletObject(originProjectile._data, newProjectileTransform, Vector3.zero,
                Quaternion.identity,
                null, proxy);
        }
        catch (Exception e)
        {
#if DEBUG
            MelonLogger.Msg(
                $"Exception {e} in ProjectileRicochet.OnProjectileImpactedSurface(...) SpawnCrate() callback. Aborting.");
#endif
        }

        yield break;
    }
}
