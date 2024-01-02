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

	void StartUserTurn()
	{
		CurrentTurn = FightManager.USER_FACTION;
		fightManager.TurnCount++;
		fightManager.ResetGameState(true);
		structureManager.SetEndTurnButton(true);
		foreach (var unit in fightManager.UnitsOnField.Where(u => u.UnitData.Faction == FightManager.USER_FACTION))
		{
			unit.FightData.currentMovement = unit.UnitData.Stats.Movement;
			unit.Movement.HasPerformedMainAction = false;
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