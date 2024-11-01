using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField]
    private Canvas lobby;
    [SerializeField]
    private Button hostButton;
    [SerializeField]
    private Button joinButton;
    [SerializeField]
    private Button play;
    [SerializeField]
    private RelayManager relayManager;
    [SerializeField]
    private TMP_InputField TMP_InputField;
    [SerializeField]
    private TMP_Text joinCodeField;


    void Start()
    {
        lobby.enabled = false;
        hostButton.onClick.AddListener(() => {

            lobby.enabled = true;
            OnHostGamePressed();

        });

        joinButton.onClick.AddListener(() =>
        {
            OnJoinGamePressed(TMP_InputField.text.Trim());
        });

        play.onClick.AddListener(() =>
        { ChangeScene("Main"); });
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void Awake()
    {

    }




    public void ChangeScene(string sceneName)
    {
        if (NetworkManager.Singleton.IsServer) 
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

    public async void OnHostGamePressed()
    {
        try
        {
            joinCodeField.text = await relayManager.CreateRelay();
            Debug.Log($"Join code{joinCodeField.text}");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public async void OnJoinGamePressed(string joinCode)
    {
        try
        {
            await relayManager.JoinRelay(joinCode);
            Debug.Log("Joined lobby");
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }

    }
}


