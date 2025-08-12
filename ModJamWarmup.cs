using HarmonyLib;
using NewHorizons.Utility;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;
using UnityEngine;

namespace ModJamWarmup
{
    public class ModJamWarmup : ModBehaviour
    {
        public static INewHorizons NewHorizonsAPI { get; private set; }

        public static ModJamWarmup Instance
        {
            get
            {
                if (instance == null) instance = FindObjectOfType<ModJamWarmup>();
                return instance;
            }
        }

        private static ModJamWarmup instance;

        private bool hasGivenItem = false;

        public static void WriteLine(string text, MessageType messageType = MessageType.Message)
        {
            Instance.ModHelper.Console.WriteLine(text, messageType);
        }

        public void Awake()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        public void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(ModJamWarmup)} is loaded!", MessageType.Success);

            // Get the New Horizons API and load configs
            NewHorizonsAPI = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizonsAPI.LoadConfigs(this);

            new Harmony("Anonymous.ModJamWarmup").PatchAll(Assembly.GetExecutingAssembly());

            // Example of accessing game code.
            OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen); // We start on title screen
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
        }

        public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
        {
            if (newScene != OWScene.SolarSystem) return;
            ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
            hasGivenItem = false;
        }

        // debug stuff
        private void Update()
        {
            if (OWInput.IsPressed(InputLibrary.toolActionPrimary, InputMode.Character) && OWInput.IsPressed(InputLibrary.toolActionSecondary, InputMode.Character) && !hasGivenItem)
            {
                hasGivenItem = true;

                // debug give item
                OWItem item = SearchUtilities.Find("RingWorld_Body/Sector_RingWorld/Sector_SecretEntrance/Interactables_SecretEntrance/ArtifactCrate/Prefab_IP_DreamLanternItem_Malfunctioning").GetComponent<OWItem>(); // get atp's advanced warp core
                Locator.GetToolModeSwapper().GetItemCarryTool().PickUpItemInstantly(item); // gives player the warp core

                // debug spawn
                SpawnPoint dreamFireSpawn = SearchUtilities.Find("RingWorld_Body/Spawns_IP/Spawn_IP_Zone_3_DreamFire").GetComponent<SpawnPoint>(); // gets vessel spawn point
                PlayerSpawner _spawner = GameObject.FindGameObjectWithTag("Player").GetRequiredComponent<PlayerSpawner>(); // gets player spawner
                _spawner.DebugWarp(dreamFireSpawn); // warps you to vessel
            }
        }
    }

}
