using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModJamWarmup
{
    public class SimVersionController : MonoBehaviour
    {
        [SerializeField]
        public bool visibleAtStart; // boolean to detect whether it should be visible
        private List<int> defaultLayers = new List<int>();
        private List<GameObject> allObjects = new List<GameObject>(); // to get all objects with a sim version controller

        public void Start()
        {
            // get all child objects
            Transform[] childTransforms = GetComponentsInChildren<Transform>(); // get all transforms of this gameobject's children
            for (int i = 0; i < childTransforms.Length; i++)
            {
                if (i == 0)
                {
                    allObjects.Add(this.gameObject); // if the index is at 0, sets first value in array to this gameobject
                    defaultLayers.Add(this.gameObject.layer);
                } else
                {
                    allObjects.Add(childTransforms[i].gameObject); // gets the gameobjects of all child transforms
                    defaultLayers.Add(allObjects[i].layer);
                }

                if (!visibleAtStart)
                {
                    allObjects[i].layer = 12; // if we want this to be invisible by default, sets layer of object to the unused layer (which is used to hide things from player sight when not needed)
                }
            }
        }

        public void ToggleObject()
        {
            // toggles all objects between layers
            for (int i = 0; i < allObjects.Count; i++)
            {
                if (allObjects[i].layer == 12)
                {
                    allObjects[i].layer = defaultLayers[i]; // if layer of the object is 12, give everything back its child layers
                } else {
                    allObjects[i].layer = 12; // if layer of the object is not 12, set them to 12.
                }
            }
        }
    }
}
