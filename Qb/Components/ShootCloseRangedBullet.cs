using EntityStates.Captain.Weapon;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Chen.Qb.Components
{
    internal class ShootCloseRangedBullet : MonoBehaviour
    {
        private static bool initialized = false;
        private static GameObject effectPrefab;
        private static GameObject tracerPrefab;
        private static GameObject hitEffectPrefab;
        private static float cooldown;
        private static string soundString;
        private static float damageCoefficient;
        private static float range;
        private static float force;

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
            tracerPrefab = FirePistol2.tracerEffectPrefab;
            cooldown = .12f;
            soundString = FirePistol2.firePistolSoundString;
            damageCoefficient = .5f;
            range = 15f;
            force = FirePistol2.force;
            hitEffectPrefab = FirePistol2.hitEffectPrefab;
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
                    string muzzle = "Muzzle";
                    foreach (Transform child in transform.Find("ModelBase").Find("MainBody"))
                    {
                        Vector3 childPosition = child.transform.position;
                        float computedDistance = Vector3.Distance(targetPosition, childPosition);
                        if (computedDistance < currentDistance)
                        {
                            aimOrigin = childPosition;
                            currentDistance = computedDistance;
                            muzzle = child.gameObject.name;
                        }
                    }
                    Quaternion rotation = Util.QuaternionSafeLookRotation((aimOrigin - transform.position).normalized);
                    Util.PlaySound(soundString, gameObject);
                    if (effectPrefab) EffectManager.SimpleEffect(effectPrefab, aimOrigin, rotation, false);
                    if (NetworkServer.active)
                    {
                        new BulletAttack
                        {
                            owner = gameObject,
                            weapon = gameObject,
                            origin = aimOrigin,
                            aimVector = (targetPosition - aimOrigin).normalized,
                            minSpread = 0f,
                            maxSpread = 0f,
                            damage = damageCoefficient * characterBody.damage,
                            force = force,
                            tracerEffectPrefab = tracerPrefab,
                            muzzleName = muzzle,
                            hitEffectPrefab = hitEffectPrefab,
                            isCrit = characterBody.RollCrit(),
                            radius = .1f,
                            smartCollision = true
                        }.Fire();
                    }
                    timer -= cooldown;
                }
                timer = Mathf.Min(cooldown, timer);
            }
        }
    }
}