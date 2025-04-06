using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class WellLogic: NetworkBehaviour
{
    public static WellLogic Instance;

    DeckManager deck;
    Animations ani = Animations.Instance;

    // local card position
    private Vector3 localPos = new Vector3(0, -2f, 0);


    public Stack<GameObject> stack = new Stack<GameObject>();


    int cardDataIndex;


    private void Awake()
    {
        Instance = this;
        deck = DeckManager.Instance;
        ani = Animations.Instance;
    }

    //
    // Fuction called at the start of the game out of Multiplayer Manager
    //

    [ServerRpc]
    public void WellSpawnCardsServerRpc(short symbolCount, string gameMode)
    {
        Debug.Log("well");
        UIManager.Instance.SetStartScoreClientRpc(deck.cardsTotal / NetworkManager.Singleton.ConnectedClientsIds.Count);
        // Spawn new Card on deck
        deck.SpawnNewCardOnDeckServerRpc(deck.cardCounter++);

        // Spawn local cards for each client
        WellSpawnLocalStartingCardsServerRpc();

        // Delay all animations due to start of the game and the count down
        StartCoroutine(ani.DelayAnimation(4f, WellCardMovedServerRpc));
        StartCoroutine(ani.DelayAnimation(4f, deck.EnableClick));
        StartCoroutine(ani.DelayAnimation(4f, ani.FLipCardWithDelay));
        
    }


    //
    // Spawns local card for each client
    //
    [ServerRpc]
    private void WellSpawnLocalStartingCardsServerRpc()
    {
        int count = deck.cardsTotal / NetworkManager.Singleton.ConnectedClientsIds.Count;

        int i = 0;
        foreach (ulong client in NetworkManager.Singleton.ConnectedClientsIds)
        {
            StartCoroutine(SpawnCardsForClient(client, i, count));
            i = i + count;
        }
    }

    private IEnumerator SpawnCardsForClient(ulong client, int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count; i++)  // Fixed loop condition
        {
            deck.cardCounter++;
            deck.SpawnLocalCopyOfCardClientRpc(i, deck.cards[i], client, deck.deckPosition, deck.nextCardLayer);
            PushCardToStackClientRpc(client);
            yield return new WaitForSeconds(0.15f);

            deck.nextCardLayer += 3;
            deck.MoveCardToPlayersDeckClientRpc(client, localPos, 1.15f);
        }

        ani.FlipCardClientRpc(client, 0.5f);
        PopCardFromStackClientRpc(client);
    }



//
//
//
[ClientRpc]
    private void PushCardToStackClientRpc(ulong client)
    {
        if(client != NetworkManager.Singleton.LocalClientId) { return; }
        Debug.Log("Pushing card to stack");
        if (stack == null)
        {
            Debug.LogError("Som pica");
        }
        if (deck.cardLocal == null)
        {
            Debug.LogError("TEST");
        }
        stack.Push(deck.cardLocal);
    }

    [ClientRpc]
    private void PopCardFromStackClientRpc(ulong client)
    {
        if (client != NetworkManager.Singleton.LocalClientId) { return; }
        Debug.Log("Pushing card to stack");
        if (stack == null)
        {
            Debug.LogError("Som pica");
        }
        stack.Pop();

    }

    [ClientRpc]
    private void DestroyCardOnStackClientRpc(ulong client, float delay)
    {
        if (client != NetworkManager.Singleton.LocalClientId) { return; };


        if (stack.Count > 0)
        {
            StartCoroutine(DestroyLocalCardWithDelay(delay));
        }


    }
    public IEnumerator DestroyLocalCardWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(deck.cardLocal);
        Debug.Log("pop2");
        Debug.Log("pop1");
        deck.cardLocal = stack.Pop();
        
        if (deck.cardLocal == null)
        {
            Debug.LogError("Peeked card is null even though stack is not empty!");
        }
        deck.cardLocal.GetComponent<Card>().FlipCard(0.5f);
        deck.cardLocal.GetComponent<Card>().isLocal = true;
    }




    [ServerRpc]
    public void WellCorrectAnswerHandlerServerRpc(ulong playerID, int cardDataIndex, bool endOfGame)
    {
        this.cardDataIndex = cardDataIndex;
        if (!endOfGame)
        {

            // disable click events for all clients
            deck.DisableClick();
            deck.cardOnDeck.GetComponent<Card>().FlipBackClientRpc(0.5f);

            Debug.Log("1localCard" + (deck.cardLocal == null));

            deck.MoveCardToPlayersDeckClientRpc(playerID, deck.deckPosition, 0.85f);


            Debug.Log("localCard" + (deck.cardLocal == null));
            DestroyCardOnStackClientRpc(playerID,1f);


            StartCoroutine(ani.DelayAnimation(1f, ani.FLipCardWithDelay));



            // add score for client
            UIManager.Instance.WellUpdateScoreBoardServerRpc(playerID);


            deck.nextCardLayer = deck.nextCardLayer + 2;
        }
        else
        {

            // disable click events for all clients
            deck.DisableClick();
            deck.cardOnDeck.GetComponent<Card>().FlipBackClientRpc(0.5f);

            Debug.Log("1localCard" + (deck.cardLocal == null));

            deck.MoveCardToPlayersDeckClientRpc(playerID, deck.deckPosition, 0.85f);


            Debug.Log("localCard" + (deck.cardLocal == null));

            
            UIManager.Instance.ShowWinnerClientRpc("Test");
            return;

        }
    }


    [ClientRpc]
    public void OnWrongSymbolClickedClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        Card card = deck.cardLocal.GetComponent<Card>();
        if (card != null)
        {
            card.SpawnCross();
        }
    }

    [ServerRpc]
    public void WellCardMovedServerRpc()
    {
        if (deck.cardOnDeck != null && deck.gameStarted)
        {
            StartCoroutine(ani.DelayAnimation(0.5f, deck.EnableClick));
            deck.cardOnDeck.GetComponent<NetworkObject>().Despawn();

        }
    }

    [ClientRpc]
    public void DestroyLocalCardClientRpc(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId) return;

        Destroy(deck.cardLocal);
        deck.SpawnNewCardOnDeckServerRpc(cardDataIndex);
    }



}
