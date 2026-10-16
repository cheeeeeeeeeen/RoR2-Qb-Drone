#undef DEBUG

using Chen.GradiusMod;
using Chen.GradiusMod.Drones;
using Chen.Helpers.GeneralHelpers;
using Chen.Helpers.UnityHelpers;
using Chen.Qb.Components;
using Chen.Qb.States;
using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static Chen.Qb.ModPlugin;
using static R2API.DirectorAPI;
using static R2API.DirectorAPI.Helpers;
using UnityObject = UnityEngine.Object;

namespace Chen.Qb
{
    internal class QbDrone : Drone<QbDrone>
    {
        public InteractableSpawnCard iSpawnCard { get; set; }
        public GameObject spiderMine { get; set; }

        public override bool canHaveOptions => true;

        private GameObject brokenObject { get; set; }
        private DirectorCardHolder iDirectorCardHolder { get; set; }
        private GameObject droneBody { get; set; }
        private GameObject droneMaster { get; set; }

        private static readonly Lazy<SkillDef> _skillBasis = LazyAddressable<SkillDef>("RoR2/Base/Drones/Drone1BodyGun.asset");
        private static readonly Lazy<InteractableSpawnCard> _iscBasis = LazyAddressable<InteractableSpawnCard>("RoR2/Base/Drones/iscBrokenDrone1.asset");
        private static readonly Lazy<GameObject> _originalSpiderMine = LazyAddressable<GameObject>("RoR2/Base/Engi/SpiderMine.prefab");

        private static InteractableSpawnCard interactableSpawnCardBasis { get => _iscBasis.Value; }

        private static SkillDef skillBasis { get => _skillBasis.Value; }

        private static GameObject originalSpiderMine { get => _originalSpiderMine.Value; }

        private static readonly Lazy<GameObject> _equipmentDroneBody = LazyAddressable<GameObject>("RoR2/Base/Drones/EquipmentDroneBody.prefab");

        private static GameObject equipmentDroneBody { get => _equipmentDroneBody.Value; }

        protected override GameObject DroneCharacterMasterObject => droneMaster;

        protected override void SetupConfig()
        {
            spawnWeightWithMachinesArtifact = 0;
            base.SetupConfig();
        }

        protected override void SetupComponents()
        {
            base.SetupComponents();
            LanguageAPI.Add("QB_DRONE_NAME", "Qb Drone");
            LanguageAPI.Add("QB_DRONE_CONTEXT", "Repair Qb Drone");
            LanguageAPI.Add("QB_DRONE_INTERACTABLE_NAME", "Broken Qb Drone");
            LanguageAPI.Add("QB_DRONE_PICKUP", "A mysterious floating cube that lobs grenades, rockets nearby enemies and periodically unleashes a salvo.");
            LanguageAPI.Add("QB_DRONE_DESCRIPTION",
                "Lobs a volley of <style=cIsDamage>15 grenades</style> for <style=cIsDamage>500% damage</style> each. " +
                "Fires rockets at enemies within <style=cIsUtility>10m</style> for <style=cIsDamage>100% damage</style>. " +
                "Every <style=cIsUtility>60 seconds</style>, unleashes a salvo of <style=cIsDamage>missiles, fireworks and mines</style> from all of its sides.");
            LanguageAPI.Add("QB_DRONE_SKILL_DESCRIPTION",
                "Lob Grenades: Lob 15 grenades for <style=cIsDamage>500% damage</style> each. " +
                "Also fires at nearby enemies and periodically unleashes a <style=cIsDamage>Mayhem Salvo</style>.");
            GameObject pickupDisplay = CreatePickupDisplay(assetBundle.LoadAsset<GameObject>("Assets/DroneBody/MainBody.prefab"));
            brokenObject = interactableSpawnCardBasis.prefab.InstantiateClone($"{name}Broken", true);
            SummonMasterBehavior summonMasterBehavior = brokenObject.GetComponent<SummonMasterBehavior>();
            droneMaster = summonMasterBehavior.masterPrefab.InstantiateClone($"{name}Master", true);
            ContentAddition.AddMaster(droneMaster);
            AISkillDriver[] skillDrivers = droneMaster.GetComponents<AISkillDriver>();
            if (skillDrivers.Length > 4)
            {
                skillDrivers[3].maxDistance = 25f;
                skillDrivers[4].maxDistance = 50f;
            }
            else Log.Warning($"{name}: The Gunner Drone skill drivers changed. Attack ranges are left at their default values.");
            skillDrivers.SetAllDriversToAimTowardsEnemies();
            CharacterMaster master = droneMaster.GetComponent<CharacterMaster>();
            droneBody = master.bodyPrefab.InstantiateClone($"{name}Body", true);
            ContentAddition.AddBody(droneBody);
            CharacterBody body = droneBody.GetComponent<CharacterBody>();
            body.baseNameToken = "QB_DRONE_NAME";
            body.baseMaxHealth *= 2;
            body.baseRegen *= 2;
            body.baseDamage *= 2;
            body.baseCrit *= 2;
            body.levelMaxHealth *= 2;
            body.levelRegen *= 2;
            body.levelDamage *= 2;
            body.levelCrit *= 2;
            body.portraitIcon = assetBundle.LoadAsset<Texture>("Assets/Icon/QbIcon.png");
            GameObject customModel = assetBundle.LoadAsset<GameObject>("Assets/DroneBody/MainBody.prefab");
            droneBody.ReplaceModel(customModel, DebugCheck());
            customModel.InitializeDroneModelComponents(body, 1.4f);
            SkillLocator locator = droneBody.GetComponent<SkillLocator>();
            ContentAddition.AddEntityState<ScatterGrenades>(out _);
            SkillDef grenadeSkillDef = UnityObject.Instantiate(skillBasis);
            grenadeSkillDef.activationState = new SerializableEntityStateType(typeof(ScatterGrenades));
            grenadeSkillDef.baseRechargeInterval = 12;
            grenadeSkillDef.beginSkillCooldownOnSkillEnd = true;
            grenadeSkillDef.baseMaxStock = 1;
            grenadeSkillDef.fullRestockOnAssign = false;
            ContentAddition.AddSkillDef(grenadeSkillDef);
            SkillFamily grenadeSkillFamily = UnityObject.Instantiate(locator.primary.skillFamily);
            grenadeSkillFamily.variants = new SkillFamily.Variant[1];
            grenadeSkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = grenadeSkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node("", false, null)
            };
            locator.primary.SetFieldValue("_skillFamily", grenadeSkillFamily);
            ContentAddition.AddSkillFamily(grenadeSkillFamily);
            UseEquipmentDroneCommand(droneBody, locator);
            CharacterDeathBehavior death = body.GetOrAddComponent<CharacterDeathBehavior>();
            death.deathState = new SerializableEntityStateType(typeof(DeathState));
            master.bodyPrefab = droneBody;
            summonMasterBehavior.masterPrefab = droneMaster;
            PurchaseInteraction purchaseInteraction = brokenObject.GetComponent<PurchaseInteraction>();
            purchaseInteraction.cost *= 4;
            purchaseInteraction.Networkcost = purchaseInteraction.cost;
            purchaseInteraction.contextToken = "QB_DRONE_CONTEXT";
            purchaseInteraction.displayNameToken = "QB_DRONE_INTERACTABLE_NAME";
            GenericDisplayNameProvider nameProvider = brokenObject.GetComponent<GenericDisplayNameProvider>();
            nameProvider.displayToken = "QB_DRONE_NAME";
            GameObject customBrokenModel = assetBundle.LoadAsset<GameObject>("Assets/DroneBroken/MainBroken.prefab");
            GameObject customBrokenInnerModel = customBrokenModel.transform.Find("BrokenCube").gameObject;
            ReplaceBrokenModel(brokenObject, customBrokenModel, customBrokenInnerModel.GetComponent<MeshRenderer>(), customBrokenInnerModel);
            Transform damagePoint = FindBrokenDamagePoint(customBrokenModel);
            if (damagePoint)
            {
                damagePoint.localPosition = Vector3.zero;
                damagePoint.localRotation = Quaternion.identity;
                damagePoint.localScale = Vector3.one;
            }
            iSpawnCard = UnityObject.Instantiate(interactableSpawnCardBasis);
            iSpawnCard.name = $"iscBroken{name}";
            iSpawnCard.prefab = brokenObject;
            iSpawnCard.slightlyRandomizeOrientation = false;
            iSpawnCard.orientToFloor = true;
            DirectorCard directorCard = new DirectorCard
            {
                spawnCard = iSpawnCard,
#if DEBUG
                selectionWeight = 1000,
                minimumStageCompletions = 0,
#else
                selectionWeight = 1,
                minimumStageCompletions = 4,
#endif
                spawnDistance = DirectorCore.MonsterSpawnDistance.Close,
                preventOverhead = false
            };
            iDirectorCardHolder = new DirectorCardHolder
            {
                Card = directorCard,
                MonsterCategory = MonsterCategory.Invalid,
                InteractableCategory = InteractableCategory.Drones,
            };
            spiderMine = originalSpiderMine.InstantiateClone("QbSpiderMine", true);
            ProjectileDeployToOwner deployToOwner = spiderMine.GetComponent<ProjectileDeployToOwner>();
            if (deployToOwner) UnityObject.DestroyImmediate(deployToOwner);
            spiderMine.AddComponent<Disappear>();
            ContentAddition.AddProjectile(spiderMine);
            droneDef = CreateDroneDef(LoadVanillaDroneDef("RoR2/Base/Drones/MegaDrone.asset"), droneBody, brokenObject, iSpawnCard, "QB_DRONE",
                                      body.portraitIcon, pickupDisplay, LoadVanillaDroneDef("RoR2/Base/Drones/EquipmentDrone.asset"));
        }

        // Swaps the Gunner Drone command inherited from the cloned body for the Ram command of the Equipment Drone.
        private void UseEquipmentDroneCommand(GameObject bodyObject, SkillLocator locator)
        {
            DroneCommandReceiver receiver = bodyObject.GetComponent<DroneCommandReceiver>();
            DroneCommandReceiver equipmentDroneReceiver = equipmentDroneBody ? equipmentDroneBody.GetComponent<DroneCommandReceiver>() : null;
            if (!receiver || !receiver.commandSkill || !equipmentDroneReceiver || !equipmentDroneReceiver.commandSkill)
            {
                Log.Warning($"{name}: The drone command components were not found. Qb keeps the Gunner Drone command.");
                return;
            }
            if (receiver.commandSkill == locator.primary)
            {
                Log.Warning($"{name}: The drone command shares the primary skill slot. Qb keeps its grenades as the command.");
                return;
            }
            receiver.commandSkill.SetFieldValue("_skillFamily", equipmentDroneReceiver.commandSkill.skillFamily);
            receiver.targetType = equipmentDroneReceiver.targetType;
            receiver.targetDistance = equipmentDroneReceiver.targetDistance;
        }

        protected override void SetupBehavior()
        {
            base.SetupBehavior();
            AddNewInteractable(iDirectorCardHolder);
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            if (obj.name.Contains("QbDroneBody"))
            {
                obj.gameObject.AddComponent<ShootCloseRangedBullet>();
                obj.gameObject.AddComponent<Salvo>();
            }
        }

        internal static Lazy<T> LazyAddressable<T>(string key)
        {
            return new Lazy<T>(() =>
            {
                T asset = Addressables.LoadAssetAsync<T>(key).WaitForCompletion();
                if (asset == null) Log.Error($"Failed to load the addressable asset {key}.");
                return asset;
            });
        }

        internal static bool DebugCheck()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}