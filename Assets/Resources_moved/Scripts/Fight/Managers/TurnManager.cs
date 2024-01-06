using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager
{
	readonly FightManager fightManager;
	readonly StructureManager structureManager;
	readonly AIManager aiManager;

	public int CurrentTurn { get; set; }

    public TurnManager(FightManager fightManager, StructureManager structureManager, AIManager aiManager)
    {
		this.fightManager = fightManager;
		this.structureManager = structureManager;
		this.aiManager = aiManager;
	}

    public void EndPhase(int faction, bool isSetup)
	{
		if (isSetup)
		{
			structureManager.gameData.IsSetup = false;
			structureManager.UpdateEndPhaseButtonText();
			fightManager.ResetGameState(true);
			return;
		}

		EndTurn(faction);
	}

	public void EndTurn(int faction)
	{
		switch (faction)
		{
			case FightManager.USER_FACTION:
				EndUserTurn();
				break;
			case FightManager.ENEMY_FACTION:
			default:
				StartUserTurn();
				break;
		}
	}

	public void StartUserTurn()
	{
		CurrentTurn = FightManager.USER_FACTION;
		fightManager.TurnCount++;
		fightManager.ResetGameState(true);
		structureManager.SetEndTurnButton(true);
		//We reset the enemies' movement too for calculations and player's effects that may influence them
		foreach (var unit in fightManager.UnitsOnField)
		{
			unit.Movement.HasPerformedMainAction = false;
			unit.FightData.StartOfTurnEffects();
		}
	}

	void EndUserTurn()
	{
		CurrentTurn = FightManager.ENEMY_FACTION;
		fightManager.ResetGameState(true);
		structureManager.SetEndTurnButton(false);
		aiManager.StartAITurn();
	}
}