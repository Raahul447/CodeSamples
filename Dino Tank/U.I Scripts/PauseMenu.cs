/*  
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
    ╔═════════════════════════════╡  DinoTank  ╞══════════════════════════════════════════════════════╗            
    ║ Authors:  Rahul Yerramneedi                       Email:    yr020409@gmail.com                  ║
    ╟─────────────────────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Purpose: This script is used to access the pause menu, which can be activated by pressing the   ║
    ║          "Esc" key. It also has other functions that for different buttons in the pause menu    ║
    ║          like the "Restart Mission", "Level Select", "Main Menu" buttons and can access several ║
    ║          events as well.                                                                        ║
    ╟─────────────────────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Usage: Add the script to the Pause Menu prefab and make sure all the correct components are     ║
    ║        present.                                                                                 ║
    ╚═════════════════════════════════════════════════════════════════════════════════════════════════╝
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
*/

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class PauseMenu : SystemBase
{
    [Header("UI Objects")] // most of the GameObjects that are present in the pause menu
    public GameObject uiComponents;
    public GameObject loadingOverlay;
    public GameObject winScreen;
    public GameObject restartMissionPrompt;
    public GameObject leaveMissionPrompt;
    public GameObject backButton;
    public GameObject restartMissionButtonRef;
    public GameObject leaveMissionButtonRef;

    [Header("Buttons")] // buttons on the pause menu
    public Button cheatXP;
    public Button cheatAmber;
    public Button cheatMetr;

    [Header("References")] // referneces to other scripts
    public LevelManager levelManagerRef;
    public ExitZone defendModeSettings;

    [Header("Miscellaneous")] // bools or any other objects
    private bool gameWon = false;
    public Canvas myCanvas;

    public override void Start()
    {
        Initialize();
    }

    // Use this for initialization
    public override void Initialize (Action<SystemBase> OnInitialized = null)
    {
        if (!APP.IsReady) // checking whether APP is active or not
        {
            APP.WaitForSubsystem(APP.PlayerTankManager, Start);
            return;
        }

        if (APP.PlayerTankManager.isArcade) // checking if current game mode is arcade mode
        {
            if (APP.PlayerTankManager._ArcadeMode == arcadeMode.defend || APP.PlayerTankManager._ArcadeMode == arcadeMode.SearchAndDestroy)
            {
                defendModeSettings = FindObjectOfType<ExitZone>();
            }
        }

        levelManagerRef = FindObjectOfType<LevelManager>();
        myCanvas = GetComponent<Canvas>();

        myCanvas.enabled = false;
        Time.timeScale = 1;

        if (loadingOverlay)
        {
            loadingOverlay.SetActive(false);
        }

        if (restartMissionPrompt)
        {
            restartMissionPrompt.SetActive(false);
        }

        if (leaveMissionPrompt)
        {
            leaveMissionPrompt.SetActive(false);
        }

        gameWon = false;
        IsInitialized = true;

        if (OnInitialized != null)
        {
            OnInitialized.Invoke(this);
        }
    }

    void OnEnable() // always use this for events 
    {
        EventManager.StartListening("PauseButton", PauseButton, this);
        EventManager.StartListening("ReturnToGame", ReturnToGame, this);
        EventManager.StartListening("LevelObjectiveAchieved", GameWon, this);
        EventManager.StartListening("ShowSettings", HideBackButton, this);
        EventManager.StartListening("HideSettings", ShowBackButton, this);
        DEBUG.Log("Pause Menu Acitvated");
    }
    void OnDisable()  
    {
        EventManager.StopListening("PauseButton", PauseButton);
        EventManager.StopListening("ReturnToGame", ReturnToGame);
        EventManager.StopListening("LevelObjectiveAchieved", GameWon);
        EventManager.StopListening("ShowSettings", HideBackButton);
        EventManager.StopListening("HideSettings", ShowBackButton);
        DEBUG.Log("Pause Menu De-Activated");
    }

    void OnDestroy()
    {
        Time.timeScale = 1;
    }


    // Function that trigger all the pause menu and its components and also for network mode capabilities
    public void PauseButton () 
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        EventManager.TriggerEvent("GamePaused");
        GetComponent<Canvas>().enabled = true;

        if (PhotonNetwork.connectionStateDetailed != ClientState.Joined)
        {
            Time.timeScale = 0;
            myCanvas.enabled = true;

            if (GameObject.Find("AudioSource"))
            {
                GameObject.Find("AudioSource").GetComponent<AudioSource>().volume = 0.2f;
            }   
            restartMissionButtonRef.SetActive(true);
            leaveMissionButtonRef.GetComponentInChildren<Text>().text = "Level Select";
        }
        // We are in network mode
        else
        {

            myCanvas.enabled = true;
            if (GameObject.Find("AudioSource"))
            {
                GameObject.Find("AudioSource").GetComponent<AudioSource>().volume = 0.2f;
            }        
            restartMissionButtonRef.SetActive(false);
            leaveMissionButtonRef.GetComponentInChildren<Text>().text = "Leave Game";
        }
    }

    public override void ControlledUpdate() // To check if in the game or if its already pasued when escape is pressed
    {
        if (!APP.IsReady)
        {
            return;
        }

        if (APP.PlayerTankManager == null)
        {
            return;
        }

        if (APP.PlayerTankManager.isArcade) // Checking if it is aracde mode
        {
            if (Input.GetKeyDown(KeyCode.Escape)) // Will either return to game if game has already been paused or will pause game 
            {
                if (myCanvas.enabled == false && !ArcadeController.instance.playerDead && !UIManager.instance.hasWon) // To pause the game
                {
                    PauseButton();
                }
                else if(myCanvas.enabled == true && !ArcadeController.instance.playerDead) // To return to the game
                {
                    ReturnToGame();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape)) // Will either return to game if game has already been paused or will pause game 
            {
                if (myCanvas.enabled == false)
                {
                    PauseButton();
                }
               else
               {
                   ReturnToGame();               
               }
            }
        }
    }

    // The function that is called to display the chooserMenu which basically activates the menu where the player can select a tank and powerups, before the game starts
    public void ChooserMenu ()
    {
        if (levelManagerRef.isArcadeMode) // checking if arcade mode has been selected by the LevelManagerRef
        {
            if (APP.PlayerTankManager._ArcadeMode == arcadeMode.defend || APP.PlayerTankManager._ArcadeMode == arcadeMode.SearchAndDestroy)
            {
                Time.timeScale = 1;
                loadingOverlay.SetActive(true);
                SceneManager.LoadScene("ChooseDino");
                defendModeSettings.GetChildObject(ArcadeController.instance.playerArmorCollider, "playerBackArmorCollider").gameObject.name = "RearArmorCollider";
                defendModeSettings.GetChildObject(ArcadeController.instance.playerArmorCollider, "playerFrontArmorCollider").gameObject.name = "FrontArmorCollider";
                ArcadeController.instance.gameObject.SetActive(false);
            }
            else
            {
                Time.timeScale = 1;
                loadingOverlay.SetActive(true);
                SceneManager.LoadScene("ChooseDino");
            }
            DEBUG.Log("Arcade mode has been selected");
        }
        else
        {
            loadingOverlay.SetActive(true);
            Time.timeScale = 1;
            if (PhotonNetwork.connectionStateDetailed == ClientState.Joined)
            {
                Debug.Log("Show Dino selection here");
            }
            else
            {
                SceneManager.LoadScene("ChooseDino");
            }
        }
    }

    public void ReturnToMainMenu() // Will returnn to main menu
    {
        Time.timeScale = 1;
        loadingOverlay.SetActive(true);

        if (APP.IsConnected) // Checking if the game is connected to a newtwork
        {
            if (PhotonNetwork.room != null)
            {
                PhotonNetwork.LeaveRoom();
            }
        }
        else if (APP.PlayerTankManager.isArcade) // Checking which arcade game mode has been selected
        {
            if (APP.PlayerTankManager._ArcadeMode == arcadeMode.defend || APP.PlayerTankManager._ArcadeMode == arcadeMode.SearchAndDestroy)
            {
                SceneManager.LoadScene("MainMenu");
                defendModeSettings.GetChildObject(ArcadeController.instance.playerArmorCollider, "playerBackArmorCollider").gameObject.name = "RearArmorCollider";
                defendModeSettings.GetChildObject(ArcadeController.instance.playerArmorCollider, "playerFrontArmorCollider").gameObject.name = "FrontArmorCollider";
                ArcadeController.instance.towerHealth.gameObject.SetActive(false);
            }
            else
            {
                APP.PlayerTankManager.isArcade = false;
                SceneManager.LoadScene("MainMenu");
            }
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }

    }
   
    public override void OnLeftRoom() // if player leaves room 
    {
        PhotonNetwork.LoadLevel("MP_Lobby");
    }

    public void ReturnToLevelSelect() // will return to level select menu
    {
        if (FindObjectOfType<LevelManager>().isArcadeMode) // finding which arcade mode has been selected
        {
            if (APP.PlayerTankManager._ArcadeMode == arcadeMode.defend || APP.PlayerTankManager._ArcadeMode == arcadeMode.SearchAndDestroy)
            {
                Time.timeScale = 1;
                loadingOverlay.SetActive(true); // will set the loadingOverlay from the canvas active
                SceneManager.LoadScene("ArcadeMenu");
                defendModeSettings.GetChildObject(ArcadeController.instance.playerArmorCollider, "playerBackArmorCollider").gameObject.name = "RearArmorCollider";
                defendModeSettings.GetChildObject(ArcadeController.instance.playerArmorCollider, "playerFrontArmorCollider").gameObject.name = "FrontArmorCollider";
                ArcadeController.instance.towerHealth.gameObject.SetActive(false);
            }

            else
            {
                Time.timeScale = 1;
                loadingOverlay.SetActive(true);
                SceneManager.LoadScene("ArcadeMenu");
            }
        }

        else
        {
            Time.timeScale = 1;
            loadingOverlay.SetActive(true);
            leaveMissionPrompt.SetActive(false);
            if (PhotonNetwork.connectionStateDetailed == ClientState.Joined) // will run during multiplayer
            {
                if (PhotonNetwork.room != null)
                {
                    PhotonNetwork.LeaveRoom();
                }
            }
            else
            {
                SceneManager.LoadScene("ChooseLevel");
            }
        }
    }

    private void GameWon() // will activate bool if game is won
    {
        gameWon = true;
        DEBUG.Log("Player won");
    }

    public void ReturnToGame() // Will return to the game by chekcing if player is alive or not
    {
        if (PlayerTankmanager.GetPlayer() && PlayerTankmanager.GetPlayer().GetComponent<RTCTankController>().CheckAlive()) // checking if player is alive
        {
                if (!gameWon) 
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        // Returinng to the game by disabling the Pause Menu's components
        GetComponent<Canvas>().enabled = false;
        EventManager.TriggerEvent("GameUnPaused"); // Triggering the event that will resume the game
        loadingOverlay.SetActive(false);

        Time.timeScale = 1;
        winScreen.GetComponent<Canvas>().enabled = false;
        gameObject.GetComponent<Canvas>().enabled = false;

        if (GameObject.Find("AudioSource")) 
        {
            GameObject.Find("AudioSource").GetComponent<AudioSource>().volume = 1f; // changing the volume 
        }
        uiComponents.SetActive(true);
    }

    
    public void ResetPlayerPosition() // will reset the player to postion to the ground, in case the player topples over or something.
    {
        LevelManager levelManagerRef = FindObjectOfType<LevelManager>(); // accessing the LevelManager
        if (levelManagerRef)
        {
            levelManagerRef.ResetPlayerPosition();
        }
        ReturnToGame();
    }

    public void RestartMissionButtonPressed() // Will activate resartMissionPrompt
    {
        restartMissionPrompt.SetActive(true);
    }

    public void LeaveMissionButtonPressed() // Will activate LeaveMissionButtonPressed
    {
        leaveMissionPrompt.SetActive(true);
    }

    public void CloseRestartMissionPromptPressed() // Will activate CloseRestartMissionPromptPressed
    {
        restartMissionPrompt.SetActive(false);
    }

    public void CloseLeaveMissionPromptPressed() // Will activate CloseLeaveMissionPromptPressed
    {
        leaveMissionPrompt.SetActive(false);
    }

    public void RestartMission() // Will restart the current mission/level name
    {
        bool isArcade = PlayerTankmanager.GetLevelFromName(SceneManager.GetActiveScene().name).isArcade;
        if (isArcade) // If current level is active, it will restart it
        {
            ArcadeController.instance.towerHealth.gameObject.SetActive(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            APP.PlayerTankManager.AddCheckPointForLevel(SceneManager.GetActiveScene().name, 0);
            restartMissionPrompt.SetActive(false);
            loadingOverlay.SetActive(true);
            SceneManager.LoadScene("LoadingScene");
        }
    }


    // Debugging function 
    public void ResetplayerPrefs()
    {
        APP.PlayerTankManager.ResetPlayerPrefs();
        DEBUG.Log("Player prefs are being reset");
    }

    public void ResetLevelProgress()
    {
        PlayerPrefs.SetInt(SceneManager.GetActiveScene().name + "_checkpoint", 0);
        PlayerPrefs.Save();
        DEBUG.Log("Player progress has been saved from the last checkpoint");
    }

    public void SetKeyboard()
    {
        InputManager.controllerType = controllerTypes.mouse_keyboard;
    }

    void ShowBackButton()
    {
        backButton.SetActive(true);
    }

    void HideBackButton()
    {
        backButton.SetActive(false);
    }
}

