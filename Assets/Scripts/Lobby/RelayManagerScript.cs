

using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System;

public class RelayManager : MonoBehaviour
{


    public async Task<string> CreateRelay()
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(1);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var relayServerData = new RelayServerData(allocation,"wss");

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


    public async Task JoinRelay(string joinCode)
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
        }
    }


}
