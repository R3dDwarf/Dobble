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

    private UIDocument UIDocument;
    public VisualElement deckContainer;
    private PLayerScore[] scoreBoard = new PLayerScore[4];


    // UI Builder Buttons
    private Button menuBtn;
    private Button exitMenuBtn;
    private Button exitGameBtn;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (Instance == null)
        {
            Instance = this;
        }

        if (IsClient)
        {
            UIDocument = GetComponent<UIDocument>();
            deckContainer = UIDocument.rootVisualElement.Q("DeckContainer");
            SetUpGameHUDServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        if (IsServer)
        {
            if (UIManager.Instance == null)
            {
                Debug.LogError("UIManager instance is null on the server!");
                return;
            }
        }
        MultiplayerManager.Instance.ClientReadyServerRpc(NetworkManager.Singleton.LocalClientId);
    }

        

    [ServerRpc(RequireOwnership = false)]
    public void SetUpGameHUDServerRpc(ulong clientID)
    {
        NetworkList<FixedString32Bytes> names = MultiplayerManager.Instance.playerNames;

        if (names == null || names.Count == 0)
        {
            Debug.LogError("Player names are not initialized!");
            return;
        }

        string player1 = names.Count > 0 ? names[0].ToString() : null;
        string player2 = names.Count > 1 ? names[1].ToString() : null;
        string player3 = names.Count > 2 ? names[2].ToString() : null;
        string player4 = names.Count > 3 ? names[3].ToString() : null;


        ShowNamesClientRpc(clientID, player1, player2, player3, player4);
        AssignPlayerHUDClientRpc();


    }

    private void ToggleMenu()
    {
        VisualElement visEl = UIDocument.rootVisualElement.Q("MenuBox");
        if (visEl != null)
        {
            if (visEl.style.display == DisplayStyle.Flex)
            {
                visEl.style.display = DisplayStyle.None;
            }
            else
            {
                visEl.style.display = DisplayStyle.Flex;
            }
        }
    }

    private void LeaveGameScene()
    {
        MultiplayerManager.Instance.ChangeScene("Lobby");

    }


    [ClientRpc]
    public void ShowNamesClientRpc(ulong clientID, string player1, string player2, string player3, string player4)
    {
        if (NetworkManager.Singleton.LocalClientId != clientID) return;

        UpdatePlayerName("PlayerName1", "Player1HUD", player1);
        UpdatePlayerName("PlayerName2", "Player2HUD", player2);
        UpdatePlayerName("PlayerName3", "Player3HUD", player3);
        UpdatePlayerName("PlayerName4", "Player4HUD", player4);
    }

    private void UpdatePlayerName(string nameLabel, string hudName, string playerName)
    {
        if (!string.IsNullOrEmpty(playerName))
        {
            Label label = UIDocument.rootVisualElement.Q(nameLabel) as Label;
            label.text = playerName;
        }
        else
        {
            VisualElement playerHUD = UIDocument.rootVisualElement.Q(hudName) as VisualElement;
            playerHUD.style.display = DisplayStyle.None;
        }
    }

    [ClientRpc]
    public void AssignPlayerHUDClientRpc()
    {
        NetworkList<ulong> playerIDs = MultiplayerManager.Instance.playerIds;
        if (playerIDs.Count == 0 || playerIDs.Count > 4) return;



        // UI buttons

        menuBtn = UIDocument.rootVisualElement.Q("MenuBtn") as Button;
        if (menuBtn != null)
        {
            menuBtn.RegisterCallback<ClickEvent>(evt => ToggleMenu());
        }

        exitMenuBtn = UIDocument.rootVisualElement.Q("ExitGameMenuBtn") as Button;
        if (exitMenuBtn != null)
        {
            exitMenuBtn.RegisterCallback<ClickEvent>(evt => ToggleMenu());
        }

        exitGameBtn = UIDocument.rootVisualElement.Q("ExitGameBtn") as Button;
        if (exitGameBtn != null)
        {
            exitGameBtn.RegisterCallback<ClickEvent>(evt => MultiplayerManager.Instance.ExitGame()); 
        }


        for (int i = 0; i < playerIDs.Count; i++)
        {
            PLayerScore pscore = new PLayerScore(playerIDs[i], UIDocument.rootVisualElement.Q("PlayerScore" + (i + 1).ToString()) as Label);
            scoreBoard[i] = pscore;
        }
    }

    [ServerRpc]
    public void TowerUpdateScoreBoardServerRpc(ulong clientID)
    {
        TowerUpdateScoreBoardClientRpc(clientID);
        RefreshScoreUIClientRpc("Cards taken:");
    }


    [ClientRpc]
    public void TowerUpdateScoreBoardClientRpc(ulong clientID)
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
    public void SetStartScoreClientRpc(int cardCount)
    {
        foreach(PLayerScore ps in scoreBoard)
        {
            if (ps == null) return;
            ps.SetScore(cardCount);
        }
        RefreshScoreUIClientRpc("Cards left:");

    }



    [ServerRpc]
    public void WellUpdateScoreBoardServerRpc(ulong clientId)
    {
        WellUpdateScoreBoardClientRpc(clientId);
        RefreshScoreUIClientRpc("Cards left:");
    }

    [ClientRpc]
    public void WellUpdateScoreBoardClientRpc(ulong clientID)
    {
        foreach (PLayerScore ps in scoreBoard)
        {
            if (ps == null) return;
            if (ps.CheckID(clientID))
            {
                ps.DecScore();
            }
        }
    }

    [ClientRpc]
    public void RefreshScoreUIClientRpc(string label)
    {
        foreach (PLayerScore ps in scoreBoard)
        {
            if (ps == null) return;
            if (ps.GetScoreLabel() != null)
            {
               ps.GetScoreLabel().text = label + ps.GetScore().ToString();
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

    private void DisplayWinnersName(string name)
    {
        VisualElement winnerBox = UIDocument.rootVisualElement.Q("WinnerBox");
        Label nameLabel = UIDocument.rootVisualElement.Q("WinnerNameLabel") as Label;
        nameLabel.text = name;
        winnerBox.style.display = DisplayStyle.Flex;
    }


    // SINGLE PLAYER - SPOT ON
    [ClientRpc]
    public void ShowStopwatchClientRpc()
    {
        VisualElement sw = UIDocument.rootVisualElement.Q("Stopwatch");
        sw.style.display = DisplayStyle.Flex;
    }


    [ClientRpc]
    public void StartStopwatchClientRpc(int sec)
    {
        StartCoroutine(StartStopwatch(sec));
    }
    public IEnumerator StartStopwatch(int sec)
    {
        Label text = UIDocument.rootVisualElement.Q("StopwatchLabel") as Label;
        text.text = sec.ToString();

        yield return new WaitForSeconds(4f);
        AudioManager.instance.PlayMusic("Spot On!", true);

        while (sec > 0)
        {
            --sec;
            text.text = sec.ToString();
            yield return new WaitForSeconds(1f);
        }
        
    }
}
