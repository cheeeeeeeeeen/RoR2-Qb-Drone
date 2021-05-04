#undef DEBUG

using BepInEx;
using BepInEx.Configuration;
using Chen.GradiusMod;
using Chen.GradiusMod.Drones;
using Chen.Helpers.GeneralHelpers;
using Chen.Helpers.LogHelpers;
using R2API.Utils;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Chen.Helpers.GeneralHelpers.AssetsManager;
using Path = System.IO.Path;

[assembly: InternalsVisibleTo("Qb.Tests")]

namespace Chen.Qb
{
    /// <summary>
    /// The Unity Plugin so that the mod is loaded in BepInEx.
    /// </summary>
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency()]
    public class ModPlugin : BaseUnityPlugin
    {
        /// <summary>
        /// This mod's version.
        /// </summary>
        public const string ModVer =
#if DEBUG
            "0." +
#endif
            "2.0.0";

        /// <summary>
        /// This mod's name.
        /// </summary>
        public const string ModName = "Qb";

        /// <summary>
        /// This mod's GUID.
        /// </summary>
        public const string ModGuid = "com.Chen.Qb";

        internal static ConfigFile cfgFile;
        internal static Log Log;
        internal static List<DroneInfo> dronesList = new List<DroneInfo>();
        internal static AssetBundle assetBundle;
        internal static ContentProvider contentProvider;

        private void Awake()
        {
            Log = new Log(Logger);
            contentProvider = new ContentProvider();

#if DEBUG
            Chen.Helpers.GeneralHelpers.MultiplayerTest.Enable(Log);
#endif
            Log.Debug("Initializing config file...");
            cfgFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);

            Log.Debug("Loading asset bundle...");
            BundleInfo bundleInfo = new BundleInfo("@Qb", "Chen.Qb.assetbundle", BundleType.UnityAssetBundle);
            assetBundle = new AssetsManager(bundleInfo).Register() as AssetBundle;

            Log.Debug("Registering Qb Drone...");
            dronesList = DroneCatalog.Initialize(ModGuid, cfgFile);
            DroneCatalog.ScopedSetupAll(dronesList);

            contentProvider.Initialize();
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