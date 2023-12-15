using System.Collections.Generic;
using UnityEngine;
using static Pathfinding;

public class UnitMovement
{
    public Unit Parent { get; set; }
    internal List<Tile> _possibleMovements;

    public Tile CurrentTile { get; set; }
    public bool HasPerformedMainAction { get; set; }
    
    public int startingTileNumber;

    public List<Tile> PossibleMovements { get { return GetPossibleMovements(); } }

	public UnitMovement(Unit parent)
	{
        Parent = parent;
	}

	public List<PossibleAttack> GetPossibleAttacks(List<Tile> possibleMovements = null)
	{
        return Parent.FightManager.GetPossibleAttacksForUnit(Parent, possibleMovements);
    }

    List<Tile> GetPossibleMovements()
    {
        //if map has changed
        if (true)
            _possibleMovements = Parent.FightManager.GetPossibleMovements(Parent);

        return _possibleMovements;
    }

    //Called at the end of an attack animation
    public void StartDamageForOpponent()
	{
        Parent.FightManager.MakeUnitTakeDamage();
	}
}
