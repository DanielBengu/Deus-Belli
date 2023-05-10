using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralManager : MonoBehaviour
{
    public const string GOD_SELECTED_PP = "God Selected";
    public const string ONGOING_RUN = "OngoingRun";
    public const string CURRENT_NODE = "CurrentNode";
    public const string SEED = "Seed";
    public const string GOLD = "Gold";

    public const int SCROLL_WHEEL_BUTTON = 2;
    public const string FIGHT_MANAGER_OBJ_NAME = "Fight Manager";
    public const string ROGUE_MANAGER_OBJ_NAME = "Rogue Manager";
    public const string GENERAL_MANAGER_OBJ_NAME = "General Manager";

    [SerializeField]
    GameObject fightSectionPrefab;
    GameObject fightSectionInstance;
    FightManager fightManager;

    [SerializeField]
    GameObject rogueSectionPrefab;
    GameObject rogueSectionInstance;
    RogueManager rogueManager;

    [SerializeField]
    CameraManager cameraManager;
    [SerializeField]
    StructureManager structureManager;

    [SerializeField]
    GameObject OptionsPrefab;

    RunData runData;

    CurrentSection currentSection = CurrentSection.Rogue;

    public bool IsScrollButtonDown { get; set; }
    public bool IsOptionOpen { get; set; }
    public bool IsGameInStandby { get { return IsGameInStandbyMethod(); } }
	public int Gold { get { return runData.gold; } set { runData.gold = value; } }
    public string GodSelected { get { return runData.godSelected; } set { runData.godSelected = value; } }

	private void Start()
	{
        bool isOngoingRun = PlayerPrefs.GetInt(ONGOING_RUN) != 0;
        if (isOngoingRun)
            runData = LoadRunData();
		else
		{
            string godSelected = PlayerPrefs.GetString(GOD_SELECTED_PP);
            int baseSeed = Random.Range(0, 1000);
            PRNG seed = new(baseSeed);
            runData = new RunData(godSelected, 0, seed, 0);

            PlayerPrefs.SetInt(SEED, baseSeed);
            PlayerPrefs.SetInt(GOLD, 0);
            PlayerPrefs.SetInt(CURRENT_NODE, 0);
            PlayerPrefs.SetInt(ONGOING_RUN, 1);
        }

        GenerateRogueSection();
    }

	private void Update()
	{
        if (!IsGameInStandby && (Input.anyKeyDown || IsScrollButtonDown))
            ManageKeysDown();

        float scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheelInput != 0f)
            switch (currentSection)
            {
                case CurrentSection.Fight:
                    cameraManager.ScrollWheel(scrollWheelInput);
                    break;
                case CurrentSection.Rogue:
                    cameraManager.ScrollWheel(scrollWheelInput, rogueSectionInstance.transform);
                    rogueManager.GenerateNewNodeLines();
                    break;
            }

        if (IsScrollButtonDown)
            cameraManager.UpdatePosition();
    }

    RunData LoadRunData()
	{
        PRNG seed = new(PlayerPrefs.GetInt(SEED));
        string godSelected = PlayerPrefs.GetString(GOD_SELECTED_PP);
        int currentNode = PlayerPrefs.GetInt(CURRENT_NODE);
        int gold = PlayerPrefs.GetInt(GOLD);
        return new RunData(godSelected, currentNode, seed, gold);
	}

	public void SaveMapProgress()
	{
        PlayerPrefs.SetInt(CURRENT_NODE, runData.currentNode);
    }

    void ManageKeysDown()
	{
        if (Input.GetKeyDown(KeyCode.Escape))
        {
			if (!IsOptionOpen)
			{
                MainMenu.GeneratePrefab(OptionsPrefab, "Options");
                IsOptionOpen = true;
            }
        } else
        if (Input.GetMouseButtonDown(SCROLL_WHEEL_BUTTON))
        {
            IsScrollButtonDown = true;
        }
        else if (Input.GetMouseButtonUp(SCROLL_WHEEL_BUTTON))
        {
            IsScrollButtonDown = false;
        }
    }

    bool IsGameInStandbyMethod()
	{
		return currentSection switch
		{
			CurrentSection.Fight => fightManager.IsAnyUnitMoving || IsOptionOpen || fightManager.CurrentTurn != 0 || fightManager.isGameOver,
			CurrentSection.Rogue => rogueManager.IsAnyUnitMoving || rogueManager.IsGameOver || IsOptionOpen,
			_ => false,
		};
	}

    public void StartFight()
	{
        cameraManager.ResetCamera();

        DestroyRogueSection();
        GenerateFightSection();

        currentSection = CurrentSection.Fight;
    }

    void DestroyFightSection()
	{
        fightManager.DisableFightSection();
        Destroy(fightSectionInstance);
    }

    void DestroyRogueSection()
    {
        runData = new(GodSelected, rogueManager.currentNode, rogueManager.seed, Gold);
        Destroy(rogueSectionInstance);
    }

    void GenerateFightSection()
	{
        fightSectionInstance = Instantiate(fightSectionPrefab);
        fightManager = GameObject.Find(FIGHT_MANAGER_OBJ_NAME).GetComponent<FightManager>();
        fightManager.structureManager = structureManager;
        fightManager.cameraManager = cameraManager;
        fightManager.generalManager = this;
        fightManager.structureManager.uiManager.SetFightVariables();
        fightManager.structureManager.spriteManager.fightManager = fightManager;
	}

    void GenerateRogueSection()
    {
        rogueSectionInstance = Instantiate(rogueSectionPrefab);
        rogueManager = GameObject.Find(ROGUE_MANAGER_OBJ_NAME).GetComponent<RogueManager>();
        rogueManager.SetupRogue(structureManager, runData.currentNode, runData.seed);
        rogueManager.structureManager.uiManager.SetRogueVariables(Gold, GodSelected);
        currentSection = CurrentSection.Rogue;

        rogueManager.IsRunCompleted();
    }

    public static void CloseRun()
	{
        PlayerPrefs.SetInt(ONGOING_RUN, 0);
        SceneManager.LoadScene(0);
    }

    public void ReturnToRogueFromFightButton()
	{
        cameraManager.ResetCamera();

        DestroyFightSection();
        GenerateRogueSection();
    }
    struct RunData
    {
        public string godSelected;
        public int currentNode;
        public PRNG seed;

        public int gold;

        public RunData(string godSelected,int currentNode, PRNG seed, int gold)
        {
            this.godSelected = godSelected;
            this.currentNode = currentNode;
            this.seed = seed;
            this.gold = gold;
        }
    }

    enum CurrentSection
	{
        Fight,
        Rogue
	}
}
