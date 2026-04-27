using System;
using System.Collections.Generic;
using System.Linq;
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

    private static ILogger<PlayerDataService> _logger;

    public PlayerDataService(ILogger<PlayerDataService> logger)
    {
        _logger = logger;
    }

    [CloudCodeFunction("GetFullPlayerData")]
    public async Task<string?> GetFullPlayerData(IExecutionContext context, IGameApiClient gameApiClient)
    {
        // Tentamos buscar a chave "PLAYER_SAVE_DATA"
        var results = await GetData(context, gameApiClient, context.PlayerId, k_PlayerSaveDataKey);
        return results.FirstOrDefault(); // Retorna o JSON ou null se não existir
    }
    
    [CloudCodeFunction("SaveFullPlayerData")]
    public async Task SaveFullPlayerData(IExecutionContext context, IGameApiClient gameApiClient, string jsonData)
    {
        // Salva a string JSON inteira em uma única chave
        await SaveData(context, gameApiClient, k_PlayerSaveDataKey, jsonData);
    }

    private async Task SaveData(IExecutionContext context, IGameApiClient gameApiClient, string key, string value)
    {
        try
        {
            await gameApiClient.CloudSaveData.SetItemAsync(
                context, 
                context.AccessToken, 
                context.ProjectId,
                context.PlayerId, 
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
                context.PlayerId, new List<string> { key });

            return result.Data.Results
                .Select(item => item.Value?.ToString())
                .Where(value => value != null)
                .ToList();
        }
        catch (ApiException ex)
        {
            _logger.LogError("Failed to get data. Error: {Error}", ex.Message);
            throw new Exception($"Failed to get data for playerId {context.PlayerId}. Error: {ex.Message}");
        }
    }

    [CloudCodeFunction("SayHello")]
    public string Hello(string name)
    {
        return $"Hello, {name}!";
    }

    [CloudCodeFunction("HandleNewPlayerNameEntry")]
    public async Task<string> HandleNewPlayerNameEntry(IExecutionContext context, IGameApiClient gameApiClient, string newName)
    {
        if(IsPlayerNameValid(newName))
        {
            await SaveData(context, gameApiClient, k_PlayerNameKey, newName);
            return newName;
        }

        throw new ArgumentException("Name is not valid");
    }

    private bool IsPlayerNameValid(string name)
    {
        if(name.Length is < 4 or > 16)
        {
            return false;
        }

        return true;
    }
}


