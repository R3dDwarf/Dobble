using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

using Assets.Scripts;

using System.Collections;
using System;
using Unity.VisualScripting;
using UnityEditor;
using System.Threading.Tasks;
using UnityEngine.VFX;
using UnityEditor.Experimental.GraphView;
using Unity.Netcode.Components;

public class DeckManager : NetworkBehaviour
{
    public static DeckManager Instance;

    // -------------------------------------------------- PREFABS

    // Card prefab for 8 symbols
    [SerializeField]
    private GameObject cardPrefab8;

    // Card prefab for 7 symbols
    [SerializeField]
    private GameObject cardPrefab7;

    // Card prefab for 6 symbols
    [SerializeField]
    private GameObject cardPrefab6;

    //
    // Multi-player
    //

    // Tower Logic prefab
    [SerializeField]
    private GameObject towerLogicPrefab;

    // Well Logic prefab
    [SerializeField]
    private GameObject wellLogicPrefab;

    [SerializeField]
    private GameObject gottaLogicPrefab;

    //
    // Single-player
    //

    // Spot On Logic prefab;
    [SerializeField]
    private GameObject spotOnLogicPrefab;


    // Animations prefab
    [SerializeField]
    private GameObject animationsPrefab;

    // Card prefab based on chosen symbol count
    public GameObject cardPrefab;

    //-------------------------------------------------- END OF PREFABS

    // Current game mode
    public int gameMode;

    // if the gamemode is multiplayer or not
    public bool isMulti;


    // Symbols on card
    short symbolCount;

    // count of card generated in total;
    public int cardsTotal;


    // Sprites for cards
    public Sprite[] sprites;


    // Storage for all cards in deck
    public NetworkList<CardData> cards = new NetworkList<CardData>();

    // location of deck
    public Vector3 deckPosition = new Vector3(0, 2.9f, 0);

    // card on top of the deck
    public GameObject cardOnDeck;
    private bool disabledClick = true;


    // first card on local screen
    public GameObject cardLocal;
    public GameObject cardUnder;

    // Next card to spawn
    [SerializeField]
    public int cardCounter = 0;

    public int nextCardLayer = 10;

    public bool gameStarted = false;


    private void Awake()
    {
        Debug.LogWarning("Spawing deck");
        base.OnNetworkSpawn();

        if (Instance == null)
        {
            Instance = this;
        }
    }
    public override void OnNetworkSpawn()
    {
        Debug.LogWarning("Spawing deck");
        base.OnNetworkSpawn();

        if (Instance == null)
        {
            Instance = this;
        }   

    }

    public void Start()
    {

    }

    //
    // Fuction called after switching scenes
    //
    [ServerRpc]
    public void GameStartedServerRpc(short symbolCount, int gameModeIndex, bool isMulti)
    {
        GameStarted(symbolCount, gameModeIndex, isMulti);
    }
    
    private async void GameStarted(short symbolCount, int gameModeIndex, bool isMulti)
    {

        GameObject ani = Instantiate(animationsPrefab);
        ani.GetComponent<NetworkObject>().Spawn();

        this.isMulti = isMulti;
        this.gameMode = gameModeIndex;

        // config the game
        GameConfigClientRpc(symbolCount, gameMode, isMulti  );

        // generate card combinations
        await GenerateDeck();



        // Forbid click events
        disabledClick = true;

        if (isMulti) {
            switch (gameModeIndex)
            {
                // multiplayer
                case 0:
                    {
                        GameObject tower = Instantiate(towerLogicPrefab);
                        tower.GetComponent<NetworkObject>().Spawn();

                        if (cards.Count > 0)
                        {
                            TowerLogic.Instance.TowerSpawnCardsServerRpc(symbolCount);
                        }
                        break;
                    }
                case 1:
                    {
                        GameObject well = Instantiate(wellLogicPrefab);
                        well.GetComponent<NetworkObject>().Spawn();

                        if (cards.Count > 0)
                        {
                            WellLogic.Instance.WellSpawnCardsServerRpc(symbolCount);
                            Debug.Log("testing");
                        }
                        break;
                    }
                case 2:
                    {
                        GameObject gotta = Instantiate(gottaLogicPrefab);
                        gotta.GetComponent<NetworkObject>().Spawn();
                        if(cards.Count > 0)
                        {
                            GottaCatchAllLogic.Instance.GottaSpawnCardsServerRpc();
                        }
                        break;
                    }
            }
        }
        else {
            switch (gameModeIndex)
            {
                // single player
                case 0:
                {
                    GameObject spotOn = Instantiate(spotOnLogicPrefab);
                    spotOn.GetComponent<NetworkObject>().Spawn();

                    GameObject tower = Instantiate(towerLogicPrefab);
                    tower.GetComponent<NetworkObject>().Spawn();

                    if (cards.Count > 0)
                    {
                        SpotOn.Instance.SpotOnSpawnCardsServerRpc(symbolCount);
                    }
                    break;
                }
            }
        }
    }

    //
    // Function responsible for generating cards on server side
    //

    private Task GenerateDeck()
    {
        // generates all cards and stores them in server
        List<List<int>> cardsList = CardGeneratorClass.GenerateDobbleCards(symbolCount, true);
        Shuffle.ShuffleFunc(cardsList);


        // card id
        int id = 0;

        // suffle symbols in card and store them in list above
        foreach (var card in cardsList)
        {
            card.ShuffleFunc();
            CardData cardData = new CardData(symbolCount, id++);

            // copy values for data structure of card
            for (int i = 0; i < card.Count; i++)
            {

                cardData.symbols.Add(card[i] - 1);          // -1 because generator indexes symbols from 1 not 0
            }
            cards.Add(cardData);
        }
        Debug.Log("Genrated" + cards.Count + "Cards");
        return Task.CompletedTask;
    }


    //
    // Spawns new network card on the deck
    //
    [ServerRpc]
    public void SpawnNewCardOnDeckServerRpc(int cardDataIndex)
    {
        cardOnDeck = Instantiate(cardPrefab, deckPosition, Quaternion.identity);
        NetworkObject networkObject = cardOnDeck.GetComponent<NetworkObject>();
  

        networkObject.Spawn(true);
        cardOnDeck.GetComponent<Card>().ChangeSymbolsClientRpc(cards[cardDataIndex], cardDataIndex);

    }



    [ServerRpc(RequireOwnership = false)]
    public void OnSymbolClickedByPlayerServerRpc(string spriteName, int cardDataIndex, int cardId, ulong playerID)
    {
        if (disabledClick == true) return;

        Debug.Log("On Sprite Clicked:" + spriteName);

        if (cardOnDeck.GetComponent<Card>().IsSymbolOnCard(spriteName))
        {
            if (isMulti)
            {
                switch (gameMode)
                {
                    case 0:
                        {
                            TowerLogic.Instance.TowerCorrectAnswerHandlerServerRpc(playerID, cardCounter + 1 == cardsTotal);
                            break;
                        }
                    case 1:
                        {
                            WellLogic.Instance.WellCorrectAnswerHandlerServerRpc(playerID, cardDataIndex, cardCounter + 1 == cardsTotal);
                            break;
                        }
                    case 2:
                        {

                            Debug.Log("test2e1e12e32r");
                            GottaCatchAllLogic.Instance.GottaCorrectAnswerHandlerServerRpc(playerID,cardCounter + 1 == cardsTotal, cardId);
                            break;
                        }
                }
            }
            else
            {
                switch (gameMode)
                {
                    case 0:
                        {
                            TowerLogic.Instance.TowerCorrectAnswerHandlerServerRpc(playerID, cardCounter + 1 == cardsTotal);
                            break;
                        }

                }
            }
        }
        else
        {
            if (isMulti)
            {
                switch (gameMode)
                {
                    case 0:
                        {
                            TowerLogic.Instance.OnWrongSymbolClickedClientRpc(playerID);
                            break;
                        }
                    case 1:
                        {
                            WellLogic.Instance.OnWrongSymbolClickedClientRpc(playerID);
                            break;
                        }

                }
            }
            else
            {
                switch (gameMode)
                {
                    case 0:
                        {
                            TowerLogic.Instance.OnWrongSymbolClickedClientRpc(playerID);
                            break;
                        }
                }

            }
        }

        }
    

    [ServerRpc(RequireOwnership = false)]
    public void CardMovedServerRpc(ulong clientId)
    {
        if (!isMulti)
        {
            switch (gameMode)
            {
                case 0:
                    {
                        TowerLogic.Instance.TowerCardMovedServerRpc(clientId);
                        break;
                    }
                case 1:
                    {
                        WellLogic.Instance.WellCardMovedServerRpc();
                        break;
                    }

            }
        }
        else
        {
            switch (gameMode)
            {
                case 0:
                    {
                        TowerLogic.Instance.TowerCardMovedServerRpc(clientId);
                        break;
                    }
            }


            }
    }

    

    [ClientRpc]
    public void SpawnLocalCopyOfCardClientRpc(int cardDataIndex, CardData data, ulong playerID, Vector3 spawnPosition, int cardLayer)
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
        newCardLocal.GetComponent<SpriteRenderer>().sortingOrder = ++nextCardLayer + 3;
        newCardComponent.isLocal = true;
        newCardComponent.ChangeSymbols(data,cardDataIndex, cardLayer);

        cardUnder = cardLocal;
        Debug.Log("assigning card to local slot");
        cardLocal = newCardLocal;
        if (cardUnder != null)
        {
            cardUnder.GetComponent<Card>().isLocal = false;
        }
    }

    [ClientRpc]
    public void MoveCardToPlayersDeckClientRpc(ulong playerID, Vector3 pos, float scale)
    {
        if (NetworkManager.Singleton.LocalClientId != playerID) return;
        cardLocal.GetComponent<Card>().MoveCard(pos, 1f, scale);
        Debug.LogWarning("testing");
    }



    [ClientRpc]
    private void GameConfigClientRpc(short symbolCount, int gameMode, bool isMulti)
    {
        // deck position
        if (isMulti && gameMode == 2)
        {
            deckPosition = new Vector3(0, 0, 0);
        }
        else
        {
            deckPosition = new Vector3(0, 2.9f, 0);
        }

        // assign chosen game mode
        this.gameMode = gameMode;
        this.isMulti = isMulti;

        int n = (symbolCount * symbolCount) - symbolCount + 1;
        cardsTotal = n;

        // assign chosen number of symbols on card
        this.symbolCount = symbolCount;


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

    [ClientRpc]
    public void DestroyCardUnderClientRpc(ulong playerID)
    {
        if (NetworkManager.Singleton.LocalClientId != playerID) { return; }

        // check if the game mode is Tower
        if (gameMode == 0)
        {
            Destroy(cardUnder);
        }
    }

    [ClientRpc]
    public void FlipLocalCardForClientRpc(ulong playerID)
    {
        if (NetworkManager.Singleton.LocalClientId != playerID) { return; }
        Debug.Log("Flipping");
        cardLocal.GetComponent<Card>().FlipCard(0.5f);
    }

    public void DisableClick()
    {
        disabledClick = true;
    }

    public void EnableClick()
    {
        disabledClick = false;
    }
}
