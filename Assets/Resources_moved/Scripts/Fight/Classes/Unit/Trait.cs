using static Unit;

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
	Wealthy,
	Swift_Attack
}

public struct Trait
{
	public TraitsEnum traitEnum;
	public string name;
	public int level;
	public bool enabled;

	public Trait(TraitsEnum traitEnum, string name, int level, bool enabled)
	{
		this.traitEnum = traitEnum;
		this.name = name;
		this.level = level;
		this.enabled = enabled;
	}

	public static int GetBonus(TraitsEnum traitEnum, int level, StatsType statType = StatsType.HP, int baseValue = 1)
	{
		return traitEnum switch
		{
			TraitsEnum.Floaty => -1,
			TraitsEnum.Healthy => level * baseValue,
			TraitsEnum.Magic_Defence => (level + 4) * 10 * baseValue / 100,//level 1: 40% of bv, level 3: 70% of bv
			TraitsEnum.Overload => GetOverloadBonus(statType, level, baseValue),
			TraitsEnum.Regeneration => level * 5 * baseValue / 100,
			TraitsEnum.Second_Wind => (level + 2) * 10 * baseValue / 100,//level 1: 30% of bv, level 3: 50% of bv
			TraitsEnum.Speedy => level + 1,
			TraitsEnum.Strong => level * baseValue,
			TraitsEnum.Tanky => GetTankyBonus(statType, level),
			TraitsEnum.Wealthy => level * 100,
			TraitsEnum.Swift_Attack => -1,
			_ => -1,
		};
	}

	static int GetOverloadBonus(StatsType statType, int level, int baseValue)
	{
		if (statType == StatsType.Attack)
			return (level + 2) * baseValue;
		else if (statType == StatsType.HP)
			return (level + 1) * baseValue;
		else return -1;
	}

	static int GetTankyBonus(StatsType statType, int level)
	{
		if (statType == StatsType.HP)
			return level * 2;
		else if (statType == StatsType.Armor)
			return level;
		else return -1;
	}

	public void DisableTrait()
	{
		enabled = false;
	}
}

public static class TraitText
{
	const string HP = "<color=#FF0000>HP</color>";
	const string ATTACK = "<color=#00ECFF>ATTACK</color>";
	const string ARMOR = "<color=#464646>ARMOR</color>";
	const string WARD = "<color=#0A7121>WARD</color>";
	const string MOVEMENT = "<color=#00FF28>MOVEMENT</color>";
	public static string GetTraitHeader(TraitsEnum traitEnum,  int level)
	{
		return $"<color=red><b>{traitEnum.ToString().Replace('_', ' ')}</b></color> lv.{level}\n";
	}
	public static string GetTraitText(TraitsEnum traitEnum, int level)
	{
		return traitEnum switch
		{
			TraitsEnum.Floaty => $"User ignores terrain condition",
			TraitsEnum.Healthy => $"Base {HP} x{Trait.GetBonus(traitEnum, level + 1)}",
			TraitsEnum.Magic_Defence => $"Increase {WARD} by {Trait.GetBonus(traitEnum, level, StatsType.Ward, 100)}%",
			TraitsEnum.Overload => $"x{Trait.GetBonus(traitEnum, level, StatsType.Attack)} base {ATTACK} but x{Trait.GetBonus(traitEnum, level, StatsType.HP)} damage received",
			TraitsEnum.Regeneration => $"Heals {Trait.GetBonus(traitEnum, level, StatsType.HP, 100)}% of {HP} at the start of turn",
			TraitsEnum.Second_Wind => $"On death revives with {Trait.GetBonus(traitEnum, level, StatsType.HP, 100)}% {HP}",
			TraitsEnum.Speedy => $"+{Trait.GetBonus(traitEnum, level)} {MOVEMENT}",
			TraitsEnum.Strong => $"Base {ATTACK} increase by x{Trait.GetBonus(traitEnum, level + 1)}",
			TraitsEnum.Tanky => $"+{Trait.GetBonus(traitEnum, level, StatsType.Armor)} {ARMOR} and +{Trait.GetBonus(traitEnum, level, StatsType.HP)} {HP}",
			TraitsEnum.Wealthy => $"Drops another {Trait.GetBonus(traitEnum, level)}g on death",
			TraitsEnum.Swift_Attack => $"Unit can't be retaliated during attacks",
			_ => string.Empty,
		};
	}
}