using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

using Assets.Scripts;

using System.Collections;

public class DeckManager : NetworkBehaviour
{
    public static DeckManager Instance;

    // Card prefab
    [SerializeField] private GameObject cardPrefab;


    // Sprites for cards
    public Sprite[] sprites;


    // Storage for all cards in deck
    private NetworkList<CardData> cards = new NetworkList<CardData>();


    // card on top of the deck
    private GameObject cardOnDeck;
    private bool disabledClick = true;


    // first card on local screen
    private GameObject cardLocal;
    // Next card to spawn
    private int cardCounter = 0;

    private int nextCardLayer = 10;


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

        if (!IsServer) return; // if not server, return 

        List<List<int>> cardsList = CardGeneratorClass.GenerateDobbleCards(8, false); // generate all possible cards for 8 symbols on each

        cardsList.ShuffleFunc();
        foreach (var card in cardsList)
        {
            card.ShuffleFunc();
            CardData cardData = new CardData(card[0] - 1, card[1] - 1, card[2] - 1, card[3] - 1, card[4] - 1, card[5] - 1, card[6] - 1, card[7] - 1);
            cards.Add(cardData);
        }
        SpawnCardsServerRpc();
       
    }

    [ServerRpc]
    public void SpawnCardsServerRpc()
    {
        SpawnNewCardOnDeckServerRpc();
        SpawnLocalStartingCardsServerRpc();
        disabledClick = false;
        StartCoroutine(StartCountDown(4f));
    }

    [ServerRpc]
    private void SpawnLocalStartingCardsServerRpc()
    {
        foreach (ulong client in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnLocalCopyOfCardClientRpc(cards[cardCounter++], client, new Vector3(0, -2f, 0), 1);
        }
    }
    [ServerRpc]
    public void SpawnNewCardOnDeckServerRpc()
    {
        if (!IsServer) return; // Ensure only the server runs this

        if(cards.Count <= cardCounter) return;

        Vector3 spawnPosition = new Vector3(0, 2.9f, 0); // Adjust position logic
        cardOnDeck = Instantiate(cardPrefab, spawnPosition, Quaternion.identity);
        NetworkObject networkObject = cardOnDeck.GetComponent<NetworkObject>();
        networkObject.Spawn(true);
        cardOnDeck.GetComponent<Card>().ChangeSymbolsClientRpc(cards[cardCounter++]);
        disabledClick = false;

    }

    [ServerRpc(RequireOwnership = false)]
    public void OnSymbolClickedByPlayerServerRpc(string spriteName, ulong cardID, ulong playerID)
    {
        Debug.Log("On Sprite Clicked:" + spriteName);
        if (disabledClick == true) return;

        if (cardOnDeck.GetComponent<Card>().IsSymbolOnCard(spriteName))
        {
            disabledClick = true;
            cardOnDeck.GetComponent<NetworkObject>().Despawn();

            SpawnLocalCopyOfCardClientRpc(cardOnDeck.GetComponent<Card>().symbolsIndexes, playerID, new Vector3(0, 2.9f, 0), nextCardLayer);
            SpawnNewCardOnDeckServerRpc();
            MoveCardToPlayersDeckClientRpc(playerID);
        }
    
    }

    [ServerRpc(RequireOwnership =false)]
    public void CardMovedServerRpc()
    {
        cardOnDeck.GetComponent<Card>().FlipCardClientRpc(0.5f);
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
        newCardComponent.isLocal = true;
        newCardComponent.ChangeSymbols(cardData, cardLayer);
        newCardComponent.FlipCard(0f);
        
        if (cardLocal)
        {
            Destroy(cardLocal);
            Debug.Log("Destroying Card");
        }
        cardLocal = newCardLocal;
    }

    [ClientRpc]
    private void MoveCardToPlayersDeckClientRpc(ulong playerID)
    {


        Debug.LogWarning("test");
        cardLocal.GetComponent<Card>().MoveCard(new Vector3(0, -2f, 0), 1f);
    }

    private IEnumerator StartCountDown(float delay)
    {
        yield return new WaitForSeconds(delay);
        CardMovedServerRpc();
    }


}
