using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TestBotRoom
{
    public class DataManager : MonoBehaviour
    {
        public static string SavePath => Path.Combine(Application.persistentDataPath, "saveData.json");

        public void Save(SaveData data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }

        public SaveData Load()
        {
            if (!File.Exists(SavePath))
            {
                return null;
            }

            string json = File.ReadAllText(SavePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            return data;
        }

        public void DeleteSaveData()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
        }
    }

    [System.Serializable]
    public class SaveData
    {
        public string playerName;
        public int bestScore;
        public int coins;
        public int gears;
        public List<string> unlockedCharacters;
        public List<string> unlockedAchivements;
    }
}