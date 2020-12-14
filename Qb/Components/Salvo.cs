using EntityStates.Captain.Weapon;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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

        private static void Initialize()
        {
            if (initialized) return;
            initialized = true;
            effectPrefab = FireTazer.chargeEffectPrefab;
            cooldown = 60f;
            mayhemDuration = 3f;
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
                Quaternion rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
                EffectManager.SimpleEffect(effectPrefab, aimRay.origin, rotation, false);
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
                }
                mayhemIntervalTimer -= mayhemInterval;
            }
        }

        private Ray SetAim(ushort aim)
        {
            Transform t = modelTransform;
            Vector3 d = inputBank.GetAimRay().direction;
            switch (aim)
            {
                case 0:
                    t = t.Find("Node (Front)");
                    d = modelTransform.forward;
                    break;

                case 1:
                    t = t.Find("Node (Top)");
                    d = modelTransform.up;
                    break;

                case 2:
                    t = t.Find("Node (Left)");
                    d = -modelTransform.right;
                    break;

                case 3:
                    t = t.Find("Node (Right)");
                    d = modelTransform.right;
                    break;

                case 4:
                    t = t.Find("Node (Bottom)");
                    d = -modelTransform.up;
                    break;

                case 5:
                    t = t.Find("Node (Back)");
                    d = -modelTransform.forward;
                    break;
            }
            return new Ray(t.position, d.normalized);
        }

        private ushort CycleAim(ref ushort aim)
        {
            aim = (ushort)(aim + 1);
            if (aim > 5) aim = 0;
            return aim;
        }
    }
}