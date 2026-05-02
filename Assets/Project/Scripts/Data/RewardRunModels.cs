using System;

namespace TestBotRoom
{
    [Serializable]
    public class RewardRunRequest
    {
        public int runScore;
        public bool usedContinue;
        public string runId;
        public int clientSaveVersion;
    }

    [Serializable]
    public class RewardRunResult
    {
        public int CoinsGranted;
        public int CurrentCoinBalance;
        public int BestScore;
        public bool IsNewBestScore;
        public bool RunAlreadyProcessed;
    }
}
