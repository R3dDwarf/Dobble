using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectSlot : MonoBehaviour
{
    private BoardManager boardManager;
    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    // Update is called once per frame
    void Update()
    {

    }


void OnMouseDown()
    {
        // Check if the left mouse button was clicked
        if (Input.GetMouseButtonDown(0)) // 0 is for the left mouse button
        {
            Debug.Log("Sprite clicked with OnMouseDown: " + gameObject.name);

            if (boardManager != null)
            {
                boardManager.IsAnswer(spriteRenderer.sprite);
            }
            else
            {
                Debug.LogError("BoardManager is null!");
            }
        }
    }
}
