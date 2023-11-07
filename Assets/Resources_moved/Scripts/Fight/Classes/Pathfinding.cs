using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Pathfinding
{
    //Substitute of the infinity value usually used in dijkstra algorithm for unvisited tiles
    public const int OUT_OF_BOUND_VALUE = 99999;
    const int FAIL_SAFE_MAX = 1000;

    private readonly int X_Length;
    private readonly int Y_Length;
    readonly Dictionary<int, Tile> mapTiles;
    readonly Func<Tile, Tile>[] directions;

    public Pathfinding(Dictionary<int, Tile> mapTiles, int X_Length, int Y_Length)
    {
        this.X_Length = X_Length;
        this.Y_Length = Y_Length;
        this.mapTiles = mapTiles;
        directions = new Func<Tile, Tile>[] { FindLeftTile, FindRightTile, FindUpTile, FindDownTile };
    }
    
    #region Find tiles in direction 

        Tile FindLeftTile(Tile startingTile){
            Tile leftTile = null;
            int tileToFind = startingTile.tileNumber - 1;

            if(startingTile.tileNumber % Y_Length != 0)
                leftTile = mapTiles[tileToFind];

            return leftTile;
        }

        Tile FindRightTile(Tile startingTile){
            Tile rightTile = null;
            int tileToFind = startingTile.tileNumber + 1;

            if(startingTile.tileNumber % X_Length != X_Length - 1)
                rightTile = mapTiles[tileToFind];

            return rightTile;
        }

        Tile FindUpTile(Tile startingTile){
            Tile upTile = null;
            int tileToFind = startingTile.tileNumber - Y_Length;
            
            if(tileToFind >= 0)
                upTile = mapTiles[tileToFind];

            return upTile;
        }

        Tile FindDownTile(Tile startingTile){
            Tile downTile = null;
            int tileToFind = startingTile.tileNumber + Y_Length;

            if(tileToFind <= (X_Length * Y_Length) - 1)
                downTile = mapTiles[tileToFind];

            return downTile;
        }
        
    #endregion


    //Dijkstra algorithm to calculate tiles movement cost
    public List<Tile> CalculateMapTilesDistance(Unit startingUnit){
        List<Tile> movementsPossible = new();

        if(!startingUnit){
            Debug.Log("CalculateMapTilesDistance - Invalid startingUnit");
            return null;
        }

        Debug.Log("Starting Dijkstra calculation");
        float maxMovementCost = startingUnit.movementCurrent;

        for (int i = 0; i < mapTiles.Count; i++)
        {
            mapTiles[i].tentativeCost = OUT_OF_BOUND_VALUE;
            mapTiles[i].IsVisited = false;
        }

        startingUnit.CurrentTile.tentativeCost = 0;
        bool pathFound = false;
        int lowestTileIndex = 0;
        Tile tileToCalculate;

        int failSafe = 0;
        while (!pathFound)
        {
            List<Tile> unvisitedTiles = mapTiles.Select(t => t.Value).Where(t => !t.IsVisited).ToList();

            if(unvisitedTiles.Count == 0){
                pathFound = true;
                Debug.Log("CALCULATED EVERY TILE COST");
                break;
            }

            lowestTileIndex = mapTiles.Select(t => t.Value).ToList().Find(t => t == unvisitedTiles.Min()).tileNumber;

            //true when we calculated all possible movements for this unit, only unreachable locations remain
            if(lowestTileIndex == OUT_OF_BOUND_VALUE)
                pathFound = true;

            tileToCalculate = mapTiles[lowestTileIndex];
            if(tileToCalculate.tentativeCost < maxMovementCost)
                FindNeighbours(startingUnit, tileToCalculate, true);

            tileToCalculate.IsVisited = true;

            failSafe++;
            if(failSafe == FAIL_SAFE_MAX){
                Debug.Log("FAILSAFE TRIGGERED");
                break;
            }
        }
        Debug.Log("Ending Dijkstra calculation");

        return mapTiles.Select(t => t.Value).Where(t => t.tentativeCost <= startingUnit.movementCurrent && !(t.unitOnTile && t.unitOnTile.faction != startingUnit.faction)).ToList();
    }

    public List<Tile> FindPossibleAttacks(Unit attacker)
    {
        List<Tile> possibleAttacks = new();
        List<Tile> possibleMovements = CalculateMapTilesDistance(attacker);
        foreach (var possibleMovementsTile in possibleMovements)
        {
            List<Tile> neighboursTile = FindNeighbours(attacker, possibleMovementsTile, false).Where(t => t != null).ToList();
            
            foreach (var neighbour in neighboursTile)
            {
                if (neighbour && neighbour.unitOnTile && neighbour.unitOnTile.faction != attacker.faction)
                {
                    possibleAttacks.Add(neighbour);
                }
            }
        }
        return possibleAttacks;
    }

    List<Tile> FindNeighbours(Unit sourceUnit, Tile source, bool calculateCosts){
        List<Tile> neighbours = new();
        foreach (var direction in directions)
        {
            Tile neighbourTile = direction(source);
            neighbours.Add(neighbourTile);

            if (calculateCosts) CalculateCost(sourceUnit, source, neighbourTile);
        }
        return neighbours;
    }

    public void CalculateCost(Unit sourceUnit, Tile source, Tile neighbourTile)
    {
        if (neighbourTile && neighbourTile.IsPassable && !(neighbourTile.unitOnTile && neighbourTile.unitOnTile.faction != sourceUnit.faction))
        {
            float newTentativeCost = source.tentativeCost + neighbourTile.MovementCost;
            if (newTentativeCost < neighbourTile.tentativeCost)
                neighbourTile.tentativeCost = newTentativeCost;
        }
    }

    //Returns the list of steps necessary to reach the destination tile with the least movement cost
    public List<Tile> FindPathToDestination(Tile destination){
        List<Tile> result = new(){destination};
        Debug.Log($"Starting calculation at tile n.{result.Last()}");
        int failSafe = 0;
        while(true){
            float lowestTentativeCost = OUT_OF_BOUND_VALUE;
            Tile lowestTile = null;

            foreach (var direction in directions)
            {
                Tile neighbourTile = direction(result.Last());
                if (neighbourTile && neighbourTile.tentativeCost < lowestTentativeCost)
                {
                    lowestTentativeCost = neighbourTile.tentativeCost;
                    lowestTile = neighbourTile;
                }
            }

            failSafe++;
            if(result.Last().tentativeCost == 0 || failSafe == FAIL_SAFE_MAX)
                break;
            
            Debug.Log($"Finding path: next tile n.{lowestTile.tileNumber}");
            result.Add(lowestTile);
        }
        result.Reverse();
        return result;
    }
}
