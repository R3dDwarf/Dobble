using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class GottaCatchAllLogic : NetworkBehaviour
{

    public static GottaCatchAllLogic Instance { get; private set; }


    DeckManager deck = DeckManager.Instance;
    Animations ani = Animations.Instance;


    // player1 pos
    private Vector3 localPos = new Vector3(-3f, -2f, 0);

    // player2 pos
    private Vector3 player2Pos = new Vector3(3f, -2f, 0);

    // player3 pos
    private Vector3 player3Pos = new Vector3(3f, 2f, 0);

    //player4 pos
    private Vector3 player4Pos = new Vector3(-3f, 2f, 0);


    // stores assigned cards to players currently displayed on screen
    private CardData[] cardsOnDeck;

    [SerializeField]
    private GameObject card1;
    private GameObject card2;
    private GameObject card3;
    private GameObject card4;

    private int cardsFound;


    private void Awake()
    {
        Instance = this;
        deck = DeckManager.Instance;
        ani = Animations.Instance;

    }


    [ServerRpc]
    public void GottaSpawnCardsServerRpc()
    {
        int playerCount = NetworkManager.Singleton.ConnectedClientsIds.Count;


        // check if there is enough cards to start new round
        if (!((deck.cardsTotal - deck.cardCounter) >= playerCount))
        {
            deck.cardLocal = deck.cardOnDeck;
            deck.DisableClick();
            deck.GetComponent<NetworkObject>().Despawn();
            UIManager.Instance.ShowWinnerClientRpc("Test");
            return;
        }

        cardsOnDeck = new CardData[playerCount];
        for (int i = 0; i < playerCount ; i++)
        {
            cardsOnDeck[i] = deck.cards[deck.cardCounter++];
        }

        if (deck.cardOnDeck != null)
        {
            deck.cardOnDeck.GetComponent<NetworkObject>().Despawn();
            Destroy(deck.cardOnDeck);
        }

        GottaShowLocalCardsClientRpc(playerCount,cardsOnDeck);


        deck.SpawnNewCardOnDeckServerRpc(deck.cardCounter++);
       

        deck.cardOnDeck.transform.localScale = new Vector3(0.75f, 0.75f, 1);
        deck.cardOnDeck.GetComponent<Card>().FlipCardClientRpc(0.5f);

        deck.EnableClick();
    }

    [ClientRpc]
    public void GottaShowLocalCardsClientRpc(int playerCount, CardData[] cardsOnDeck)
    {
        if (cardsOnDeck == null)
        {
            Debug.LogWarning("cardsOndeck minus");
        }
        this.cardsOnDeck = cardsOnDeck;
        int index = Random.Range(0, playerCount - 1);


       
        if (playerCount >= 1)
        {
            card1 = GottaSpawnLocalCard(0, localPos);
            ani.FLipLocalardWithDelay(card1, 1.5f);
        }
        if (playerCount >= 2)
        {

            card2 = GottaSpawnLocalCard(1, player2Pos);
            ani.FLipLocalardWithDelay(card2, 1.75f);
        }
        if (playerCount >= 3)
        {

            card3 = GottaSpawnLocalCard(2, player3Pos);
            ani.FLipLocalardWithDelay(card3,2f);
        }
        if (playerCount == 4)
        {

            card4 = GottaSpawnLocalCard(3, player4Pos);
            ani.FLipLocalardWithDelay(card3, 2.25f);
        }

    }

    private GameObject GottaSpawnLocalCard(int index, Vector3 spawnPosition)
    {
        GameObject newCardLocal = Instantiate(deck.cardPrefab, spawnPosition, Quaternion.identity);


        Card newCardComponent = newCardLocal.GetComponent<Card>();

        newCardComponent.isLocal = true;
        newCardComponent.ChangeSymbols(cardsOnDeck[index], index, deck.cardCounter);

        newCardLocal.transform.localScale = new Vector3(0.90f, 0.90f, 1);

        StartCoroutine(ani.CardFadeIn(newCardLocal));
        return newCardLocal;



    }

    [ServerRpc]
    public void GottaCorrectAnswerHandlerServerRpc(ulong playerID, bool endOfGame, int cardId)
    {
        if (!endOfGame)
        {
            deck.DisableClick();
            UIManager.Instance.TowerUpdateScoreBoardServerRpc(playerID);
            FadeAwayCardClientRpc(cardId);

            cardsFound++;

            if (cardsFound == NetworkManager.Singleton.ConnectedClients.Count)
            {
                ani.FlipCardBack(deck.cardOnDeck, 1f);

                // Start the coroutine for delay
                StartCoroutine(DelayedSpawnCards());
                cardsFound = 0;
            }
        }
        else
        {
            deck.cardLocal = deck.cardOnDeck;
            deck.DisableClick();
            deck.GetComponent<NetworkObject>().Despawn();
            deck.MoveCardToPlayersDeckClientRpc(playerID, localPos, 1.15f);
            UIManager.Instance.ShowWinnerClientRpc("Test");
            return;
        }
    }

    // New Coroutine
    private IEnumerator DelayedSpawnCards()
    {
        yield return new WaitForSeconds(3f);
        GottaSpawnCardsServerRpc();
    }


    [ClientRpc]
    public void FadeAwayCardClientRpc(int cardId)
    {
        if (card1 != null)
        {
            if (card1.GetComponent<Card>().GetId() == cardId) { StartCoroutine(ani.CardFadeAway(card1)); return; }
        }

        if (card2 != null)
        {
            if (card2.GetComponent<Card>().GetId() == cardId) { StartCoroutine(ani.CardFadeAway(card2)); return; }
        }

        if (card3 != null) 
        {
            if (card3.GetComponent<Card>().GetId() == cardId) { StartCoroutine(ani.CardFadeAway(card3)); return; }
        }

        if (card4 != null)
        {
            if (card4.GetComponent<Card>().GetId() == cardId) { StartCoroutine(ani.CardFadeAway(card4)); return; }
        }


    }

}
