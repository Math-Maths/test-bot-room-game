using System.Collections.Generic;

namespace TestRoomCloud;

public class RewardRunResult
{
    public int CoinsGranted { get; set; }
    public int CurrentCoinBalance { get; set; }
    public int BestScore { get; set; }
    public bool IsNewBestScore { get; set; }
    public bool RunAlreadyProcessed { get; set; }
}

public class PlayerSaveDataDocument
{
    public int saveVersion { get; set; } = 2;
    public string playerName { get; set; } = "New Player";
    public int coins { get; set; }
    public int bestScore { get; set; }
    public List<string> unlockedAchivements { get; set; } = new();
}
