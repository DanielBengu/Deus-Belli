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

    public Transform playerUnitTransform;

    public int currentNode;
	readonly int maxNode = 5;
    public PRNG seed;

	public int Gold { get { return generalManager.Gold; } }


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
        originTile.SetupTile(this, RogueTileType.Fight);
		for (int i = 0; i < maxNode; i++) originTile = CreateNewNode(tileLength, originTile, origin.transform);
    }

    RogueTile CreateNewNode(int randomLength, RogueTile destinationTile, Transform parent)
	{
        RogueTile newTileScript = structureManager.GenerateRogueTiles(randomLength, destinationTile, tile.transform, parent, this);
        structureManager.GenerateRogueLine(newTileScript.transform, destinationTile.transform, link, parent, randomLength);

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
    }

    public void EndRun()
	{
        SceneManager.LoadScene(0);
    }
}