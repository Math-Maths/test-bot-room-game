using UnityEngine;

namespace TestBotRoom.Utils
{

public static class DifficultyMultiplier
{
    public static float coinsToMaxDifficulty = 50;
    public static float currentCoinCount;

    public static float GetDifficulty()
    {
        float difficulty = Mathf.Clamp01(currentCoinCount / coinsToMaxDifficulty);

        return difficulty;
    }

    public static void ResetDifficulty()
    {
        currentCoinCount = 0;
    }
}
}
