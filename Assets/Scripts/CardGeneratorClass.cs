using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using System.Linq;
using Random = System.Random;
using Unity.Netcode;
using Assets.Scripts;
using System.Threading.Tasks;
class CardGeneratorClass
{
    // Zdroj : #The Dobble Algorithm - www.101computing.net/the-dobble-algorithm/
    public static List<List<int>> GenerateDobbleCards(int numberOfSymbolsOnCard, bool shuffleSymbolsOnCard = false)
    {
        int n = numberOfSymbolsOnCard - 1;
        int numberOfCards = n * n + n + 1;
        List<List<int>> cards = new List<List<int>>();
        Random rand = new Random();

        // Add first set of n+1 cards
        for (int i = 0; i <= n; i++)
        {
            List<int> card = new List<int> { 1 };
            for (int j = 0; j < n; j++)
            {
                card.Add((j + 1) + (i * n) + 1);
            }
            if (shuffleSymbolsOnCard)
                card = card.OrderBy(x => rand.Next()).ToList();
            cards.Add(card);
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                List<int> card = new List<int> { i + 2 };
                for (int k = 0; k < n; k++)
                {
                    int val = (n + 1 + n * k + (i * k + j) % n) + 1;
                    card.Add(val);
                }
                if (shuffleSymbolsOnCard)
                    card = card.OrderBy(x => rand.Next()).ToList();
                cards.Add(card);
            }
        }

        return cards;
    }
}

