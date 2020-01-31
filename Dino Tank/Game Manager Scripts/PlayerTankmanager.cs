

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Declarinng the different enumerations for the different values
public enum tank : int { AMX, B2, Churchill, KV2, M3, M6, Maus, Sherman, Tiger, T34, A7V, Elephant, Ratte}; 
public enum dino : int {None, Stego, Trex, Tricera, Bronto, Kentrosaurus, Duckbill, Croc };
public enum resource : int { xp, amber, meteorite };
public enum cameraModes : int { topDown, thirdPerson}

public class PlayerTankmanager : SystemBase
{
    // Variables    
    public string playerName; 
    public bool isArcade = false;
    public int levelChoice;

    // Modes
    public arcadeMode _ArcadeMode = arcadeMode.survival;
    public cameraModes cameraMode;

    // Singleton
    private static PlayerTankmanager Inst; // Main Instance
    public static PlayerTankmanager instance
    {
        get
        {
            if (Inst)
            {
                return Inst;
            }

            Inst = FindObjectOfType(typeof(PlayerTankmanager)) as PlayerTankmanager; // Get the PlayerTankManager
            if (Inst)
            {
                DEBUG.Log("App already exists in scene, will use it while in dev... in the future it should only exist in boot scene");
                APP.PlayerTankManager = Inst;
                return Inst;
            }

            GameObject tankmanager = Instantiate(Resources.Load("Managers/PlayerTankmanager", typeof(GameObject)) as GameObject);
            if (tankmanager)
            {
                APP.PlayerTankManager = tankmanager.GetComponent<PlayerTankmanager>();
                Inst = tankmanager.GetComponentInChildren<PlayerTankmanager>();
                return Inst;
            }
            return null;
        }
    }
    
   // Main Attributes
    public static bool _DEBUG = true;
    public int viewedTutorial = 0;
    public int viewedArcadeTutorial = 0;
    public GameObject player;
    public bool loadFromPlayerPrefs = false;
    public tank SelectedTank = tank.T34; // default tank
    public dino SelectedDino = dino.Tricera; // default dino

    // Network 
    public gamemodeNetwork networkMode = gamemodeNetwork.noThreat;
    public int deathmatchKillsNeeded = 10;
    public float deathmatchTime = 180;
    public int spawnpointIndex;

    // Tank Values
    [SerializeField]
    public bool allowWASDControl = true;
    [Header("Player Resources")]
    [SerializeField]
    public float m_xp;
    [SerializeField]
    public int m_amber;
    [SerializeField]
    public int m_meteorite;
    [SerializeField]
    public float m_xp_Total;
    [SerializeField]
    public float m_coins;
    [SerializeField]
    public int m_curDay;

    // Player Loadout
    [Header("Player loadout")]
    public int m_unlockedSlots = 5;
    public string slotContents1 = "RTCBullet";
    public string slotContents2;
    public string slotContents3;
    public string slotContents4;
    public string slotContents5;
    public List<tank> m_unlockedTanks = new List<tank>();
    [Space]

    // used in editor for persistance
    [SerializeField]
    public tank selectedTank;

    // Tank Related to Show
    [SerializeField]
    public int showCosts;
    [SerializeField]
    public int showCurrency;
    [SerializeField]
    public int showInventory;
    [SerializeField]
    public int showTanks;
    [SerializeField]
    public int showDinos;
    [SerializeField]
    public int showPickUps;
    [SerializeField]
    public int showLevelConfigs;

    // Weapons and Loadouts
    [Space]
    [Header("Player Loadout")]
    public List<GameObject> m_Weapons = new List<GameObject>(); // All weapons in game
    public List<GameObject> m_loadout = new List<GameObject>(); // Player inventory reapons
    public bool loadoutsFull = false;

    [SerializeField]
    public List<GameObject> m_EquipmentAll = new List<GameObject>(); // Equipments in game
    public List<GameObject> m_equipmentPlayer = new List<GameObject>();  // Player equiments
    public List<GameObject> m_equipmentInstances = new List<GameObject>();  // Player equiments instances at runtime  
    public List<int> SlotCosts = new List<int>();
    public List<PowerUps> m_powerupsPlayer = new List<PowerUps>();

    [Header("Containers")]
    public List<GameObject> m_Tanks = new List<GameObject>();   
    public List<DinoEffectsContainer> m_DinoEffects = new List<DinoEffectsContainer>();
    public List<DropContainer> m_PickUps = new List<DropContainer>();
   
    [Header("Level Configuration")]
	public List<Level> m_Levels = new List<Level>(); // List of actual levels for the system

    public int m_Levelindex = 0; // This is the level number in the eaary that has been coded in
    public int m_pointIndex = 0; // This is the last level selector point that was selected
    public bool continuePressed = false; // Used for the MM continue button in order to return to previously played level

    // Scene related
    public string lastScene;
    [Header("Continue Mode")]
    public bool isContinue = false;
    public int laststackIndex = 0;
    public string lastLevelName = "";
    public bool loadUnlockedFromSaved = true;

    [Header("Technicals")]
    //public int numUnlockedLevels = 1;
    public bool returningFromComplete = false;
    public bool showHelpBoxes = true;

    [Header("Loyalty Rewards Setup")]
    public int m_unlockedPrizes;
    public int m_prizeSlot1;
    public int m_prizeSlot2;
    public int m_prizeSlot3;
    public int m_prizeSlot4;
    public int m_prizeSlot5;


    #region 
    public int usedMeteoriteToRevive;
    [SerializeField]
    public int dailyArcadeLives;
    [SerializeField]
    public List<int> highScore = new List<int>(); // High score value
    [SerializeField]
    public List<float> totalKills = new List<float>(); // Total kills
    [SerializeField]
    public List<int> wavesCompleted = new List<int>(); // Waves completed
    [SerializeField]
    public List<float> survivalTime = new List<float>(); // SUrvival Time
    [SerializeField]
    public int round = 1; // Game rounds
    [SerializeField]
    public List<playerTurn> matchRound = new List<playerTurn>();
    [SerializeField]
    public int BLUETeamTotalScoresVal; // Team Scores
    [SerializeField]
    public int REDTeamTotalScoresVal; // team Scores
    [SerializeField]
    public List<GameObject> redLabels = new List<GameObject>();
    [SerializeField]
    public List<GameObject> blueLabels = new List<GameObject>();
    public string highScoreText;
    public string killsText;
    public string wavesText;
    public string timeText;
    #endregion

    public override void Initialize(Action<SystemBase> OnInitialized = null)
    {
        if (Inst == null)
        {
            Inst = this;
        }

        if (IsInitialized)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
        LoadPlayerPrefs();

        DinoEffectsContainer firstItem;
        firstItem = new DinoEffectsContainer();
        m_DinoEffects.Insert(0, firstItem);
        
        if (m_loadout.Count == 0 || m_loadout[0] == null) // Add bullets to weapon
        {
            m_loadout.Clear();
            m_loadout.Add(Resources.Load("RTCBullet") as GameObject);
        }

        // Auto populate arcade bool in case we are loading into level direct
        Level currentMatchingLevel = PlayerTankmanager.GetLevelFromName(SceneManager.GetActiveScene().name);
        if (currentMatchingLevel != null)
        {
            isArcade = currentMatchingLevel.isArcade;
        }
        else
        {
            isArcade = false;
        }

        // overriding for the specific build 
        m_loadout.CleanList();

        IsInitialized = true;
        if (OnInitialized != null)
        {
            OnInitialized.Invoke(this);
        }
    }

    #region NewSceneLoad
    void OnEnable()
    {
        EventManager.StartListening("LevelObjectiveAchieved", AddLevelComplete, this); // called from level manager  
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        EventManager.StopListening("LevelObjectiveAchieved", AddLevelComplete);// called from level manager
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;       
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        LoadPlayerPrefs();
    }
    
    #endregion


    public static GameObject GetPlayer()
    {
        return instance.player;
    }

    void OnDestroy()
    {
        SavePlayerPrefs();
    }
   
    public void AddXP(float _xp)
    {
        m_xp += _xp;
        m_xp_Total += _xp;
    }

    public void LoadPlayerPrefs()
    {
        // Increasing the rounds in the match 
        for (int i = 0; i < matchRound.Count; i++)
        {
            if (PlayerPrefs.HasKey("Match Round " + i)) PlayerPrefs.GetString("Match Round " + i); else PlayerPrefs.SetString("Match Round " + i, matchRound.ToArray()[i].ToString());
        }

        if (PlayerPrefs.HasKey("RedScore")) REDTeamTotalScoresVal = PlayerPrefs.GetInt("RedScore"); else PlayerPrefs.SetInt("RedScore", REDTeamTotalScoresVal);
        if (PlayerPrefs.HasKey("BlueScore")) BLUETeamTotalScoresVal = PlayerPrefs.GetInt("BlueScore"); else PlayerPrefs.SetInt("BlueScore", BLUETeamTotalScoresVal);
        if (PlayerPrefs.HasKey("SDRound")) round = PlayerPrefs.GetInt("SDRound"); else PlayerPrefs.SetInt("SDRound", round);
        if (PlayerPrefs.HasKey("MeteoriteToRevive")) usedMeteoriteToRevive = PlayerPrefs.GetInt("MeteoriteToRevive"); else PlayerPrefs.SetInt("MeteoriteToRevive", usedMeteoriteToRevive);
        if (PlayerPrefs.HasKey("DailyArcadeLives")) dailyArcadeLives = PlayerPrefs.GetInt("DailyArcadeLives"); else PlayerPrefs.SetInt("DailyArcadeLives", dailyArcadeLives);

        for (int i = 0; i < highScore.Count; i++)
        {
            if (PlayerPrefs.HasKey("HighScore " + i)) highScore[i] = PlayerPrefs.GetInt("HighScore " + i); else PlayerPrefs.SetInt("HighScore " + i, highScore[i]);
            if (PlayerPrefs.HasKey("TotalKills " + i)) totalKills[i] = PlayerPrefs.GetFloat("TotalKills " + i); else PlayerPrefs.SetFloat("TotalKills " + i, totalKills[i]);
            if (PlayerPrefs.HasKey("Waves " + i)) wavesCompleted[i] = PlayerPrefs.GetInt("Waves " + i); else PlayerPrefs.SetInt("Waves " + i, wavesCompleted[i]);
            if (PlayerPrefs.HasKey("Time " + i)) survivalTime[i] = PlayerPrefs.GetFloat("Time " + i); else PlayerPrefs.SetFloat("Time " + i, survivalTime[i]);
        }

        playerName = PlayerPrefs.GetString("PlayerName", "none");
        if (PlayerPrefs.HasKey("PlayerXP")) m_xp = PlayerPrefs.GetFloat("PlayerXP"); else PlayerPrefs.SetFloat("PlayerXP", m_xp);
        if (PlayerPrefs.HasKey("PlayerAmber")) m_amber = PlayerPrefs.GetInt("PlayerAmber"); else PlayerPrefs.SetInt("PlayerAmber", m_amber);
        if (PlayerPrefs.HasKey("PlayerMeteorite")) m_meteorite = PlayerPrefs.GetInt("PlayerMeteorite"); else PlayerPrefs.SetInt("PlayerMeteorite", m_meteorite);
        if (PlayerPrefs.HasKey("PlayerXP_Total")) m_xp_Total = PlayerPrefs.GetFloat("PlayerXP_Total"); else PlayerPrefs.SetFloat("PlayerXP_Total", m_xp_Total);
        if (loadFromPlayerPrefs)
        {
            if (PlayerPrefs.HasKey("playerTank")) SelectedTank = (tank)PlayerPrefs.GetInt("playerTank"); else PlayerPrefs.SetInt("playerTank", (int)SelectedTank);
            if (PlayerPrefs.HasKey("playerDino")) SelectedDino = (dino)PlayerPrefs.GetInt("playerDino"); else PlayerPrefs.SetInt("playerDino", (int)SelectedDino);
        }

          // Loadout Parameteres
        if (PlayerPrefs.HasKey("slotContents1"))
            slotContents1 = PlayerPrefs.GetString("slotContents1", "");
        else
            PlayerPrefs.SetString("slotContents1", slotContents1);
        if (PlayerPrefs.HasKey("slotContents2"))
            slotContents2 = PlayerPrefs.GetString("slotContents2", "");
        else
            PlayerPrefs.SetString("slotContents2", slotContents2);
        if (PlayerPrefs.HasKey("slotContents3"))
            slotContents3 = PlayerPrefs.GetString("slotContents3", "");
        else
            PlayerPrefs.SetString("slotContents3", slotContents3);
        if (PlayerPrefs.HasKey("slotContents4"))
            slotContents4 = PlayerPrefs.GetString("slotContents4", "");
        else
            PlayerPrefs.SetString("slotContents4", slotContents4);
        if (PlayerPrefs.HasKey("slotContents5"))
            slotContents5 = PlayerPrefs.GetString("slotContents5", "");
        else
            PlayerPrefs.SetString("slotContents5", slotContents5);


        if (slotContents4.Length != 0)
            m_equipmentPlayer.Add(Instantiate(GetEquipmentBadgeFromName(slotContents4)));


        if (slotContents5.Length != 0)
            m_equipmentPlayer.Add(Instantiate(GetEquipmentBadgeFromName(slotContents5)));

        m_loadout.Clear();
        if (slotContents1.Length != 0)
        {
            for (int i = 0; i < m_Weapons.Count; i++)
            {
                if (slotContents1 == m_Weapons[i].name)
                {
                    m_loadout.Add(m_Weapons[i]);
                }                
            }
            for (int i = 0; i < m_Weapons.Count; i++)
            {
                if (slotContents2 == m_Weapons[i].name)
                {
                    m_loadout.Add(m_Weapons[i]);
                }
            }
            for (int i = 0; i < m_Weapons.Count; i++)
            {
                if (slotContents3 == m_Weapons[i].name)
                {
                    m_loadout.Add(m_Weapons[i]);
                }
            }

        }

        // unlocked levels
        // if (PlayerPrefs.HasKey("numUnlockedLevels")) numUnlockedLevels = PlayerPrefs.GetInt("numUnlockedLevels"); else PlayerPrefs.SetInt("numUnlockedLevels", numUnlockedLevels);
        if (PlayerPrefs.HasKey("laststackIndex")) laststackIndex = PlayerPrefs.GetInt("laststackIndex"); else PlayerPrefs.SetInt("laststackIndex", laststackIndex);
        if (PlayerPrefs.HasKey("m_pointIndex")) m_pointIndex = PlayerPrefs.GetInt("m_pointIndex"); else PlayerPrefs.SetInt("m_pointIndex", m_pointIndex);
        if (PlayerPrefs.HasKey("lastLevelName")) lastLevelName = PlayerPrefs.GetString("lastLevelName"); else PlayerPrefs.SetString("lastLevelName", lastLevelName);

        // tutorial 
        if (PlayerPrefs.HasKey("viewedTutorial")) viewedTutorial = PlayerPrefs.GetInt("viewedTutorial", 0); else PlayerPrefs.SetInt("viewedTutorial", viewedTutorial);


        if (PlayerPrefs.HasKey("m_curDay")) m_curDay = PlayerPrefs.GetInt("m_curDay", m_curDay); else PlayerPrefs.SetInt("m_curDay", m_curDay);
       
        //Prize slots
        if (PlayerPrefs.HasKey("UnlockedPrizes")) m_unlockedPrizes = PlayerPrefs.GetInt("UnlockedPrizes", m_unlockedPrizes);
        if (PlayerPrefs.HasKey("prizeBox1")) m_prizeSlot1 = PlayerPrefs.GetInt("prizeBox1", m_prizeSlot1);
        if (PlayerPrefs.HasKey("prizeBox2")) m_prizeSlot2 = PlayerPrefs.GetInt("prizeBox2", m_prizeSlot2);
        if (PlayerPrefs.HasKey("prizeBox3")) m_prizeSlot3 = PlayerPrefs.GetInt("prizeBox3", m_prizeSlot3);
        if (PlayerPrefs.HasKey("prizeBox4")) m_prizeSlot4 = PlayerPrefs.GetInt("prizeBox4", m_prizeSlot4);
        if (PlayerPrefs.HasKey("prizeBox5")) m_prizeSlot5 = PlayerPrefs.GetInt("prizeBox5", m_prizeSlot5);     

        // check for unlocked levels
        if (loadUnlockedFromSaved)
        {
            foreach (Level level in m_Levels)
            {
                if (PlayerPrefs.HasKey(level.levelName))
                {
                    int retrievedValue = PlayerPrefs.GetInt(level.levelName);
                    if (retrievedValue == 1)
                    {
                        level.isLocked = true;
                    }
                    else
                        level.isLocked = false;
                }
            }
        }
    }

    // Unlocking tanks and adding to the player prefs
    public void UnlockTank(tank _tank)
    {
        m_unlockedTanks.Add(_tank);
        SavePlayerPrefs();
    }

    // The amount of weapons player has
    public void SetWeaponQuantity(string _weaponName, int _quantity)
    {     
       PlayerPrefs.SetInt(_weaponName, _quantity);
       PlayerPrefs.Save();
    }

    // Getting the data for the weapons in the player prefs
    public int GetWeaponQuantity(string _weaponName)
    {
        if (PlayerPrefs.HasKey(_weaponName))
        {
            return PlayerPrefs.GetInt(_weaponName);
        }
        else
        {
            return 0;
        }
    }

    public void AddCollectedItem(collectableTypes _type, int _qty, Collect _colletRef)
    {
        switch (_type)
        {
            case collectableTypes.Amber: m_amber += _qty;         FindObjectOfType<LevelManager>().AmberCollected(_qty); break;
            case collectableTypes.Meteorite: m_meteorite += _qty; FindObjectOfType<LevelManager>().meteoriteCollected += _qty; break;
            case collectableTypes.FuelTank:
                player.GetComponent<RTCTankController>().CurrentFuel += _qty;
                if (player.GetComponent<RTCTankController>().CurrentFuel > player.GetComponent<RTCTankController>().maxFuel)
                {
                    player.GetComponent<RTCTankController>().CurrentFuel = player.GetComponent<RTCTankController>().maxFuel;
                }
                //loadoutsFull = false;
                break;
            case collectableTypes.FuelTankBig:
                player.GetComponent<RTCTankController>().CurrentFuel += _qty;
                if (player.GetComponent<RTCTankController>().CurrentFuel > player.GetComponent<RTCTankController>().maxFuel)
                {
                    player.GetComponent<RTCTankController>().CurrentFuel = player.GetComponent<RTCTankController>().maxFuel;
                }
                //loadoutsFull = false;
                break;
            case collectableTypes.FuelObj:
                player.GetComponent<RTCTankController>().CurrentFuel += _qty;
                if (player.GetComponent<RTCTankController>().CurrentFuel > player.GetComponent<RTCTankController>().maxFuel)
                {
                    player.GetComponent<RTCTankController>().CurrentFuel = player.GetComponent<RTCTankController>().maxFuel;
                }
                //loadoutsFull = false;
                break;
            case collectableTypes.Heal:
                if (player)
                {
                    player.GetComponent<RTCTankController>().SetLife(player.GetComponent<RTCTankController>().CurrentLife + _qty);
                    if (player.GetComponent<RTCTankController>().CurrentLife > player.GetComponent<RTCTankController>().MaxLife)
                     {
                        player.GetComponent<RTCTankController>().SetLife(player.GetComponent<RTCTankController>().MaxLife);
                     }
                }
                //loadoutsFull = false;
                break;
            default:
                // we do this if we are adding a weapon that already has a prefab in Resources and we need to add it
                FindOrAddWepon(_type.ToString(), _qty);
                break;
        }
        //UIManager.instance.GenerateAdditionIcon(_type, _qty, _colletRef);

        SavePlayerPrefs();
        EventManager.TriggerEvent("RefreshLoadout");
    }

    // Adding or Finding weapons in the container
    void FindOrAddWepon(string _weaponName, int _qty)
    {
        GameObject weapon = null;

        foreach(GameObject w in m_Weapons)
        {
            if(w.name == _weaponName)
            {
                weapon = w;
            }
        }

        if (!weapon)
        {
            DEBUG.LogWarning("Weapon drop (" + _weaponName + ") is not listed in PlayerTankManager weapons container, add it");
            return;
        }

        // cap QTY and save
        int currentQty = GetWeaponQuantity(_weaponName);
        int qtyCap = weapon.GetComponent<WeaponIcon>().ammoCap;

        for (int i = 0; i < m_loadout.Count; i++)
        {
            if(_weaponName == m_loadout[i].name)
            {
                SetWeaponQuantity(_weaponName, Math.Min(currentQty + _qty, qtyCap));
                return;
            }
        }

        // Limit Max Slots for weapons
        if (m_loadout.Count < 3)
        {
            m_loadout.Add(weapon);
            SetWeaponQuantity(_weaponName, Math.Min(currentQty + _qty, qtyCap));
        }
        
        // Loadouts if full
        if(m_loadout.Count >= 3)
        {
            loadoutsFull = true;
        }

        if (m_loadout.Count >= 1) slotContents1 = m_loadout[0].name;
        if (m_loadout.Count >= 2) slotContents2 = m_loadout[1].name;
        if (m_loadout.Count >= 3) slotContents3 = m_loadout[2].name;
        if (m_loadout.Count >= 4) slotContents4 = m_loadout[3].name;
        if (m_loadout.Count >= 5) slotContents5 = m_loadout[4].name;        
    }

    #region Arcade Statistics, Functions for arcade mode
    public void SetNewHighScore(string level)
    {
        switch (level)
        {
            case "ArcadeWavesHimalayas":
                if (ArcadeController.instance.sum > highScore[0])
                {
                    highScore[0] = (int)ArcadeController.instance.sum;
                    PlayerPrefs.SetInt("HighScore 0", highScore[0]);
                }
                break;
            case "ArcadeWavesLava":
                if (ArcadeController.instance.sum > highScore[1])
                {
                    highScore[1] = (int)ArcadeController.instance.sum;
                    PlayerPrefs.SetInt("HighScore 1", highScore[1]);
                }
                break;
            case "ArcadeWavesGrasslands":
                if (ArcadeController.instance.sum > highScore[2])
                {
                    highScore[2] = (int)ArcadeController.instance.sum;
                    PlayerPrefs.SetInt("HighScore 2", highScore[2]);
                }
                break;
            case "ArcadeWavesArctic":
                if (ArcadeController.instance.sum > highScore[3])
                {
                    highScore[3] = (int)ArcadeController.instance.sum;
                    PlayerPrefs.SetInt("HighScore 3", highScore[3]);
                }
                break;
            case "ArcadeWavesSwamp":
                if (ArcadeController.instance.sum > highScore[4])
                {
                    highScore[4] = (int)ArcadeController.instance.sum;
                    PlayerPrefs.SetInt("HighScore 4", highScore[4]);
                }
                break;
            case "Harmeet_Arcadetest":
                if (ArcadeController.instance.sum > highScore[5])
                {
                    highScore[5] = (int)ArcadeController.instance.sum;
                    PlayerPrefs.SetInt("HighScore 5", highScore[5]);
                }
                break;
            case "ArcadeWavesDesert":
                if (ArcadeController.instance.sum > highScore[6])
                {
                    highScore[6] = (int)ArcadeController.instance.sum;
                    PlayerPrefs.SetInt("HighScore 6", highScore[6]);
                }
                break;
        }
    }
    public void GetHighScore(string level)
    {
        
        switch (level)
        {
            case "ArcadeWavesHimalayas":
                PlayerPrefs.GetInt("HighScore 0", highScore[0]);
                highScoreText = highScore[0].ToString();
                break;
            case "ArcadeWavesLava":
                PlayerPrefs.GetInt("HighScore 1", highScore[1]);
                highScoreText = highScore[1].ToString();
                break;
            case "ArcadeWavesGrasslands":
                PlayerPrefs.GetInt("HighScore 2", highScore[2]);
                highScoreText = highScore[2].ToString();
                break;
            case "ArcadeWavesArctic":
                PlayerPrefs.GetInt("HighScore 3", highScore[3]);
                highScoreText = highScore[3].ToString();
                break;
            case "ArcadeWavesSwamp":
                PlayerPrefs.GetInt("HighScore 4", highScore[4]);
                highScoreText = highScore[4].ToString();
                break;
            case "Harmeet_Arcadetest":
                PlayerPrefs.GetInt("HighScore 5", highScore[5]);
                highScoreText = highScore[5].ToString();
                break;
            case "ArcadeWavesDesert":
                PlayerPrefs.GetInt("HighScore 6", highScore[6]);
                highScoreText = highScore[6].ToString();
                break;

        }
    }

    public void SetKillsAmount(string level)
    {
        switch (level)
        {
            case "ArcadeWavesHimalayas":
                if (ArcadeController.instance.killSum > totalKills[0])
                {
                    totalKills[0] = FindObjectOfType<LevelManager>().killsMade;
                    PlayerPrefs.SetFloat("TotalKills 0", totalKills[0]);
                }
                break;
            case "ArcadeWavesLava":
                if (ArcadeController.instance.killSum > totalKills[1])
                {
                    totalKills[1] = FindObjectOfType<LevelManager>().killsMade;
                    PlayerPrefs.SetFloat("TotalKills 1", totalKills[1]);
                }           
                break;
            case "ArcadeWavesGrasslands":
                if (ArcadeController.instance.killSum > totalKills[2])
                {
                    totalKills[2] = FindObjectOfType<LevelManager>().killsMade;
                    PlayerPrefs.SetFloat("TotalKills 2", totalKills[2]);
                }           
                break;
            case "ArcadeWavesArctic":
                if (ArcadeController.instance.killSum > totalKills[3])
                {
                    totalKills[3] = FindObjectOfType<LevelManager>().killsMade;
                    PlayerPrefs.SetFloat("TotalKills 3", totalKills[3]);
                }
                break;
            case "ArcadeWavesSwamp":
                if (ArcadeController.instance.killSum > totalKills[4])
                {
                    totalKills[4] = FindObjectOfType<LevelManager>().killsMade;
                    PlayerPrefs.SetFloat("TotalKills 4", totalKills[4]);
                }
                break;
            case "Harmeet_Arcadetest":
                if (ArcadeController.instance.killSum > totalKills[5])
                {
                    totalKills[5] = FindObjectOfType<LevelManager>().killsMade;
                    PlayerPrefs.SetFloat("TotalKills 5", totalKills[5]);
                }          
                break;
            case "ArcadeWavesDesert":
                if (ArcadeController.instance.killSum > totalKills[6])
                {
                    totalKills[6] = FindObjectOfType<LevelManager>().killsMade;
                    PlayerPrefs.SetFloat("TotalKills 6", totalKills[6]);
                }
                break;

        }
    }

    public void GetKillsAmount(string level)
    {
        switch (level)
        {
            case "ArcadeWavesHimalayas":
                PlayerPrefs.GetFloat("TotalKills 0", totalKills[0]);
                killsText = totalKills[0].ToString();
                break;
            case "ArcadeWavesLava":
                PlayerPrefs.GetFloat("TotalKills 1", totalKills[1]);
                killsText = totalKills[1].ToString();
                break;
            case "ArcadeWavesGrasslands":
                PlayerPrefs.GetFloat("TotalKills 2", totalKills[2]);
                killsText = totalKills[2].ToString();
                break;
            case "ArcadeWavesArctic":
                PlayerPrefs.GetFloat("TotalKills 3", totalKills[3]);
                killsText = totalKills[3].ToString();
                break;
            case "ArcadeWavesSwamp":
                PlayerPrefs.GetFloat("TotalKills 4", totalKills[4]);
                killsText = totalKills[4].ToString();
                break;
            case "Harmeet_Arcadetest":
                PlayerPrefs.GetFloat("TotalKills 5", totalKills[5]);
                killsText = totalKills[5].ToString();
                break;
            case "ArcadeWavesDesert":
                PlayerPrefs.GetFloat("TotalKills 6", totalKills[6]);
                killsText = totalKills[6].ToString();
                break;

        }
    }

    public void SetWavesAmount(string level)
    {
        switch (level)
        {
            case "ArcadeWavesHimalayas":
                if (ArcadeController.instance.waveSum > wavesCompleted[0])
                {
                    wavesCompleted[0] = EnemyWaveController.instance.currentWaveIndex;
                    PlayerPrefs.SetInt("Waves 0", wavesCompleted[0]);
                }
                break;
            case "ArcadeWavesLava":
                if (ArcadeController.instance.waveSum > wavesCompleted[1])
                {
                    wavesCompleted[1] = EnemyWaveController.instance.currentWaveIndex;
                    PlayerPrefs.SetInt("Waves 1", wavesCompleted[1]);
                }
                break;
            case "ArcadeWavesGrasslands":
                if (ArcadeController.instance.waveSum > wavesCompleted[2])
                {
                    wavesCompleted[2] = EnemyWaveController.instance.currentWaveIndex;
                    PlayerPrefs.SetInt("Waves 2", wavesCompleted[2]);
                }
                break;
            case "ArcadeWavesArctic":
                if (ArcadeController.instance.waveSum > wavesCompleted[3])
                {
                    wavesCompleted[3] = EnemyWaveController.instance.currentWaveIndex;
                    PlayerPrefs.SetInt("Waves 3", wavesCompleted[3]);
                }
                break;
            case "ArcadeWavesSwamp":
                if (ArcadeController.instance.waveSum > wavesCompleted[4])
                {
                    wavesCompleted[4] = EnemyWaveController.instance.currentWaveIndex;
                    PlayerPrefs.SetInt("Waves 4", wavesCompleted[4]);
                }
                break;
            case "Harmeet_Arcadetest":
                if (ArcadeController.instance.waveSum > wavesCompleted[5])
                {
                    wavesCompleted[5] = EnemyWaveController.instance.currentWaveIndex;
                    PlayerPrefs.SetInt("Waves 5", wavesCompleted[5]);
                }
                break;
            case "ArcadeWavesDesert":
                if (ArcadeController.instance.waveSum > wavesCompleted[6])
                {
                    wavesCompleted[6] = EnemyWaveController.instance.currentWaveIndex;
                    PlayerPrefs.SetInt("Waves 6", wavesCompleted[6]);
                }
                break;

        }
    }

    public void GetWavesAmount(string level)
    {
        switch (level)
        {
            case "ArcadeWavesHimalayas":
                PlayerPrefs.GetInt("Waves 0", wavesCompleted[0]);
                wavesText = wavesCompleted[0].ToString();
                break;
            case "ArcadeWavesLava":
                PlayerPrefs.GetInt("Waves 1", wavesCompleted[1]);
                wavesText = wavesCompleted[1].ToString();
                break;
            case "ArcadeWavesGrasslands":
                PlayerPrefs.GetInt("Waves 2", wavesCompleted[2]);
                wavesText = wavesCompleted[2].ToString();
                break;
            case "ArcadeWavesArctic":
                PlayerPrefs.GetInt("Waves 3", wavesCompleted[3]);
                wavesText = wavesCompleted[3].ToString();
                break;
            case "ArcadeWavesSwamp":
                PlayerPrefs.GetInt("Waves 4", wavesCompleted[4]);
                wavesText = wavesCompleted[4].ToString();
                break;
            case "Harmeet_Arcadetest":
                PlayerPrefs.GetInt("Waves 5", wavesCompleted[5]);
                wavesText = wavesCompleted[5].ToString();
                break;
            case "ArcadeWavesDesert":
                PlayerPrefs.GetInt("Waves 6", wavesCompleted[6]);
                wavesText = wavesCompleted[6].ToString();
                break;

        }
    }

    public void SetTimeAmount(string level)
    {
        switch (level)
        {
            case "ArcadeWavesHimalayas":
                if (ArcadeController.instance.timeSum > survivalTime[0])
                {
                    survivalTime[0] += FindObjectOfType<LevelManager>().levelTime;
                    PlayerPrefs.SetFloat("Time 0", survivalTime[0]);
                }
                break;
            case "ArcadeWavesLava":
                if (ArcadeController.instance.timeSum > survivalTime[1])
                {
                    survivalTime[1] += FindObjectOfType<LevelManager>().levelTime;
                    PlayerPrefs.SetFloat("Time 1", survivalTime[1]);
                }
                break;
            case "ArcadeWavesGrasslands":
                if (ArcadeController.instance.timeSum > survivalTime[2])
                {
                    survivalTime[2] += FindObjectOfType<LevelManager>().levelTime;
                    PlayerPrefs.SetFloat("Time 2", survivalTime[2]);
                }
                break;
            case "ArcadeWavesArctic":
                if (ArcadeController.instance.timeSum > survivalTime[3])
                {
                    survivalTime[3] += FindObjectOfType<LevelManager>().levelTime;
                    PlayerPrefs.SetFloat("Time 3", survivalTime[3]);
                }
                break;
            case "ArcadeWavesSwamp":
                if (ArcadeController.instance.timeSum > survivalTime[4])
                {
                    survivalTime[4] += FindObjectOfType<LevelManager>().levelTime;
                    PlayerPrefs.SetFloat("Time 4", survivalTime[4]);
                }
                break;
            case "Harmeet_Arcadetest":
                if (ArcadeController.instance.timeSum > survivalTime[5])
                {
                    survivalTime[5] += FindObjectOfType<LevelManager>().levelTime;
                    PlayerPrefs.SetFloat("Time 5", survivalTime[5]);
                }
                break;
            case "ArcadeWavesDesert":
                if (ArcadeController.instance.timeSum > survivalTime[6])
                {
                    survivalTime[6] += FindObjectOfType<LevelManager>().levelTime;
                    PlayerPrefs.SetFloat("Time 6", survivalTime[6]);
                }
                break;

        }
    }

    public void GetTimeAmount(string level)
    {
        switch (level)
        {
            case "Arcade_Himalayas":
                PlayerPrefs.GetFloat("Time 0", survivalTime[0]);
                timeText = ConverTimeToMinutesSeconds(survivalTime[0]);
                break;
            case "Arcade_Lava":
                PlayerPrefs.GetFloat("Time 1", survivalTime[1]);
                timeText = ConverTimeToMinutesSeconds(survivalTime[1]);
                break;
            case "Arcade_Arctic":
                PlayerPrefs.GetFloat("Time 2", survivalTime[2]);
                timeText = ConverTimeToMinutesSeconds(survivalTime[2]);
                break;
            case "Arcade_Swamp":
                PlayerPrefs.GetFloat("Time 3", survivalTime[3]);
                timeText = ConverTimeToMinutesSeconds(survivalTime[3]);
                break;
            case "Arcade_Desert":
                PlayerPrefs.GetFloat("Time 4", survivalTime[4]);
                timeText = ConverTimeToMinutesSeconds(survivalTime[4]);
                break;

        }
    }

    string ConverTimeToMinutesSeconds(float _timer)
    {
        int minutes = Mathf.FloorToInt(_timer / 60F);
        int seconds = Mathf.FloorToInt(_timer - minutes * 60);
        return string.Format("{0:0}:{1:00}", minutes, seconds);

    }

    #endregion Arcade Statistics

    public void SavePlayerPrefs()
    {
      for (int i = 0; i < matchRound.Count; i++)
      {
          if (PlayerPrefs.HasKey("Match Round " + i)) PlayerPrefs.SetString("Match Round " + i, matchRound.ToArray()[i].ToString());
      }

        if (PlayerPrefs.HasKey("RedScore")) PlayerPrefs.SetInt("RedScore", REDTeamTotalScoresVal);
        if (PlayerPrefs.HasKey("BlueScore")) PlayerPrefs.SetInt("BlueScore", BLUETeamTotalScoresVal);
        if (PlayerPrefs.HasKey("SDRound")) PlayerPrefs.SetInt("SDRound", round);
        if (PlayerPrefs.HasKey("MeteoriteToRevive")) PlayerPrefs.SetInt("MeteoriteToRevive", usedMeteoriteToRevive);
        if (PlayerPrefs.HasKey("DailyArcadeLives")) PlayerPrefs.SetInt("DailyArcadeLives", dailyArcadeLives);
        for (int i = 0; i < highScore.Count; i++)
        {
            if (PlayerPrefs.HasKey("HighScore " + i)) PlayerPrefs.SetInt("HighScore " + i, highScore[i]);
            if (PlayerPrefs.HasKey("TotalKills " + i)) PlayerPrefs.SetFloat("TotalKills " + i, totalKills[i]);
            if (PlayerPrefs.HasKey("Waves " + i)) PlayerPrefs.SetInt("Waves " + i, wavesCompleted[i]);
            if (PlayerPrefs.HasKey("Time " + i)) PlayerPrefs.SetFloat("Time " + i, survivalTime[i]);
        }

        if (PlayerPrefs.HasKey("PlayerXP")) PlayerPrefs.SetFloat("PlayerXP", m_xp);
        if (PlayerPrefs.HasKey("PlayerAmber")) PlayerPrefs.SetInt("PlayerAmber", m_amber);
        if (PlayerPrefs.HasKey("PlayerMeteorite")) PlayerPrefs.SetInt("PlayerMeteorite", m_meteorite);
        if (PlayerPrefs.HasKey("PlayerXP_Total")) PlayerPrefs.SetFloat("PlayerXP_Total", m_xp_Total);
        if (loadFromPlayerPrefs)
        {
            if (PlayerPrefs.HasKey("playerTank")) PlayerPrefs.SetInt("playerTank", (int)SelectedTank);
            if (PlayerPrefs.HasKey("playerDino")) PlayerPrefs.SetInt("playerDino", (int)SelectedDino);
        }

        //Unlock slots
        if (PlayerPrefs.HasKey("UnlockedSlots"))
            PlayerPrefs.SetInt("UnlockedSlots", m_unlockedSlots);
        if (PlayerPrefs.HasKey("slotContents1"))
            PlayerPrefs.SetString("slotContents1", slotContents1);
        if (PlayerPrefs.HasKey("slotContents2"))
            PlayerPrefs.SetString("slotContents2", slotContents2);
        if (PlayerPrefs.HasKey("slotContents3"))
            PlayerPrefs.SetString("slotContents3", slotContents3);
        if (PlayerPrefs.HasKey("slotContents4"))
            PlayerPrefs.SetString("slotContents4", slotContents4);
        if (PlayerPrefs.HasKey("slotContents5"))
            PlayerPrefs.SetString("slotContents5", slotContents5);

        if (PlayerPrefs.HasKey("laststackIndex")) PlayerPrefs.SetInt("laststackIndex", laststackIndex);
        if (PlayerPrefs.HasKey("m_pointIndex"))  PlayerPrefs.SetInt("m_pointIndex", m_pointIndex);
        if (PlayerPrefs.HasKey("lastLevelName")) PlayerPrefs.SetString("lastLevelName", lastLevelName);


        if (PlayerPrefs.HasKey("viewedTutorial"))  PlayerPrefs.SetInt("viewedTutorial", viewedTutorial);
  
        // Unlocked tanks are saved
        foreach (tank item in m_unlockedTanks)
        {
            if (!PlayerPrefs.HasKey(item.ToString()))
                PlayerPrefs.SetString(item.ToString(), item.ToString());
        }
        // Save unlocked levels
        foreach (Level level in m_Levels)
        {
            if (level.isLocked)
                PlayerPrefs.SetInt(level.levelName, 1);
            else
                PlayerPrefs.SetInt(level.levelName, 0);
        }

        //Prize slots
        if (PlayerPrefs.HasKey("UnlockedPrizes")) PlayerPrefs.SetInt("UnlockedPrizes", m_unlockedPrizes);
        if (PlayerPrefs.HasKey("prizeBox1")) PlayerPrefs.SetInt("prizeBox1", m_prizeSlot1);
        if (PlayerPrefs.HasKey("prizeBox2")) PlayerPrefs.SetInt("prizeBox2", m_prizeSlot2);
        if (PlayerPrefs.HasKey("prizeBox3")) PlayerPrefs.SetInt("prizeBox3", m_prizeSlot3);
        if (PlayerPrefs.HasKey("prizeBox4")) PlayerPrefs.SetInt("prizeBox4", m_prizeSlot4);
        if (PlayerPrefs.HasKey("prizeBox5")) PlayerPrefs.SetInt("prizeBox5", m_prizeSlot5);
        PlayerPrefs.Save();  
    }

    // Resetting player tanks values
    public void ResetPlayerPrefs()
    {
        m_xp = 0;
        m_amber = 0;
        m_meteorite = 0;
        m_xp_Total = 0;
        dailyArcadeLives = 3;
        usedMeteoriteToRevive = 0;
        round = 1;
        REDTeamTotalScoresVal = 0;
        BLUETeamTotalScoresVal = 0;
        for(int i = 0; i < highScore.Count; i++)
        {
            highScore[i] = 0;
            survivalTime[i] = 0;
            totalKills[i] = 0;
            wavesCompleted[i] = 0;
        }

        int numEnums = System.Enum.GetValues(typeof(tank)).Length;
        for (int i = 0; i < numEnums; i++)
        {
            PlayerPrefs.SetInt(((tank)i).ToString() + "_ATK", 0 );
            PlayerPrefs.SetInt(((tank)i).ToString() + "_DEF", 0 );
            PlayerPrefs.SetInt(((tank)i).ToString() + "_SPD", 0 );
        }

      
        m_unlockedSlots = 5;
        slotContents1 = "RTCBullet";
        slotContents2 = "";
        slotContents3 = "";
        slotContents4 = "";
        slotContents5 = "";

        lastLevelName = "";
        m_pointIndex = 0;
        laststackIndex = 0;

        viewedTutorial = 0;

        for (int i = 0; i < m_Weapons.Count; i++)
        {
            if (PlayerPrefs.HasKey(m_Weapons[i].gameObject.name))
            {
                PlayerPrefs.DeleteKey(m_Weapons[i].gameObject.name);
            }
        }

        string[] enumNames = System.Enum.GetNames(typeof(tank));
        for (int i = 0; i < enumNames.Length; i++)
        {
            if (PlayerPrefs.HasKey(enumNames[i]))
                PlayerPrefs.DeleteKey(enumNames[i]);
        }

        m_unlockedTanks.Clear();

    

        SavePlayerPrefs();
    }
 
    // Getting current scene and checking if complete, then add level complete
    public void AddLevelComplete()
    {
        Level curentLevel = GetLevelFromName(SceneManager.GetActiveScene().name);
        if (curentLevel != null)
        {
            curentLevel.isLocked = false;
            PlayerPrefs.SetInt(curentLevel.levelName, 0);
            PlayerPrefs.Save();
        }
        returningFromComplete = true;
        CheckForGameEnd();
    }

    // Adding checkpoint for level
    public void AddCheckPointForLevel(string _levelName, int checkpointIndex)
    {
        if(_levelName.Contains("_checkpoint"))
        {
            PlayerPrefs.SetInt(_levelName, checkpointIndex);
        }
        else
        {
            PlayerPrefs.SetInt(_levelName + "_checkpoint", checkpointIndex);
        }
        PlayerPrefs.Save();
    }

    // Get last checkpoint in level
    public int GetLastCheckpoint(string _levelName)
    {
        int lastCheckpointValue = 0;
        if (PlayerPrefs.HasKey(_levelName + "_checkpoint"))
        {
            lastCheckpointValue = PlayerPrefs.GetInt(_levelName + "_checkpoint");
            return lastCheckpointValue;
        }
        else
        {
            return 0;
        }
    }

    void CheckForGameEnd()
    {
            Debug.Log("TODO!!!!! Write implementation for level complete ");
    }

    /// This is needed for sorting equipments that are not clickable to the end of the UI display
    public void SortEquipmentListPermanentToEnd()
    {
        List<GameObject> sortedList = new List<GameObject>();
        for (int i = 0; i < m_equipmentInstances.Count; i++)
        {
            if (m_equipmentInstances[i].GetComponent<Equipment>().equipmentType != equipmentTypes.permanent)
            {
                sortedList.Add(m_equipmentInstances[i]);
            }
        }
        for (int i = 0; i < m_equipmentInstances.Count; i++)
        {
            if (m_equipmentInstances[i].GetComponent<Equipment>().equipmentType == equipmentTypes.permanent)
            {
                sortedList.Add(m_equipmentInstances[i]);
            }
        }
        for (int i = 0; i < sortedList.Count; i++)
        {
            m_equipmentInstances[i] = sortedList[i];
        }
    }

    public List<GameObject> GetAllTanksOfTier(tiers _tier, bool _onlyUnlocked = false)
    {
        //New list of game objects
        List<GameObject> newList = new List<GameObject>();

        //For loop through m_tanks.count
        for (int i = 0; i < m_Tanks.Count; i++)
        {
            //If only unlocked = true
            if (!_onlyUnlocked)
            {
                //If tank tier is equal to what we want
                if (m_Tanks[i].GetComponent<RTCTankController>().tier == _tier)
                {
                    //Add that tank
                    newList.Add(m_Tanks[i]);
                }
            }
            else
            {
                if (m_Tanks[i].GetComponent<RTCTankController>().tier == _tier && m_Tanks[i].GetComponent<RTCTankController>().availableToPlayer)
                {
                    newList.Add(m_Tanks[i]);
                }
            }
        }
        return newList;
    }

    // Get current level number from the current scene using its index
    public static int GetLevelIndexFromName(string _name)
    {
        for (int i = 0; i < instance.m_Levels.Count; i++)
        {
            if (instance.m_Levels[i].levelName == _name)
                return i;
        }
        return 0;
    }

    public static bool IsInBattleLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Level level = GetLevelFromName(currentScene);
        if (level != null)
        {
            return level.gameLevel;
        }
        else
        {
            DEBUG.Log(currentScene + " level is not listed in PTM levels container, add it", Warning_Types.Warning);
            return false;
        }
    }

    public static Sprite GetIconFromDino(dino _dino, bool getSilhette = false)
    {
        for (int i = 0; i < instance.m_DinoEffects.Count; i++)
        {
            if (_dino == instance.m_DinoEffects[i].m_dino)
            {
                if (getSilhette)
                    return instance.m_DinoEffects[i].iconSilhuette;
                else
                    return instance.m_DinoEffects[i].iconNormal;
            }
        }
        return null;
    }

    public static DinoEffectsContainer GetEffectFromDino(dino _dino)
    {
        foreach (DinoEffectsContainer item in instance.m_DinoEffects)
        {
            if (item.m_dino == _dino) return item;
        }
        return null;
    }

    public static GameObject GetEquipmentBadgeFromName(string _name)
    {
        for (int i = 0; i < instance.m_EquipmentAll.Count; i++)
        {
            if (instance.m_EquipmentAll[i].name == _name)
                return instance.m_EquipmentAll[i];
        }
        return null;
    }

    // Applying all the effects to the tanks
    public void ApplyEffects()
    {
        
        player.GetComponent<RTCTankController>().GetComponent<Rigidbody>().mass += this.m_DinoEffects[(int)SelectedDino].delataMass; // delataMass;
        player.GetComponent<RTCTankController>().SetMaxLife(player.GetComponent<RTCTankController>().MaxLife + this.m_DinoEffects[(int)SelectedDino].delataMaxLife);  //delataMaxLife);
        player.GetComponent<RTCTankController>().SetLife(player.GetComponent<RTCTankController>().MaxLife + this.m_DinoEffects[(int)SelectedDino].delataMaxLife);
        player.GetComponent<RTCTankController>().EngineTorque += this.m_DinoEffects[(int)SelectedDino].delataEngineTorque;
        player.GetComponent<RTCTankController>().engineRPM += this.m_DinoEffects[(int)SelectedDino].delataMaxRPM;
        player.GetComponent<RTCTankController>().SetMaxSpeed(player.GetComponent<RTCTankController>().speed + (this.m_DinoEffects[(int)SelectedDino].delataMaxSpeed * player.GetComponent<RTCTankController>().speed));
        player.GetComponent<RTCTankController>().maximumAngularVelocity += this.m_DinoEffects[(int)SelectedDino].delataAngularVelovity;
    }
}

[System.Serializable]
public class Level
{  
    public bool gameLevel = false;
    public string levelName = "none";
    public string displayName = "";
	public string levelInfo = "Description here.";
    public bool isMultiplayer = false;
    public bool isLocked = false;
    public bool isArcade = false;
    public bool isSinglePlayer = false;
    public Sprite icon;
}

[System.Serializable]
public class UnlockTimeContainer
{   
   // Time related variables
   public int hours = 0;
   public int minutes = 0;
   public int seconds = 30;
}

[System.Serializable]
public class DinoEffectsContainer
{
    // Tank details
    public dino m_dino = dino.None;
    public string dinoName = "Name of Dino";
    public string dinoDescription = "The general overview of the dino goes here";
    public string effectDescription = "Outline of what the effects of this dino are";

    // Variables for slots
    public bool isLocked = false;
    public Sprite iconNormal;
    public Sprite iconSilhuette;
    public int pwrSlotsToDrop;
    public int defSlotsToDrop;
    public int spdSlotsToDrop;

    // All Tank Movement and Fire Variables
    public float delataMass = 1000;
    public float delataMaxLife = 0;
    public float delataEngineTorque;
    public float delataMaxRPM = 7000;
    public float delataMaxSpeed = 0;
    public float delataAngularVelovity = 0.1f;
    public float deltaFireRate = 0.2f;
    public float deltaDamage = 20;

    // Settig all the effects that are present in the game
    public void ApplyEffects(RTCTankController _controller)
    {
        _controller.ResetToBackUpValuesForUpgrades();
        _controller.GetComponent<Rigidbody>().mass += delataMass;
        _controller.SetMaxLife(_controller.MaxLife + delataMaxLife);
        _controller.SetLife(_controller.MaxLife + delataMaxLife);
        _controller.EngineTorque += delataEngineTorque;
        _controller.engineRPM += delataMaxRPM;
        _controller.SetMaxSpeed(_controller.MaxSpeed + (delataMaxSpeed * _controller.MaxSpeed));
        _controller.maximumAngularVelocity += delataAngularVelovity;
        _controller.fireRate += deltaFireRate;
        _controller.damageMult += deltaDamage;
    }
}