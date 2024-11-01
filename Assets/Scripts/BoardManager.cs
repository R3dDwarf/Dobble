using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

using Random = System.Random;

public class BoardManager : NetworkBehaviour
{
    public Sprite[] Sprites;
    public GameObject Card1, Card2, SelectBar;
    private Sprite Answer;
    private GameManager GameManager;


    // Start is called before the first frame update
    void Start()
    {
        GameManager = FindAnyObjectByType<GameManager>();
        NewBoard();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void NewBoard()
    {

            GameManager = FindAnyObjectByType<GameManager>();
            //Find all card components
            SpriteRenderer[] card1Renderer = Card1.GetComponentsInChildren<SpriteRenderer>();
            SpriteRenderer[] card2Renderer = Card2.GetComponentsInChildren<SpriteRenderer>();
            SpriteRenderer[] SelectBarRenderer = SelectBar.GetComponentsInChildren<SpriteRenderer>();

            //Shuffle sprites
            Shuffle(Sprites);
            //Create 2 Sprite arrays
            Sprite[] Card1Sprites = new Sprite[8];
            Sprite[] Card2Sprites = new Sprite[8];
            Sprite[] SelectBarSprites = new Sprite[8];

            //Fill them with random Sprites and manage that they will share 1 same Sprite
            int i = 0;
            Answer = Sprites[7];
            for (i = 0; i < 8; i++)
            {
                Card1Sprites[i] = Sprites[i];
                Card2Sprites[i] = Sprites[i + 7];
            }

            Sprite result = Sprites[7];
            SelectBarSprites[0] = result;
            Shuffle(Sprites);

            i = 1;
            int index = 0;
            while (i < 8)
            {
                if (Sprites[index] != result && (Card1Sprites.Contains(Sprites[index]) || Card2Sprites.Contains(Sprites[index])))
                {
                    SelectBarSprites[i] = Sprites[index];
                    i++;
                }
                index++;
            }
            //Shuffle them one more time
            Shuffle(Card1Sprites);
            Shuffle(Card2Sprites);
            Shuffle(SelectBarSprites);


            //Fill the Cards with random Sprites
            for (i = 2; i < card1Renderer.Length; i++)
            {
                Sprite add = Sprites[i];
                card1Renderer[i].sprite = Card1Sprites[i - 2];
                card1Renderer[i].transform.localScale = Vector3.one;
                card2Renderer[i].sprite = Card2Sprites[i - 2];
                card2Renderer[i].transform.localScale = Vector3.one;
                SelectBarRenderer[i].sprite = SelectBarSprites[i - 2];
                SelectBarRenderer[i].transform.localScale = Vector3.one;

            }
        
    }

    public void IsAnswer(Sprite clicked)
    {
        if (clicked == null) return;
        else if (clicked == Answer)
        {
            UnityEngine.Debug.Log("Correct answer clicked");
            if (GameManager == null)
            {
                UnityEngine.Debug.Log("dfef");
            }
            GameManager.CorrectAswerFound();
        }
        else
        {
            return;
        }
    }




    //Shuffle function for our symbols
    private Random rng = new Random();
    public void Shuffle<T>(IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}