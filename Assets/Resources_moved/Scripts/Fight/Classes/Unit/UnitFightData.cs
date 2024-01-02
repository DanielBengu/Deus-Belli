using UnityEngine;

public class UnitFightData
{
    Unit parent;
	public Sprite sprite;
	public int currentHp;
	public int currentMovement;

    public UnitFightData(Unit parent)
    {
        this.parent = parent;

        Sprite unitSprite = AddressablesManager.LoadResource<Sprite>(AddressablesManager.TypeOfResource.Sprite, parent.UnitData.PortraitName);
		sprite = unitSprite;

        currentHp = parent.UnitData.Stats.Hp;
        currentMovement = parent.UnitData.Stats.Movement;
    }

    public void StartOfTurnEffects()
    {
        currentMovement = parent.UnitData.Stats.Movement;
    }
}