using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;
using Unity.Services.Economy.Model;

namespace TestRoomCloud
{
    public class PlayerEconomyService
    {
        public const string k_CoinCurrencyKey = "COIN";

        private readonly ILogger<PlayerEconomyService> m_logger;

        public PlayerEconomyService(ILogger<PlayerEconomyService> logger)
        {
            m_logger = logger;
        }

        [CloudCodeFunction("GetPlayerCoins")]
        public async Task<int> GetPlayerCoins(IExecutionContext context, IGameApiClient gameApiClient)
        {
            return await GetPlayerCurrency(context, gameApiClient, k_CoinCurrencyKey);
        }

        private async Task<int> GetPlayerCurrency(IExecutionContext context, IGameApiClient gameApiClient, string key)
        {
            try
            {
                var playerCurrenciesData = await gameApiClient.EconomyCurrencies.GetPlayerCurrenciesAsync(
                    context, 
                    context.AccessToken, 
                    context.ProjectId, 
                    context.PlayerId);

                CurrencyBalanceResponse? targetCurrency = playerCurrenciesData.Data.Results.FirstOrDefault(c => c.CurrencyId == key);

                if(targetCurrency != null)
                {
                    return (int)targetCurrency.Balance;
                }
                else
                {
                    throw new Exception($"Currency with key {key} not found for playerId {context.PlayerId}");
                }
            }
            catch (ApiException ex)
            {
                throw new Exception($"Failed to get player currency for playerId {context.PlayerId}. Error: {ex.Message}");
            }
        }
    }
}