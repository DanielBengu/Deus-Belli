using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
	FightManager fm;

	private void Start()
	{
		fm = GameObject.Find(GeneralManager.FIGHT_MANAGER_OBJ_NAME).GetComponent<FightManager>();
	}

	public void FinishAnimation()
	{
		fm.structureManager.FinishAnimation(gameObject);
	}
	public void FinishSimpleAttack()
	{
		FinishAnimation();
		fm.MakeUnitTakeDamage();
	}

	public void FinishTakingDamage()
	{
		FinishAnimation();
	}

	public void Die()
	{
		Unit unit = GetComponent<Unit>();
		fm.UnitDies(unit);
	}
}
