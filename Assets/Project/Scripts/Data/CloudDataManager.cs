using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using System.Threading.Tasks;
using Unity.Services.Core;

namespace TestBotRoom
{
    public class CloudDataManager : MonoBehaviour
    {
        private PlayerDataServiceBindings m_Bindings;
        private PlayerEconomyServiceBindings m_EconomyServiceBindings;

        private void EnsureBindings()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                throw new InvalidOperationException("Unity Services must be initialized before CloudDataManager bindings are created.");
            }

            if (m_Bindings != null && m_EconomyServiceBindings != null)
            {
                return;
            }

            m_Bindings = new PlayerDataServiceBindings(CloudCodeService.Instance);
            m_EconomyServiceBindings = new PlayerEconomyServiceBindings(CloudCodeService.Instance);
        }

        public async Task<string> GetPlayerDataFromCloud()
        {
            try
            {
                EnsureBindings();
                return await m_Bindings.GetFullPlayerData();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Cloud load failed: {ex.Message}");
                return null;
            }
        }

        public async Task SavePlayerDataToCloud(SaveData data)
        {
            try
            {
                EnsureBindings();
                string json = JsonUtility.ToJson(data);
                await m_Bindings.SaveFullPlayerData(json);
                Debug.Log("Progresso sincronizado com a nuvem.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Erro ao sincronizar: {ex.Message}");
            }
        }

        public async Task<int?> GetPlayerCoinsFromEconomy()
        {
            try
            {
                EnsureBindings();
                return await m_EconomyServiceBindings.GetPlayerCoins();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Economy coin load failed: {ex.Message}");
                return null;
            }
        }

        public async Task<RewardRunResult> RewardRun(RewardRunRequest request)
        {
            try
            {
                EnsureBindings();
                return await CloudCodeService.Instance.CallModuleEndpointAsync<RewardRunResult>(
                    "TestRoomCloud",
                    "RewardRun",
                    new Dictionary<string, object>
                    {
                        { "runScore", request.runScore },
                        { "usedContinue", request.usedContinue },
                        { "runId", request.runId },
                        { "clientSaveVersion", request.clientSaveVersion }
                    });
            }
            catch (Exception ex)
            {
                Debug.LogError($"RewardRun failed: {ex.Message}");
                throw;
            }
        }

        public async Task<string> HandleNewPlayerNameEntry(string newName)
        {
            try
            {
                EnsureBindings();
                return await m_Bindings.HandleNewPlayerNameEntry(newName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player name update failed: {ex.Message}");
                throw;
            }
        }
    }
}
