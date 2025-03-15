using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

using Assets.Scripts;

using System.Collections;
using System;

public class DeckManager : NetworkBehaviour
{
    public static DeckManager Instance;

    // Card prefab for 8 symbols
    [SerializeField]
    private GameObject cardPrefab8;

    // Card prefab for 7 symbols
    [SerializeField]
    private GameObject cardPrefab7;

    // Card prefab for 6 symbols
    [SerializeField]
    private GameObject cardPrefab6;



    // Card prefab based on chosen symbol count
    private GameObject cardPrefab;

    // Current game mode
    string gameMode;

    // Symbols on card
    short symbolCount;


    // Sprites for cards
    public Sprite[] sprites;


    // Storage for all cards in deck
    private NetworkList<CardData> cards = new NetworkList<CardData>();

    // location of deck
    Vector3 deckPosition;


    // card on top of the deck
    private GameObject cardOnDeck;
    private bool disabledClick = true;


    // first card on local screen
    private GameObject cardLocal;
    private GameObject cardUnder;

    // Next card to spawn
    [SerializeField]
    private int cardCounter = 0;

    private int nextCardLayer = 10;

    private bool gameStarted = false;


    private void Awake()
    {
        Instance = this;

    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
    }

    public void Start()
    {
 
    }




    //
    // Fuction called at the start of the game out of Multiplayer Manager
    //
    [ServerRpc]
    public void SpawnCardsServerRpc(short symbolCount, string gameMode)
    {

        // config the game
        GameConfig(symbolCount, gameMode);


        // generates all cards and stores them in server
        List<List<int>> cardsList = CardGeneratorClass.GenerateDobbleCards(symbolCount, true);
        Shuffle.ShuffleFunc(cardsList);


        // suffle symbols in card and store them in list above
        foreach (var card in cardsList)
        {
            card.ShuffleFunc();
            CardData cardData = new CardData(symbolCount);

            // copy values for data structure of card
            for(int i = 0; i< card.Count; i++)
            {

                cardData.symbols.Add(card[i] - 1);          // -1 because generator indexes symbols from 1 not 0
            }
            cards.Add(cardData);
        }

        // Forbid click events
        disabledClick = true;

        // Spawn new Card on deck
        SpawnNewCardOnDeckServerRpc();

        // Spawn local cards for each client
        SpawnLocalStartingCardsServerRpc();

        // Delay all animations due to start of the game and the count down
        StartCoroutine(DelayAnimation(4f, CardMovedServerRpc));
        StartCoroutine(DelayAnimation(4f, EnableClick));
        StartCoroutine(DelayAnimation(4f, FLipCardWithDelay));
    }



    //
    // Spawns local card for each client
    //
    [ServerRpc]
    private void SpawnLocalStartingCardsServerRpc()
    {
        foreach (ulong client in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // spawns local card
            SpawnLocalCopyOfCardClientRpc(cards[cardCounter++], client, new Vector3(0, 2.9f, 0), 3);
            // move it to correct place
            MoveCardToPlayersDeckClientRpc(client);
            
        }
    }

    //
    // Spawns new network card on the deck
    //
    [ServerRpc]
    public void SpawnNewCardOnDeckServerRpc()
    {
        cardOnDeck = Instantiate(cardPrefab, deckPosition, Quaternion.identity);
        NetworkObject networkObject = cardOnDeck.GetComponent<NetworkObject>();
        networkObject.Spawn(true);
        cardOnDeck.GetComponent<Card>().ChangeSymbolsClientRpc(cards[cardCounter++]);

    }

    [ServerRpc(RequireOwnership = false)]
    public void OnSymbolClickedByPlayerServerRpc(string spriteName, ulong cardID, ulong playerID)
    {
        Debug.Log("On Sprite Clicked:" + spriteName);
        if (disabledClick == true) return;

        if (cardOnDeck.GetComponent<Card>().IsSymbolOnCard(spriteName))
        {
            if (cardCounter > 56)
            {
                cardLocal = cardOnDeck;
                disabledClick = true;
                cardOnDeck.GetComponent<NetworkObject>().Despawn();
                MoveCardToPlayersDeckClientRpc(playerID);
                UIManager.Instance.ShowWinnerClientRpc("Test");
                return;
            }

            disabledClick = true;
            cardOnDeck.GetComponent<NetworkObject>().Despawn();

            UIManager.Instance.UpdateScoreBoardServerRpc(playerID);

            SpawnLocalCopyOfCardClientRpc(cardOnDeck.GetComponent<Card>().symbolsIndexes, playerID, new Vector3(0, 2.9f, 0), nextCardLayer);
            nextCardLayer = nextCardLayer + 2;
            SpawnNewCardOnDeckServerRpc(); 
            MoveCardToPlayersDeckClientRpc(playerID);
        }
        else
        {
            OnWrongSymbolClickedClientRpc(playerID);
        }
    
    }

    [ServerRpc(RequireOwnership =false)]
    public void CardMovedServerRpc()
    {
        if (cardOnDeck != null && gameStarted)
        {
            cardOnDeck.GetComponent<Card>().FlipCardClientRpc(0.5f);
            StartCoroutine(DelayAnimation(0.5f, EnableClick));
            Destroy(cardUnder);
        }
    }

    [ClientRpc]
    private void SpawnLocalCopyOfCardClientRpc(CardData cardData, ulong playerID, Vector3 spawnPosition, int cardLayer)
    {
        if (NetworkManager.Singleton.LocalClientId != playerID) return;


        Debug.Log($"Spawning a local copy of the card for player {playerID}");
        GameObject newCardLocal = Instantiate(cardPrefab, spawnPosition, Quaternion.identity);

        Card newCardComponent = newCardLocal.GetComponent<Card>();
        if (newCardComponent == null)
        {
            Debug.LogError("Spawned card has no Card component!");
            return;
        }
        newCardLocal.GetComponent<SpriteRenderer>().sortingOrder = ++nextCardLayer +3;
        newCardComponent.isLocal = true;
        newCardComponent.ChangeSymbols(cardData, cardLayer);
        newCardComponent.FlipCard(0f);
        
        cardUnder = cardLocal;
        cardLocal = newCardLocal;
        if (cardUnder != null)
        {
            cardUnder.GetComponent<Card>().isLocal = false;
        }
    }

    [ClientRpc]
    private void MoveCardToPlayersDeckClientRpc(ulong playerID)
    {
        if (NetworkManager.Singleton.LocalClientId != playerID) return;
        cardLocal.GetComponent<Card>().MoveCard(new Vector3(0, -2f, 0), 1f);
        Debug.LogWarning("testing");
    }

    [ClientRpc]
    private void OnWrongSymbolClickedClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        Card card = cardLocal.GetComponent<Card>();
        if (card != null)
        {
            card.SpawnCross();
        }
    }


    private void GameConfig(short symbolCount, string gameMode)
    {
        // assign chosen game mode
        this.gameMode = gameMode;

        // assign chosen number of symbols on card
        this.symbolCount = symbolCount;

        // configuration of the Game
        if (string.Compare(gameMode, "Tower") == 0)
        {
            deckPosition = new Vector3(0, 2.9f, 0);
        }

        switch (symbolCount)
        {
            case 6:
                {
                    cardPrefab = cardPrefab6;
                    break;
                }
            case 7:
                {
                    cardPrefab = cardPrefab7;
                    break;
                }
            case 8:
                {
                    cardPrefab = cardPrefab8;
                    break;
                }
        }

    }

    private IEnumerator DelayAnimation(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback();
        gameStarted = true;

    }

    private void DisableClick()
    {
        disabledClick = true;
    }

    private void EnableClick()
    {
        disabledClick = false;
    }

    private void FLipCardWithDelay()
    {
        if (cardOnDeck != null)
        {
            cardOnDeck.GetComponent<Card>().FlipCardClientRpc(0.5f);
            StartCoroutine(DelayAnimation(0.5f, EnableClick));
            Destroy(cardUnder);
        }

    }


}
