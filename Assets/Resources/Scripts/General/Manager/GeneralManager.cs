using System.Collections;
using System.Collections.Generic;
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

    CurrentSection currentSection = CurrentSection.Rogue;

    public bool IsGameInStandby { get { return IsGameInStandbyMethod(); } }

	private void Start()
	{
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
    }

    public void ReturnToRogueFromFightButton()
	{
        DestroyFightSection();
        GenerateRogueSection();
    }

    enum CurrentSection
	{
        Fight,
        Rogue
	}
}
