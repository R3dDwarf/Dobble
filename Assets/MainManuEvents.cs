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

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        // assign all containers
        mainMenuContainer = document.rootVisualElement.Q("MainMenuBox");
        selectContainer = document.rootVisualElement.Q("SelectModeBox");
        joinContailer = document.rootVisualElement.Q("JoinMenuBox");
        createContainer = document.rootVisualElement.Q("CreateMenuBox");
        // assign all buttons
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
        playVsBotBtn = AssignButton("PlayBsBotBtn");

        // Join Menu Box Buttons
        backToSelectMenuBtn = AssignButton("BackToSelectMenuBtn");
        if (backToSelectMenuBtn != null) backToSelectMenuBtn.RegisterCallback<ClickEvent>(evt => OnBackToSelectMenuClicked());  



        // Create Menu Box Buttons
        
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



    // Join Menu

    private void OnBackToSelectMenuClicked()
    {
        Debug.Log("Back to Select Menu button clicked!");
        joinContailer.style.display= DisplayStyle.None;
        selectContainer.style.display = DisplayStyle.Flex;
    }








}
