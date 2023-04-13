using System.Collections.Generic;
using UnityEngine;

public class RogueManager : MonoBehaviour
{
    [SerializeField]
    GeneralManager generalManager;

    StructureManager structureManager;

    PRNG random;
    public RogueTile origin;
    public GameObject tile;
    public GameObject link;
    int currentNode = 0;
    readonly List<GameObject> mapObjectsList = new();

    public Transform playerUnitTransform;

    // Start is called before the first frame update
    void Start()
    {
        structureManager = GetComponent<StructureManager>();
        structureManager.SetupClasses();

        int rngSeed = Random.Range(0, 100);
        Debug.Log($"ROGUE - SEED GENERATED: {rngSeed}");

        random = new(rngSeed);

        GenerateMap();
    }

    public int GetPlayerCurrentNode()
	{
        return currentNode;
    }

    void GenerateMap()
	{
        int tileLength = random.Next(10);
        RogueTile originTile = origin;
		for (int i = 0; i < 5; i++) originTile = CreateNewNode(tileLength, originTile);
    }

    RogueTile CreateNewNode(int seed, RogueTile originTile)
	{
        Vector3 tilePosition = tile.transform.position;
        tilePosition.x = originTile.transform.position.x + 150 + (50 * seed);

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
        structureManager.MoveUnit(playerUnitTransform, tile);
        generalManager.StartFight();
	}

    public void DisableRogueSection()
	{
        ClearMap();
    }

    void ClearMap()
	{
		foreach (var item in mapObjectsList)
		{
            Destroy(item);
		}
	}
}

public enum RogueChoices
{
    StandardFight,
    EliteFight,
    Merchant,
    Smith
}