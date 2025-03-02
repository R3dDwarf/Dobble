using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;


namespace Assets.Scripts.Shared
{
    class PLayerScore
    {
        ulong playerID;
        int score;
        Label scoreLabel;

        public PLayerScore(ulong playerID, Label scoreLabel)
        {
            this.playerID = playerID;
            score = 0;
            this.scoreLabel = scoreLabel;
        }

        public bool CheckID(ulong playerID1)
        {
            return playerID == playerID1;
        }

        public void IncScore() { score++; }
    }
}
