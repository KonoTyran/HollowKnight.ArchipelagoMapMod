﻿using GlobalEnums;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using MapModS.Data;
using MapModS.Settings;
using MapModS.Trackers;
using System;

namespace MapModS.Map
{
    public static class WorldMap
    {
        public static GameObject goCustomPins = null;
        public static PinsCustom CustomPins => goCustomPins?.GetComponent<PinsCustom>();

        public static void Hook()
        {
            On.GameManager.SetGameMap += GameManager_SetGameMap;
            On.GameMap.WorldMap += GameMap_WorldMap;
            On.GameMap.SetupMapMarkers += GameMap_SetupMapMarkers;
            On.GameMap.DisableMarkers += GameMap_DisableMarkers;
            On.GameManager.UpdateGameMap += GameManager_UpdateGameMap;
        }

        // The function that is called every time after a new GameMap is created (once per save load)
        private static void GameManager_SetGameMap(On.GameManager.orig_SetGameMap orig, GameManager self, GameObject go_gameMap)
        {
            orig(self, go_gameMap);

            GameMap gameMap = go_gameMap.GetComponent<GameMap>();

            // Necessary if player goes straight to Pause Menu
            SyncMap(gameMap);

            DataLoader.FindPoolGroups();

            if (goCustomPins != null)
            {
                goCustomPins.GetComponent<PinsCustom>().DestroyPins();
                UnityEngine.Object.Destroy(goCustomPins);
            }

            MapModS.Instance.Log("Adding Custom Pins...");

            goCustomPins = new GameObject($"MMS Custom Pin Group");
            goCustomPins.AddComponent<PinsCustom>();

            // Setting parent here is only for controlling local position,
            // not active/not active (need separate mechanism)
            goCustomPins.transform.SetParent(go_gameMap.transform);

            CustomPins.MakePins(gameMap);

            CustomPins.FindRandomizedGroups();

            MapModS.Instance.Log("Adding Custom Pins done.");
        }

        // Called every time we open the World Map
        private static void GameMap_WorldMap(On.GameMap.orig_WorldMap orig, GameMap self)
        {
            orig(self);

            if (!MapModS.LS.ModEnabled) return;

            // Easiest way to force AdditionalMaps custom areas to show
            if (MapModS.LS.mapState == MapState.FullMap)
            {
                foreach (Transform child in self.transform)
                {
                    if (child.name == "WHITE_PALACE"
                        || child.name == "GODS_GLORY")
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            }

            UpdateMap(self, MapZone.NONE);

            //foreach (Transform areaObj in self.transform)
            //{
            //    MapModS.Instance.Log(areaObj.name);

            //    foreach (Transform roomObj in areaObj.transform)
            //    {
            //        MapModS.Instance.Log($"- {roomObj.name}");
            //    }
            //}
        }

        // Following two behaviours necessary since GameMap is actually persistently active
        private static void GameMap_SetupMapMarkers(On.GameMap.orig_SetupMapMarkers orig, GameMap self)
        {
            orig(self);

            if (!MapModS.LS.ModEnabled) return;

            if (goCustomPins == null) return;

            CustomPins.gameObject.SetActive(true);
        }

        private static void GameMap_DisableMarkers(On.GameMap.orig_DisableMarkers orig, GameMap self)
        {
            if (goCustomPins != null)
            {
                CustomPins.gameObject.SetActive(false);
            }

            orig(self);
        }

        // Remove the "Map Updated" idle animation, since it occurs when the return value is true
        public static bool GameManager_UpdateGameMap(On.GameManager.orig_UpdateGameMap orig, GameManager self)
        {
            orig(self);

            return false;
        }

        // The main method for updating map objects and pins when opening either World Map or Quick Map
        public static void UpdateMap(GameMap gameMap, MapZone mapZone)
        {
            try
            {
                ItemTracker.UpdateObtainedItems();

                PinsVanilla.UpdatePins(gameMap.gameObject);

                SyncMap(gameMap);

                PinsVanilla.RefreshGroups();
                //PinsVanilla.ResizePins();

                if (goCustomPins == null) return;

                CustomPins.ResizePins();
                CustomPins.UpdatePins(mapZone);
                CustomPins.RefreshGroups();

            }
            catch (Exception e)
            {
                MapModS.Instance.LogError(e);
            }
            
        }

        public static void SyncMap(GameMap gameMap)
        {
            // If the mod is installed for an existing game
            //SettingsUtil.SyncPlayerDataSettings();

            // Refresh map
            gameMap.SetupMap();
        }

        
    }
}