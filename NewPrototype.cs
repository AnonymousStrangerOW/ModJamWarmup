using UnityEngine;
using OWML.Common;
using System.Collections;
using NewHorizons.Utility;
using NewHorizons;
using OWML.ModHelper;

namespace ModJamWarmup
{
    public class NewPrototype : MonoBehaviour // this script has parts of the base game script DreamExplosionController.cs within it
    {
        // variables
        [SerializeField]
        private LanternFluidDetector fluidDetector; // to store the custom fluid detector
        [SerializeField]
        private GameObject explosionObject; // to store the entire object
        [SerializeField]
        private GameObject geyser; // to store the geyser that makes the player go up

        private bool hasJumped = false; // boolean to check if the leap has been triggered

        private void Awake()
        {
            GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld); // checks if player leaves the sim
        }

        private void Start()
        {
            explosionObject.SetActive(false);
            geyser.SetActive(false);
        }

        private void Update()
        {
            if (PlayerHasPrototype())
            {
                foreach (FluidVolume vol in fluidDetector._activeVolumes)
                {
                    if (vol._fluidType == FluidVolume.Type.WATER)
                    {
                        if (!hasJumped)
                        {
                            StartCoroutine(PlayerLeap()); // initiates the leap coroutine
                            break;
                        }
                    }
                }
            }
        }

        private void OnExitDreamWorld()
        {
            explosionObject.GetComponent<DreamExplosionOnAwakeController>().Awake();
            explosionObject.SetActive(false);
            geyser.SetActive(false);
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

        public IEnumerator PlayerLeap()
        {
            hasJumped = true; // sets the has jumped boolean checker to true
            if (explosionObject.activeSelf)
            {
                explosionObject.SetActive(false);
            }
            explosionObject.SetActive(true);
            if (geyser.activeSelf)
            {
                geyser.SetActive(false);
            }
            geyser.SetActive(true);
            explosionObject.GetComponent<DreamExplosionOnAwakeController>().Awake();

            // custom code again
            yield return new WaitForSeconds(0.15f); // wait a second before getting rid of the geyser
            geyser.SetActive(false);
            yield return new WaitForSeconds(1); // wait a second before allowing another jump to occur
            hasJumped = false; // sets the has jumped boolean checker to false a second after being flung in the air

            yield return new WaitForSeconds(3); // wait 8 seconds before setting it disabled
            explosionObject.SetActive(false);
        }

        public bool PlayerHasPrototype()
        {
            if (Locator.GetToolModeSwapper()?.GetItemCarryTool()?.GetHeldItem() is DreamLanternItem lantern
                && lantern.GetLanternType() == DreamLanternType.Malfunctioning
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