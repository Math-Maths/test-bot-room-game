using UnityEngine;

namespace TestBotRoom.Utils
{

public static class DifficultyMultiplier
{
    private static float secondsToMaxDifficulty = 60f;

    public static float GetDifficulty()
    {
        return Mathf.Clamp01(Time.timeSinceLevelLoad / secondsToMaxDifficulty);
    }

}
}
