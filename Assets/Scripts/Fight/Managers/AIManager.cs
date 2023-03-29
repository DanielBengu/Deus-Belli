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
                fightManager.EndTurn(fightManager.CurrentTurn);
                Debug.Log($"END AI TURN FOR FACTION {fightManager.CurrentTurn}");
            }
        }
    }

    void CalculateTurnForUnit(Unit unit){
        Debug.Log($"CALCULATING MOVE FOR UNIT {unit.unitName}");
        List<Tile> possibleMovements = structureManager.GeneratePossibleMovementForUnit(unit, false);
        int randomInt = UnityEngine.Random.Range(0,possibleMovements.Count);
        Tile destinationTile = GameObject.Find($"Terrain_{possibleMovements[randomInt].tileNumber}").GetComponent<Tile>();
        structureManager.CalculateMapTilesDistance(unit);
        structureManager.StartUnitMovement(unit, destinationTile, UNIT_MOVEMENT_SPEED);
    }

    public void StartAITurn(){
        Debug.Log($"START AI TURN FOR FACTION {fightManager.CurrentTurn}");
        foreach (var unit in fightManager.units.Where(u => u.faction == fightManager.CurrentTurn))
        {
            unitsToCalculate.Enqueue(unit);
        }
        CalculateTurnForUnit(unitsToCalculate.Dequeue());
    }
}
