using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Collections.Generic;

public class MultiplayerManager : NetworkBehaviour
{
    public static MultiplayerManager Instance;

    public NetworkList<FixedString32Bytes> playerNames;


    private void Awake()
    {
        Instance = this;
        playerNames = new NetworkList<FixedString32Bytes>(); // Initialize the list

        // Listen for list changes and update UI when modified
        playerNames.OnListChanged += (NetworkListEvent<FixedString32Bytes> changeEvent) =>
        {
            UpdateUI();
        };
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
            Debug.Log("MultiplayerManager started on client");
        }
    }

    private void OnPlayerJoined(ulong clientId)
    {
        Debug.Log($"Player {clientId} joined!");

        if (IsServer) // Only the server modifies the list
        {
            string playerName = "Player_" + clientId;
            AddPlayerServerRpc(playerName);
        }
    }

    [ServerRpc]
    private void AddPlayerServerRpc(string playerName)
    {
        if (!playerNames.Contains(playerName))
        {
            playerNames.Add(playerName);
        }
    }


    private void UpdateUI()
    {
        if (MainMenuScript.Instance)
        {
            MainMenuScript.Instance.UpdatePlayerList(playerNames);
        }
    }
}
