using Assets.Scripts.Shared;
using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance { get; private set; }


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

        Debug.Log("TowerUIManager is now spawned, setting up HUD...");

        SetUpGameHUDServerRpc(NetworkManager.Singleton.LocalClientId);


        MultiplayerManager.Instance.ClientReadyServerRpc(NetworkManager.LocalClientId);
        

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
        if (!IsServer) return;


    }

    [ClientRpc]
    public void AssignPlayerHUDClientRpc()
    {
        NetworkList<ulong> playerIDs = MultiplayerManager.Instance.playerIds;
        if (playerIDs.Count == 0 || playerIDs.Count > 4) { throw new System.Exception("Too few or too many players!"); }

        for (int i = 0; i < playerIDs.Count; i++)
        {

            PLayerScore pscore = new PLayerScore(playerIDs[i], UIDocument.rootVisualElement.Q("PlayerScore" + (i + 1).ToString()) as Label);
            scoreBoard[i] = pscore;
        }

    }

    [ClientRpc]
    public void ShowNamesClientRpc(ulong clientID, string player1, string player2, string player3, string player4)
    {
        if (NetworkManager.Singleton.LocalClientId != clientID) return;

        if (!string.IsNullOrEmpty(player1))
        {
            Label newName1 = UIDocument.rootVisualElement.Q("PlayerName1") as Label;
            newName1.text = player1;
        }
        else
        {
            VisualElement playerHUD = UIDocument.rootVisualElement.Q("Player1HUD") as VisualElement;
            playerHUD.style.display = DisplayStyle.None;
        }

        if (!string.IsNullOrEmpty(player2))
        {
            Label newName2 = UIDocument.rootVisualElement.Q("PlayerName2") as Label;
            newName2.text = player2;
        }
        else
        {
            VisualElement playerHUD = UIDocument.rootVisualElement.Q("Player2HUD") as VisualElement;
            playerHUD.style.display = DisplayStyle.None;
        }

        if (!string.IsNullOrEmpty(player3))
        {
            Label newName3 = UIDocument.rootVisualElement.Q("PlayerName3") as Label;
            newName3.text = player3;
        }
        else
        {
            VisualElement playerHUD = UIDocument.rootVisualElement.Q("Player3HUD") as VisualElement;
            playerHUD.style.display = DisplayStyle.None;
        }

        if (!string.IsNullOrEmpty(player4))
        {
            Label newName4 = UIDocument.rootVisualElement.Q("PlayerName4") as Label;
            newName4.text = player4;
        }
        else
        {
            VisualElement playerHUD = UIDocument.rootVisualElement.Q("Player4HUD") as VisualElement;
            playerHUD.style.display = DisplayStyle.None;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetUpGameHUDServerRpc(ulong clientID)
    {
        NetworkList<FixedString32Bytes> names = MultiplayerManager.Instance.playerNames;

        string player1 = null;
        string player2 = null;
        string player3 = null;
        string player4 = null;

        if (names != null)
        {
            if (names.Count > 0) player1 = names[0].ToString();
            if (names.Count > 1) player2 = names[1].ToString();
            if (names.Count > 2) player3 = names[2].ToString();
            if (names.Count > 3) player4 = names[3].ToString();
        }

        ShowNamesClientRpc(clientID, player1, player2, player3, player4);
        AssignPlayerHUDClientRpc();
        RefreshScoreUIClientRpc();
    }


    [ServerRpc]
    public void UpdateScoreBoardServerRpc(ulong clientID)
    {

        UpdateScoreBoardClientRpc(clientID);
        RefreshScoreUIClientRpc();
    }

    [ClientRpc]
    public void UpdateScoreBoardClientRpc(ulong clientID)
    {
        foreach (PLayerScore ps in scoreBoard)
        {
            if (ps == null) return;
            if (ps.CheckID(clientID))
            {
                ps.IncScore();

            }
        }
    }

    [ClientRpc]
    public void RefreshScoreUIClientRpc()
    {
        foreach (PLayerScore ps in scoreBoard)
        {
            if (ps == null) return;
            if (ps.GetScoreLabel() != null)
            {
                Debug.LogWarning("fwefwefwefwefwefwefwefwef");
                ps.GetScoreLabel().text = ps.GetScore().ToString();
            }
        }
    }

    [ClientRpc]
    public void StartCountDownClientRpc()
    {
        StartCoroutine(StartCountdownCoroutine());
    }


    IEnumerator StartCountdownCoroutine()
    {
        Label countdownText = UIDocument.rootVisualElement.Q("CountDownLabel") as Label;
        VisualElement countdownBox = UIDocument.rootVisualElement.Q("CountDown") as VisualElement;

        countdownText.text = "Starting in 3...";
        yield return new WaitForSeconds(1f);

        countdownText.text = "Starting in 2...";
        yield return new WaitForSeconds(1f);

        countdownText.text = "Starting in 1...";
        yield return new WaitForSeconds(1f);

        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);

        countdownBox.style.display = DisplayStyle.None;
    }

    [ClientRpc]
    public void ShowWinnerClientRpc(string name)
    {
        StartCoroutine(DelayAnimation(1f, DisplayWinnersName, name));
    }

    private IEnumerator DelayAnimation(float delay, Action<string> callback, string message)
    {
        yield return new WaitForSeconds(delay);
        callback(message);
    }


    private void DisplayWinnersName(string name) {
        Debug.LogWarning("Started");
        VisualElement winnerBox = UIDocument.rootVisualElement.Q("WinnerBox");
        Label nameLabel = UIDocument.rootVisualElement.Q("WinnerNameLabel") as Label;
        nameLabel.text = name;
        winnerBox.style.display = DisplayStyle.Flex;

    }
}
