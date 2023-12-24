using UnityEngine;

public class UnitFightData
{
	public Sprite sprite;
	public int currentHp;
	public int currentMovement;

    public UnitFightData(string portraitName, int hp, int movement)
    {
        Sprite unitSprite = AddressablesManager.LoadResource<Sprite>(AddressablesManager.TypeOfResource.Sprite, portraitName);

		sprite = unitSprite;
        currentHp = hp;
        currentMovement = movement;
    }
}