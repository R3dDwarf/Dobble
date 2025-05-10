using Assets.Scripts.Shared;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TowerLogic : NetworkBehaviour
{
    public static TowerLogic Instance;

    DeckManager deck = DeckManager.Instance;
    Animations ani = Animations.Instance;

    // local card position
    private Vector3 localPos = new Vector3(0, -2f, 0);



    private void Awake()
    {
        Instance = this;
    }


    //
    // Fuction called at the start of the game out of Multiplayer Manager
    //

    [ServerRpc]
    public void TowerSpawnCardsServerRpc(short symbolCount)
    {

        UIManager.Instance.RefreshScoreUIClientRpc("Cards taken: ");
        // Spawn new Card on deck
        deck.SpawnNewCardOnDeckServerRpc(deck.cardCounter++, false);

        // Spawn local cards for each client
        TowerSpawnLocalStartingCardsServerRpc();

        // Delay all animations due to start of the game and the count down
        StartCoroutine(ani.DelayAnimation(4f, deck.EnableClick));
        StartCoroutine(ani.DelayAnimation(4f, ani.FLipCardWithDelay));
        FlipLocalStartingCardsClientRpc();
    }


    //
    // Spawns local card for each client
    //
    [ServerRpc]
    private void TowerSpawnLocalStartingCardsServerRpc()
    {
        foreach (ulong client in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // spawns local card
            deck.cardCounter = deck.cardCounter + 2;
            deck.SpawnLocalCopyOfCardClientRpc(deck.cardCounter, deck.cards[deck.cardCounter], client, new Vector3(0, 2.9f, 0), 3);

            // move it to correct place
            deck.MoveCardToPlayersDeckClientRpc(client, localPos, 1.15f);
            
        }
    }
    [ServerRpc]
    public void TowerCorrectAnswerHandlerServerRpc(ulong playerID, bool endOfGame)
    {
        if (!endOfGame)
        {

            // disable click events for all clients
            deck.DisableClick();

            deck.cardOnDeck.GetComponent<NetworkObject>().Despawn();

            // add score for client
            TowerUpdateScoreBoardServerRpc(playerID);

            Card cardScript = deck.cardOnDeck.GetComponent<Card>();
            deck.SpawnLocalCopyOfCardClientRpc(cardScript.cardDataIndex,cardScript.symbolsIndexes, playerID, new Vector3(0, 2.9f, 0), deck.nextCardLayer);

            deck.nextCardLayer = deck.nextCardLayer + 5;
            deck.SpawnNewCardOnDeckServerRpc(++deck.cardCounter, false);

            deck.MoveCardToPlayersDeckClientRpc(playerID, localPos, 1.15f);

            ani.FlipCardClientRpc(playerID, 0f);
        }
        else
        {
            deck.cardOnDeck.GetComponent<NetworkObject>().Despawn();

            // add score for client
            TowerUpdateScoreBoardServerRpc(playerID);

            Card cardScript = deck.cardOnDeck.GetComponent<Card>();
            deck.SpawnLocalCopyOfCardClientRpc(cardScript.cardDataIndex, cardScript.symbolsIndexes, playerID, new Vector3(0, 2.9f, 0), deck.nextCardLayer);
            ani.FlipCardClientRpc(playerID, 0f);
            deck.MoveCardToPlayersDeckClientRpc(playerID, localPos, 1.15f);


            ani.StartDelayScore(2f, " points", true);
            deck.DisableClick();
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
    public void TowerCardMovedServerRpc(ulong clientId)
    {
        if (deck.cardOnDeck != null && deck.gameStarted)
        {
            deck.cardOnDeck.GetComponent<Card>().FlipCardClientRpc(0.5f);
            StartCoroutine(ani.DelayAnimation(0.5f, deck.EnableClick));
            deck.DestroyCardUnderClientRpc(clientId);
        }
    }

    [ClientRpc]
    private void FlipLocalStartingCardsClientRpc()
    {
        StartCoroutine(ani.DelayAnimation(4f,deck.EnableClick));
        deck.cardLocal.GetComponent<Card>().FlipCard(0.5f);
    }


    [ServerRpc]
    private void FlipServerCardServerRpc()
    {
        if (deck.cardOnDeck != null && deck.gameStarted)
        {
            deck.cardOnDeck.GetComponent<Card>().FlipCardClientRpc(0.5f);
            StartCoroutine(ani.DelayAnimation(0.5f, deck.EnableClick));
        }
    }

    [ServerRpc]
    public void TowerUpdateScoreBoardServerRpc(ulong clientID)
    {
        TowerUpdateScoreBoardClientRpc(clientID);
        UIManager.Instance.RefreshScoreUIClientRpc("Cards taken: ");
    }


    [ClientRpc]
    public void TowerUpdateScoreBoardClientRpc(ulong clientID)
    {
        foreach (PLayerScore ps in UIManager.Instance.scoreBoard)
        {
            if (ps == null) return;
            if (ps.CheckID(clientID))
            {
                ps.IncScore();
            }
        }
    }

}
