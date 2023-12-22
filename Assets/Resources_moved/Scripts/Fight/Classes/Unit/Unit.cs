using System.Collections.Generic;
using UnityEngine;
public class Unit : MonoBehaviour
{
    public string ModelName { get; set; }
    public UnitMovement Movement { get; set; }

    public FightManager FightManager { get; set; }

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

	public void Load(Unit unit)
	{
        Movement = new(this);
        LoadStats(unit);
	}

	void LoadStats(Unit unit)
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
		Movement.CurrentTile.OnMouseDown();
    }

    public void LoadData(string[] data)
	{
        unitName = data[1];
        unitImage = AddressablesManager.LoadResource<Sprite>(AddressablesManager.TypeOfResource.Sprite, data[2]);
        hpMax = int.Parse(data[3]);
        movementMax = int.Parse(data[4]);
        attack = int.Parse(data[5]);
        range = int.Parse(data[6]);
        Movement.startingTileNumber = int.Parse(data[7]);
    }

    //Called at the end of an attack animation
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
