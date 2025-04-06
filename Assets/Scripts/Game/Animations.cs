using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Animations : NetworkBehaviour
{
    public static Animations Instance;

    DeckManager deck = DeckManager.Instance;
    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator DelayAnimation(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback();
        deck.gameStarted = true;

    }


    public void FLipCardWithDelay()
    {
        if (deck.cardOnDeck != null)
        {
            deck.cardOnDeck.GetComponent<Card>().FlipCardClientRpc(0.5f);
            StartCoroutine(DelayAnimation(0.5f, deck.EnableClick));
        }

    }


    [ClientRpc]
    public void FlipCardClientRpc(ulong client, float dur)
    {
        if (NetworkManager.Singleton.LocalClientId != client) { return; }

        deck.cardLocal.GetComponent<Card>().FlipCard(dur);
    }


    [ClientRpc]
    public void FLipCardWithDelayClientRpc(ulong client, float dur)
    {
        if (client != NetworkManager.Singleton.LocalClientId) { return; }
        StartCoroutine(FlipLocalCardWithDelay(client, dur));   

    }

    public IEnumerator FlipLocalCardWithDelay( float delay, float dur)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("pop2");
        deck.cardLocal.GetComponent<Card>().FlipCard(0.5f);
        deck.cardLocal.GetComponent<Card>().isLocal= true;
    }
}
