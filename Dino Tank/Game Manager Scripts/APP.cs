/*  
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
    ╔═════════════════════════════╡  DinoTank  ╞══════════════════════════════════════════════════════╗            
    ║ Authors:  Rahul Yerramneedi                       Email:    yr020409@gmail.com                  ║
    ╟─────────────────────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Purpose: This script is used to boot up the game and launch all the main and sub-systems        ║
    ║          present in the game.                                                                   ║
    ╟─────────────────────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Usage: The APP prefab should have this script and it should be placed in the Booting scene.     ║                             
    ╚═════════════════════════════════════════════════════════════════════════════════════════════════╝
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
*/

// NOTE: This system spawns the DebugMenu, for release remove the DEBUG_ENABLED define from Edit > Project Settings > Player > Other Settings > Scripting Define Symbols
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class APP : Photon.PunBehaviour
{
    // Instance
    private static APP Instance;

    // Public
    [Header("Start game from the beginning")]
    public bool BootstrapGame = false; // Launch Game 

    [Header("Logging")]
    public bool OnScreenLog = true;
    public bool WarningsAndErrorsOnly = false;

    [Header("Stats")]
    public float GameClock;

    // Static
    public static float DeltaTime;

    // Private
    [SerializeField]
    List<SystemBase> Systems = new List<SystemBase>();

    [NonSerialized]
    public AppLog AppLogger;
 
    public static PlayerTankmanager PlayerTankManager;
    
    //  Contains all the loading states of the game
    public enum LoadStates
    {
        Idle, 
        Starting, // State which loads the data for game to start
        LoadingData, // State where the game will grab all the data for the game from the google sheets and also determine whether if ts offline mode
        OpeningScene, // State which loads the first scene
        StartingSystems, // State where all the subsytems like audio, unlock and event managers are initialized
        Connecting, // State where the game will be connected to a neetwor
        LoadingPlayer, // State in which the level and player tank manager is loaded 
        Ready // State in basically which the APP is ready
    }

    public static LoadStates LoadState = LoadStates.Idle;
    public static bool DataLoaded = false;

    public static bool IsReady { get { return LoadState == LoadStates.Ready; } }
    public static bool IsConnected { get { return PhotonNetwork.connectionStateDetailed == ClientState.Joined; } }

    public static APP Inst // Main Instance
    {
        get
        {
            if (Instance)
                return Instance;

            Instance = FindObjectOfType(typeof(APP)) as APP;
            if (Instance)
            {
                if (SceneManager.GetActiveScene().buildIndex != 0)
                {
                    DEBUG.Log("App already exists in scene, will use it while in dev... in the future it should only exist in boot scene");
                }
                return Instance;
            }

            GameObject APP = Instantiate(Resources.Load("Managers/APP", typeof(GameObject)) as GameObject);
            if (APP)
            {
                Instance = APP.GetComponentInChildren<APP>();
                return Instance;
            }
            return null;
        }
    }

    // App code and stability handling in here
    private void Update()
    {
        // Safe game Update
        ControlledUpdate();
    }

    public static void ControlledUpdate()
    {
        if (!APP.IsReady)
        {
            return;
        }

        // Treat Like Update
        DeltaTime = Time.deltaTime;
        Instance.GameClock += Time.deltaTime;

        // Update subscribers
        foreach (SystemBase system in Instance.Systems)
        {
            system.ControlledUpdate();
        }
    }

    // Initilizing the varibales and game states before the game starts
    private void Awake()
    {
        // Making sure the build index is not 0 and BootstrapGame bool is true
        if (BootstrapGame && SceneManager.GetActiveScene().buildIndex != 0)
        {
            BootstrapGame = false;
            SceneManager.LoadScene(0);
            Destroy(gameObject);
        }
        BootstrapGame = false;

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Initialize();
    }


    // Is intializing for the systems and the logs
    public void Initialize()
    {
        if (OnScreenLog)
        {
            GameObject DeugLog = Instantiate(Resources.Load("Managers/AppLog"), null) as GameObject;
            AppLogger = DeugLog.GetComponent<AppLog>();
            DontDestroyOnLoad(DeugLog.gameObject);
        }

        if (IsReady)
        {
            // Ensure there is only one App, ever
            Destroy(gameObject);
            return;
        }

        // Start App
        Instance = Inst;

        SetLoadState(LoadStates.Starting);
    }

    void SetLoadState(LoadStates loadState)
    {
        LoadState = loadState;
        switch (LoadState)
        {
            case LoadStates.Idle:
                break;

            case LoadStates.Starting:
                DEBUG.Log("Starting Game...");
                SetLoadState(LoadStates.LoadingData);
                break;

            case LoadStates.LoadingData:
                // Fetch Google Sheet Data               
                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    GameData.Reload(onCallLoaded: () =>
                    {                     
                       DEBUG.Log("Data Loaded...", Warning_Types.Good);
                      DataLoaded = true;
                        
                       SetLoadState(LoadStates.OpeningScene);
                    });                  
                }
                else
                {
                    DEBUG.Log("Not Connected to the internet, GameData and multiplayer offline", Warning_Types.Error);
                    DataLoaded = true;
                    SetLoadState(LoadStates.OpeningScene);
                }
                break;

            case LoadStates.OpeningScene:
                DEBUG.Log(string.Format("Opening scene {0}...", SceneManager.GetActiveScene().name));
                SceneManager.sceneLoaded += ReloadCurrentScene;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;

            case LoadStates.StartingSystems:
                SceneManager.sceneLoaded -= ReloadCurrentScene;

                DEBUG.Log("Scene Loaded...", Warning_Types.Log);
                CreateSubsystem<ObjectPool>();
                PlayerTankManager = CreateSubsystem<PlayerTankmanager>() as PlayerTankmanager;
                CreateSubsystem<UnlockManager>();
                CreateSubsystem<AudioManager>();
                CreateSubsystem<EventManager>();      
                DEBUG.Log("Systems Started...", Warning_Types.Good);
                SetLoadState(LoadStates.Connecting);
                break;

            case LoadStates.Connecting:
                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    DEBUG.Log("Connecting...", Warning_Types.Log);
                    PhotonNetwork.ConnectUsingSettings("v1");
                }
                else
                {
                    DEBUG.Log("Network not reachable...", Warning_Types.Warning);
                    SetLoadState(LoadStates.LoadingPlayer);
                }
                break;

            case LoadStates.LoadingPlayer:
                StartLevelManager();
                SetLoadState(LoadStates.Ready);
                break;

            case LoadStates.Ready:
                // App is ready, nothing further to do here
                DEBUG.Log("Ready", Warning_Types.Good);
                break;
        }
    }

    // Callbacks and Hooks
    public override void OnConnectedToPhoton()
    {
        DEBUG.Log("Connected...", Warning_Types.Good);
        SetLoadState(LoadStates.LoadingPlayer);
    }

    // Loading the game's splash screen
    private void SplashScreenLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= SplashScreenLoaded;       
        SetLoadState(LoadStates.LoadingData);
    }

    // Loading the game's main menu
    private void MainMenuLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= MainMenuLoaded;
        SetLoadState(LoadStates.Ready);
    }

    // Loading the game's current scene
    private void ReloadCurrentScene(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= ReloadCurrentScene;
        SetLoadState(LoadStates.StartingSystems);
    }

    // Booting up the level manager
    private void StartLevelManager()
    {
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager)
        {
            DEBUG.Log("Initializing LevelManager...", Warning_Types.Log);
            levelManager.Initialize();
        }
    }


    // Helps initialize a system when triggered
    SystemBase CreateSubsystem<T>() where T : SystemBase
    {
        T foundSystem = FindObjectOfType<T>();
        if (foundSystem)
        {
            if (!foundSystem.IsInitialized)
            {
                Type foundSysType = typeof(T);
                DEBUG.Log(foundSysType.FullName + " detected in scene, this is not good as the data could be stale. Will use it for now ", Warning_Types.Warning);

                foundSystem.Initialize();
                foundSystem.IsInitialized = true;
                foundSystem.gameObject.transform.SetParent(null);
                DontDestroyOnLoad(foundSystem.gameObject);
            }

            foundSystem.name = foundSystem.name + "_FOUND";
            Systems.Add(foundSystem);
            return foundSystem.GetComponent<SystemBase>();
        }

        // Use reflection to get the prefab name
        Type systemType = typeof(T);
        if (systemType == null)
        {
            DEBUG.Log("Could not determine the system type for given system", Warning_Types.Error);
            return null;
        }

        GameObject system = Instantiate(Resources.Load("Managers/" + systemType.FullName) as GameObject, null);
        DontDestroyOnLoad(system);
        system.name = system.name.Replace("(Clone)", "_APP");
        SystemBase systemBase = system.GetComponent<SystemBase>();
        if (systemBase != null)
        {
            systemBase.Initialize();
            systemBase.IsInitialized = true;
            systemBase.gameObject.transform.SetParent(transform);
        }

        Systems.Add(system.GetComponent<SystemBase>());
        DEBUG.Log(systemType.FullName + " created...", Warning_Types.Good);
        return system.GetComponent<SystemBase>();
    }

    // Holds the game's loading progress til the sub-system is intialized
    public static void WaitForSubsystem(Component instance, Action OnExisting)
    {
        Instance.StartCoroutine(Instance.SpinUpWaitRoutine(instance, OnExisting));
    }

    // Holds the game's loading progress til a specific bool coniditon is met
    public static void WaitForCondition(Func<bool> predicate, Action OnMet)
    {
        Instance.StartCoroutine(Instance.SpinUpWaitCondition(predicate, OnMet));
    }

    IEnumerator SpinUpWaitRoutine(Component instance, Action OnExisting)
    {
        yield return new WaitWhile(() => instance != null);
        OnExisting.SafeInvoke();
    }

    IEnumerator SpinUpWaitCondition(Func<bool> predicate, Action OnMet)
    {
        yield return new WaitUntil(predicate);
        OnMet.SafeInvoke();
    }

    IEnumerator Delay()
    {
        DEBUG.Log("Delay happening...", Warning_Types.Log);
        yield return new WaitForSeconds(10f);
    }

    // Coroutine that gathers objectPool
    IEnumerator LoadUpObjectPool(Action OnExisting)
    {
        SystemBase sb = CreateSubsystem<ObjectPool>();
        yield return new WaitWhile(() => sb != null);
        OnExisting.SafeInvoke();
        StartCoroutine(LoadUpPTM(OnExisting));
    }

    // Coroutine that loads up the PlayerTankManager, as it necessary for the player to interact
    IEnumerator LoadUpPTM(Action OnExisting)
    {
        PlayerTankManager = CreateSubsystem<PlayerTankmanager>() as PlayerTankmanager;
        yield return new WaitWhile(() => PlayerTankManager != null);
        OnExisting.SafeInvoke();
        StartCoroutine(LoadUpUnlock(OnExisting));
    }

    // Coroutine that loads up the Audio Manager for all the in game audio
    IEnumerator LoadUpAudio(Action OnExisting)
    {
        SystemBase sb = CreateSubsystem<AudioManager>();
        yield return new WaitWhile(() => sb != null);
        OnExisting.SafeInvoke();
        StartCoroutine(LoadUpEvent(OnExisting));
    }

    // Coroutine that loads up the Event Manager, that certain scripts use to carry out events
    IEnumerator LoadUpEvent(Action OnExisting)
    {
        SystemBase sb = CreateSubsystem<EventManager>();
        yield return new WaitWhile(() => sb != null);
        OnExisting.SafeInvoke();
        SetLoadState(LoadStates.Connecting);
    }
}
