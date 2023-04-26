using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RogueManager : MonoBehaviour
{
    GeneralManager generalManager;
    public StructureManager structureManager;

    public RogueTile origin;
    public GameObject tile;
    public GameObject link;
    readonly List<GameObject> mapObjectsList = new();

    public Transform playerUnitTransform;

    public int currentNode;
	readonly int maxNode = 5;
    public PRNG seed;

    public bool IsAnyUnitMoving { get { return structureManager.IsObjectMoving; } }

    public bool IsGameInStandby { get { return generalManager.IsGameInStandby; } }

    // Start is called before the first frame update
    void Start()
    {
        generalManager = GameObject.Find(GeneralManager.GENERAL_MANAGER_OBJ_NAME).GetComponent<GeneralManager>();
    }

	private void Update()
	{
        bool isPlayerMovementFinished = structureManager.MovementTick();
        if (isPlayerMovementFinished)
            generalManager.StartFight();
    }

    public void IsRunCompleted()
	{
        if (currentNode == maxNode)
            structureManager.GetRogueVictoryScreen();
	}

    public void SetupRogue(StructureManager structureManager, int currentNode, PRNG seed, float playerX)
	{
        this.currentNode = currentNode;
        this.seed = seed;
        this.structureManager = structureManager;
        if(playerX != 0)
            playerUnitTransform.position = new Vector3(playerX, playerUnitTransform.position.y, playerUnitTransform.position.z);
        GenerateMap();
    }

	void GenerateMap()
	{
        int tileLength = seed.Next(10);
        RogueTile originTile = origin;
        originTile.SetupManager(this);
		for (int i = 0; i < maxNode; i++) originTile = CreateNewNode(tileLength, originTile);
    }

    RogueTile CreateNewNode(int randomLength, RogueTile originTile)
	{
        Vector3 tilePosition = tile.transform.position;
        tilePosition.x = originTile.transform.position.x + 150 + (50 * randomLength);

        GameObject newTile = Instantiate(tile, tilePosition, tile.transform.rotation);
        mapObjectsList.Add(newTile);
        RogueTile newTileScript = newTile.GetComponent<RogueTile>();
        newTileScript.SetupManager(this);
        newTileScript.nodeNumber = originTile.nodeNumber + 1;


        Vector3 linkPosition = newTile.transform.position;
        linkPosition.x = (originTile.transform.position.x + newTile.transform.position.x) / 2;

        GameObject newLine = Instantiate(link, linkPosition, link.transform.rotation);
        mapObjectsList.Add(newLine);
        Vector3 linkScale = newLine.transform.localScale;
        linkScale.y = Mathf.Abs(originTile.transform.position.x - newTile.transform.position.x) / 2;
        newLine.transform.localScale = linkScale;

        return newTileScript;
    }

    public void NodeClicked(RogueTile tile)
	{
		if (IsAnyUnitMoving) return;
        
        if (currentNode == tile.nodeNumber - 1)
		{
            currentNode++;
            structureManager.MoveUnit(playerUnitTransform, tile);
        }
            
    }

    public void DisableRogueSection()
	{
        foreach (var item in mapObjectsList)
            Destroy(item);
    }

    public void EndRun()
	{
        SceneManager.LoadScene(0);
    }

    enum RogueChoices
    {
        StandardFight,
        EliteFight,
        Merchant,
        Smith
    }
}