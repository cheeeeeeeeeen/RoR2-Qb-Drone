using EntityStates.Captain.Weapon;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2Util = RoR2.Util;

namespace Chen.Qb.Components
{
    internal class Salvo : MonoBehaviour
    {
        private static bool initialized = false;
        private static GameObject effectPrefab;
        private static float cooldown;
        private static float mayhemDuration;
        private static List<GameObject> projectilePrefabs;
        private static List<float> damageCoefficients;
        private static List<float> forces;
        private static float mayhemInterval;

        private CharacterBody characterBody;
        private float timer;
        private bool mayhemMode;
        private float mayhemTimer;
        private ushort currentAim;
        private InputBankTest inputBank;
        private float mayhemIntervalTimer;
        private Transform modelTransform;
        private Transform root;

        private static void Initialize()
        {
            if (initialized) return;
            initialized = true;
            effectPrefab = FireTazer.chargeEffectPrefab;
            cooldown = 60f;
            mayhemDuration = 1.5f;
            projectilePrefabs = new List<GameObject>
            {
                GlobalEventManager.instance.missilePrefab,
                Resources.Load<GameObject>("prefabs/projectiles/FireworkProjectile"),
                QbDrone.instance.spiderMine
            };
            damageCoefficients = new List<float> { 3f, 1.5f, 5f };
            forces = new List<float> { 1.5f, 3f, 2.5f };
            mayhemInterval = .05f;
        }

        private void Awake()
        {
            Initialize();
            characterBody = gameObject.GetComponent<CharacterBody>();
            inputBank = characterBody.inputBank;
            mayhemIntervalTimer = mayhemTimer = timer = 0f;
            mayhemMode = false;
            currentAim = 10;
            modelTransform = gameObject.transform.Find("ModelBase").Find("MainBody");
        }

        private void FixedUpdate()
        {
            if (mayhemMode)
            {
                mayhemTimer += Time.fixedDeltaTime;
                if (mayhemTimer >= mayhemDuration)
                {
                    mayhemMode = false;
                    mayhemIntervalTimer = mayhemTimer = 0f;
                    currentAim = 10;
                }
                else DoMayhem();
            }
            else
            {
                timer += Time.fixedDeltaTime;
                if (timer >= cooldown)
                {
                    timer -= cooldown;
                    mayhemMode = true;
                }
            }
        }

        private void DoMayhem()
        {
            mayhemIntervalTimer += Time.fixedDeltaTime;
            if (mayhemIntervalTimer >= mayhemInterval)
            {
                Ray aimRay = SetAim(CycleAim(ref currentAim));
                Quaternion rotation = RoR2Util.QuaternionSafeLookRotation(aimRay.direction);
                if (effectPrefab)
                {
                    EffectData effectData = new EffectData
                    {
                        origin = aimRay.origin,
                        rootObject = root.gameObject,
                        rotation = rotation
                    };
                    EffectManager.SpawnEffect(effectPrefab, effectData, false);
                    Util.EffectOptions(characterBody, effectPrefab, false);
                }
                if (NetworkServer.active)
                {
                    int projectileIndex = Random.Range(0, projectilePrefabs.Count);
                    FireProjectileInfo info = new FireProjectileInfo
                    {
                        projectilePrefab = projectilePrefabs[projectileIndex],
                        crit = characterBody.RollCrit(),
                        damage = characterBody.damage * damageCoefficients[projectileIndex],
                        damageColorIndex = DamageColorIndex.Default,
                        force = forces[projectileIndex],
                        owner = gameObject,
                        position = aimRay.origin,
                        rotation = rotation
                    };
                    ProjectileManager.instance.FireProjectile(info);
                    Util.FireOptions(characterBody, info, (_o, _d) => aimRay.direction);
                }
                mayhemIntervalTimer -= mayhemInterval;
            }
        }

        private Ray SetAim(ushort aim)
        {
            root = modelTransform;
            Vector3 d = inputBank.GetAimRay().direction;
            switch (aim)
            {
                case 0:
                    root = modelTransform.Find("Node (Front)");
                    d = modelTransform.forward;
                    break;

                case 1:
                    root = modelTransform.Find("Node (Top)");
                    d = modelTransform.up;
                    break;

                case 2:
                    root = modelTransform.Find("Node (Left)");
                    d = -modelTransform.right;
                    break;

                case 3:
                    root = modelTransform.Find("Node (Right)");
                    d = modelTransform.right;
                    break;

                case 4:
                    root = modelTransform.Find("Node (Bottom)");
                    d = -modelTransform.up;
                    break;

                case 5:
                    root = modelTransform.Find("Node (Back)");
                    d = -modelTransform.forward;
                    break;
            }
            return new Ray(root.position, d.normalized);
        }

        private ushort CycleAim(ref ushort aim)
        {
            aim = (ushort)(aim + 1);
            if (aim > 5) aim = 0;
            return aim;
        }
    }
}