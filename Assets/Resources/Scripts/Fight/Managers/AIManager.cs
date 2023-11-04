using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    //private const int UNIT_MOVEMENT_SPEED = 1600;

    StructureManager structureManager;
    FightManager fightManager;
    readonly Queue<Unit> unitsToCalculate = new();
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

        if(unitsToCalculate.Count > 0){
            currentUnitTurn = unitsToCalculate.Dequeue();
            CalculateTurnForUnit(currentUnitTurn);
        }else{
            Debug.Log($"END AI TURN FOR FACTION {fightManager.CurrentTurnCount}");
            fightManager.EndTurn(fightManager.CurrentTurnCount);
        }
    }

    public void StartAITurn(){
        Debug.Log($"START AI TURN FOR FACTION {fightManager.CurrentTurnCount}");
        foreach (var unit in structureManager.gameData.unitsOnField.Where(u => u.faction == fightManager.CurrentTurnCount))
        {
            unit.movementCurrent = unit.movementMax;
            unitsToCalculate.Enqueue(unit);
        }

        if (unitsToCalculate.Count > 0)
            CalculateTurnForUnit(unitsToCalculate.Dequeue());
    }

    void CalculateTurnForUnit(Unit unit){
        Debug.Log($"CALCULATING MOVE FOR UNIT {unit.unitName}");
        List<Tile> possibleMovements = unit.PossibleMovements;
        List<Tile> possibleAttacks = unit.GetPossibleAttacks();

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

    ActionAI DecideAction(List<Tile> possibleMovements, List<Tile> possibleAttacks)
    {
        ActionAI action = ActionAI.Skip;

        if (possibleAttacks.Count > 0) action = ActionAI.Attack;
        else if (possibleMovements.Count > 0) action = ActionAI.Flee;

        return action;
    }

    public void Attack(List<Tile> possibleAttacks, Unit unit)
    {
        if (possibleAttacks.Count == 0)
            return;

        int randomChoice = RandomManager.GetRandomValue(seed, 0, possibleAttacks.Count);
        Tile attackTarget = possibleAttacks[randomChoice]; //Attack at random possible targets
        Debug.Log($"AI ATTACKING TILE N.{attackTarget.tileNumber}");
        fightManager.UnitSelected = unit;
        fightManager.QueueAttack(unit, attackTarget.unitOnTile);
    }

    public void Flee(List<Tile> possibleMovements, Unit unit)
    {
        if (possibleMovements.Count == 0)
            return;

        //Enemy AI Randomness is NOT based on run seed and performs casually every time, even if 2 players do the same things
        int randomInt = Random.Range(0, possibleMovements.Count);
        Debug.Log($"AI MOVING TO TILE N.{possibleMovements[randomInt].tileNumber}");
        Tile destinationTile = GameObject.Find($"Terrain_{possibleMovements[randomInt].tileNumber}").GetComponent<Tile>();
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