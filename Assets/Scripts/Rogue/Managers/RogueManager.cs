using UnityEngine;

public class RogueManager : MonoBehaviour
{
    PRNG random;
    public GameObject origin;
    public GameObject tile;
    public GameObject link;

    // Start is called before the first frame update
    void Start()
    {
        int rngSeed = Random.Range(0, 100);
        Debug.Log($"ROGUE - SEED GENERATED: {rngSeed}");

        random = new(rngSeed);

        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void GenerateMap()
	{
        int tileLength = random.Next(10);
        GameObject originTile = origin;
		for (int i = 0; i < 6; i++)
		{
            originTile = CreateNewNode(tileLength, originTile);
        }
       
    }

    GameObject CreateNewNode(int seed, GameObject originTile)
	{
        Vector3 tilePosition = tile.transform.position;
        tilePosition.x = originTile.transform.position.x + 150 + (50 * seed);

        GameObject newTile = Instantiate(tile, tilePosition, tile.transform.rotation);

        Vector3 linkPosition = newTile.transform.position;
        linkPosition.x = (originTile.transform.position.x + newTile.transform.position.x) / 2;

        GameObject newLine = Instantiate(link, linkPosition, link.transform.rotation);
        Vector3 linkScale = newLine.transform.localScale;
        linkScale.y = Mathf.Abs(originTile.transform.position.x - newTile.transform.position.x) / 2;
        newLine.transform.localScale = linkScale;

        return newTile;
    }
}

public enum RogueChoices
{
    StandardFight,
    EliteFight,
    Merchant,
    Smith
}