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
        if(fightManager.CurrentTurnCount == FightManager.USER_FACTION || fightManager.IsAnyUnitMoving)
            return;

        if (unitQueue.Count > 0)
        {
            currentUnitTurn = unitQueue.Dequeue();
            CalculateTurnForUnit(currentUnitTurn);
            return;
        }

        Debug.Log($"END AI TURN FOR FACTION {fightManager.CurrentTurnCount}");

        if(turnQueue.Count > 0)
        {
			CalculateFaction(turnQueue.Dequeue());
            return;
		}

        fightManager.EndTurn(fightManager.CurrentTurnCount);
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

		fightManager.CurrentTurnCount = faction;
		
		foreach (var unit in structureManager.gameData.unitsOnField.Where(u => u.UnitData.Faction == faction))
		{
			unit.FightData.StartOfTurnEffects();
			unitQueue.Enqueue(unit);
		}

		if (unitQueue.Count > 0)
			CalculateTurnForUnit(unitQueue.Dequeue());
	}

    void CalculateTurnForUnit(Unit unit){
        Debug.Log($"CALCULATING MOVE FOR UNIT {unit.UnitData.Name}");
        //We force the unit to move to a different tile, it cant stay still
        List<Tile> possibleMovements = unit.Movement.PossibleMovements.Where(t => t.data.PositionOnGrid != unit.Movement.CurrentTile.data.PositionOnGrid).ToList();
        List<PossibleAttack> possibleAttacks = unit.Movement.GetPossibleAttacks(possibleMovements);

        ActionAI action = DecideAction(possibleMovements,possibleAttacks);

		switch (action)
		{
			case ActionAI.Flee:
                Flee(possibleMovements, unit);
                break;
			case ActionAI.Attack:
                Attack(possibleAttacks, unit);
                break;
		} 
    }

    ActionAI DecideAction(List<Tile> possibleMovements, List<PossibleAttack> possibleAttacks)
    {
        ActionAI action = ActionAI.Skip;

        if (possibleAttacks.Count > 0) action = ActionAI.Attack;
        else if (possibleMovements.Count > 0) action = ActionAI.Flee;

        return action;
    }

    public void Attack(List<PossibleAttack> possibleAttacks, Unit unit)
    {
        if (possibleAttacks.Count == 0)
            return;

        int randomChoice = RandomManager.GetRandomValue(seed, 0, possibleAttacks.Count);
        PossibleAttack attackTarget = possibleAttacks[randomChoice]; //Attack at random possible targets
        Debug.Log($"AI ATTACKING TILE N.{attackTarget.tileToAttack.data.PositionOnGrid}");
        fightManager.UnitSelected = unit;
        fightManager.QueueAttack(unit, attackTarget.tileToAttack.unitOnTile, attackTarget.tileToMoveTo);
    }

    public void Flee(List<Tile> possibleMovements, Unit unit)
    {
        if (possibleMovements.Count == 0)
            return;

        //Enemy AI Randomness is NOT based on run seed and performs casually every time, even if 2 players do the same things
        int randomInt = Random.Range(0, possibleMovements.Count);
        Debug.Log($"AI MOVING TO TILE N.{possibleMovements[randomInt].data.PositionOnGrid}");
        Tile destinationTile = GameObject.Find($"Terrain_{possibleMovements[randomInt].data.PositionOnGrid}").GetComponent<Tile>();
        structureManager.CalculateMapTilesDistance(unit);
        structureManager.MoveUnit(unit, destinationTile, false);
    }
}

enum ActionAI
{
    Flee,
    Attack, 
    Skip
}