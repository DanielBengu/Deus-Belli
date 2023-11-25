using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    List<Tile> _possibleMovements;

    public Tile CurrentTile { get; set; }
    public bool HasPerformedMainAction { get; set; }

    public FightManager FightManager { get; set; }
    
    public int startingTileNumber;

    #region Info Panel

    public string unitName;
    public Sprite unitImage;

    #region Stats
    public List<TraitsEnum> Traits { get; set; } = new();
    public int hpMax;
    public int hpCurrent;
    public float movementMax;
    public float movementCurrent;
    public int attack;
    public int faction;
    public int range;
    #endregion

    #endregion

    public List<Tile> PossibleMovements { get { return GetPossibleMovements(); } }

    public void LoadStats(Unit unit)
	{
        Traits = unit.Traits;
        hpMax = unit.hpMax;
        movementMax = unit.movementMax;
        attack = unit.attack;
        faction = unit.faction;
        range = unit.range;
	}

    public void OnMouseDown()
    {
        CurrentTile.OnMouseDown();
    }

    public List<Tile> GetPossibleAttacks()
	{
        return FightManager.GetPossibleAttacksForUnit(this);
    }

    List<Tile> GetPossibleMovements()
    {
        //if map has changed
        if (true)
            _possibleMovements = FightManager.GetPossibleMovements(this);

        return _possibleMovements;
    }

    public void LoadData(string[] data)
	{
        unitName = data[1];
        unitImage = AddressablesManager.LoadResource<Sprite>(AddressablesManager.TypeOfResource.Sprite, data[2]);
        hpMax = int.Parse(data[3]);
        movementMax = int.Parse(data[4]);
        attack = int.Parse(data[5]);
        range = int.Parse(data[6]);
        startingTileNumber = int.Parse(data[7]);
    }

    public void StartDamageForOpponent()
	{
        FightManager.MakeUnitTakeDamage();
	}

    public enum TraitsEnum
	{
        Floaty,
        Healthy,
        Magic_Defence,
        Overload,
        Regeneration,
        Second_Wind,
        Speedy,
        Strong,
        Tanky,
        Wealthy
	}
}
