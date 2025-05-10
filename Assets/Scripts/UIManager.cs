using Assets.Scripts.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance { get; private set; }

    private UIDocument UIDocument;
    public VisualElement deckContainer;
    public PLayerScore[] scoreBoard = new PLayerScore[4];


    // UI Builder Buttons
    private Button menuBtn;
    private Button exitMenuBtn;
    private Button exitGameBtn;

    private Button leaveGameBtn;

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

        leaveGameBtn = UIDocument.rootVisualElement.Q("LeaveGameBtn") as Button;
        if (leaveGameBtn != null)
        {
            leaveGameBtn.RegisterCallback<ClickEvent>(evt => MultiplayerManager.Instance.ExitGame());
        }


        for (int i = 0; i < playerIDs.Count; i++)
        {
            PLayerScore pscore = new PLayerScore(playerIDs[i], UIDocument.rootVisualElement.Q("PlayerScore" + (i + 1).ToString()) as Label);
            scoreBoard[i] = pscore;
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

    [ServerRpc]
    public void ShowScoreBoardServerRpc(string suffix, bool order)
    {
        if (order)
        {
            // Sort by descending score
            var topScores = scoreBoard
                .Where(ps => ps != null)
                .OrderByDescending(ps => ps.GetScore())
                .Take(4)
                .ToList();

            // Prepare name and score arrays
            string[] names = new string[4];
            int[] scores = new int[4];

            for (int i = 0; i < topScores.Count; i++)
            {
                ulong playerId = topScores[i].GetId();
                int indexOfId = MultiplayerManager.Instance.playerIds.IndexOf(playerId);
                names[i] = MultiplayerManager.Instance.playerNames[indexOfId].ToString();
                scores[i] = topScores[i].GetScore();
            }

            // Fill remaining entries with default values
            for (int i = topScores.Count; i < 4; i++)
            {
                names[i] = string.Empty;
                scores[i] = -1;
            }

            // Send to clients
            ShowScoreBoardClientRpc(suffix, names[0], names[1], names[2], names[3],
                                    scores[0], scores[1], scores[2], scores[3]);
        }
        else
        {
            // Sort by ascending score
            var topScores = scoreBoard
                .Where(ps => ps != null)
                .OrderBy(ps => ps.GetScore()) // vzestupné øazení
                .Take(4)
                .ToList();


            // Prepare name and score arrays
            string[] names = new string[4];
            int[] scores = new int[4];

            for (int i = 0; i < topScores.Count; i++)
            {
                ulong playerId = topScores[i].GetId();
                int indexOfId = MultiplayerManager.Instance.playerIds.IndexOf(playerId);
                names[i] = MultiplayerManager.Instance.playerNames[indexOfId].ToString();
                scores[i] = topScores[i].GetScore();
            }

            // Fill remaining entries with default values
            for (int i = topScores.Count; i < 4; i++)
            {
                names[i] = string.Empty;
                scores[i] = -1;
            }

            // Send to clients
            ShowScoreBoardClientRpc(suffix, names[0], names[1], names[2], names[3],
                                    scores[0], scores[1], scores[2], scores[3]);

        }
    }



    [ClientRpc]
    public void ShowScoreBoardClientRpc(string suffix, string name1, string name2, string name3, string name4, int score1, int score2, int score3, int score4)
    {
        VisualElement box = UIDocument.rootVisualElement.Q("WinnerBox");
        if (box != null)
        {
            box.style.display = DisplayStyle.Flex;
        }


        VisualElement player1 = UIDocument.rootVisualElement.Q("Player1");
        if (!string.IsNullOrEmpty(name1))
        {
            player1.style.display = DisplayStyle.Flex;

            Label player1Label = UIDocument.rootVisualElement.Q("Player1Label") as Label;
            player1Label.text = name1 + " : " + score1.ToString() + suffix;
        }
        else
        {
            player1.style.display = DisplayStyle.None;
        }


        VisualElement player2 = UIDocument.rootVisualElement.Q("Player2");
        if (!string.IsNullOrEmpty(name2))
        {
            
            player2.style.display = DisplayStyle.Flex;

            Label player2Label = UIDocument.rootVisualElement.Q("Player2Label") as Label;
            player2Label.text = name2 + " : " + score2.ToString() + suffix;
        }
        else
        {
            player2.style.display = DisplayStyle.None;
        }


        VisualElement player3 = UIDocument.rootVisualElement.Q("Player3");
        if (!string.IsNullOrEmpty(name3))
        {
            
            player3.style.display = DisplayStyle.Flex;

            Label player3Label = UIDocument.rootVisualElement.Q("Player3Label") as Label;
            player3Label.text = name3 + " : " + score3.ToString() + suffix;

        }
        else
        {
            player3.style.display = DisplayStyle.None;
        }


        VisualElement player4 = UIDocument.rootVisualElement.Q("Player4");
        if (!string.IsNullOrEmpty(name4))
        {
            player4.style.display = DisplayStyle.Flex;

            Label player4Label = UIDocument.rootVisualElement.Q("Player4Label") as Label;
            player4Label.text = name4 + " : " + score4.ToString() + suffix;

        }
        else
        {
            player4.style.display = DisplayStyle.None;
        }
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

        DeckManager.Instance.StopWatchAlertServerRpc(NetworkManager.Singleton.LocalClientId);



    }
}
