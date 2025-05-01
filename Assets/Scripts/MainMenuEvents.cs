
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.Collections.AllocatorManager;
public class MainMenuEvents : MonoBehaviour
{

    public static  MainMenuEvents Instance;
    

    private UIDocument document;


    // Main Manu Container
    private VisualElement mainMenuContainer;
    
    // Setting Side Buttons
    private Button settingsBtn;
    private Button quitSettingsBtn;
    public Slider sfxSlider;


    // Buttons of Main Menu
    [SerializeField]
    private TextField playerName;
    private Button playBtn;

    // Select Mode Menu Container
    private VisualElement selectContainer;

    // Butons of Select Mode Menu
    private Button backToMainBtn;
    private Button joinLobbyOptionBtn;
    private Button createLobbyOptionbBtn;
    private Button SPModeBtn;

    // Join Menu Container
    private VisualElement joinContainer;
    private Button backToSelectMenuBtn;
    private TextField joinCodeInput;
    private Button joinLobbyBtn;

    // Create Menu Container
    private VisualElement createContainer;

    private Button backToSelectMenuBtn2;

    private Button leftArrowPCBoxBtn;
    private Label TextPCBox;
    private Button rightArrowPCBoxBtn;

    private Button leftArrowGMBoxBtn;
    private Label TextGMBox;
    private Button rightArrowGMBoxBtn;

    private Button instantiateLobbyBtn;

    // Single player Menu Container
    private VisualElement SPContainer;

    private Button SPbacktoSelectMenuBtn;

    private Button SPleftArrowSCBoxBtn;
    private Label SPTextSCBox;
    private Button SPrightArrowSCBoxBtn;

    private Button SPleftArrowGMBoxBtn;
    private Label SPTextGMBox;
    private Button SPrightArrowGMBoxBtn;

    private Button SPPlay;


    // Lobby Container
    private Button LobbyBackToSelectBtn;

    private VisualElement lobbyContainer;
    private ListView playerList;
    [SerializeField]
    private VisualTreeAsset listItemTemplate;

    private Button backToCreateLobbyBtn;
    private Button startGameBtn;

    private Label joinCodeLabel;

    //--------------------------------------------- LOGIC

    private short[] symbolCount = { 6, 7, 8 };   // selector for count of symbols on each card
    private short indexSC = 0;

    private string[] gameModes = { "Tower", "Well", "Catch 'em all"};
    private short indexGM = 0;

    private string[] SPGameModes = { "Spot on!" };
    private short indexSPGM = 0;


    private List<string> players = new List<string>();


    private void Awake()
    {
        Instance = this;
        document = GetComponent<UIDocument>();
        // assign all containers
        mainMenuContainer = document.rootVisualElement.Q("MainMenuBox");
        selectContainer = document.rootVisualElement.Q("SelectModeBox");
        joinContainer = document.rootVisualElement.Q("JoinMenuBox");
        createContainer = document.rootVisualElement.Q("CreateMenuBox");
        SPContainer = document.rootVisualElement.Q("SinglePlayerMenuBox");
        lobbyContainer = document.rootVisualElement.Q("LobbyBox");

        // assign player name Input
        playerName = document.rootVisualElement.Q("PlayerNameInput") as TextField;

        // assign join code Label
        joinCodeInput = document.rootVisualElement.Q("JoinCodeInput") as TextField;


        // assign default symbol count
        TextPCBox = document.rootVisualElement.Q("TextPCBox") as Label;
        TextPCBox.text = symbolCount[indexSC].ToString();

        SPTextSCBox = document.rootVisualElement.Q("SPTextPCBox") as Label;
        SPTextSCBox.text = symbolCount[indexSC].ToString();

        // assign default game mode
        TextGMBox = document.rootVisualElement.Q("TextGMBox") as Label;
        TextGMBox.text = gameModes[indexSC];

        SPTextGMBox = document.rootVisualElement.Q("SPTextGMBox") as Label;
        SPTextGMBox.text = SPGameModes[indexSPGM].ToString();
        //assign player ListView
        playerList = document.rootVisualElement.Q("PlayerListView") as ListView;

        //assign join code Label
        joinCodeLabel = document.rootVisualElement.Q("JoinCodeLabel") as Label;

        // Set up buttons
        InitButtons();


    }

    private void Start()
    {

        //Set up Audio
        SetupAudio();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnDisable()
    {
        
    }

    // Fuction for assigning button from UI to script
    private Button AssignButton(string name)
    {
        try
        {
            Button btn = document.rootVisualElement.Q(name) as Button;
            btn.RegisterCallback<ClickEvent>(evt => AudioManager.instance.PlaySound("ButtonSfx"));
            return btn;
        }
        catch
        {
            Debug.Log($"Couldnt assign {name} Button to its Button variable");
            return null;
        }
    }



    // Initialize all buttons from Main Manu
    private void InitButtons()
    {
        // Settings
        settingsBtn = AssignButton("MenuBtn");
        if (settingsBtn != null)  settingsBtn.RegisterCallback<ClickEvent>(evt => ToggleMenu());

        quitSettingsBtn = AssignButton("ExitGameMenuBtn");
        if (quitSettingsBtn != null) quitSettingsBtn.RegisterCallback<ClickEvent>(evt => ToggleMenu());

        // Main Manu Box buttons
        playBtn = AssignButton("PlayBtn");
        if (playBtn != null) playBtn.RegisterCallback<ClickEvent>(evt => OnPlayBtnClicked());


        // Select Mode Box Buttons
        backToMainBtn = AssignButton("BackToMainMenuBtn");
        if (backToMainBtn != null) backToMainBtn.RegisterCallback<ClickEvent>(evt => OnBackToMainBtnClicked());

        joinLobbyOptionBtn = AssignButton("JoinLobbyBtn");
        if (joinLobbyOptionBtn != null) joinLobbyOptionBtn.RegisterCallback<ClickEvent>(evt => OnJoinOptionBtnClicked());

        createLobbyOptionbBtn = AssignButton("CreateLobbyBtn");
        if(createLobbyOptionbBtn != null) createLobbyOptionbBtn.RegisterCallback<ClickEvent>(evt => OnCreateLobbyOptionBtnClicked());

        SPModeBtn = AssignButton("SinglePlayer");
        if (SPModeBtn != null) SPModeBtn.RegisterCallback<ClickEvent>(evt => OnSPModeOptionBtnClicked());


        // Join Menu Box Buttons
        backToSelectMenuBtn = AssignButton("BackToSelectMenuBtn");
        if (backToSelectMenuBtn != null) backToSelectMenuBtn.RegisterCallback<ClickEvent>(evt => OnBackToSelectMenuClicked());

        joinLobbyBtn = AssignButton("JoinBtn");
        if (joinLobbyBtn != null) joinLobbyBtn.RegisterCallback<ClickEvent>(evt => OnJoinLobbyBtnClicked());


        // Create Menu Box Buttons - Symbol Count

        leftArrowPCBoxBtn = AssignButton("LeftArrowPCBoxBtn");
        if (leftArrowPCBoxBtn != null) leftArrowPCBoxBtn.RegisterCallback<ClickEvent>(evt => OnLeftArrowPCClicked());

        rightArrowPCBoxBtn = AssignButton("RightArrowPCBoxBtn");
        if (rightArrowPCBoxBtn != null) rightArrowPCBoxBtn.RegisterCallback<ClickEvent>(evt => OnRightArrowPCClicked());

        // Create Menu Box Buttons
        backToSelectMenuBtn2 = AssignButton("BackToSelectMenuBtn2");
        if (backToSelectMenuBtn2 != null) backToSelectMenuBtn2.RegisterCallback<ClickEvent>(evt => OnBackToSelectMenuClicked());


        // Create Menu Box Buttons - Game Mode

        leftArrowGMBoxBtn = AssignButton("LeftArrowGMBoxBtn");
        if (leftArrowGMBoxBtn != null) leftArrowGMBoxBtn.RegisterCallback<ClickEvent>(evt => OnLeftArrowGMClicked());

        rightArrowGMBoxBtn = AssignButton("RightArrowGMBoxBtn");
        if (rightArrowGMBoxBtn != null) rightArrowGMBoxBtn.RegisterCallback<ClickEvent>(evt => OnRightArrowGMClicked());

        // Create Menu Box Buttons - Create Lobby
        LobbyBackToSelectBtn = AssignButton("LobbyBackToSelectMenu");
        if (LobbyBackToSelectBtn != null) LobbyBackToSelectBtn.RegisterCallback<ClickEvent>(evt => OnBackToSelectMenuBtnClicked());

        instantiateLobbyBtn = AssignButton("InstantiateLobbyBtn");
        if (instantiateLobbyBtn != null)
        {
            instantiateLobbyBtn.RegisterCallback<ClickEvent>(async evt => await OnCreateLobbyBtnClickedAsync());
        }



        // Single player select box

        SPbacktoSelectMenuBtn = AssignButton("SPBackToSelectMenuBtn2");
        if (SPbacktoSelectMenuBtn != null) SPbacktoSelectMenuBtn.RegisterCallback<ClickEvent>(evt => OnBackToSelectMenuClicked());
        SPleftArrowSCBoxBtn = AssignButton("SPLeftArrowSCBoxBtn");
        if (SPleftArrowGMBoxBtn != null) SPleftArrowGMBoxBtn.RegisterCallback<ClickEvent>(evt => OnLeftArrowSPGMClicked());

        SPrightArrowSCBoxBtn = AssignButton("SPRightArrowSCBoxBtn");
        if (SPrightArrowGMBoxBtn != null) SPrightArrowSCBoxBtn.RegisterCallback<ClickEvent>(evt => OnRightArrowSPGMClicked());

        SPleftArrowGMBoxBtn = AssignButton("SPLeftArrowPCBoxBtn");
        if (SPleftArrowGMBoxBtn != null) SPleftArrowGMBoxBtn.RegisterCallback<ClickEvent>(evt => OnLeftArrowSPSCClicked());

        SPrightArrowGMBoxBtn = AssignButton("SPRightArrowPCBoxBtn");
        if (SPrightArrowGMBoxBtn != null) SPrightArrowGMBoxBtn.RegisterCallback<ClickEvent>(evt => OnRightArrowSPSCClicked());

        SPPlay = AssignButton("SPPlay");
        if (SPPlay != null) SPPlay.RegisterCallback<ClickEvent>(async evt => await OnStartTrainingBtnClicked());

        // Lobby Box Buttons
        startGameBtn = AssignButton("StartGameBtn");
        if (startGameBtn != null) startGameBtn.RegisterCallback<ClickEvent> (evt => OnStartGameClicked());

        // Lobby Box Buttons

        backToCreateLobbyBtn = AssignButton("BackToCreateLobbyBtn");
        if (backToCreateLobbyBtn != null) backToCreateLobbyBtn.RegisterCallback<ClickEvent> (evt => OnBackToSelectMenuClicked());

        
    }


    public void LoadSceneAfterDisctFromLobby()
    {
        mainMenuContainer.style.display = DisplayStyle.None;
        selectContainer.style.display = DisplayStyle.Flex;
        joinContainer.style.display = DisplayStyle.None;
        lobbyContainer.style.display = DisplayStyle.None;
        createContainer.style.display = DisplayStyle.None;
        SPContainer.style.display = DisplayStyle.None;
    }
    private void SetupAudio()
    {
 
        sfxSlider = document.rootVisualElement.Q("SfxSlider") as Slider;

        sfxSlider.SetValueWithoutNotify(AudioManager.instance.sfxSource.volume * 100);

        sfxSlider.RegisterValueChangedCallback(evt =>
        {
            AudioManager.instance.sfxSource.volume = evt.newValue / 100;
        });

    }




    private void SetupPlayerList()
    {
        playerList.itemsSource = players;
        playerList.style.flexDirection = FlexDirection.Column;
        playerList.style.justifyContent = Justify.FlexStart;
        playerList.fixedItemHeight = 80;
        playerList.style.height = 340;

        playerList.makeItem = () =>
        {
            return listItemTemplate.CloneTree();
        };

        playerList.bindItem = (element, index) =>
        {
            Label label = element.Q<Label>("PlayerNameLabel");
            if (label != null)
            {
                label.text = players[index];
            }

            Button kickButton = element.Q<Button>("KickPlayer");
            if (kickButton != null)
            {
                if (NetworkManager.Singleton.IsServer && index != 0)
                {
                    kickButton.style.visibility = Visibility.Visible;
                    kickButton.clicked += () => OnKickPlayerBtnClicked(index);
                }
                else
                {
                    kickButton.style.visibility = Visibility.Hidden;
                }
            }
            else
            {
                Debug.LogError("KickPlayer button not found at index " + index);
            }

            element.style.marginBottom = 10;
        };

        playerList.RefreshItems();
        playerList.Rebuild();
    }




    public void UpdatePlayerList(NetworkList<FixedString32Bytes> playersNO)
    {
        players.Clear();

        foreach (var player in playersNO)
        {
            players.Add(player.ToString());
        }

        playerList.Rebuild();
    }


    public string GetPlayerName()
    {
        return playerName.text;
    }



    //------------------------------------ Onclick event fuctions

    // Settings 

    private void ToggleMenu()
    {
        VisualElement visEl = document.rootVisualElement.Q("MenuBox");
        if (visEl != null)
        {
            if (visEl.style.display == DisplayStyle.Flex)
            {
                visEl.style.display = DisplayStyle.None;
            }
            else
            {
                visEl.style.display = DisplayStyle.Flex;
            }
        }
    }

    // Main Menu
    private void OnPlayBtnClicked()
    {
        Debug.Log("Play button clicked!");
        if (!string.IsNullOrEmpty(playerName.text))
        {   
            mainMenuContainer.style.display = DisplayStyle.None;
            selectContainer.style.display = DisplayStyle.Flex;
        }
    }

    // Select Mode Menu
    
    private void OnBackToMainBtnClicked()
    {
        Debug.Log("Back to Main Menu button clicked!");
        mainMenuContainer.style.display = DisplayStyle.Flex;
        selectContainer.style.display = DisplayStyle.None;
    }
    private void OnJoinOptionBtnClicked()
    {
        Debug.Log("Join Menu button clicked!");
        selectContainer.style.display = DisplayStyle.None;
        joinContainer.style.display = DisplayStyle.Flex;  
    }
    private void OnCreateLobbyOptionBtnClicked()
    {
        selectContainer.style.display = DisplayStyle.None;
        createContainer.style.display = DisplayStyle.Flex;
    }
    
    private void OnSPModeOptionBtnClicked()
    {
        selectContainer.style.display = DisplayStyle.None;
        SPContainer.style.display = DisplayStyle.Flex;
    }

    // Join Menu

    private void OnBackToSelectMenuClicked()
    {
        Debug.Log("Back to Select Menu button clicked!");
        RelayManager.Instance.CloseRelayLobby();
        joinContainer.style.display= DisplayStyle.None;
        createContainer.style.display = DisplayStyle .None;
        lobbyContainer.style.display = DisplayStyle .None;
        SPContainer.style.display = DisplayStyle.None;
        selectContainer.style.display = DisplayStyle.Flex;
    }

    private async void OnJoinLobbyBtnClicked()
    {
        {
            if (string.IsNullOrWhiteSpace(joinCodeInput.text))
            {
                Debug.Log("Join code is empty!");
                return;
            }

            bool success = await MultiplayerManager.Instance.JoinLobby(joinCodeInput.text);

            if (success)
            {
                Debug.Log("Lobby joined successfully!");
                SetupPlayerList();
                joinContainer.style.display = DisplayStyle.None;
                lobbyContainer.style.display = DisplayStyle.Flex;
            }
            else
            {
                //TODO
            }
        }

    }


    // Create Lobby Menu

    private void OnLeftArrowPCClicked()             // left arrow for Player Count
    {
        if (--indexSC < 0)
        {
            indexSC = 2;
        }
        TextPCBox.text = symbolCount[indexSC].ToString();
    }

    private void OnRightArrowPCClicked()            // Right arrow to Player Count
    {
        if (++indexSC > 2)
        {
            indexSC = 0;
        }
        TextPCBox.text = symbolCount[indexSC].ToString();
    }

    private void OnLeftArrowGMClicked()             // Left arrow to Game Mode
    {
        if (--indexGM < 0)
        {
            indexGM = 2;
        }
        TextGMBox.text =gameModes[indexGM].ToString();

    }

    private void OnRightArrowGMClicked()            // Right arrow to Game Mode
    {
        if (++indexGM > 2)
        {
            indexGM = 0;
        }
        TextGMBox.text = gameModes[indexGM].ToString();
    }

    private async Task OnCreateLobbyBtnClickedAsync()
    {

        string code = await MultiplayerManager.Instance.CreateLobby(symbolCount[indexSC], true,indexGM);
        joinCodeLabel.text = code; 

        //UI
        SetupPlayerList();
        createContainer.style.display = DisplayStyle.None;
        lobbyContainer.style.display = DisplayStyle.Flex;

        Debug.Log("Lobby succesfully created!");

        if (!string.IsNullOrEmpty(code))
        {
            Debug.Log($"Lobby created! Join Code: {code}");
        }
        else
        {
            Debug.LogError("Failed to create lobby.");
        }
    }

    private void OnKickPlayerBtnClicked(int playerIndex)
    {
        Debug.Log("trying to kick");
        MultiplayerManager.Instance.KickPlayer(playerIndex);
    }

    // Single player Box
    private void OnLeftArrowSPSCClicked()             // left arrow for Symbol Counts
    {
        if (--indexSC < 0)
        {
            indexSC = 2;
        }
        SPTextSCBox.text = symbolCount[indexSC].ToString();
    }

    private void OnRightArrowSPSCClicked()            // Right arrow to Symbol Count
    {
        if (++indexSC > 2)
        {
            indexSC = 0;
        }
        SPTextSCBox.text = symbolCount[indexSC].ToString();
    }

    private void OnLeftArrowSPGMClicked()             // Left arrow to Game Mode
    {
        if (--indexSPGM < 0)
        {
            indexSPGM = 0;
        }
        SPTextGMBox.text = SPGameModes[indexSPGM].ToString();

    }

    private void OnRightArrowSPGMClicked()            // Right arrow to Game Mode
    {
        if (++indexSPGM > 1)
        {
            indexSPGM = 0;
        }
        SPTextGMBox.text = SPGameModes[indexSPGM].ToString();
    }

    private async Task OnStartTrainingBtnClicked()
    {
         await MultiplayerManager.Instance.CreateLobby(symbolCount[indexSC], false, indexSPGM);   

        Debug.Log("Lobby succesfully created!");

        MultiplayerManager.Instance.ChangeScene("Game");
    }


    // Lobby Box
    private void OnBackToSelectMenuBtnClicked()
    {
        NetworkManager.Singleton.Shutdown();
        lobbyContainer.style.display = DisplayStyle.None;
        selectContainer.style.display = DisplayStyle.Flex;
    }

    private void OnStartGameClicked()
    {
        MultiplayerManager.Instance.ChangeScene("Game");
    }


}
