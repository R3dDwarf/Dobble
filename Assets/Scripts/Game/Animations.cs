using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
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

    public void StartDelayScore(float delay, string suffix, bool order )
    {
        StartCoroutine(DelayScoreCoroutine(delay,suffix, order));
    }

    private IEnumerator DelayScoreCoroutine(float delay, string suffix, bool order)
    {
        yield return new WaitForSeconds(delay);
        UIManager.Instance.ShowScoreBoardServerRpc(suffix, order);
    }





    public void FLipCardWithDelay()
    {
        if (deck.cardOnDeck != null)
        {
            deck.cardOnDeck.GetComponent<Card>().FlipCardClientRpc(0.5f);
            StartCoroutine(DelayAnimation(0.5f, deck.EnableClick));
        }

    }
    public void InstantFLipCardWithDelay(ulong clientId)
    {
        if (deck.cardOnDeck != null)
        {
            deck.cardOnDeck.GetComponent<Card>().FlipCardClientRpc(0f);
            StartCoroutine(DelayAnimation(0.5f, deck.EnableClick));
        }

    }


    [ClientRpc]
    public void FlipCardClientRpc(ulong client, float dur)
    {
        if (NetworkManager.Singleton.LocalClientId != client) { return; }

        deck.cardLocal.GetComponent<Card>().FlipCard(dur);
    }



    public void FLipLocalardWithDelay(GameObject card, float dur)
    {
        StartCoroutine(FlipLocalCardWithDelayCoroutine(card, dur));
    }

    public IEnumerator FlipLocalCardWithDelayCoroutine(GameObject card, float dur)
    {
        yield return new WaitForSeconds(dur);
        card.GetComponent<Card>().FlipCard(0.5f);
    }

    public IEnumerator FlipLocalCardWithDelay( float delay, float dur)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("pop2");
        deck.cardLocal.GetComponent<Card>().FlipCard(0.5f);
        deck.cardLocal.GetComponent<Card>().isLocal= true;
    }

    public IEnumerator CardFadeAway(GameObject card)
    {

        StartCoroutine(card.GetComponent<Card>().FlipBackCardCouroutine(0.5f));

        SpriteRenderer[] srp = card.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in srp)
        {
            renderer.DOFade(0f, 0.5f);
        }

        yield return new WaitForSeconds(0.5f);
        Destroy(card);
    }

    public IEnumerator CardFadeIn(GameObject card)
    {

        SpriteRenderer srp = card.GetComponent<SpriteRenderer>();

        // Set initial alpha to 0
        Color currentColor = srp.material.color;
        currentColor.a = 0f;
        srp.material.color = currentColor;

        // Fade alpha to 1
        srp.material.DOFade(1f, 0.5f);

        yield return new WaitForSeconds(0.5f);
    }


    public void FlipCardBack(GameObject card,float delay)
    {
        StartCoroutine(FlipCardBackWithDelay(card,delay));
    }

    public IEnumerator FlipCardBackWithDelay(GameObject card, float delay) 
    {
        yield return new WaitForSeconds(delay);
        card.GetComponent<Card>().FlipBackClientRpc(0.5f);
        
    }
}
