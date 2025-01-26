using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SlotScript : NetworkBehaviour
{

    // store index of symbol which the slot was assigned
    private int symbolIndex;
    // select slot script
    [SerializeField]
    private SelectSlot selectSlotScript;

    [SerializeField]
    private Sprite[] sprites;


    void Start()
    {
        if (GetComponent<BoxCollider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(2,2);
        }
        // assign script of parent
        selectSlotScript = GetComponentInParent<SelectSlot>();
        if (!selectSlotScript)
        {
            Debug.LogError("Couldnt find SelectSlot Script!");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void OnMouseDown()
    {
        Debug.Log($"Slot with Sprite ID:{symbolIndex} was clicked!");
        selectSlotScript.Clicked(symbolIndex);
    }


    // assign symbol and index to local variables
    [ClientRpc]
    public void SetSlotClientRpc(int index) { 
        symbolIndex = index;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sortingOrder = 1;
        sr.sprite = sprites[index];
        sr.enabled = true;

    }


}
