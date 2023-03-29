using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    StructureManager structureManager;
    FightManager fightManager;

    List<Unit> unitsToCalculate = new();
    
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
            fightManager.EndTurn(fightManager.CurrentTurn);
        }
    }

    public void CalculateTurn(){
        Debug.Log($"START AI TURN FOR FACTION {fightManager.CurrentTurn}");
        List<Unit> unitsOfFaction = fightManager.units.Where(u => u.faction == fightManager.CurrentTurn).ToList();
        foreach (var unit in unitsOfFaction)
        {
            Debug.Log($"CALCULATING MOVE FOR UNIT {unit.unitName}");
            List<Tile> possibleMovements = structureManager.GeneratePossibleMovementForUnit(unit, false);
            int randomInt = UnityEngine.Random.Range(0,possibleMovements.Count);
            Tile destinationTile = GameObject.Find($"Terrain_{possibleMovements[randomInt].tileNumber}").GetComponent<Tile>();
            structureManager.CalculateMapTilesDistance(unit);
            structureManager.StartUnitMovement(unit, destinationTile);
        }
        
        Debug.Log($"END AI TURN FOR FACTION {fightManager.CurrentTurn}");
    }
}
