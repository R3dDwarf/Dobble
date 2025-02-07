using System.Threading.Tasks;
using System;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps it across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task<string> CreateRelay(short count)
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(count);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var relayServerData = new RelayServerData(allocation, "wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay create error: {e.Message}");
            return null;
        }
    }

    public async Task<bool> JoinRelay(string joinCode)
    {
        try
        {
            var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var relayServerData = new RelayServerData(allocation, "wss");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay join error: {e.Message} + {joinCode}");
            return false;
        }
        return true;
    }



}
