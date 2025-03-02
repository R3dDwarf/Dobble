using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Collections;

public class MultiplayerManager : NetworkBehaviour
{
    public static MultiplayerManager Instance { get; private set; }

    public GameObject deckManagerPrefab;

    // Store player names and clientIds in separate lists
    public NetworkList<FixedString32Bytes> playerNames;
    public NetworkList<ulong> playerIds;

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




    // Creates Lobby via relay
    public async Task<string> CreateLobby(short lobbySize)
    {
        return await RelayManager.Instance.CreateRelay(lobbySize);
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


    private void UpdateUI()
    {
        if (MainMenuEvents.Instance)
        {
            MainMenuEvents.Instance.UpdatePlayerList(playerNames);
        }
    }
}
