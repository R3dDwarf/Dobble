using Assets.Scripts.Shared;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class TowerUIManager : NetworkBehaviour
{
    public static TowerUIManager Instance { get; private set; }


    // UI documment
    private UIDocument UIDocument;

    // deck Container
    public VisualElement deckContainer;


    private PLayerScore[] scoreBoard = new PLayerScore[4];

    void Start()
    {
        UIDocument = GetComponent<UIDocument>();


        // find deck Container
        deckContainer = UIDocument.rootVisualElement.Q("DeckContainer");

        if (!IsServer) return;

        Debug.Log("TowerUIManager is now spawned, setting up HUD...");
        SetUpGameHUDServerRpc();


    }

    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

    }

    [ClientRpc]
    public void AssignPlayerHUDClientRpc()
    {
        NetworkList<ulong> playerIDs = MultiplayerManager.Instance.playerIds;
        if (playerIDs.Count == 0 || playerIDs.Count > 4) { throw new System.Exception("Too few or too many players!"); }

        for (int i = 0; i < playerIDs.Count; i++)
        {

            PLayerScore pscore = new PLayerScore(playerIDs[i], UIDocument.rootVisualElement.Q("PlayerScore" + i.ToString()) as Label);
            scoreBoard[i] = pscore;
        }
    }

    [ClientRpc]
    public void ShowNamesClientRpc()
    {
        NetworkList<FixedString32Bytes> playerNames = MultiplayerManager.Instance.playerNames;
        if (playerNames.Count == 0 || playerNames.Count > 4) { throw new System.Exception("Too few or too many players!"); }

        for (int i = 0;i < playerNames.Count; i++)
        {  
            Label newName = UIDocument.rootVisualElement.Q("PlayerName" + (i + 1).ToString()) as Label;
            newName.text = playerNames[i].ToString();
        }

    }

    [ServerRpc]
    public void SetUpGameHUDServerRpc()
    {
        ShowNamesClientRpc();
        AssignPlayerHUDClientRpc();
    }

}
