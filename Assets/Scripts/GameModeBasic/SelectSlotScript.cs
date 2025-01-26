using UnityEngine;
using Unity.Netcode;
using JetBrains.Annotations;

public class SelectSlot : NetworkBehaviour
{

    [SerializeField]
    private GameManager gameManager;

    private int storeCorrectIndex;  

    //Slots for symbols
    [SerializeField]
    private SpriteRenderer[] Slots;
    private void Start()
    {  
       // set correct index on -1 for because its not assigned yet
        gameManager = FindAnyObjectByType<GameManager>();
    }



    [ClientRpc]
    public void AssignSymbolsClientRpc(int[] indexes, int correctIndex)
    {
        for (int i = 0; i < indexes.Length; i++) 
        {
            SpriteRenderer slot = Slots[i];
            SlotScript script = slot.gameObject.GetComponent<SlotScript>();
            if (!script)
            {
                Debug.LogError("Couldnt find box script");
                return;
            }
            
            script.SetSlotClientRpc(indexes[i]);
        }
        storeCorrectIndex = correctIndex;

        Debug.LogWarning(correctIndex);
    }

    public void Clicked(int index)
    {
        Debug.Log($"Calling for refresh after correct asnwer correct index{index} and saved index{storeCorrectIndex}");
        if (index == storeCorrectIndex && gameManager)
        {
            Debug.Log("Calling for refresh after correct asnwer");
            gameManager.CorrectAnswerFoundByServerRpc(storeCorrectIndex);
        }

    }
    


}
