using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour{ 
    
    private static PlayerController instance;

    private NetworkVariable<int> playersInGame = new NetworkVariable<int>(); // toto je zkouska


    public int PlayersInGame { get { return playersInGame.Value; } }

    /*
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    */
}
