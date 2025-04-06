using DG.Tweening;
using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Card : NetworkBehaviour
{
    public int cardDataIndex;
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

    [SerializeField]
    private Sprite cross;

    [SerializeField]
    private SpriteRenderer crossSR;


    private int sortingOrder;
    private void Start()
    {
        backgroundRenderer.sprite = backSprite;
    }

    [ClientRpc]
    public void ChangeSymbolsClientRpc(CardData symbols, int cardDataIndex)
    {
        ChangeSymbols(symbols, cardDataIndex, 1);
    }

    public void ChangeSymbols(CardData symbols, int cardDataIndex, int sortingOrder)
    {
        this.cardDataIndex = cardDataIndex;
        this.symbolsIndexes = symbols;
        this.sortingOrder = sortingOrder;
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
    public IEnumerator ShowSymbols()
    {
        foreach (SpriteRenderer sp in spriteRenderes) { sp.enabled = true; }
        isFront = true;
        yield return null;
        
    }




    private void AddBoxColliders(int sortingOrder)
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
                        newBoxCol.layerOverridePriority = sortingOrder;

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
        if (isLocal && isFront)
        {
            DeckManager deckManager = FindObjectOfType<DeckManager>();
            if (deckManager)
            {
                Debug.Log("Card clicked");
                deckManager.OnSymbolClickedByPlayerServerRpc(symbol, cardDataIndex, NetworkManager.Singleton.LocalClientId);
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
    [ClientRpc]
    public void FlipCardClientRpc(float duration)
    {
        FlipCard(duration);
    }


    public void FlipCard(float duration)
    {
        StartCoroutine(FlipCardCouroutine(duration));
    }
    public IEnumerator FlipCardCouroutine(float duration)
    {
        yield return StartCoroutine(Flip90Degrees(duration));
        backgroundRenderer.sprite = frontSprite;
        yield return ShowSymbols();
        yield return StartCoroutine(Flip90Degrees(duration));
        AddBoxColliders(sortingOrder);
    }


    [ClientRpc]
    public void FlipBackClientRpc(float duration)
    {
        FlipCardBack(duration);
    }

    public void FlipCardBack(float duration)
    {
        StartCoroutine(FlipBackCardCouroutine(duration));
    }

    public IEnumerator FlipBackCardCouroutine(float duration)
    {
        DisableBoxColliders();
        yield return StartCoroutine(Flip90Degrees(duration));
        backgroundRenderer.sprite = backSprite;
        
        HideSymbols();

        backgroundRenderer.sprite = backSprite;
        yield return StartCoroutine(Flip90Degrees(duration));
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
    public void MoveCardClientRpc(Vector3 targetPosition, float moveDuration, float scale)
    {
        MoveCard(targetPosition, moveDuration, scale);
        Debug.LogWarning("CLientRPc");
    }
    public void MoveCard(Vector3 targetPosition, float moveDuration, float scale)
    {
        StartCoroutine(MoveCardCoroutine(targetPosition, moveDuration, scale));
        Debug.LogWarning("CLientRPc fwef");
    }
    public IEnumerator MoveCardCoroutine(Vector3 targetPosition, float moveDuration, float scale)
    {
        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = startScale * scale;
        float scaleDuration = 0.2f; // Duration for the scale animation
        float elapsedTime = 0;

        // Smoothly scale up
        while (elapsedTime < scaleDuration)
        {
            elapsedTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / scaleDuration);
            yield return null;
        }

        transform.localScale = targetScale; // Ensure it reaches the exact size
        elapsedTime = 0; // Reset for movement

        // Move the card smoothly
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, Mathf.SmoothStep(0, 1, elapsedTime));     
           yield return null;
        }

        transform.position = targetPosition;

        DeckManager.Instance.CardMovedServerRpc(NetworkManager.Singleton.LocalClientId);
        
    }


    public void SpawnCross()
    {

        crossSR.enabled =false;
        crossSR.sortingOrder = sortingOrder + 2;
        isLocal = false;

        crossSR.transform.DOScale(new Vector3(0.3f, 0.3f, 1), 1);
        crossSR.sprite = cross;

        Color color = crossSR.color;
        color.a = 0;
        crossSR.color = color;

        crossSR.enabled = true;
        crossSR.DOFade(1, 1f);

        DespawnCross(2f);
        
    }

    private void DespawnCross(float delay)
    {
        StartCoroutine(DespawnCrossCoroutine(delay));
    }

    private IEnumerator DespawnCrossCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        crossSR.DOFade(0, 0.5f);
        isLocal = true;
        crossSR.transform.DOScale(new Vector3(1f, 1f, 0), 1);
    }







}

