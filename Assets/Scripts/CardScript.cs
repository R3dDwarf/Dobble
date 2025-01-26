using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class CardScript : NetworkBehaviour
{
    // Symbols
    [SerializeField]
    private Sprite[] Symbols;

    //Slots for symbols
    [SerializeField]
    private SpriteRenderer[] Slots;


    [ClientRpc]
    public void AssignSymbolsClientRpc(int[] indexes)
    {
        // return if array doesnt have 8 indexes
        if (indexes.Length != 8)
        {
            Debug.LogError("Cannot assgin symbols");
            return;
        }

        for (int i = 0; i < indexes.Length; i++)
        {
            Slots[i].sprite = Symbols[indexes[i]];

        }
    }

    // Fuction to assign symbols to card slots
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}