using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralManager : MonoBehaviour
{
    [SerializeField]
    GameObject fightSection;
    [SerializeField]
    FightManager fightManager;


    [SerializeField]
    GameObject rogueSection;
    [SerializeField]
    RogueManager rogueManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartFight()
	{
        rogueManager.DisableRogueSection();
        rogueSection.SetActive(false);

        fightSection.SetActive(true);
    }
}
