using UnityEngine;

public class RogueManager : MonoBehaviour
{
    PRNG random;


    // Start is called before the first frame update
    void Start()
    {
        int rngSeed = Random.Range(0, 100);
        Debug.Log($"ROGUE - SEED GENERATED: {rngSeed}");
        random = new(rngSeed);
    }

    // Update is called once per frame
    void Update()
    {
    }
}

public enum RogueChoices
{
    StandardFight,
    EliteFight,
    Merchant,
    Smith
}