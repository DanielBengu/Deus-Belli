using System;
using System.Collections.Generic;
using UnityEngine;
using static Unit;

public class UnitFightData
{
	readonly Unit parent;
	public Sprite sprite;
	public int currentHp;
	public int currentMovement;
    public List<Trait> traitList = new();

    public UnitFightData(Unit parent)
    {
        this.parent = parent;

        Sprite unitSprite = AddressablesManager.LoadResource<Sprite>(AddressablesManager.TypeOfResource.Sprite, parent.UnitData.PortraitName);
		sprite = unitSprite;

        currentHp = parent.UnitData.Stats.Hp;
        currentMovement = parent.UnitData.Stats.Movement;

        LoadTraits(parent.UnitData.Traits);
	}

    public void StartOfTurnEffects()
    {
        currentMovement = parent.UnitData.Stats.Movement;
    }

    public void LoadTraits(List<string> traitFromJSON)
    {
        foreach (var trait in traitFromJSON)
        {
            TraitsEnum traitEnum = (TraitsEnum)Enum.Parse(typeof(TraitsEnum), trait);
			switch (traitEnum)
			{
				case TraitsEnum.Floaty:
					traitList.Add(new Trait("Floaty", "User ignores terrain condition", 1));
					break;
				case TraitsEnum.Healthy:
					traitList.Add(new Trait("Healthy", "x2 HP", 1));
					break;
				case TraitsEnum.Magic_Defence:
					traitList.Add(new Trait("Magic Defence", "Magical damage is halved", 2));
					break;
				case TraitsEnum.Overload:
					traitList.Add(new Trait("Overload", "x3 Attack but x2 damage received", 1));
					break;
				case TraitsEnum.Regeneration:
					traitList.Add(new Trait("Regeneration", "Heals 10% of HP at the start of turn", 3));
					break;
				case TraitsEnum.Second_Wind:
					traitList.Add(new Trait("Second Wind", "On death revives with 50% HP", 1));
					break;
				case TraitsEnum.Speedy:
					traitList.Add(new Trait("Speedy", "+2 movement", 1));
					break;
				case TraitsEnum.Strong:
					traitList.Add(new Trait("Strong", "x2 damage", 1));
					break;
				case TraitsEnum.Tanky:
					traitList.Add(new Trait("Tanky", "+1 damage, +2 hp", 1));
					break;
				case TraitsEnum.Wealthy:
					traitList.Add(new Trait("Wealthy", "Drops double gold on death", 1));
					break;
			}
		}
	}
}