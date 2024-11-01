using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameObject boardPrefab;
    public GameObject currentBoard;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        if (IsHost) // Only the host creates and spawns the board
        {
            CreateNewBoard();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void CreateNewBoard()
    {
        if (currentBoard != null)
        {
            Destroy(currentBoard); 
        }

        currentBoard = Instantiate(boardPrefab, Vector3.zero, Quaternion.identity);

        Debug.Log("New board created!");

        BoardManager boardManager = currentBoard.GetComponent<BoardManager>();
    }

    public void CorrectAswerFound()
    {
        Debug.Log("Creating new board");
        CreateNewBoard();
        
    }

}
