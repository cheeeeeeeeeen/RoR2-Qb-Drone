using Chen.GradiusMod.Items.GradiusOption;
using RoR2;
using RoR2.Projectile;
using System;
using UnityEngine;
using RoR2Util = RoR2.Util;

namespace Chen.Qb
{
    internal static class Util
    {
        public static void FireOptions(CharacterBody owner, FireProjectileInfo ownerInfo, Func<GameObject, Vector3, Vector3> customDirection)
        {
            GradiusOption.instance.FireForAllOptions(owner, (option, _b, _t, direction) =>
            {
                if (customDirection != null) direction = customDirection(option, direction);
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = ownerInfo.projectilePrefab,
                    crit = ownerInfo.crit,
                    damage = ownerInfo.damage,
                    damageColorIndex = ownerInfo.damageColorIndex,
                    force = ownerInfo.force,
                    owner = ownerInfo.owner,
                    position = option.transform.position,
                    rotation = RoR2Util.QuaternionSafeLookRotation(direction)
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            });
        }

        public static void EffectOptions(CharacterBody owner, GameObject effectPrefab, bool transmit)
        {
            GradiusOption.instance.FireForAllOptions(owner, (option, _b, _t, direction) =>
            {
                EffectData effectData = new EffectData
                {
                    origin = option.transform.position,
                    rootObject = option,
                    rotation = RoR2Util.QuaternionSafeLookRotation(direction)
                };
                EffectManager.SpawnEffect(effectPrefab, effectData, transmit);
            });
        }
    }
}