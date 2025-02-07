using UnityEngine;
using Unity.Netcode;

public class MultiplayerManager : NetworkBehaviour
{
    public static MultiplayerManager Instance;

    private void Awake()
    {
        Instance = this;
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
    }
}
