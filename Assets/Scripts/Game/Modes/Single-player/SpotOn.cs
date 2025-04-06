using System.Collections;
using System.Collections.Generic;
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
    public void SpotOnSpawnCardsServerRpc(short symbolCount, string gameMode)
    {
        ui.ShowStopwatchClientRpc();
        ui.StartStopwatchClientRpc(90);
        TowerLogic.Instance.TowerSpawnCardsServerRpc(symbolCount, gameMode);

    }
}
