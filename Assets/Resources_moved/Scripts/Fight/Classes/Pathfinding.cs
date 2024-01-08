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
            int tileToFind = startingTile.data.PositionOnGrid - 1;

            if(startingTile.data.PositionOnGrid % Y_Length != 0)
                leftTile = mapTiles[tileToFind];

            return leftTile;
        }

        Tile FindRightTile(Tile startingTile){
            Tile rightTile = null;
            int tileToFind = startingTile.data.PositionOnGrid + 1;

            if(startingTile.data.PositionOnGrid % X_Length != X_Length - 1)
                rightTile = mapTiles[tileToFind];

            return rightTile;
        }

        Tile FindUpTile(Tile startingTile){
            Tile upTile = null;
            int tileToFind = startingTile.data.PositionOnGrid - Y_Length;
            
            if(tileToFind >= 0)
                upTile = mapTiles[tileToFind];

            return upTile;
        }

        Tile FindDownTile(Tile startingTile){
            Tile downTile = null;
            int tileToFind = startingTile.data.PositionOnGrid + Y_Length;

            if(tileToFind <= (X_Length * Y_Length) - 1)
                downTile = mapTiles[tileToFind];

            return downTile;
        }
        
    #endregion


    //Dijkstra algorithm to calculate tiles movement cost
    public List<Tile> CalculateMapTilesDistance(Unit startingUnit){
        if(!Validator.Validate(startingUnit)){
            Debug.Log("CalculateMapTilesDistance - Invalid startingUnit");
            return null;
        }

        PerformDijkstraAlgorithm(startingUnit);

        return GetPath(startingUnit);
	}

    //If needed we can optimize by removing from the search the tiles already searched in previous loops, somehow
	public List<PossibleAttack> FindPossibleAttacks(Unit attacker, List<Tile> possibleMovements)
	{
        //If no list of possible movements was passed, we generate it
        possibleMovements ??= CalculateMapTilesDistance(attacker);
		List<PossibleAttack> possibleAttacks = new();
		List<Tile> startingPointsForAttack = possibleMovements;
		List<Tile> tilesToSearch = new();

		foreach (var tile in possibleMovements)
        {
            tilesToSearch = new() { tile };
            List<Tile> tempTiles = new();
			for (int i = 0; i < attacker.UnitData.Stats.Range; i++)
            {
                foreach (var tileToSearch in tilesToSearch)
                {
					List<Tile> neighboursTile = FindNeighbours(attacker, tileToSearch, false).Where(t => t != null).ToList();
					tempTiles.AddRange(neighboursTile);

                    GetPossibleAttacksOnNeighbours(neighboursTile, tile, attacker.UnitData.Faction, possibleAttacks);
				}
                tilesToSearch.AddRange(tempTiles);
                tempTiles= new();
			}
		}
		return possibleAttacks;
	}

	void GetPossibleAttacksOnNeighbours(List<Tile> neighboursTile, Tile tileOfOrigin, int attackerFaction, List<PossibleAttack> possibleAttacks)
    {
        foreach (var neighbour in neighboursTile)
        {
            if (!NeighbourHasAnEnemy(neighbour, attackerFaction))
                continue;

            PossibleAttack attack = new(neighbour, tileOfOrigin);
            if(!possibleAttacks.Contains(attack))
				possibleAttacks.Add(attack);
		}
	}

    bool NeighbourHasAnEnemy(Tile neighbour, int attackerFaction) {
        return neighbour && neighbour.unitOnTile && neighbour.unitOnTile.UnitData.Faction != attackerFaction;
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
        if (neighbourTile && neighbourTile.data.ValidForMovement && !IsTileOccupiedByAnotherFaction(sourceUnit, neighbourTile))
        {
            float newTentativeCost = source.tentativeCost + GetMovementCost(neighbourTile, sourceUnit);
            if (newTentativeCost < neighbourTile.tentativeCost)
                neighbourTile.tentativeCost = newTentativeCost;
        }
    }

    bool IsTileOccupiedByAnotherFaction(Unit sourceUnit, Tile neighbourTile)
    {
        return neighbourTile.unitOnTile && neighbourTile.unitOnTile.UnitData.Faction != sourceUnit.UnitData.Faction;
	}

    //Returns the list of steps necessary to reach the destination tile with the least movement cost
    public List<Tile> FindPathToDestination(Tile destination, out float cost, int startingTileNumber){
		float lowestTentativeCost;
		List<Tile> result = new(){destination};
        Debug.Log($"Starting calculation at tile n.{result.Last()}");
        int failSafe = 0;
        while(true){
            lowestTentativeCost = OUT_OF_BOUND_VALUE;
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
            if(failSafe == FAIL_SAFE_MAX || startingTileNumber == result.Last().data.PositionOnGrid)
                break;
            
            Debug.Log($"Finding path: next tile n.{lowestTile.data.PositionOnGrid}");
            result.Add(lowestTile);
        }
        result.Reverse();
        cost = result.Last().tentativeCost;
		return result;
    }

    void PerformDijkstraAlgorithm(Unit startingUnit)
    {
		Debug.Log("Starting Dijkstra calculation");

		ResetMapTilesTentativeCost();
		CalculateTiles(startingUnit);

		Debug.Log("Ending Dijkstra calculation");
	}

    List<Tile> GetPath(Unit startingUnit)
    {
		return mapTiles.Where(t => t.Value.tentativeCost <= startingUnit.FightData.currentStats.MOVEMENT && !EnemyIsBlockingTile(startingUnit, t.Value)).Select(t => t.Value).ToList();
	}
    bool EnemyIsBlockingTile(Unit source, Tile target) {
        return (target.unitOnTile && target.unitOnTile.UnitData.Faction != source.UnitData.Faction);
    }
	void ResetMapTilesTentativeCost()
    {
		for (int i = 0; i < mapTiles.Count; i++)
		{
			mapTiles[i].tentativeCost = OUT_OF_BOUND_VALUE;
			mapTiles[i].IsVisited = false;
		}
	}

    void CalculateTiles(Unit startingUnit)
    {
		float maxMovementCost = startingUnit.Movement.GetUnitsMovementForPathfindingCalculations();
		startingUnit.Movement.CurrentTile.tentativeCost = 0;

		bool pathFound = false;
		Tile tileToCalculate;
		int failSafe = 0;
		while (!pathFound && failSafe < FAIL_SAFE_MAX)
		{
            int lowestTileIndex = GetNextUnvisitedTileIndex(out bool calculatedEveryCost);

            if (calculatedEveryCost || lowestTileIndex == OUT_OF_BOUND_VALUE)
                break;

			tileToCalculate = mapTiles[lowestTileIndex];
			if (tileToCalculate.tentativeCost < maxMovementCost)
				FindNeighbours(startingUnit, tileToCalculate, true);

			tileToCalculate.IsVisited = true;
			failSafe++;
		}

		if (failSafe >= FAIL_SAFE_MAX)
			Debug.LogError("FAILSAFE TRIGGERED!");
	}

    int GetNextUnvisitedTileIndex(out bool calculatedEveryCost)
    {
		List<Tile> unvisitedTiles = mapTiles.Select(t => t.Value).Where(t => !t.IsVisited).ToList(); //Could be optimized by using a fixed list and removing each loop instead of selecting from the full list each time
        calculatedEveryCost = false;

		if (unvisitedTiles.Count == 0)
		{
			calculatedEveryCost = true;
			Debug.Log("CALCULATED EVERY TILE COST");
            return -1;
		}

		return mapTiles.Select(t => t.Value).ToList().Find(t => t == unvisitedTiles.Min()).data.PositionOnGrid;
	}

    int GetMovementCost(Tile neighbourTile, Unit unit)
    {
		if (unit.FightData.ContainsTrait(Unit.TraitsEnum.Floaty, out _))
            return 1;

		return neighbourTile.data.MovementCost;
	}

    public struct PossibleAttack
    {
        public Tile tileToAttack;
        public Tile tileToMoveTo;

        public PossibleAttack(Tile tileToAttack, Tile tileToMoveTo)
        {
            this.tileToAttack = tileToAttack;
            this.tileToMoveTo = tileToMoveTo;
        }
    }
}
