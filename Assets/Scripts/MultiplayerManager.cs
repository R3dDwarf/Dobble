using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Collections;
using DG.Tweening.Plugins;
using System.Collections.Generic;
using System.Collections;

public class MultiplayerManager : NetworkBehaviour
{
    public static MultiplayerManager Instance { get; private set; }

    public GameObject deckManagerPrefab;

    public GameObject UIManagerPrefab;

    // Store player names and clientIds in separate lists
    public NetworkList<FixedString32Bytes> playerNames = new NetworkList<FixedString32Bytes>();

    // connected players
    public NetworkList<ulong> playerIds = new NetworkList<ulong>();

    // Stores count of symbols on card
    public short symbolsOnCard;

    // Stores the index of the Game Mode
    int gameModeIndex;

    // indicates if game mode is multiplayer or not
    bool isMulti;

    // Check if all clients have sent ready message
    public List<ulong> clientsReady;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        playerNames = new NetworkList<FixedString32Bytes>();
        playerIds = new NetworkList<ulong>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerJoined;
            Debug.Log("MultiplayerManager started on host");
        }
        else
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleDisconnect;
            Debug.Log("MultiplayerManager started on client");
        }

        playerNames.OnListChanged += (NetworkListEvent<FixedString32Bytes> changeEvent) =>
        {
            UpdateUI();
        };
        playerIds.OnListChanged += (NetworkListEvent<ulong> changeEvent) =>
        {
            UpdateUI();
        };
    }
    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Local client connected! Sending RPC...");
            SetPlayerNameServerRpc(clientId, MainMenuEvents.Instance.GetPlayerName());
        }
    }
    private void HandleDisconnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Odpojení klienta. Vracím se do menu...");
            NetworkManager.Singleton.Shutdown();

            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                MainMenuEvents.Instance.LoadSceneAfterDisctFromLobby();
            }
            else if(SceneManager.GetActiveScene().buildIndex == 2){
                SceneManager.LoadScene(0);
            }
        }
    }



    //
    // Creates Lobby via relay
    //
    public async Task<string> CreateLobby(short symbolsOnCard, bool isMulti, int gameIndex)
    {
        // assign count of symbols on card
        this.symbolsOnCard = symbolsOnCard;
        this.isMulti = isMulti;
        this.gameModeIndex = gameIndex;


        // inicialize empty list for client
        clientsReady = new List<ulong>();

        // return established connection
        return await RelayManager.Instance.CreateRelay(3);
    }
    
    
    public void  KickPlayer(int index)
    {
        Debug.Log($"Kick Player {index}");
        if (IsServer)
        {
            NetworkManager.Singleton.DisconnectClient(playerIds[index]);
            Debug.Log($"Kick Player {index}");
            playerIds.RemoveAt(index);
            playerNames.RemoveAt(index);

        }

    }

   


    // Joins Lobby via relay
    public async Task<bool> JoinLobby(string joinCode)
    {
        return await RelayManager.Instance.JoinRelay(joinCode);
    }

    // Action if another client joins
    private void OnPlayerJoined(ulong clientId)
    {
        Debug.Log($"Player {clientId} joined!");

        if (IsServer)
        {
            if (MainMenuEvents.Instance)
            {
                string ownerName = MainMenuEvents.Instance.GetPlayerName();
                SetPlayerNameServerRpc(clientId, ownerName);
            }
        }
    }

    // Start Game scene
    public void ChangeScene(string sceneName)
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

    // Exit Game Scene
    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        AudioManager.instance.StopMusic();

        ChangeScene("Lobby");
    }

    // SERVER RPCs
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerNameServerRpc(ulong clientId, string playerName)
    {
        FixedString32Bytes name = new FixedString32Bytes(playerName);

        if (!playerIds.Contains(clientId))
        {
            playerIds.Add(clientId);
            playerNames.Add(name);
        }
        if (string.Compare(playerNames[playerIds.IndexOf(clientId)].ToString(), playerName) != 0)
        {
            playerNames[playerIds.IndexOf(clientId)] = playerName;
        }
        Debug.LogWarning(playerName);

    }


        
    [ServerRpc(RequireOwnership =false)]
    public void ClientReadyServerRpc(ulong clientId)
    {
        clientsReady.Add(clientId);
        foreach (ulong client in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!clientsReady.Contains(client))
            {
                Debug.Log("Adding client");
                Debug.Log("client not ready");
                return;
            }
        }

        // if all players are connected, send Start gamemode 
        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            StartCoroutine(WaitAndStartGame());
        }


    }
    IEnumerator WaitAndStartGame()
    {
        yield return new WaitUntil(() => DeckManager.Instance != null && DeckManager.Instance.IsSpawned);

        DeckManager.Instance.GameStartedServerRpc(symbolsOnCard, gameModeIndex, isMulti);
        UIManager.Instance.StartCountDownClientRpc();
    }



    private void UpdateUI()
    {
        if (MainMenuEvents.Instance)
        {
            MainMenuEvents.Instance.UpdatePlayerList(playerNames);
        }
    }

}
