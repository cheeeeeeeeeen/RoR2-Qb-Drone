using EntityStates.Captain.Weapon;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.Drone.DroneWeapon;
using RoR2;
using RoR2.Projectile;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using RoR2Util = RoR2.Util;

namespace Chen.Qb.Components
{
    internal class ShootCloseRangedBullet : MonoBehaviour
    {
        private static bool initialized = false;
        private static GameObject effectPrefab;
        private static GameObject projectilePrefab;
        private static float cooldown;
        private static string soundString;
        private static float damageCoefficient;
        private static float range;
        private static float force;
        private static float velocity;

        private BullseyeSearch enemyFinder;
        private HurtBox target;
        private CharacterBody characterBody;
        private InputBankTest inputBank;
        private TeamComponent teamComponent;
        private float timer;

        private static void Initialize()
        {
            if (initialized) return;
            initialized = true;
            effectPrefab = FireTazer.chargeEffectPrefab;
            projectilePrefab = FireTwinRocket.projectilePrefab;
            cooldown = .3f;
            soundString = FirePistol2.firePistolSoundString;
            damageCoefficient = 1f;
            range = 15f;
            force = FireTwinRocket.force;
            velocity = projectilePrefab.GetComponent<ProjectileSimple>().desiredForwardSpeed * 2f;
        }

        private void Awake()
        {
            Initialize();
            characterBody = gameObject.GetComponent<CharacterBody>();
            inputBank = characterBody.inputBank;
            teamComponent = characterBody.teamComponent;
            timer = 0f;
        }

        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (timer >= cooldown)
            {
                enemyFinder = new BullseyeSearch
                {
                    maxDistanceFilter = range,
                    maxAngleFilter = 180f,
                    searchOrigin = transform.position,
                    searchDirection = inputBank.GetAimRay().direction,
                    filterByLoS = true,
                    sortMode = BullseyeSearch.SortMode.Distance,
                    teamMaskFilter = TeamMask.allButNeutral
                };
                if (teamComponent) enemyFinder.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
                enemyFinder.RefreshCandidates();
                target = enemyFinder.GetResults().FirstOrDefault();
                if (target)
                {
                    Vector3 targetPosition = target.transform.position;
                    float currentDistance = range * 2;
                    Vector3 aimOrigin = transform.position;
                    GameObject root = gameObject;
                    foreach (Transform child in transform.Find("ModelBase").Find("MainBody"))
                    {
                        Vector3 childPosition = child.transform.position;
                        float computedDistance = Vector3.Distance(targetPosition, childPosition);
                        if (computedDistance < currentDistance)
                        {
                            aimOrigin = childPosition;
                            currentDistance = computedDistance;
                            root = child.gameObject;
                        }
                    }
                    Quaternion rotation = RoR2Util.QuaternionSafeLookRotation((targetPosition - aimOrigin).normalized);
                    RoR2Util.PlaySound(soundString, gameObject);
                    if (effectPrefab)
                    {
                        EffectData effectData = new EffectData
                        {
                            origin = aimOrigin,
                            rootObject = root,
                            rotation = rotation
                        };
                        EffectManager.SpawnEffect(effectPrefab, effectData, false);
                        Util.EffectOptions(characterBody, effectPrefab, false);
                    }
                    if (NetworkServer.active)
                    {
                        float computedDamage = characterBody.damage * damageCoefficient;
                        FireProjectileInfo info = new FireProjectileInfo
                        {
                            projectilePrefab = projectilePrefab,
                            position = aimOrigin,
                            rotation = rotation,
                            owner = gameObject,
                            damage = computedDamage,
                            force = force,
                            crit = characterBody.RollCrit(),
                            damageColorIndex = DamageColorIndex.Default,
                            speedOverride = velocity
                        };
                        ProjectileManager.instance.FireProjectile(info);
                        Util.FireOptions(characterBody, info, (option, _d) =>
                        {
                            return (targetPosition - option.transform.position).normalized;
                        });
                        Util.TriggerArmsRace(characterBody, computedDamage);
                    }
                    timer -= cooldown;
                }
                timer = Mathf.Min(cooldown, timer);
            }
        }
    }
}