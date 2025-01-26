using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerHud : NetworkBehaviour
{
    private NetworkVariable<NetworkString> playersName = new NetworkVariable<NetworkString>();

    private bool overLaySet  = false;

    public override void OnNetworkSpawn()
    {
    }

    public void SetOverlay() 
    {
        
    }
}
