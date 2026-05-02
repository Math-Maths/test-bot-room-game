using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;
using Unity.Services.Economy.Model;

namespace TestRoomCloud;

public class PlayerEconomyService
{
    public const string k_CoinCurrencyKey = "COIN";
    public const string k_ProcessedRunIdsKey = "PROCESSED_RUN_IDS";
    private const int k_MaxTrackedRunIds = 50;

    private readonly ILogger<PlayerEconomyService> m_logger;
    private readonly PlayerDataService m_PlayerDataService;

    public PlayerEconomyService(ILogger<PlayerEconomyService> logger, PlayerDataService playerDataService)
    {
        m_logger = logger;
        m_PlayerDataService = playerDataService;
    }

    [CloudCodeFunction("GetPlayerCoins")]
    public async Task<int> GetPlayerCoins(IExecutionContext context, IGameApiClient gameApiClient)
    {
        return await GetPlayerCurrency(context, gameApiClient, k_CoinCurrencyKey);
    }

    [CloudCodeFunction("RewardRun")]
    public async Task<RewardRunResult> RewardRun(
        IExecutionContext context,
        IGameApiClient gameApiClient,
        int runScore,
        bool usedContinue,
        string runId,
        int clientSaveVersion)
    {
        if (runScore < 0)
        {
            throw new ArgumentException("runScore must be zero or greater.");
        }

        if (string.IsNullOrWhiteSpace(runId))
        {
            throw new ArgumentException("runId is required.");
        }

        if (m_PlayerDataService == null)
        {
            throw new InvalidOperationException("PlayerDataService dependency was not resolved.");
        }

        PlayerSaveDataDocument saveData = await m_PlayerDataService.GetPlayerSaveDataDocument(context, gameApiClient);
        List<string> processedRunIds = await GetProcessedRunIds(context, gameApiClient);

        if (processedRunIds.Contains(runId, StringComparer.Ordinal))
        {
            return new RewardRunResult
            {
                CoinsGranted = 0,
                CurrentCoinBalance = await GetPlayerCurrency(context, gameApiClient, k_CoinCurrencyKey),
                BestScore = saveData.bestScore,
                IsNewBestScore = false,
                RunAlreadyProcessed = true
            };
        }

        int previousBestScore = saveData.bestScore;
        bool isNewBestScore = runScore > previousBestScore;
        int coinsGranted = CalculateCoinsGranted(runScore, usedContinue);

        int currentCoinBalance = await IncrementPlayerCurrency(
            context,
            gameApiClient,
            k_CoinCurrencyKey,
            coinsGranted);

        saveData.saveVersion = Math.Max(clientSaveVersion, saveData.saveVersion);
        saveData.bestScore = Math.Max(previousBestScore, runScore);
        saveData.coins = currentCoinBalance;

        await m_PlayerDataService.SavePlayerSaveDataDocument(context, gameApiClient, saveData);

        processedRunIds.Add(runId);
        if (processedRunIds.Count > k_MaxTrackedRunIds)
        {
            processedRunIds = processedRunIds
                .Skip(processedRunIds.Count - k_MaxTrackedRunIds)
                .ToList();
        }

        await SaveProcessedRunIds(context, gameApiClient, processedRunIds);

        return new RewardRunResult
        {
            CoinsGranted = coinsGranted,
            CurrentCoinBalance = currentCoinBalance,
            BestScore = saveData.bestScore,
            IsNewBestScore = isNewBestScore,
            RunAlreadyProcessed = false
        };
    }

    private int CalculateCoinsGranted(int runScore, bool usedContinue)
    {
        return Math.Max(0, runScore);
    }

    private async Task<int> IncrementPlayerCurrency(
        IExecutionContext context,
        IGameApiClient gameApiClient,
        string currencyId,
        int amount)
    {
        try
        {
            CurrencyModifyBalanceRequest request = new(currencyId, amount);
            var response = await gameApiClient.EconomyCurrencies.IncrementPlayerCurrencyBalanceAsync(
                context,
                context.AccessToken,
                context.ProjectId,
                context.PlayerId!,
                currencyId,
                request);

            return (int)response.Data.Balance;
        }
        catch (ApiException ex)
        {
            m_logger.LogError("Failed to increment currency {CurrencyId}. Error: {Error}", currencyId, ex.Message);
            throw new Exception($"Failed to increment currency {currencyId} for playerId {context.PlayerId}. Error: {ex.Message}");
        }
    }

    private async Task<int> GetPlayerCurrency(IExecutionContext context, IGameApiClient gameApiClient, string key)
    {
        try
        {
            var playerCurrenciesData = await gameApiClient.EconomyCurrencies.GetPlayerCurrenciesAsync(
                context,
                context.AccessToken,
                context.ProjectId,
                context.PlayerId!);

            if (playerCurrenciesData.Data?.Results == null)
            {
                throw new Exception($"No currency results were returned for playerId {context.PlayerId}.");
            }

            CurrencyBalanceResponse? targetCurrency = playerCurrenciesData.Data.Results.FirstOrDefault(c => c.CurrencyId == key);

            if (targetCurrency == null)
            {
                throw new Exception($"Currency with key {key} not found for playerId {context.PlayerId}");
            }

            return (int)targetCurrency.Balance;
        }
        catch (ApiException ex)
        {
            throw new Exception($"Failed to get player currency for playerId {context.PlayerId}. Error: {ex.Message}");
        }
    }

    private async Task<List<string>> GetProcessedRunIds(IExecutionContext context, IGameApiClient gameApiClient)
    {
        try
        {
            var result = await gameApiClient.CloudSaveData.GetItemsAsync(
                context,
                context.AccessToken,
                context.ProjectId,
                context.PlayerId!,
                new List<string> { k_ProcessedRunIdsKey });

            if (result.Data?.Results == null)
            {
                return new List<string>();
            }

            string? json = result.Data.Results.FirstOrDefault()?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<string>();
            }

            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch (ApiException ex)
        {
            throw new Exception($"Failed to get processed run ids for playerId {context.PlayerId}. Error: {ex.Message}");
        }
    }

    private async Task SaveProcessedRunIds(IExecutionContext context, IGameApiClient gameApiClient, List<string> processedRunIds)
    {
        try
        {
            await gameApiClient.CloudSaveData.SetItemAsync(
                context,
                context.AccessToken,
                context.ProjectId,
                context.PlayerId!,
                new SetItemBody(k_ProcessedRunIdsKey, JsonSerializer.Serialize(processedRunIds)));
        }
        catch (ApiException ex)
        {
            throw new Exception($"Failed to save processed run ids for playerId {context.PlayerId}. Error: {ex.Message}");
        }
    }
}
