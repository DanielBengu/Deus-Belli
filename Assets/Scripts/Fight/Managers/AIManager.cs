using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    private const int UNIT_MOVEMENT_SPEED = 1600;

    StructureManager structureManager;
    FightManager fightManager;

    Queue<Unit> unitsToCalculate = new();
    
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        structureManager = this.GetComponent<StructureManager>();
        fightManager = this.GetComponent<FightManager>();
    }

    void Update(){
        if(fightManager.CurrentTurn == 0)
            return;

        if(!fightManager.IsObjectMoving){
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
        foreach (var unit in fightManager.units.Where(u => u.faction == fightManager.CurrentTurn))
        {
            unit.movementCurrent = unit.movementMax;
            unitsToCalculate.Enqueue(unit);
        }
        CalculateTurnForUnit(unitsToCalculate.Dequeue());
    }

    void CalculateTurnForUnit(Unit unit){
        Debug.Log($"CALCULATING MOVE FOR UNIT {unit.unitName}");
        if(unit.movementCurrent > 0){
            List<Tile> possibleMovements = structureManager.GeneratePossibleMovementForUnit(unit, false);
            if(possibleMovements.Count > 0){
                int randomInt = Random.Range(0,possibleMovements.Count);
                Debug.Log($"AI MOVING TO TILE N.{possibleMovements[randomInt].tileNumber}");
                Tile destinationTile = GameObject.Find($"Terrain_{possibleMovements[randomInt].tileNumber}").GetComponent<Tile>();
                structureManager.CalculateMapTilesDistance(unit);
                structureManager.StartUnitMovement(unit, destinationTile, UNIT_MOVEMENT_SPEED);
            }
            
        }
    }
}
