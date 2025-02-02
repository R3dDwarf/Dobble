using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

public class MainManuScript : MonoBehaviour
{
    private UIDocument document;

    // Main Manu Container
    private VisualElement mainMenuContainer;

    // Buttons of Main Menu 
    private Button playBtn;
    private Button settingsBtn;
    private Button quitBtn;

    // Select Mode Menu Container
    private VisualElement selectContainer;

    // Butons of Select Mode Menu
    private Button backToMainBtn;
    private Button joinLobbyBtn;
    private Button createLobbyBtn;
    private Button playVsBotBtn;

    // Join Menu Container
    private VisualElement joinContailer;
    private Button backToSelectMenuBtn;

    // Create Menu Container
    private VisualElement createContainer;

    private Button leftArrowPCBoxBtn;
    private Label TextPCBox;
    private Button rightArrowPCBoxBtn;

    private Button leftArrowGMBoxBtn;
    private Label TextGMBox;
    private Button rightArrowGMBoxBtn;

    private Button instantiateLobbyBtn;

    private short[] playerSize = { 2,3,4 };   // selector for lobby size
    private short indexPC = 0;

    private string[] gameModes = { "basic" };
    private short indexGM = 0;
    // index of current size


    private void Awake()
    {
        document = GetComponent<UIDocument>();
        // assign all containers
        mainMenuContainer = document.rootVisualElement.Q("MainMenuBox");
        selectContainer = document.rootVisualElement.Q("SelectModeBox");
        joinContailer = document.rootVisualElement.Q("JoinMenuBox");
        createContainer = document.rootVisualElement.Q("CreateMenuBox");

        // assign default lobby size
        TextPCBox = document.rootVisualElement.Q("TextPCBox") as Label;
        TextPCBox.text = playerSize[indexPC].ToString();

        // assign default game mode
        TextGMBox = document.rootVisualElement.Q("TextGMBox") as Label;
        TextGMBox.text = gameModes[indexPC];
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


        settingsBtn = AssignButton("SettingsBtn");
        quitBtn = AssignButton("QuitBtn");

        // Select Mode Box Buttons

        backToMainBtn = AssignButton("BackToMainMenuBtn");
        if (backToMainBtn != null) backToMainBtn.RegisterCallback<ClickEvent>(evt => OnBackToMainBtnClicked());

        joinLobbyBtn = AssignButton("JoinLobbyBtn");
        if (joinLobbyBtn != null) joinLobbyBtn.RegisterCallback<ClickEvent>(evt => OnJoinOptionBtnClicked());

        createLobbyBtn = AssignButton("CreateLobbyBtn");
        if(createLobbyBtn != null) createLobbyBtn.RegisterCallback<ClickEvent>(evt => OnCreateLobbyOptionBtnClicked());
        playVsBotBtn = AssignButton("PlayBsBotBtn");

        // Join Menu Box Buttons
        backToSelectMenuBtn = AssignButton("BackToSelectMenuBtn");
        if (backToSelectMenuBtn != null) backToSelectMenuBtn.RegisterCallback<ClickEvent>(evt => OnBackToSelectMenuClicked());



        // Create Menu Box Buttons - Player Count

        leftArrowPCBoxBtn = AssignButton("LeftArrowPCBoxBtn");
        if (leftArrowPCBoxBtn != null) leftArrowPCBoxBtn.RegisterCallback<ClickEvent>(evt => OnLeftArrowPCClicked());

        rightArrowPCBoxBtn = AssignButton("RightArrowPCBoxBtn");
        if (rightArrowPCBoxBtn != null) rightArrowPCBoxBtn.RegisterCallback<ClickEvent>(evt => OnRightArrowPCClicked());

        // Create Menu Box Buttons - Game Mode

        leftArrowGMBoxBtn = AssignButton("LeftArrowGMBoxBtn");
        if (leftArrowGMBoxBtn != null) leftArrowGMBoxBtn.RegisterCallback<ClickEvent>(evt => OnLeftArrowGMClicked());

        rightArrowGMBoxBtn = AssignButton("RightArrowGMBoxBtn");
        if (rightArrowGMBoxBtn != null) rightArrowGMBoxBtn.RegisterCallback<ClickEvent>(evt => OnRightArrowGMClicked());

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

    //------------------------------------ Onclick event fuctions

    // Main Menu
    private void OnPlayBtnClicked()
    {
        Debug.Log("Play button clicked!");
        mainMenuContainer.style.display = DisplayStyle.None;
        selectContainer.style.display = DisplayStyle.Flex;  
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
        joinContailer.style.display = DisplayStyle.Flex;  
    }
    private void OnCreateLobbyOptionBtnClicked()
    {
        Debug.Log("Create Lobby Menu button clicked!");
        selectContainer.style.display = DisplayStyle.None;
        createContainer.style.display = DisplayStyle.Flex;
    }


    // Join Menu

    private void OnBackToSelectMenuClicked()
    {
        Debug.Log("Back to Select Menu button clicked!");
        joinContailer.style.display= DisplayStyle.None;
        selectContainer.style.display = DisplayStyle.Flex;
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









}
