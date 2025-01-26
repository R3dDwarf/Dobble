using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using JetBrains.Annotations;

public class GameManager : NetworkBehaviour
{
    //final
    public static GameManager Instance { get; private set; }

    // Prefabs
    public GameObject cardPrefab;
    public GameObject selectSlotPrefab;
    // positions for prefabs
    public GameObject card1SpawnPositions;
    public GameObject card2SpawnPositions;
    public Transform selectSlotSpawnPosition;

    private GameObject leftCard;
    private GameObject rightCard;
    private GameObject selectSlot;

    // Sprites
    public Sprite[] allSymbols;
    // indexes of sprites
    private int commonSymbolIndex;
    private int[] cardSymbols1;
    private int[] cardSymbols2;
    private int[] slootSymbols;


    //testing
    [SerializeField] private TMP_Text textMeshPro;
    [SerializeField] private Button button;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    private void Start()
    {
        if (!IsServer)
        {
            return;
        }
        StartRound();
      
    }

    private void StartRound()
    {
        leftCard = Instantiate(cardPrefab, card1SpawnPositions.transform.position, card1SpawnPositions.transform.rotation);
        rightCard = Instantiate(cardPrefab, card2SpawnPositions.transform.position, card2SpawnPositions.transform.rotation);
        selectSlot = Instantiate(selectSlotPrefab, selectSlotSpawnPosition.transform.position, selectSlotSpawnPosition.transform.rotation);


        // Spawn them across the network
        NetworkObject leftCardNetworkObject = leftCard.GetComponent<NetworkObject>();
        NetworkObject rightCardNetworkObject = rightCard.GetComponent<NetworkObject>();
        NetworkObject selectSlotNetworkObject = selectSlot.GetComponent<NetworkObject>();
        leftCardNetworkObject.Spawn();
        rightCardNetworkObject.Spawn();
        selectSlotNetworkObject.Spawn();

        Debug.Log("Starting Round");
        SpawnCards();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnButtonCLickedServerRpc()
    {
    }   
    private void SpawnCards()
    {
        // Spawn the cards on the server and ensure synchronization

        // Assign symbols to the cards
        CardScript leftCardScript = leftCard.GetComponent<CardScript>();
        CardScript rightCardScript = rightCard.GetComponent<CardScript>();
        SelectSlot selectSlotScript = selectSlot.GetComponent<SelectSlot>();  

        if (leftCardScript == null || rightCardScript == null)
        {
            Debug.LogError("One or both cards are missing the CardScript.");
            return;
        }
        // Generate array of randomly placed indexes representing symbols
        int[] indexes = CreateRandomArrayOfIndexes();

        // Assign indexes for cards and select slot
        int[] indexesLeft = indexes.Take(8).ToArray();
        int[] indexesRight = indexes.Skip(7).Take(8).ToArray();
        int[] indexesSelect = indexes.Skip(4).Take(10).ToArray();


        ShuffleArray(indexesLeft);
        ShuffleArray(indexesRight);
        ShuffleArray(indexesSelect);
        
        
        leftCardScript.AssignSymbolsClientRpc(indexesLeft);
        rightCardScript.AssignSymbolsClientRpc(indexesRight);
        selectSlotScript.AssignSymbolsClientRpc(indexesSelect,indexes[7]);
        
    }


    [ServerRpc(RequireOwnership = false)]
    public void CorrectAnswerFoundByServerRpc(int id)
    {
        Debug.Log($"Answer found by{id}!");
        RefreshBoardServerRpc();
    }

    [ServerRpc]
    private void RefreshBoardServerRpc()
    {
        Destroy(leftCard);
        Destroy(rightCard);
        Destroy(selectSlot);

        StartRound();
    }



    ///
    /// Help functions
    ///
    private void ShuffleArray(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            // Swap the elements
            int temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }

    private int[] CreateRandomArrayOfIndexes()
    {
        int[] indexes = new int[this.allSymbols.Length];
        for (int i = 0; i < this.allSymbols.Length; i++)
        {
            indexes[i] = i;
        }
        ShuffleArray(indexes);
        return indexes;
       
    }

}
