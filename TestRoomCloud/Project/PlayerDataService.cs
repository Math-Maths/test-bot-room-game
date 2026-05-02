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

namespace TestRoomCloud;

public class PlayerDataService
{
    public const string k_PlayerNameKey = "PLAYER_NAME";
    public const string k_PlayerSaveDataKey = "PLAYER_SAVE_DATA";

    private static ILogger<PlayerDataService> _logger = null!;

    public PlayerDataService(ILogger<PlayerDataService> logger)
    {
        _logger = logger;
    }

    [CloudCodeFunction("GetFullPlayerData")]
    public async Task<string?> GetFullPlayerData(IExecutionContext context, IGameApiClient gameApiClient)
    {
        List<string?> results = await GetData(context, gameApiClient, context.PlayerId!, k_PlayerSaveDataKey);
        return results.FirstOrDefault();
    }

    [CloudCodeFunction("SaveFullPlayerData")]
    public async Task SaveFullPlayerData(IExecutionContext context, IGameApiClient gameApiClient, string jsonData)
    {
        await SaveData(context, gameApiClient, k_PlayerSaveDataKey, jsonData);
    }

    [CloudCodeFunction("SayHello")]
    public string Hello(string name)
    {
        return $"Hello, {name}!";
    }

    [CloudCodeFunction("HandleNewPlayerNameEntry")]
    public async Task<string> HandleNewPlayerNameEntry(IExecutionContext context, IGameApiClient gameApiClient, string newName)
    {
        if (!IsPlayerNameValid(newName))
        {
            throw new ArgumentException("Name is not valid");
        }

        await SaveData(context, gameApiClient, k_PlayerNameKey, newName);

        PlayerSaveDataDocument saveData = await GetPlayerSaveDataDocument(context, gameApiClient);
        saveData.playerName = newName;

        await SaveData(
            context,
            gameApiClient,
            k_PlayerSaveDataKey,
            JsonSerializer.Serialize(saveData));

        return newName;
    }

    internal async Task<PlayerSaveDataDocument> GetPlayerSaveDataDocument(IExecutionContext context, IGameApiClient gameApiClient)
    {
        List<string?> results = await GetData(context, gameApiClient, context.PlayerId!, k_PlayerSaveDataKey);
        string? json = results.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(json))
        {
            return new PlayerSaveDataDocument();
        }

        PlayerSaveDataDocument? saveData = JsonSerializer.Deserialize<PlayerSaveDataDocument>(json);
        return saveData ?? new PlayerSaveDataDocument();
    }

    internal async Task SavePlayerSaveDataDocument(IExecutionContext context, IGameApiClient gameApiClient, PlayerSaveDataDocument saveData)
    {
        await SaveData(
            context,
            gameApiClient,
            k_PlayerSaveDataKey,
            JsonSerializer.Serialize(saveData));
    }

    private async Task SaveData(IExecutionContext context, IGameApiClient gameApiClient, string key, string value)
    {
        try
        {
            await gameApiClient.CloudSaveData.SetItemAsync(
                context,
                context.AccessToken,
                context.ProjectId,
                context.PlayerId!,
                new SetItemBody(key, value));
        }
        catch (ApiException ex)
        {
            _logger.LogError("Failed to save data. Error: {Error}", ex.Message);
            throw new Exception($"Failed to save data for playerId {context.PlayerId}. Error: {ex.Message}");
        }
    }

    private async Task<List<string?>> GetData(IExecutionContext context, IGameApiClient gameApiClient, string playerId, string key)
    {
        try
        {
            var result = await gameApiClient.CloudSaveData.GetItemsAsync(
                context,
                context.AccessToken,
                context.ProjectId,
                playerId,
                new List<string> { key });

            if (result.Data?.Results == null)
            {
                _logger.LogWarning("Cloud Save returned no results for key {Key} and playerId {PlayerId}", key, playerId);
                return new List<string?>();
            }

            return result.Data.Results
                .Select(item => item.Value?.ToString())
                .Where(value => value != null)
                .ToList();
        }
        catch (ApiException ex)
        {
            _logger.LogError("Failed to get data. Error: {Error}", ex.Message);
            throw new Exception($"Failed to get data for playerId {playerId}. Error: {ex.Message}");
        }
    }

    private bool IsPlayerNameValid(string name)
    {
        return name.Length is >= 4 and <= 16;
    }
}
