/*  
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
    ╔═════════════════════════════╡  DinoTank  ╞══════════════════════════════════════════════════════╗            
    ║ Authors:  Rahul Yerramneedi                       Email:    yr020409@gmail.com                  ║
    ╟─────────────────────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Purpose: This script is used as manager to control all the major UI elements present in the     ║
    ║          game.                                                                                  ║
    ║          It contains everything from all the elements of the different game mode's UI to all    ║ 
    ║          the screen effects.                                                                    ║
    ╟─────────────────────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Usage: Script is added on to the UI prefab with all its compnents intact.                       ║                                                   
    ╚═════════════════════════════════════════════════════════════════════════════════════════════════╝
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum severety : byte { none, mild, moderate, severe};

public class UIManager : MonoBehaviour {

    private static UIManager uiManager;
    public static UIManager instance
    {
        get
        {
            if (!uiManager)
            {
                uiManager = FindObjectOfType(typeof(UIManager)) as UIManager;
                if (!uiManager)
                {
                    GameObject manager = Instantiate(Resources.Load("Managers/UI", typeof(GameObject)) as GameObject);
                    if (manager)
                    {
                        uiManager = manager.GetComponentInChildren<UIManager>();
                    }
                }

            }
            return uiManager;
        }

    }

    [Header("Runtime Referneces")] // Major references from other sripts/gameobjects
    public GameObject target;
    private RTCTankController tankController;
    public RTCTankGunController tankGunController;

    [Header("References")] // References from other objects/scripts
    public Slider healthSlider;
    public Text HealthSliderText;
    public Image healthSliderFillBar;
    public Slider fuelSlider;
    public Text fuelSliderText;
    public Image fuelSliderFillBar;
    public Text speedText;
    public GameObject miniMap;
    public GameObject skipButton;
    public Button speedomiter;
    public GameObject bigTextDisplayGo;

    [Header("Input")] // Control Inputs
    public VJ1 joystickDirectional;
    public VJ1 joystickWASD;
    public GameObject controllerForHideREF;

    [Header("UI Objects")] // All the UI objects being controlled
    public Text smallText;
    public GameObject win;
    public GameObject lose;
    public Text mp_team1Text;
    public Text mp_team2Text;
    public Text mp_teamsTotal;
    public Text TextDisplayText;
    public Canvas scoresCanvas;
    public GameObject playersListDM;
    public GameObject playersListTDM;
    public GameObject edgeFade;
    public GameObject damageFade;
    public GameObject OutOfBounds;
    public GameObject errors;
    public Text errorlevel;
    public GameObject selectedPanel;
    public GameObject loadoutbadgePrefab;
    public GameObject additionPanel;
    public GameObject weaponBadgeAddition;
    public GameObject briefingScreen;
    public GameObject fuelEmptyLabel;
    public GameObject blackFade;
    public GameObject viewSlit;
    public GameObject tankAwardBadge;
    public GameObject statsPanel;
    public GameObject scopeFade;
    public GameObject modelRepresentationPrefab;

    [Header("Miscellaneous")] // Other references and bools
    LevelManager levelManagerRef;
    [HideInInspector]
    public bool hasWon = false;
    private bool claimReward = false;
    public List<GameObject> objectsToHide = new List<GameObject>();
    private MapCanvasController mapCanvasController;
    GameObject modelRepresentation = null;

    [Header("Variables")] // Numerical variables
    public float lifeRatio;

    [Header("Buttons")] // Buttons
    public Button fireButton;

    [Header("Fuel Arrow")] // Objects related to the fuel arrow
    public GameObject EmptyFuelArrow;
    public Scene CurrentScene;
    public RTCTankController RTCTank;

    [Header("Compass and Loadout Panel")] // Objects related to the compass and loadout panel
    public DirectionCompass compassRef;
    [SerializeField]
    private Loadoutpanel LoadoutPanel;
    public GameObject Compass;
    public NetworkController nc;
    public GameObject ObjPanel;

    void Awake()
    {
        playersListDM.SetActive(false); // On awake will set the playerListDM (UI element) inactive
        playersListTDM.SetActive(false); // On awake will set the playerListTDM (UI element) inactive
    }

    // Adding a new _target GameObject
    public void SetTarget(GameObject _target) 
    {
        target = _target; // Giving the same information as the target GameObject
        tankController = _target.GetComponent<RTCTankController>();
        tankGunController = _target.GetComponentInChildren<RTCTankGunController>();
    }

    public void OnEnable() // Always use this for events 
    {
        EventManager.StartListening("LevelObjectiveAchieved", ShowWinOverlay, this); // Called from level manager
        EventManager.StartListening("PlayerDies", ShowLossOverlay, this); // Called from level manager
        EventManager.StartListening("LevelObjectiveFailed", ShowLossOverlay, this);
        EventManager.StartListening("PlayerRespawned", PlayerRespawned, this);
        EventManager.StartListening("SwapJoystick", SwapJoystick, this);
        EventManager.StartListening("BackSreen", BlackScreen, this);
        EventManager.StartListening("UnBackSreen", UnBlackScreen, this);
        EventManager.StartListening("SkipUIEvent", SkipButtonPressed, this);
        EventManager.StartListening("ShowScopeFade", ShowScopeFade, this);
        EventManager.StartListening("HideScopeFade", HideScopeFade, this);
        EventManager.StartListening("ShowViewSlit", ViewSlit, this);
        EventManager.StartListening("HideViewSlit", UnViewSlit, this);
        HideScopeFade();
    }

    void OnDisable() 
    {
        EventManager.StopListening("LevelObjectiveAchieved", ShowWinOverlay);// Called from level manager
        EventManager.StopListening("PlayerDies", ShowLossOverlay); // Called from level manager
        EventManager.StopListening("LevelObjectiveFailed", ShowLossOverlay);
        EventManager.StopListening("PlayerRespawned", PlayerRespawned);
        EventManager.StopListening("SwapJoystick", SwapJoystick); // Called from networkController
        EventManager.StopListening("BackSreen", BlackScreen);
        EventManager.StopListening("UnBackSreen", UnBlackScreen);
        EventManager.StopListening("SkipUIEvent", SkipButtonPressed);
        EventManager.StopListening("ShowScopeFade", ShowScopeFade);
        EventManager.StopListening("HideScopeFade", HideScopeFade);
        EventManager.StopListening("ShowViewSlit", ViewSlit);
        EventManager.StopListening("HideViewSlit", UnViewSlit);
    }

    // Coroutine for Player Tank Manager to spawn and start functioning
    IEnumerator WaitForPTMToSpawn() 
    {
        yield return new WaitUntil(() => APP.PlayerTankManager != null);
        Start();
    }

    void Start()
    {
        nc = GameObject.FindObjectOfType<NetworkController>();
        errorlevel = GetComponent<Text>();
        CurrentScene  = SceneManager.GetActiveScene();

        if (EmptyFuelArrow) 
        {
            EmptyFuelArrow.SetActive(false);
        }

        if (APP.PlayerTankManager == null || APP.PlayerTankManager.player == null) // if the player doesn't have a player tank manager, then wait and spawn one
        {
            StartCoroutine(WaitForPTMToSpawn());
            return;
        }

        
        // Setting UI elements inactive or active
        HideScopeFade();
        win.GetComponent<Canvas>().enabled = false;
        lose.GetComponent<Canvas>().enabled = false;
        scoresCanvas.enabled = false;
        skipButton.SetActive(false);
        edgeFade.SetActive(false);
        damageFade.SetActive(true);
        blackFade.SetActive(false);
        viewSlit.SetActive(false);

        // Assigning/Finding data from objects present in the scene 
        levelManagerRef = FindObjectOfType<LevelManager>();
        mapCanvasController = miniMap.GetComponent<MapCanvasController>();
        tankController = APP.PlayerTankManager.player.GetComponent<RTCTankController>();
        tankGunController = tankController.GetComponentInChildren<RTCTankGunController>();

        if (!tankController)
        {
            StartCoroutine("WaitForController");
        }

        // Cursor Related 
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (levelManagerRef.errorInStack)
        {           
            errors.SetActive(true);            
            errorlevel.text = levelManagerRef.checkpointsCount +
            levelManagerRef.destroyTargetCount+
            levelManagerRef.collectCount + 
            levelManagerRef.waveCount +
            levelManagerRef.destinationsCount +
            levelManagerRef.deliverCount +
            levelManagerRef.captureCount +
            levelManagerRef.cutSceneCount + "Error in Level Manager";
            DEBUG.Log("Errors in LevelManager detected, behaviour may be broken", Warning_Types.Error);
        }
        else
        {
            errors.SetActive(false);
        }

        LoadoutPanel.Initialize();

        // Fuel Arrow only for the grasslands level
        if (CurrentScene.name.Contains("Campaign_Grasslands"))
        {
            EmptyFuelArrow.SetActive(tankController.CurrentFuel <= 0 ? true : false);
        }
        else
        {
            EmptyFuelArrow.SetActive(false);
        }
    }

    IEnumerator WaitForController()
    {
        tankController = target.GetComponent<RTCTankController>();
        yield return new WaitWhile(() => !tankController);
        tankGunController = tankController.GetComponentInChildren<RTCTankGunController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target) 
        {
            if (!mapCanvasController)
            {
                mapCanvasController = miniMap.GetComponent<MapCanvasController>();
                return;
            }

            mapCanvasController.playerTransform = target.transform;
            if (!tankController)
            {
                tankController = target.GetComponent<RTCTankController>();
                return;
            }

            // Player health related
            lifeRatio = tankController.GetLifeRatio();
            healthSlider.value = lifeRatio;
            HealthSliderText.text = ((int)tankController.CurrentLife).ToString() + " / " + ((int)tankController.MaxLife).ToString();
            healthSliderFillBar.color = Color.green;
            if (lifeRatio < 0.45f) // yellow state for the health bar
            {
                healthSliderFillBar.color = Color.yellow;
            }
            if (lifeRatio < 0.2f) // red state for the health bar
            {
                healthSliderFillBar.color = Color.red;
            }

            speedText.text = "" + Mathf.RoundToInt(tankController.speed);

            // Player tank fuel related
            fuelSlider.value = tankController.GetFuelRatio();
            fuelSliderText.text = ((int)tankController.CurrentFuel).ToString() + " / " + ((int)tankController.maxFuel).ToString();
        }

        // Checking if the player has joined the network and enable the UI elements accordingly
        if (PhotonNetwork.connectionStateDetailed == ClientState.Joined)
        {
            Compass.SetActive(false);
            ObjPanel.SetActive(false);
        }
        else
        {
            Compass.SetActive(true);
            ObjPanel.SetActive(true);
        }

        // Check which game mode is active (DM or TDM) and based on that activate the scoreCanvas
        if (playersListTDM.activeSelf || playersListDM.activeSelf)
        {
            scoresCanvas.enabled = true;
        }
        else if (!playersListTDM.activeSelf || !playersListDM.activeSelf)
        {
            scoresCanvas.enabled = false;
        }
    }

    // Used for the fading effect
    void BlackScreen() 
    {
        blackFade.SetActive(true);
    }

    // Used for the fading effect
    void UnBlackScreen() 
    {
        blackFade.SetActive(false);
    }

    // Displaying the scope slit
    void ViewSlit() 
    {
       viewSlit.SetActive(true);
    }

    // Displaying the scope slit
    void UnViewSlit()
    {
        viewSlit.SetActive(false);
    }

    void ShowWinOverlay()  // Registered by level manager 
    {
        StartCoroutine("ObjectieComplete");
    }

    void ShowLossOverlay()
    {
        StartCoroutine("WaitAndRestartLevel");
    }

    void PlayerRespawned()
    {
        lose.GetComponent<Canvas>().enabled = false;
    }

    // Show the timer (numerical)
    public void ShowTimerDisplay(float _time)
    {
        bigTextDisplayGo.SetActive(_time > 0);
        TextDisplayText.text = ConverTimeToMinutesSeconds(_time);
    }

    // Show the timer (text)
    public void ShowTimerDisplay(string _message) 
    {
        bigTextDisplayGo.SetActive(true);
        TextDisplayText.text = _message;
    }

    public void HideTimerDisplay()
    {
        bigTextDisplayGo.SetActive(false);
    }

    public string ConvertTimeToMinutesSeconds(float _timer)
    {
        int minutes = Mathf.FloorToInt(_timer / 60F);
        int seconds = Mathf.FloorToInt(_timer - minutes * 60);
        return string.Format("{0:0}:{1:00}", minutes, seconds);
    }

    IEnumerator ObjectiveComplete() // Coroutine to check if the objective has been complete and based on that assign a winning team, switching of the UI elements
    {
        EventManager.TriggerEvent("DeathCam");
        PlayerTankmanager.GetPlayer().GetComponent<RTCTankController>().gasInput = 0;
        PlayerTankmanager.GetPlayer().GetComponent<RTCTankController>().steerInput = 0;

        // Chekcing if the current game mode is arcade and the based on that get all the details and info required for this mode, from scores to all the stats
        if (APP.PlayerTankManager.isArcade)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
            ArcadeController.instance.arcadeRetryScreen.GetComponent<Canvas>().enabled = true;
            ArcadeController.instance.yesButton.gameObject.SetActive(false);
            ArcadeController.instance.noButton.gameObject.SetActive(false);
            ArcadeController.instance.info.enabled = false;
            ArcadeController.instance._Overlay.enabled = false;
            ArcadeController.instance.reviveText.enabled = false;
            statsPanel.transform.GetChild(2).gameObject.SetActive(false);
            win.transform.GetChild(6).gameObject.SetActive(false);
            win.GetComponent<Canvas>().enabled = true;
            ArcadeController.instance.towerHealth.gameObject.SetActive(false);
            if(GameObject.Find("ArcadeLives"))
            GameObject.Find("ArcadeLives").SetActive(false);
            if(GameObject.Find("ScoreName"))
            GameObject.Find("ScoreName").SetActive(false);
            ArcadeController.instance.waveText.SetActive(false);
            win.transform.GetChild(4).GetComponent<Text>().text = "Round Complete";
            statsPanel.SetActive(true);
            statsPanel.GetComponent<StatsDisplayController>().PopulateStats();
            hasWon = true;
            if (APP.PlayerTankManager._ArcadeMode == arcadeMode.SearchAndDestroy)
            {
                foreach(GameObject tank in EnemyWaveController.instance.spawnedTanks)
                {
                    tank.GetComponent<RTCTankController>().targetToAttack = null;
                    tank.GetComponent<RTCTankController>().aiState = aiStates.idle;
                }
                ArcadeController.instance.labels.SetActive(false);
                ArcadeController.instance.playerPanel_RED.SetActive(false);
                ArcadeController.instance.playerPanel_BLUE.SetActive(false);
                ArcadeController.instance.UpdateRounds();
                if (APP.PlayerTankManager.REDTeamTotalScoresVal == 2)
                {
                    win.transform.GetChild(4).GetComponent<Text>().text = "Red Team Wins";
                    ArcadeController.instance.arcadeButton.gameObject.SetActive(true);
                    ArcadeController.instance.retryButton.gameObject.SetActive(true);
                }
            }
        }

        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            win.GetComponent<Canvas>().enabled = true;
            LevelEndAwardController awardController = FindObjectOfType<LevelEndAwardController>();
            if (awardController != null)
            {
                if (awardController.awardTankAtEndOfLevel)
                {
                    statsPanel.SetActive(false);
                    tankAwardBadge.SetActive(true);
                    tankAwardBadge.GetComponent<TankAwardBadge>().SetBadgeImage(awardController.tankToAward);
                    yield return new WaitUntil(() => claimReward);
                }
            }
            tankAwardBadge.SetActive(false);
            statsPanel.SetActive(true);
            statsPanel.GetComponent<StatsDisplayController>().PopulateStats();
        }
    }
    
    public void ClaimButtonPressed()
    {
        claimReward = true;
    }

    // Coroutine that basically checks which mode is being played and after its completed it will either restart it or quit
    IEnumerator WaitAndRestartLevel()
    {
        yield return new WaitForSeconds(1);  // seconds to pause before the level is restarted
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (levelManagerRef.isArcadeMode)
        {
            lose.transform.GetChild(4).gameObject.SetActive(false);
            lose.transform.GetChild(6).gameObject.SetActive(false);
            lose.GetComponent<Canvas>().enabled = true;
            ArcadeController.instance.arcadeRetryScreen.GetComponent<Canvas>().enabled = true;
            ArcadeController.instance.yesButton.gameObject.SetActive(false);
            ArcadeController.instance.noButton.gameObject.SetActive(false);
            ArcadeController.instance.info.enabled = false;
            ArcadeController.instance._Overlay.enabled = false;
            ArcadeController.instance.reviveText.enabled = false;
            ArcadeController.instance.waveText.gameObject.SetActive(false);
            if (APP.PlayerTankManager._ArcadeMode == arcadeMode.defend)
            {
                ArcadeController.instance.towerHealth.gameObject.SetActive(false);
            }
            if (APP.PlayerTankManager._ArcadeMode == arcadeMode.SearchAndDestroy)
            {
                statsPanel.SetActive(true);
                statsPanel.GetComponent<StatsDisplayController>().PopulateStats();
                foreach (GameObject tank in EnemyWaveController.instance.spawnedTanks)
                {
                    tank.GetComponent<RTCTankController>().targetToAttack = null;
                    tank.GetComponent<RTCTankController>().aiState = aiStates.idle;
                }
                lose.transform.GetChild(5).GetComponent<Text>().text = "Round Lost";
                ArcadeController.instance.labels.SetActive(false);
                ArcadeController.instance.playerPanel_RED.SetActive(false);
                ArcadeController.instance.playerPanel_BLUE.SetActive(false);
                if (APP.PlayerTankManager.BLUETeamTotalScoresVal == 2)
                {
                    lose.transform.GetChild(5).GetComponent<Text>().text = "Blue Team Wins";
                    ArcadeController.instance.arcadeButton.gameObject.SetActive(true);
                    ArcadeController.instance.retryButton.gameObject.SetActive(true);
                    DEBUG.Log("Blue team won");
                }
                ArcadeController.instance.UpdateRounds();
            }
            else
            {
                ArcadeController.instance.arcadeButton.gameObject.SetActive(true);
                ArcadeController.instance.retryButton.gameObject.SetActive(true);
            }
        }
        else
            lose.GetComponent<Canvas>().enabled = true;
    }

    // For the player to try the level again, if defeated
    public void TryAgain()
    {
        switch (levelManagerRef.levelMode)
        {
            case levelGameMode.arcade:
                ArcadeController.instance.RetryLevel();
                break;
        
            case levelGameMode.campaign:
                   SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                   break;
        }        
    } 

    public void ShowHideUI(bool _show)
    {
        if (APP.PlayerTankManager.player)
        {
            APP.PlayerTankManager.player.GetComponent<RTCTankController>().brakeInput = 1;
            APP.PlayerTankManager.player.GetComponent<RTCTankController>().controlOverride = !_show;
        }
        foreach (GameObject item in objectsToHide)
        {
            if(item)
            item.SetActive(_show);
        }
        skipButton.SetActive(!_show);
        edgeFade.SetActive(!_show);
        DynamicCrosshairController.ShowHideCrosshairElements(_show);

        if (ModelRepresentation.instance)
            ModelRepresentation.instance.gameObject.SetActive(_show);
        if (_show)
        {
            SwapJoystick();                      
        }
    }

    // Start displaying the score board for the death match mode 
    public void StartDMScorePanel(int _neededToWin) 
    {
        mp_teamsTotal.text = "First to " + _neededToWin.ToString() + " Kills Wins";       
        StartCoroutine("UpdateDMscore");
    }

    // Start displaying the score board for the team death match mode
    public void StartTDMScorePanel(int _neededToWin) 
    {
        playersListTDM.SetActive(true);
        mp_teamsTotal.text = "First to " + _neededToWin.ToString() + " Kills Wins";
        StartCoroutine("UpdateTDMScore");
        playersListTDM.GetComponent<MaintainPlayerList>().gameMode = gamemodeNetwork.teamDeathmatch;
    }

    // Coroutine that is being used to update the score of the death match mode
    IEnumerator UpdateDMscore() 
    {
        playersListDM.SetActive(true);
        playersListDM.GetComponent<MaintainPlayerList>().gameMode = gamemodeNetwork.deathMatch;
        scoresCanvas.enabled = true;
        while (true)
        {
            scoresCanvas.transform.localScale = Vector3.one; // setting the vectors position at (1,1,1)
            mp_team1Text.text = PhotonNetwork.player.GetScore().ToString(); // converting score to string
            PhotonPlayer[] playersConnected = PhotonNetwork.playerList;
            int topScore = 0;
            for (int i = 0; i < playersConnected.Length; i++)
            {
                if (playersConnected[i].GetScore() > topScore)
                {
                    topScore = playersConnected[i].GetScore();
                }
            }
            mp_team2Text.text = topScore.ToString();
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Coroutine that is being used to update the score of the death match mode
    IEnumerator UpdateTDMScore() 
    {
        playersListTDM.SetActive(true);
        playersListTDM.GetComponent<MaintainPlayerList>().gameMode = gamemodeNetwork.teamDeathmatch;
        scoresCanvas.enabled = true;
        while (true)
        {
            scoresCanvas.transform.localScale = Vector3.one;
            mp_team1Text.text = PhotonNetwork.player.GetScore().ToString();

            PhotonPlayer[] playersConnected = PhotonNetwork.playerList;
            int topScore = 0;
            for (int i = 0; i < playersConnected.Length; i++) // check for the players connected
            {
                if (playersConnected[i].GetScore() > topScore) // comparing the top scores
                {
                    topScore = playersConnected[i].GetScore();
                }
            }
            mp_team2Text.text = topScore.ToString();
            yield return new WaitForSeconds(0.1f);
        }
    }
   
    public void ShowSettings()
    {
        EventManager.TriggerEvent("ShowSettings");
    }
 
    // When the UI is disabled, the button that covers the screen and allows you to skip, calls these functions
    public void SkipButtonPressed()
    {
        if (RTCCamera.instance.isPanningToTargets)
        {
            EventManager.TriggerEvent("StopPanCamera");
            return;
        }
    }

    public static void ShowDialogue(dino _dino, string _message,bool _silhouette,bool isPlayer)
    {
        instance.edgeFade.GetComponent<DialogueController>().CreateDialogueBox(_dino, _message, _silhouette, isPlayer);
    }

    // Will clear the dialogue box
    public static void ClearDialogue()
    {
        foreach (Transform item in instance.edgeFade.transform)
        {
            Destroy(item.gameObject);
        }
    }

    // Controls the damage being faded by an animator
    public void ShowDamageFade(severety _severety)
    {
        Animator animator = damageFade.GetComponent<Animator>();
        animator.ResetTrigger(_severety.ToString());
        animator.SetTrigger(_severety.ToString());
    }

    void ShowScopeFade()
    {
        if(scopeFade)
            scopeFade.SetActive(true);
        if (!modelRepresentation)
        {
            modelRepresentation = Instantiate(modelRepresentationPrefab) as GameObject;
            modelRepresentation.transform.SetParent(null);
            modelRepresentation.gameObject.SetActive(true);
        }
        else
        {
            modelRepresentation.gameObject.SetActive(true);
        }
        DEBUG.Log("Scope is activated");
    }

    void HideScopeFade()
    {
        if (scopeFade)
        {
            scopeFade.SetActive(false);
        }

        if (modelRepresentation)
        {
            modelRepresentation.gameObject.SetActive(false);
        }
        DEBUG.Log("Scope has been hidden");
    }

    // Used to Show the epty fuel 
    public void ShowFuelEmptyLabel(bool _showFuelLabel)
    {
        fuelEmptyLabel.SetActive(_showFuelLabel);
    }

    // Used for swapping weapon icons
    public void SwapSelectedWeapon()
    {
        foreach (Transform item in selectedPanel.transform)
        {
            Destroy(item.gameObject);
        }
        GameObject badge = (GameObject)Instantiate(loadoutbadgePrefab); // under badge a prefab will be instantiated 
        badge.transform.SetParent(selectedPanel.transform, false); // set the parent of the badge
        badge.transform.localScale = new Vector3(1, 1, 1); // set the position of the badge icon
        badge.GetComponent<Animator>().enabled = false;
        Sprite tempSprte = APP.PlayerTankManager.m_loadout[tankGunController.selectedWeaponIndex].GetComponent<WeaponIcon>().icon;
        LoadoutBadge badgeScriptRef = badge.GetComponent<LoadoutBadge>();
        badgeScriptRef.iconImage.sprite = tempSprte; // get the temporary sprite from the badge reference
        badgeScriptRef.weaponName = APP.PlayerTankManager.m_loadout[tankGunController.selectedWeaponIndex].gameObject.name; // get the weapon name from the selected weapon of the tankGunController
        badgeScriptRef.selectImage.enabled = true;
        if (APP.PlayerTankManager.m_loadout[tankGunController.selectedWeaponIndex].GetComponent<WeaponIcon>().isInfinate)
        {
            badgeScriptRef.qtyText.text = "";
        }
        else
        {
            badgeScriptRef.qtyText.text = APP.PlayerTankManager.GetWeaponQuantity(APP.PlayerTankManager.m_loadout[tankGunController.selectedWeaponIndex].gameObject.name).ToString();

        }
        DEBUG.Log("Weapon has been swapped");
    }

    // Variables for the collectibles 
    Collect holdCollectScript;
    int holdQty;
    collectableTypes holdType;

    public void GenerateAdditionIcon(collectableTypes _type, int _qty,Collect _colletRef)
    {
        if (_type == collectableTypes.Amber // the different collectible types
            || _type == collectableTypes.FuelTank 
            || _type == collectableTypes.FuelTankBig 
            || _type == collectableTypes.Heal 
            || _type == collectableTypes.Meteorite)
            return;

        if (additionPanel.transform.childCount >= 3)
        {
            holdType = _type;
            holdCollectScript = _colletRef;
            holdQty = _qty;
            StartCoroutine("WaitToSpawnBadge");
        }
        else
        {
            GameObject badge = Instantiate(weaponBadgeAddition) as GameObject;
            badge.transform.SetParent(additionPanel.transform, false);
            badge.transform.localScale = new Vector3(1, 1, 1);
            WeaponBadgeAddition badgeScr = badge.GetComponent<WeaponBadgeAddition>();
            
            badgeScr.SetIconImage(_colletRef.icon);
            badgeScr.SetName(_colletRef.displayName);
            badgeScr.SetQuantity(_qty);        
        }
    }

    IEnumerator WaitToSpawnBadge()
    {
        yield return new WaitWhile(() => additionPanel.transform.childCount >= 3);
        GenerateAdditionIcon(holdType, holdQty, holdCollectScript);
    }

    // Briefing stuff
    public void ShowBriefingScreen()
    {
        briefingScreen.SetActive(true);
        DEBUG.Log("Briefing screen has been activated");
    }
}
