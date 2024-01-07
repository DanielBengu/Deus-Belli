using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Pathfinding;

public class AIManager : MonoBehaviour
{
    //private const int UNIT_MOVEMENT_SPEED = 1600;

    StructureManager structureManager;
    FightManager fightManager;
	readonly Queue<int> turnQueue = new();
	readonly Queue<Unit> unitQueue = new();
    public int seed = 0;
    public Unit currentUnitTurn;
    
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        fightManager = GetComponent<FightManager>();
        structureManager = fightManager.structureManager;
    }

    void Update(){
        if(IsManagerWaiting())
            return;

        if (unitQueue.Count > 0)
        {
            currentUnitTurn = unitQueue.Dequeue();
            CalculateTurnForUnit(currentUnitTurn);
            return;
        }

        Debug.Log($"END AI TURN FOR FACTION {fightManager.CurrentTurn}");

        if(turnQueue.Count > 0)
        {
			CalculateFaction(turnQueue.Dequeue());
            return;
		}

        fightManager.TurnManager.EndTurn(fightManager.CurrentTurn);
    }

    public void StartAITurn(){
        int[] differentFactionsOtherThanPlayer = structureManager.gameData.unitsOnField.Where(u => u.UnitData.Faction != FightManager.USER_FACTION).Select(u => u.UnitData.Faction).Distinct().ToArray();

        for (int i = 0; i < differentFactionsOtherThanPlayer.Length; i++)
        {
            turnQueue.Enqueue(differentFactionsOtherThanPlayer[i]);
		}

        CalculateFaction(turnQueue.Dequeue());
	}

    void CalculateFaction(int faction)
    {
		Debug.Log($"START AI TURN FOR FACTION {faction}");

		fightManager.CurrentTurn = faction;
		
		foreach (var unit in structureManager.gameData.unitsOnField.Where(u => u.UnitData.Faction == faction))
		{
			unit.FightData.StartOfTurnEffects();
			unitQueue.Enqueue(unit);
		}

		if (unitQueue.Count > 0)
			CalculateTurnForUnit(unitQueue.Dequeue());
	}

    void CalculateTurnForUnit(Unit unit){
        Debug.Log($"CALCULATING AI MOVE FOR UNIT {unit.UnitData.Name}");
        //We force the unit to move to a different tile if possible
        List<Tile> possibleMovements = unit.Movement.PossibleMovements.Where(t => t.data.PositionOnGrid != unit.Movement.CurrentTile.data.PositionOnGrid).ToList();
        if (possibleMovements.Count == 0)
            possibleMovements = unit.Movement.CurrentTile.ToList();
        List<PossibleAttack> possibleAttacks = unit.Movement.GetPossibleAttacks(possibleMovements);

        ActionAI action = DecideAction(possibleMovements,possibleAttacks);
        ManageAIAction(unit, action, possibleMovements, possibleAttacks);
	}

    ActionAI DecideAction(List<Tile> possibleMovements, List<PossibleAttack> possibleAttacks)
    {
        if (possibleAttacks.Count > 0) 
            return ActionAI.Attack;

        if (possibleMovements.Count > 0) 
            return ActionAI.JustMove;

        return ActionAI.Skip;
    }

    void ManageAIAction(Unit unit, ActionAI action, List<Tile> possibleMovements, List<PossibleAttack> possibleAttacks)
    {
		switch (action)
		{
			case ActionAI.JustMove:
				Move(possibleMovements, unit);
				break;
			case ActionAI.Attack:
				Attack(possibleAttacks, unit);
				break;
			case ActionAI.Skip:
			default:
				break;
		}
	}

    public void Attack(List<PossibleAttack> possibleAttacks, Unit unit)
    {
        if (possibleAttacks.Count == 0)
            return;

        PossibleAttack attackTarget = FindRandomAttackTarget(possibleAttacks);  //Attack at random possible targets
        fightManager.QueueAttack(unit, attackTarget.tileToAttack.unitOnTile, attackTarget.tileToMoveTo);
    }

    public void Move(List<Tile> possibleMovements, Unit unit)
    {
        if (possibleMovements.Count == 0)
            return;

        Tile destination = FindRandomMovementTarget(possibleMovements);

        structureManager.CalculateMapTilesDistance(unit);
        structureManager.MoveUnit(unit, destination, false);
    }

    bool IsManagerWaiting()
    {
        bool isNotAITurn = fightManager.CurrentTurn == FightManager.USER_FACTION;
        bool isAnyUnitMoving = fightManager.IsAnyUnitMoving;
        bool isAnyObjectAnimating = structureManager.ObjectsAnimating.Count > 0;

		return isNotAITurn || isAnyUnitMoving || isAnyObjectAnimating;
	}

    PossibleAttack FindRandomAttackTarget(List<PossibleAttack> possibleAttacks)
    {
		int randomChoice = RandomManager.GetRandomValue(seed, 0, possibleAttacks.Count);
        return possibleAttacks[randomChoice];
	}

    Tile FindRandomMovementTarget(List<Tile> possibleMovements)
    {
		//Enemy AI Randomness is NOT based on run seed and performs casually every time, even if 2 players do the same things
		int randomInt = Random.Range(0, possibleMovements.Count);
		return GameObject.Find($"Terrain_{possibleMovements[randomInt].data.PositionOnGrid}").GetComponent<Tile>();
	}
}

enum ActionAI
{
    JustMove,
    Attack, 
    Skip
}