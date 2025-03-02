
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;

using UnityEngine;
using UnityEngine.UIElements;
using static Unity.Collections.AllocatorManager;
public class MainMenuEvents : MonoBehaviour
{

    public static  MainMenuEvents Instance;
    

    private UIDocument document;


    // Main Manu Container
    private VisualElement mainMenuContainer;

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
    private Button playVsBotBtn;

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


    // Lobby Container
    private VisualElement lobbyContainer;
    private ListView playerList;
    [SerializeField]
    private VisualTreeAsset listItemTemplate;

    private Button backToCreateLobbyBtn;
    private Button startGameBtn;

    private Label joinCodeLabel;

    //--------------------------------------------- LOGIC

    private short[] playerSize = { 2, 3, 4 };   // selector for lobby size
    private short indexPC = 0;

    private string[] gameModes = { "Tower" };
    private short indexGM = 0;


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
        lobbyContainer = document.rootVisualElement.Q("LobbyBox");

        // assign player name Input
        playerName = document.rootVisualElement.Q("PlayerNameInput") as TextField;

        // assign join code Label
        joinCodeInput = document.rootVisualElement.Q("JoinCodeInput") as TextField;


        // assign default lobby size
        TextPCBox = document.rootVisualElement.Q("TextPCBox") as Label;
        TextPCBox.text = playerSize[indexPC].ToString();

        // assign default game mode
        TextGMBox = document.rootVisualElement.Q("TextGMBox") as Label;
        TextGMBox.text = gameModes[indexPC];
        //assign player ListView
        playerList = document.rootVisualElement.Q("PlayerListView") as ListView;

        //assign join code Label
        joinCodeLabel = document.rootVisualElement.Q("JoinCodeLabel") as Label;

        InitButtons();

    }

    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnDisable()
    {
        
    }




    // Initialize all buttons from Main Manu
    private void InitButtons()
    {
        // Main Manu Box buttons
        playBtn = AssignButton("PlayBtn");
        if (playBtn != null) playBtn.RegisterCallback<ClickEvent>(evt => OnPlayBtnClicked());

        // Select Mode Box Buttons

        backToMainBtn = AssignButton("BackToMainMenuBtn");
        if (backToMainBtn != null) backToMainBtn.RegisterCallback<ClickEvent>(evt => OnBackToMainBtnClicked());

        joinLobbyOptionBtn = AssignButton("JoinLobbyBtn");
        if (    joinLobbyOptionBtn != null) joinLobbyOptionBtn.RegisterCallback<ClickEvent>(evt => OnJoinOptionBtnClicked());

        createLobbyOptionbBtn = AssignButton("CreateLobbyBtn");
        if(createLobbyOptionbBtn != null) createLobbyOptionbBtn.RegisterCallback<ClickEvent>(evt => OnCreateLobbyOptionBtnClicked());
        playVsBotBtn = AssignButton("PlayBsBotBtn");

        // Join Menu Box Buttons
        backToSelectMenuBtn = AssignButton("BackToSelectMenuBtn");
        if (backToSelectMenuBtn != null) backToSelectMenuBtn.RegisterCallback<ClickEvent>(evt => OnBackToSelectMenuClicked());

        joinLobbyBtn = AssignButton("JoinBtn");
        if (joinLobbyBtn != null) joinLobbyBtn.RegisterCallback<ClickEvent>(evt => OnJoinLobbyBtnClicked());


        // Create Menu Box Buttons - Player Count

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
        
        instantiateLobbyBtn = AssignButton("InstantiateLobbyBtn");
        if (instantiateLobbyBtn != null)
        {
            instantiateLobbyBtn.RegisterCallback<ClickEvent>(async evt => await OnCreateLobbyBtnClickedAsync());
        }


        // Lobby Box Buttons
        startGameBtn = AssignButton("StartGameBtn");
        if (startGameBtn != null) startGameBtn.RegisterCallback<ClickEvent> (evt => OnStartGameClicked());

        // Lobby Box Buttons

        backToCreateLobbyBtn = AssignButton("BackToCreateLobbyBtn");
        if (backToCreateLobbyBtn != null) backToCreateLobbyBtn.RegisterCallback<ClickEvent> (evt => OnBackToSelectMenuClicked());

        
    }


    // Fuction for assigning button from UI to script
    private Button AssignButton(string name)
    {
        try
        {
            return document.rootVisualElement.Q(name) as Button;
        }
        catch
        {
            Debug.Log($"Couldnt assign {name} Button to its Button variable");
            return null;
        }
    }


    private void SetupPlayerList()
    {
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
            element.style.marginBottom = 50;
        };
        playerList.style.flexDirection = FlexDirection.Column;
        playerList.style.justifyContent = Justify.FlexStart;
        playerList.style.alignItems = Align.Stretch;
        playerList.style.height = 50;
        playerList.itemsSource = players;
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


    // Join Menu

    private void OnBackToSelectMenuClicked()
    {
        Debug.Log("Back to Select Menu button clicked!");
        RelayManager.Instance.CloseRelayLobby();
        joinContainer.style.display= DisplayStyle.None;
        createContainer.style.display = DisplayStyle .None;
        lobbyContainer.style.display = DisplayStyle .None;
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
        if (--indexPC < 0)
        {
            indexPC = 2;
        }
        TextPCBox.text = playerSize[indexPC].ToString();
    }

    private void OnRightArrowPCClicked()            // Right arrow to Player Count
    {
        if (++indexPC > 2)
        {
            indexPC = 0;
        }
        TextPCBox.text = playerSize[indexPC].ToString();
    }

    private void OnLeftArrowGMClicked()             // Left arrow to Game Mode
    {
        if (--indexGM < 0)
        {
            indexGM = 0;
        }
        TextGMBox.text =gameModes[indexGM].ToString();

    }

    private void OnRightArrowGMClicked()            // Right arrow to Game Mode
    {
        if (++indexGM > 0)
        {
            indexGM = 0;
        }
        TextGMBox.text = gameModes[indexGM].ToString();
    }

    private async Task OnCreateLobbyBtnClickedAsync()
    {

        string code = await MultiplayerManager.Instance.CreateLobby((short)(playerSize[indexPC] - 1));
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

    // Lobby Box

    private void OnBackToCreateLobbyBtnClicked() // TODO delete
    {
        lobbyContainer.style.display = DisplayStyle.None;
        RelayManager.Instance.CloseRelayLobby();
        OnCreateLobbyOptionBtnClicked();

    }
    private void OnStartGameClicked()
    {
        MultiplayerManager.Instance.ChangeScene(gameModes[indexGM]);
    }


}
