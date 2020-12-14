#undef DEBUG

using Chen.GradiusMod.Drones;
using Chen.GradiusMod.Items.GradiusOption;
using Chen.Helpers.CollectionHelpers;
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
using System.Collections.Generic;
using UnityEngine;
using static R2API.DirectorAPI;

namespace Chen.Qb
{
    internal class QbDrone : Drone<QbDrone>
    {
        public InteractableSpawnCard iSpawnCard { get; set; }
        public GameObject spiderMine { get; set; }

        private GameObject brokenObject { get; set; }
        private DirectorCardHolder iDirectorCardHolder { get; set; }
        private GameObject droneBody { get; set; }
        private GameObject droneMaster { get; set; }

        protected override void SetupComponents()
        {
            base.SetupComponents();
            LanguageAPI.Add("QB_DRONE_NAME", "Qb Drone");
            LanguageAPI.Add("QB_DRONE_CONTEXT", "Repair Qb Drone");
            LanguageAPI.Add("QB_DRONE_INTERACTABLE_NAME", "Broken Qb Drone");
            InteractableSpawnCard origIsc = Resources.Load<InteractableSpawnCard>("spawncards/interactablespawncard/iscBrokenDrone1");
            brokenObject = origIsc.prefab;
            brokenObject = brokenObject.InstantiateClone($"{name}Broken");
            SummonMasterBehavior summonMasterBehavior = brokenObject.GetComponent<SummonMasterBehavior>();
            droneMaster = summonMasterBehavior.masterPrefab.InstantiateClone($"{name}Master");
            MasterCatalog.getAdditionalEntries += (list) => list.Add(droneMaster);
            AISkillDriver[] skillDrivers = droneMaster.GetComponents<AISkillDriver>();
            skillDrivers[3].maxDistance = 25f;
            skillDrivers[4].maxDistance = 50f;
            CharacterMaster master = droneMaster.GetComponent<CharacterMaster>();
            droneBody = master.bodyPrefab.InstantiateClone($"{name}Body");
            BodyCatalog.getAdditionalEntries += (list) => list.Add(droneBody);
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
            //body.portraitIcon = Resources.Load<Texture>("@ChensGradiusMod:Assets/Drones/LaserDrone2/Icon/texLaserDrone2Icon.png");
            GameObject customModel = Resources.Load<GameObject>("@Qb:Assets/DroneBody/MainBody.prefab");
            Object.Destroy(droneBody.transform.Find("Model Base").gameObject);
            GameObject modelBase = new GameObject("ModelBase");
            modelBase.transform.parent = droneBody.transform;
            modelBase.transform.localPosition = Vector3.zero;
            modelBase.transform.localRotation = Quaternion.identity;
            modelBase.transform.localScale = Vector3.one;
            Transform modelTransform = customModel.transform;
            modelTransform.parent = modelBase.transform;
            modelTransform.localPosition = Vector3.zero;
            modelTransform.localRotation = Quaternion.identity;
            ModelLocator modelLocator = droneBody.GetComponent<ModelLocator>();
            modelLocator.modelTransform = modelTransform;
            modelLocator.modelBaseTransform = modelBase.transform;
            CharacterModel characterModel = customModel.AddComponent<CharacterModel>();
            characterModel.body = body;
            characterModel.BuildRendererInfos(customModel);
            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();
            CapsuleCollider capsuleCollider = droneBody.GetComponent<CapsuleCollider>();
            capsuleCollider.center = new Vector3(0f, 0f, 0f);
            capsuleCollider.radius = .72f;
            capsuleCollider.height = 1f;
            capsuleCollider.direction = 2;
            HurtBoxGroup hurtBoxGroup = customModel.AddComponent<HurtBoxGroup>();
            HurtBox hurtBox = customModel.GetComponentInChildren<CapsuleCollider>().gameObject.AddComponent<HurtBox>();
            hurtBox.gameObject.layer = LayerIndex.entityPrecise.intVal;
            hurtBox.healthComponent = droneBody.GetComponent<HealthComponent>();
            hurtBox.isBullseye = true;
            hurtBox.damageModifier = HurtBox.DamageModifier.Normal;
            hurtBox.hurtBoxGroup = hurtBoxGroup;
            hurtBox.indexInGroup = 0;
            hurtBoxGroup.hurtBoxes = new HurtBox[] { hurtBox };
            hurtBoxGroup.mainHurtBox = hurtBox;
            hurtBoxGroup.bullseyeCount = 1;
            SkillLocator locator = droneBody.GetComponent<SkillLocator>();
            SkillDef origSkillDef = Resources.Load<SkillDef>("skilldefs/drone1body/Drone1BodyGun");
            LoadoutAPI.AddSkill(typeof(ScatterGrenades));
            SkillDef grenadeSkillDef = Object.Instantiate(origSkillDef);
            grenadeSkillDef.activationState = new SerializableEntityStateType(typeof(ScatterGrenades));
            grenadeSkillDef.baseRechargeInterval = 12;
            grenadeSkillDef.beginSkillCooldownOnSkillEnd = true;
            grenadeSkillDef.baseMaxStock = 1;
            grenadeSkillDef.fullRestockOnAssign = false;
            LoadoutAPI.AddSkillDef(grenadeSkillDef);
            SkillFamily grenadeSkillFamily = Object.Instantiate(locator.primary.skillFamily);
            grenadeSkillFamily.variants = new SkillFamily.Variant[1];
            grenadeSkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = grenadeSkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node("", false, null)
            };
            locator.primary.SetFieldValue("_skillFamily", grenadeSkillFamily);
            LoadoutAPI.AddSkillFamily(grenadeSkillFamily);
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
            GameObject customBrokenModel = Resources.Load<GameObject>("@Qb:Assets/DroneBroken/MainBroken.prefab");
            customBrokenModel.transform.parent = brokenObject.transform;
            Object.Destroy(brokenObject.transform.Find("mdlDrone1").gameObject);
            ModelLocator brokenModelLocator = brokenObject.GetComponent<ModelLocator>();
            brokenModelLocator.modelTransform = customBrokenModel.transform;
            Highlight highlight = brokenObject.GetComponent<Highlight>();
            GameObject customBrokenInnerModel = customBrokenModel.transform.Find("BrokenCube").gameObject;
            highlight.targetRenderer = customBrokenInnerModel.GetComponent<MeshRenderer>();
            customBrokenModel.AddComponent<EntityLocator>().entity = brokenObject;
            customBrokenInnerModel.AddComponent<EntityLocator>().entity = brokenObject;
            iSpawnCard = Object.Instantiate(origIsc);
            iSpawnCard.name = $"iscBroken{name}";
            iSpawnCard.prefab = brokenObject;
            iSpawnCard.slightlyRandomizeOrientation = true;
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
                allowAmbushSpawn = true,
                preventOverhead = false,
                requiredUnlockable = "",
                forbiddenUnlockable = ""
            };
            iDirectorCardHolder = new DirectorCardHolder
            {
                Card = directorCard,
                MonsterCategory = MonsterCategory.None,
                InteractableCategory = InteractableCategory.Drones,
            };
            spiderMine = Resources.Load<GameObject>("prefabs/projectiles/SpiderMine").InstantiateClone("QbSpiderMine");
            Object.Destroy(spiderMine.GetComponent<ProjectileDeployToOwner>());
            ProjectileCatalog.getAdditionalEntries += list => list.Add(spiderMine);
        }

        protected override void SetupBehavior()
        {
            base.SetupBehavior();
            GradiusOption.instance.SupportMinionType(name);
            InteractableActions += DirectorAPI_InteractableActions;
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

        private void DirectorAPI_InteractableActions(List<DirectorCardHolder> arg1, StageInfo arg2)
        {
            arg1.ConditionalAdd(iDirectorCardHolder, card => iDirectorCardHolder == card);
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