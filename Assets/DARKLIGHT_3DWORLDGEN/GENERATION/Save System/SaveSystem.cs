using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.ThirdDimensional.World.Data
{
    public class WorldSaveSystem : MonoBehaviour
    {
        private IDataService DataService = new JsonDataService(); // Assuming JsonDataService implements IDataService
        private bool EncryptionEnabled = false;
        private long SaveTime;
        private long LoadTime;

        public void ToggleEncryption(bool EncryptionEnabled)
        {
            this.EncryptionEnabled = EncryptionEnabled;
        }

        [EasyButtons.Button]
        public void SaveWorldSettings()
        {
            WorldData saveData = new WorldData();

            long startTime = DateTime.Now.Ticks;
            if (DataService.SaveData("/world-data.json", saveData, EncryptionEnabled))
            {
                SaveTime = DateTime.Now.Ticks - startTime;
                Debug.Log($"Save Time: {(SaveTime / 10000):N4}ms");
            }
            else
            {
                Debug.LogError("Could not save file!");
            }
        }

        [EasyButtons.Button]
        public void LoadWorldSettings()
        {
            long startTime = DateTime.Now.Ticks;
            try
            {
                // Assuming DataService is an instance of JsonDataService
                WorldData worldSaveData = DataService.LoadData<WorldData>("/world-data.json", EncryptionEnabled);
                if (worldSaveData == null)
                {
                    Debug.LogError("Failed to load world data or world data is null.");
                    // Handle the situation, such as by initializing worldSaveData with default values.
                }
                LoadTime = DateTime.Now.Ticks - startTime;
                Debug.Log($"Load Time: {(LoadTime / 10000):N4}ms");
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not read file!\n{e}");
            }
        }
    }
}