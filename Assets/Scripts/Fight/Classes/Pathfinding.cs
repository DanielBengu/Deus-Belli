using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Pathfinding
{
    //Substitute of the infinity value usually used in dijkstra algorithm for unvisited tiles
    const int OUT_OF_BOUND_VALUE = 99999;
    const int FAIL_SAFE_MAX = 1000;

    private int X_Length;
    private int Y_Length;

    Dictionary<int, Tile> mapTiles;

    Func<Tile, Tile>[] directions;

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
    public void CalculateMapTilesDistance(Unit startingUnit){
        if(!startingUnit){
            Debug.Log("CalculateMapTilesDistance - Invalid startingUnit");
            return;
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
                CalculateNeighbours(tileToCalculate);

            tileToCalculate.IsVisited = true;

            failSafe++;
            if(failSafe == FAIL_SAFE_MAX){
                Debug.Log("FAILSAFE TRIGGERED");
                break;
            }
        }
        Debug.Log("Ending Dijkstra calculation");
    }

    void CalculateNeighbours(Tile source){
        foreach (var direction in directions)
        {
            Tile neighbourTile = direction(source);
            if(neighbourTile && neighbourTile.IsPassable && !neighbourTile.unitOnTile){
                float newTentativeCost = source.tentativeCost + neighbourTile.MovementCost;
                if(newTentativeCost < neighbourTile.tentativeCost)
                    neighbourTile.tentativeCost = newTentativeCost;
            }
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
