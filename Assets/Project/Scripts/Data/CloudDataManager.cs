using System;
using System.Linq;
using Unity.Services.Authentication;
using UnityEngine;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.Core;
using System.Threading.Tasks;

namespace TestBotRoom
{
    public class CloudDataManager : MonoBehaviour
    {
        private PlayerDataServiceBindings m_Bindings;
        private PlayerEconomyServiceBindings m_EconomyServiceBindings;

        private void Start()
        {
            m_Bindings = new PlayerDataServiceBindings(CloudCodeService.Instance);
            m_EconomyServiceBindings = new PlayerEconomyServiceBindings(CloudCodeService.Instance);
        }

        public async Task<string> GetPlayerDataFromCloud()
        {
            try 
            {
                return await m_Bindings.GetFullPlayerData();
            }
            catch { return null; }
        }

        public async Task SavePlayerDataToCloud(SaveData data)
        {
            try 
            {
                string json = JsonUtility.ToJson(data);
                await m_Bindings.SaveFullPlayerData(json);
                Debug.Log("Progresso sincronizado com a nuvem.");
            }
            catch (Exception ex) 
            {
                Debug.LogError($"Erro ao sincronizar: {ex.Message}");
            }
        }
    }
}