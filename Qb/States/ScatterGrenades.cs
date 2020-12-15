using Chen.GradiusMod.Items.GradiusOption;
using EntityStates;
using EntityStates.Captain.Weapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using RoR2Util = RoR2.Util;

namespace Chen.Qb.States
{
    internal class ScatterGrenades : BaseState
    {
        private static bool initialized = false;
        private static GameObject effectPrefab;
        private static GameObject grenadePrefab;
        private static float duration;
        private static int projectileNumber;
        private static float intervalFire;
        private static string soundString;
        private static float spread;
        private static float damageCoefficient;

        private Transform aimOrigin;
        private float timer;
        private int soundId;

        private static void Initialize()
        {
            if (initialized) return;
            initialized = true;
            effectPrefab = FireTazer.chargeEffectPrefab;
            grenadePrefab = Resources.Load<GameObject>("prefabs/projectiles/CommandoGrenadeProjectile");
            projectileNumber = 15;
            intervalFire = .1f;
            duration = intervalFire * projectileNumber;
            soundString = ChargeCaptainShotgun.enterSoundString;
            spread = 20f;
            damageCoefficient = 5f;
        }

        private void StopSound()
        {
            if (soundId >= 0) AkSoundEngine.StopPlayingID((uint)soundId);
        }

        public override void OnEnter()
        {
            Initialize();
            base.OnEnter();
            aimOrigin = transform.Find("ModelBase").Find("MainBody").Find("Node (Front)");
            timer = 0f;
            soundId = -1;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            timer += Time.fixedDeltaTime;
            if (timer >= intervalFire)
            {
                timer -= intervalFire;
                Vector3 position = aimOrigin.position;
                Vector3 direction = transform.forward;
                direction = RoR2Util.ApplySpread(direction, 0, spread, 1, 1);
                Quaternion rotation = RoR2Util.QuaternionSafeLookRotation(direction);
                StopSound();
                soundId = (int)RoR2Util.PlaySound(soundString, gameObject);
                if (effectPrefab)
                {
                    EffectData effectData = new EffectData
                    {
                        origin = position,
                        rootObject = aimOrigin.gameObject,
                        rotation = rotation
                    };
                    EffectManager.SpawnEffect(effectPrefab, effectData, false);
                    Util.EffectOptions(characterBody, effectPrefab, false);
                }
                if (isAuthority)
                {
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                    {
                        projectilePrefab = grenadePrefab,
                        crit = RollCrit(),
                        damage = damageStat * damageCoefficient,
                        damageColorIndex = DamageColorIndex.Default,
                        force = 0f,
                        owner = gameObject,
                        position = position,
                        rotation = rotation
                    };
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                    Util.FireOptions(characterBody, fireProjectileInfo, (_o, optionDirection) =>
                    {
                        return RoR2Util.ApplySpread(optionDirection, 0, spread, 1, 1);
                    });
                }
            }
            if (isAuthority && fixedAge >= duration) outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}