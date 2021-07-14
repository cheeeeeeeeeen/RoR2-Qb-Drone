#undef DEBUG

using Chen.GradiusMod;
using Chen.GradiusMod.Drones;
using Chen.GradiusMod.Items.GradiusOption;
using Chen.Helpers.CollectionHelpers;
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
using System.Collections.Generic;
using UnityEngine;
using static Chen.Qb.ModPlugin;
using static R2API.DirectorAPI;
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

        private static readonly Lazy<SkillDef> _skillBasis = new Lazy<SkillDef>(() => Resources.Load<SkillDef>("skilldefs/drone1body/Drone1BodyGun"));
        private static readonly Lazy<InteractableSpawnCard> _iscBasis = new Lazy<InteractableSpawnCard>(() => Resources.Load<InteractableSpawnCard>("spawncards/interactablespawncard/iscBrokenDrone1"));
        private static readonly Lazy<GameObject> _originalSpiderMine = new Lazy<GameObject>(() => Resources.Load<GameObject>("prefabs/projectiles/SpiderMine"));

        private static InteractableSpawnCard interactableSpawnCardBasis { get => _iscBasis.Value; }

        private static SkillDef skillBasis { get => _skillBasis.Value; }

        private static GameObject originalSpiderMine { get => _originalSpiderMine.Value; }

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
            brokenObject = interactableSpawnCardBasis.prefab.InstantiateClone($"{name}Broken", true);
            SummonMasterBehavior summonMasterBehavior = brokenObject.GetComponent<SummonMasterBehavior>();
            droneMaster = summonMasterBehavior.masterPrefab.InstantiateClone($"{name}Master", true);
            contentProvider.masterObjects.Add(droneMaster);
            AISkillDriver[] skillDrivers = droneMaster.GetComponents<AISkillDriver>();
            skillDrivers[3].maxDistance = 25f;
            skillDrivers[4].maxDistance = 50f;
            skillDrivers.SetAllDriversToAimTowardsEnemies();
            CharacterMaster master = droneMaster.GetComponent<CharacterMaster>();
            droneBody = master.bodyPrefab.InstantiateClone($"{name}Body", true);
            contentProvider.bodyObjects.Add(droneBody);
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
            LoadoutAPI.AddSkill(typeof(ScatterGrenades));
            SkillDef grenadeSkillDef = UnityObject.Instantiate(skillBasis);
            grenadeSkillDef.activationState = new SerializableEntityStateType(typeof(ScatterGrenades));
            grenadeSkillDef.baseRechargeInterval = 12;
            grenadeSkillDef.beginSkillCooldownOnSkillEnd = true;
            grenadeSkillDef.baseMaxStock = 1;
            grenadeSkillDef.fullRestockOnAssign = false;
            LoadoutAPI.AddSkillDef(grenadeSkillDef);
            SkillFamily grenadeSkillFamily = UnityObject.Instantiate(locator.primary.skillFamily);
            grenadeSkillFamily.variants = new SkillFamily.Variant[1];
            grenadeSkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = grenadeSkillDef,
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
            GameObject customBrokenModel = assetBundle.LoadAsset<GameObject>("Assets/DroneBroken/MainBroken.prefab");
            brokenObject.ReplaceModel(customBrokenModel, DebugCheck());
            Highlight highlight = brokenObject.GetComponent<Highlight>();
            GameObject customBrokenInnerModel = customBrokenModel.transform.Find("BrokenCube").gameObject;
            highlight.targetRenderer = customBrokenInnerModel.GetComponent<MeshRenderer>();
            customBrokenModel.AddComponent<EntityLocator>().entity = brokenObject;
            customBrokenInnerModel.AddComponent<EntityLocator>().entity = brokenObject;
            GameObject brokenEffects = brokenObject.transform.Find("ModelBase").Find("BrokenDroneVFX").gameObject;
            brokenEffects.transform.parent = customBrokenModel.transform;
            GameObject sparks = brokenEffects.transform.Find("Small Sparks, Mesh").gameObject;
            ParticleSystem.ShapeModule sparksShape = sparks.GetComponent<ParticleSystem>().shape;
            sparksShape.shapeType = ParticleSystemShapeType.MeshRenderer;
            sparksShape.meshShapeType = ParticleSystemMeshShapeType.Edge;
            sparksShape.meshRenderer = (MeshRenderer)highlight.targetRenderer;
            GameObject damagePoint = brokenEffects.transform.Find("Damage Point").gameObject;
            damagePoint.transform.localPosition = Vector3.zero;
            damagePoint.transform.localRotation = Quaternion.identity;
            damagePoint.transform.localScale = Vector3.one;
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
                allowAmbushSpawn = true,
                preventOverhead = false
            };
            iDirectorCardHolder = new DirectorCardHolder
            {
                Card = directorCard,
                MonsterCategory = MonsterCategory.None,
                InteractableCategory = InteractableCategory.Drones,
            };
            spiderMine = originalSpiderMine.InstantiateClone("QbSpiderMine", true);
            UnityObject.Destroy(spiderMine.GetComponent<ProjectileDeployToOwner>());
            spiderMine.AddComponent<Disappear>();
            ProjectileAPI.Add(spiderMine);
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