using UnityEngine;
using OWML.Common;
using System.Collections;
using NewHorizons.Utility;
using NewHorizons;
using OWML.ModHelper;
using System.Collections.Generic;
using NewHorizons.Utility.Files;

namespace ModJamWarmup
{
    public class TimeVision : MonoBehaviour // this script has parts of the base game script DreamExplosionController.cs within it
    {
        // variables
        [SerializeField]
        private OWAudioSource audio; // to store the custom fluid detector
        [SerializeField]
        private GameObject effects;
        private List<SimVersionController> simControllers = new List<SimVersionController>(); // to get all objects with a sim version controller

        private ScreenPrompt thirdArtifactPrompt = new ScreenPrompt(InputLibrary.toolOptionY, "Toggle Sim Version");
        private bool isInDream = false; // boolean to check if the leap has been triggered
        private bool toggleOn = false;
        private bool isCurrentlyToggling = false;
        private bool effectsSetUp;
        private MeshRenderer flameRenderer;
        private Material flameMat;
        private Light pointLight;
        private Color lightDefaultColor;
        private Color defaultColor;
        private readonly Color newColor = new Color(0.05043631f, 0.1683768f, 3.211117f);
        private GameObject[] baseGameObjects = new GameObject[5]; 

        private void Awake()
        {
            GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld); // checks if player leaves the sim
            GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld); // checks if player leaves the sim
        }

        private void Start()
        {
            // set base game objects to 
            baseGameObjects[0] = SearchUtilities.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_2/Simulation_DreamZone_2/Structure_DreamZone_2/LowerLevel/BurntHouse");
            baseGameObjects[1] = SearchUtilities.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_2/Structure_DreamZone_2/LowerLevel/BurntBuilding");
            baseGameObjects[2] = SearchUtilities.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_2/Props_DreamZone_2/Props_PrisonerHouse");
            baseGameObjects[3] = SearchUtilities.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_2/Props_DreamZone_2/Prefab_IP_PictureFrame_PrisonerHouse");
            baseGameObjects[4] = SearchUtilities.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_2/Simulation_DreamZone_2/Props_DreamZone_2/Props_PrisonerHouse");

            for (int i = 0; i < baseGameObjects.Length; i++)
            {
                baseGameObjects[i].AddComponent<SimVersionController>();
                baseGameObjects[i].GetComponent<SimVersionController>().visibleAtStart = true;
                //simControllers.Add(baseGameObjects[i].GetComponent<SimVersionController>());
            }

            SimVersionController[] controllers = FindObjectsOfType<SimVersionController>(); // get all components with sim version controller in the scene

            // give basegame houses that change a value


            // Add each instance to the list
            foreach (SimVersionController controller in controllers)
            {
                simControllers.Add(controller);
            }

            // setup effects
            effectsSetUp = false;
            effects.SetActive(false);
            Locator.GetPlayerCamera().cullingMask = 239068671; // set player culling mask so they can't see layer 12 things.
        }

        private void Update()
        {
            if (isInDream)
            {
                if (OWInput.IsPressed(InputLibrary.toolOptionY, InputMode.Character) && !isCurrentlyToggling)
                {
                    
                    if (!toggleOn)
                    {
                        toggleOn = true;
                        effects.SetActive(true);
                        AudioUtilities.SetAudioClip(audio, "assets/Audio/Loading Tunnel - Load.wav", ModJamWarmup.Instance);
                        audio.Play();
                        StartCoroutine(LanternEffects(newColor, newColor, 0.7f, 12));
                    } else
                    {
                        toggleOn = false;
                        effects.SetActive(false);
                        AudioUtilities.SetAudioClip(audio, "assets/Audio/Loading Tunnel - Unload.wav", ModJamWarmup.Instance);
                        audio.Play();
                        StartCoroutine(LanternEffects(defaultColor, lightDefaultColor, 3, 0.25f));
                    }
                    effects.SetActive(toggleOn);
                    foreach (SimVersionController controller in simControllers)
                    {
                        controller.ToggleObject();
                    }
                }
            }
        }

        private IEnumerator LanternEffects(Color targetColor, Color lightColorTarget, float lightTargetIntensity, float lightTargetRange)
        {
            isCurrentlyToggling = true;
            float elapsedTime = 0f; // create variable for elapsed time

            while (elapsedTime < 1)
            {
                // time setup
                elapsedTime += Time.deltaTime; // keep track of time
                float t = Mathf.Clamp01(elapsedTime / 1);

                // lerps
                Color currentFlameColor = Color.Lerp(flameMat.color, targetColor, t); // change flame color overtime
                Color currentLightColor = Color.Lerp(pointLight.color, lightColorTarget, t); // change light color overtime
                float currentIntensity = Mathf.Lerp(pointLight.intensity, lightTargetIntensity, t); // change intensity overtime
                float currentRange = Mathf.Lerp(pointLight.range, lightTargetRange, t); // change range overtime

                // continuous setting
                flameMat.SetColor("_EmissionColor", currentFlameColor); // continuously set flame color
                pointLight.color = currentLightColor; // continuously set light color
                pointLight.intensity = currentIntensity; // continuously set intensity
                pointLight.range = currentRange; // continuously set range

                yield return null; // Wait for the next frame
            }

            // finalize values
            flameMat.SetColor("_EmissionColor", targetColor); // when finished, set final flame color
            pointLight.color = lightColorTarget; // when finished, set final light color
            pointLight.intensity = lightTargetIntensity; // when finished, set final intensity
            pointLight.range = lightTargetRange; // when finished, set final range
            yield return new WaitForSeconds(0.4f); // wait a bit until the player can do this again
            isCurrentlyToggling = false;
        }

        private void OnExitDreamWorld()
        {
            isInDream = false;
            Locator.GetPromptManager().RemoveScreenPrompt(thirdArtifactPrompt);
        }

        private void OnEnterDreamWorld()
        {
            isInDream = true;
            Locator.GetPromptManager().AddScreenPrompt(thirdArtifactPrompt, PromptPosition.UpperRight);

            if (!effectsSetUp)
            {
                // effects setup
                flameRenderer = GetDreamLantern().gameObject.transform.Find("Props_IP_Artifact/Flame").gameObject.GetComponent<MeshRenderer>(); // get the mesh renderer of the artifact flame
                Material[] mats = flameRenderer.materials;
                flameMat = mats[0];
                defaultColor = flameMat.color; // set starting color as default color

                pointLight = GetDreamLantern().gameObject.transform.Find("PointLight_Lantern").gameObject.GetComponent<Light>(); // get the point light component
                lightDefaultColor = pointLight.color; // set starting color as default color
                effectsSetUp = true;
            }
        }

        public DreamLanternItem GetDreamLantern()
        {
            if (Locator.GetToolModeSwapper()?.GetItemCarryTool()?.GetHeldItem() is DreamLanternItem lantern)
            {
                return lantern;
            }
            else
            {
                return null;
            }
        }

        public bool IsLanternActive()
        {
            if (Locator.GetToolModeSwapper()?.GetItemCarryTool()?.GetHeldItem() is DreamLanternItem lantern
                && lantern.GetLanternController().IsLit()) // checks if player has prototype
            {
                return true; // returns true if player has prototype
            }
            else
            {
                return false; // returns false if player has prototype
            }
        }
    }
}