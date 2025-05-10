using Assets.Scripts.Shared;

using Unity.Netcode;

using UnityEngine;

public class SpotOn : MonoBehaviour
{

    public static SpotOn Instance;

    DeckManager deck = DeckManager.Instance;
    Animations ani = Animations.Instance;
    UIManager ui = UIManager.Instance;

    // local card position
    private Vector3 localPos = new Vector3(0, -2f, 0);



    private void Awake()
    {
        Instance = this;
    }

    [ServerRpc]
    public void SpotOnSpawnCardsServerRpc(short symbolCount)
    {
        ui.ShowStopwatchClientRpc();
        ui.StartStopwatchClientRpc(90);
        TowerLogic.Instance.TowerSpawnCardsServerRpc(symbolCount);

    }

    [ServerRpc]
    public void ShowScoreServerRpc(ulong clientId)
    {
        ShowScoreClientRpc(clientId);
    }

    [ClientRpc]
    public void ShowScoreClientRpc(ulong clientId)
    {
        foreach (PLayerScore ps in UIManager.Instance.scoreBoard)
        {
            if (ps == null) return;
            if (ps.CheckID(clientId))
            {
                UIManager.Instance.ShowWinnerClientRpc(ps.GetScore().ToString());
            }
        }
    }
}
