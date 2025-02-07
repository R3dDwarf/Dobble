using System;

using TMPro;
using Unity.Netcode;

using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.UIElements;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private RelayManager relayManager;
    [SerializeField]
    private TMP_InputField TMP_InputField;
    [SerializeField]
    private TMP_Text joinCodeField;


    void Start()
    {
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

    public async void HostGame(Label joinCodeLabel, short count)
    {
        try
        {
            var joinCode = await relayManager.CreateRelay(count);
            Debug.Log($"Join code{joinCode}");    
            joinCodeLabel.text = joinCode;
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


