using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
	FightManager fm;

	private void Start()
	{
		try
		{
            fm = GameObject.Find(GeneralManager.FIGHT_MANAGER_OBJ_NAME).GetComponent<FightManager>();
        }
		catch
		{
			//Rogue section, should not be called there in theory but in case it happens we disable it since it's a fight only script
			enabled = false;
		}
	}

	public void FinishAnimation()
	{
		fm.StructureManager.FinishAnimation(gameObject);
	}
	public void FinishSimpleAttack()
	{
		FinishAnimation();
		fm.MakeUnitTakeDamage();
	}

	public void FinishTakingDamage()
	{
		FinishAnimation();
		fm.MakeUnitRetaliate();
	}

	public void Die()
	{
		Unit unit = GetComponent<Unit>();
		fm.UnitDies(unit);
	}
}
