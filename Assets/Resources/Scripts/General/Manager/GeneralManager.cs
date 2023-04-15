using UnityEngine;
public class GeneralManager : MonoBehaviour
{
    [SerializeField]
    GameObject fightSectionPrefab;
    GameObject fightSectionInstance;
    FightManager fightManager;

    [SerializeField]
    GameObject rogueSectionPrefab;
    GameObject rogueSectionInstance;
    RogueManager rogueManager;

    RunData runData;

    CurrentSection currentSection = CurrentSection.Rogue;

    public bool IsGameInStandby { get { return IsGameInStandbyMethod(); } }

	private void Start()
	{
        PRNG seed = new(Random.Range(0, 100));
        runData = new RunData(0, seed, 0);
        GenerateRogueSection();
    }

	bool IsGameInStandbyMethod()
	{
		return currentSection switch
		{
			CurrentSection.Fight => fightManager.IsAnyUnitMoving || fightManager.IsOptionOpen || fightManager.CurrentTurn != 0,
			CurrentSection.Rogue => rogueManager.IsAnyUnitMoving,
			_ => false,
		};
	}

    public void StartFight()
	{
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
        runData = new(rogueManager.currentNode, rogueManager.seed, rogueManager.playerUnitTransform.position.x);
        rogueManager.DisableRogueSection();
        Destroy(rogueSectionInstance);
    }

    void GenerateFightSection()
	{
        fightSectionInstance = Instantiate(fightSectionPrefab);
        fightManager = GameObject.Find("Fight Manager").GetComponent<FightManager>();
	}

    void GenerateRogueSection()
    {
        rogueSectionInstance = Instantiate(rogueSectionPrefab);
        rogueManager = GameObject.Find("Rogue Manager").GetComponent<RogueManager>();
        rogueManager.SetupRogue(runData.currentNode, runData.seed, runData.playerX);
    }

    public void ReturnToRogueFromFightButton()
	{
        DestroyFightSection();
        GenerateRogueSection();
    }
    struct RunData
    {
        public int currentNode;
        public PRNG seed;
        public float playerX;

        public RunData(int currentNode, PRNG seed, float playerX)
        {
            this.currentNode = currentNode;
            this.seed = seed;
            this.playerX = playerX;
        }
    }

    enum CurrentSection
	{
        Fight,
        Rogue
	}
}
