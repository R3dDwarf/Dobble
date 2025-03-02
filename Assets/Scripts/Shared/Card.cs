using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Card : NetworkBehaviour
{

    public CardData symbolsIndexes = new CardData();


    public bool isLocal = false;
    private bool isFront= false;

    // Card prefab for front of the card
    [SerializeField]
    private GameObject cardPrefab;

    // Sprite renderer for the backside of the card
    [SerializeField]
    private SpriteRenderer backgroundRenderer;

    // Sprite for the backside of the card
    [SerializeField]
    private Sprite backSprite;

    [SerializeField]
    private Sprite frontSprite;

    [SerializeField]
    private SpriteRenderer[] spriteRenderes;

    [SerializeField]
    private SlotScript slotScript;
    private void Start()
    {
        backgroundRenderer.sprite = backSprite;
    }

    [ClientRpc]
    public void ChangeSymbolsClientRpc(CardData symbols)
    {
        ChangeSymbols(symbols, 1);
    }

    public void ChangeSymbols(CardData symbols, int sortingOrder)
    {
        this.symbolsIndexes = symbols;
        GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
        UpdateRenders(sortingOrder);
        HideSymbols();

    }

    public void UpdateRenders(int sortingOrder)
    {
        int index = 0;
        foreach (var symbol in this.symbolsIndexes.symbols)
        {
            Debug.Log("Sprites:" + symbol);
        }
        foreach (SpriteRenderer sp in spriteRenderes)
        {
            sp.sprite = DeckManager.Instance.sprites[symbolsIndexes.symbols[index++]];
            sp.transform.localScale *= 0.08f;
            sp.sortingOrder = sortingOrder +1;


            sp.transform.position = new Vector3(
            sp.transform.position.x,
            sp.transform.position.y,
            backgroundRenderer.transform.position.z + 0.1f
            ); 
        }
    }

    public void DisableBoxColliders()
    {
        foreach(Transform child in transform)
        {
            GameObject slot = child.gameObject;
            if (slot.GetComponent<BoxCollider2D>() != null)
            {
                Destroy(slot.GetComponent<BoxCollider2D>());
            }
        }
    }

    public void HideSymbols()
    {
        foreach (SpriteRenderer sp in spriteRenderes) { sp.enabled = false; }
        isFront = false;
    }
    public void ShowSymbols()
    {
        foreach (SpriteRenderer sp in spriteRenderes) { sp.enabled = true; }
        isFront = true;
        
    }


    [ClientRpc]
    public void FlipCardClientRpc(float duration)
    {
        FlipCard(duration);
    }


    private void AddBoxColliders()
    {
        if (isLocal)
        {
            foreach (Transform child in transform)
            {
                child.AddComponent<SlotScript>();
                GameObject slot = child.gameObject;

                if (slot != null)
                {
                    if (slot.GetComponent<BoxCollider2D>() == null)
                    {
                        BoxCollider2D newBoxCol = slot.AddComponent<BoxCollider2D>();

                        SpriteRenderer spriteRenderer = slot.GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null && spriteRenderer.sprite != null)
                        {
                            newBoxCol.size = spriteRenderer.sprite.bounds.size;
                        }

                        newBoxCol.isTrigger = true;
                   
                    }
                }
                Vector3 newPos = child.position;
                newPos.z = -1;
                child.transform.position = newPos;
                Debug.Log("assigning boxCol");
            }
        }
    }



    public void OnSymbolClicked(string symbol)
    {
        Debug.Log("fewfwefwef");
        if (isLocal && isFront)
        {
            DeckManager deckManager = FindObjectOfType<DeckManager>();
            if (deckManager)
            {
                deckManager.OnSymbolClickedByPlayerServerRpc(symbol, NetworkObject.NetworkObjectId, NetworkManager.Singleton.LocalClientId);
            }
        }
        else
        {

        }
    }


    public bool IsSymbolOnCard(string symbol)
    {
        foreach (SpriteRenderer sp in spriteRenderes)
        {
            Debug.LogWarning("'" + symbol + "'");
            Debug.LogWarning("'" + sp.sprite.name + "'");
            if (string.Compare(symbol,sp.sprite.name)==0)
            {
               
                return true;
            }
        }
        return false;
    }


    // Animations

    public void FlipCard(float duration)
    {
        StartCoroutine(FlipCardCouroutine(duration));
    }
    public IEnumerator FlipCardCouroutine(float duration)
    {
        yield return StartCoroutine(Flip90Degrees(duration));
        backgroundRenderer.sprite = frontSprite;
        ShowSymbols();
        yield return StartCoroutine(Flip90Degrees(duration));
        AddBoxColliders();
    }

    private IEnumerator Flip90Degrees(float flipDuration)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + 90, transform.eulerAngles.z);
        float timeElapsed = 0;
        float adjustedDuration = flipDuration + 0.01f;
        while (timeElapsed < adjustedDuration) // Use adjustedDuration directly
        {
            timeElapsed += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, Mathf.SmoothStep(0, 1, timeElapsed / adjustedDuration));
            yield return null;
        }

        transform.rotation = endRotation;
    }

    [ClientRpc]
    public void MoveCardClientRpc(Vector3 targetPosition, float moveDuration)
    {
        MoveCard(targetPosition, moveDuration);
        Debug.LogWarning("CLientRPc");
    }
    public void MoveCard(Vector3 targetPosition, float moveDuration)
    {
        StartCoroutine(MoveCardCoroutine(targetPosition, moveDuration));
        Debug.LogWarning("CLientRPc fwef");
    }
    public IEnumerator MoveCardCoroutine(Vector3 targetPosition, float moveDuration)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, Mathf.SmoothStep(0, 1, elapsedTime));
            yield return null;
        }

        transform.position = targetPosition;
        DeckManager.Instance.CardMovedServerRpc();
    }



}

