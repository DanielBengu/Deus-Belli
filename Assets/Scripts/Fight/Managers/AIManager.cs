using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    //private const int UNIT_MOVEMENT_SPEED = 1600;

    StructureManager structureManager;
    FightManager fightManager;
    readonly Queue<Unit> unitsToCalculate = new();
    
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        structureManager = GetComponent<StructureManager>();
        fightManager = GetComponent<FightManager>();
    }

    void Update(){
        if(fightManager.CurrentTurn == 0)
            return;

        if(!fightManager.IsAnyUnitMoving){
            if(unitsToCalculate.Count > 0){
                CalculateTurnForUnit(unitsToCalculate.Dequeue());
            }else{
                Debug.Log($"END AI TURN FOR FACTION {fightManager.CurrentTurn}");
                fightManager.EndTurn(fightManager.CurrentTurn);
            }
        }
    }

    public void StartAITurn(){
        Debug.Log($"START AI TURN FOR FACTION {fightManager.CurrentTurn}");
        foreach (var unit in structureManager.gameData.unitsOnField.Where(u => u.faction == fightManager.CurrentTurn))
        {
            unit.movementCurrent = unit.movementMax;
            unitsToCalculate.Enqueue(unit);
        }

        if (unitsToCalculate.Count > 0)
            CalculateTurnForUnit(unitsToCalculate.Dequeue());
    }

    void CalculateTurnForUnit(Unit unit){
        Debug.Log($"CALCULATING MOVE FOR UNIT {unit.unitName}");
        List<Tile> possibleMovements = structureManager.GeneratePossibleMovementForUnit(unit, false);
        List<Tile> possibleAttacks = structureManager.FindPossibleAttacks(unit, possibleMovements);

        int action = DecideAction();

        if (action == 0)
            Flee(possibleMovements, unit);
        else
            Attack(possibleAttacks, unit);
    }

    int DecideAction()
    {
        return Random.Range(0, 2); // 0 = flee, 1 = attack
    }

    public void Attack(List<Tile> possibleAttacks, Unit unit)
    {
        if (possibleAttacks.Count == 0)
            return;

        Tile attackTarget = possibleAttacks[Random.Range(0, possibleAttacks.Count)]; //Attack at random possible targets
        Debug.Log($"AI ATTACKING TILE N.{attackTarget.tileNumber}");
        fightManager.UnitSelected = unit;
        structureManager.FindPathToDestination(attackTarget, false, true);
        fightManager.QueueAttack(unit, attackTarget.unitOnTile);
    }

    public void Flee(List<Tile> possibleMovements, Unit unit)
    {
        if (possibleMovements.Count == 0)
            return;
        int randomInt = Random.Range(0, possibleMovements.Count);
        Debug.Log($"AI MOVING TO TILE N.{possibleMovements[randomInt].tileNumber}");
        Tile destinationTile = GameObject.Find($"Terrain_{possibleMovements[randomInt].tileNumber}").GetComponent<Tile>();
        structureManager.CalculateMapTilesDistance(unit);
        structureManager.StartUnitMovement(unit, destinationTile);
    }
}
