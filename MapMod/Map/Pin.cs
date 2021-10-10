﻿using System;
using VanillaMapMod.Data;
using VanillaMapMod.Settings;
using UnityEngine;

namespace VanillaMapMod.Map
{
    internal class Pin : MonoBehaviour
    {
        //private Vector3 _origScale;
        public PinDef PinData { get; private set; } = null;

        //private SpriteRenderer SR => gameObject.GetComponent<SpriteRenderer>();

        public void SetPinData(PinDef pd)
        {
            PinData = pd;
        }

        //public void SetPinSprite()
        //{
        //    SR.sprite = GetSprite(PinData.pool);
        //}

        public void UpdatePin(string mapAreaName)
        {
            try
            {
                if (PinData == null)
                {
                    throw new Exception("Cannot enable pin with null pindata. Ensure game object is disabled before adding as component, then call SetPinData(<pd>) before enabling.");
                }

                ShowBasedOnMap(mapAreaName);
                HideIfNotBought();
                HideIfFound();
            }
            catch (Exception e)
            {
                VanillaMapMod.Instance.LogError(message: $"Failed to update pin! ID: {PinData.name}\n{e}");
            }
        }

        // This method hides or shows the pin depending on the state of the map
        private void ShowBasedOnMap(string mapAreaName)
        {
            if (mapAreaName == PinData.mapArea || mapAreaName == "WorldMap")
            {
                if (VanillaMapMod.LS.GotFullMap)
                {
                    gameObject.SetActive(true);
                    return;
                }

                //Show these pins if the corresponding map item has been picked up
                if (PinData.pool == "Skill"
                    || PinData.pool == "Charm"
                    || PinData.pool == "Key"
                    || PinData.pool == "Notch"
                    || PinData.pool == "Mask"
                    || PinData.pool == "Vessel"
                    || PinData.pool == "Ore"
                    || PinData.pool == "EssenceBoss")
                {
                    if (SettingsUtil.GetPlayerDataMapSetting(PinData.mapArea))
                    {
                        gameObject.SetActive(true);
                        return;
                    }
                }

                // For the rest, show pin if the corresponding area has been mapped
                if (PinData.pinScene != null)
                {
                    if (PlayerData.instance.scenesMapped.Contains(PinData.pinScene))
                    {
                        gameObject.SetActive(true);
                        return;
                    }
                }
                else
                {
                    if (PlayerData.instance.scenesMapped.Contains(PinData.sceneName))
                    {
                        gameObject.SetActive(true);
                        return;
                    }
                }
            }

            gameObject.SetActive(false);
        }

        private void HideIfNotBought()
        {
            //if (!SettingsUtil.GetMapModSettingFromPool(PinData.pool))
            //{
            //    gameObject.SetActive(false);
            //}
            if (!VanillaMapMod.LS.GetHasFromGroup(PinData.pool))
            {
                gameObject.SetActive(false);
            }
        }

        private void HideIfFound()
        {
            if (PinData.objectName == null)
            {
                return;
            }

            // Don't hide pin if something isn't in the obtained items dictionary
            foreach (string oName in PinData.objectName)
            {
                if (!VanillaMapMod.LS.ObtainedItems.ContainsKey(oName + PinData.sceneName))
                {
                    return;
                }
            }

            gameObject.SetActive(false);
        }
    }
}