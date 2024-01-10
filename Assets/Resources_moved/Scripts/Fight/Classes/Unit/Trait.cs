using Unity.VisualScripting;
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
	Wealthy
}

public struct Trait
{
	public TraitsEnum traitEnum;
	public string name;
	public readonly string Description { get { return TraitText.GetTraitText(traitEnum, level); } }
	public int level;

	public Trait(TraitsEnum traitEnum, string name, int level)
	{
		this.traitEnum = traitEnum;
		this.name = name;
		this.level = level;
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
		else if (statType == StatsType.Attack)
			return level;
		else return -1;
	}
}

public static class TraitText
{
	public static string GetTraitText(TraitsEnum traitEnum, int level)
	{
		return traitEnum switch
		{
			TraitsEnum.Floaty => $"{traitEnum} lv.{level}\nUser ignores terrain condition",
			TraitsEnum.Healthy => $"{traitEnum} lv.{level}\nBase HP x{Trait.GetBonus(traitEnum, level + 1)}",
			TraitsEnum.Magic_Defence => $"{traitEnum} lv.{level}\nIncrease Ward by {Trait.GetBonus(traitEnum, level, StatsType.Ward, 100)}%",
			TraitsEnum.Overload => $"{traitEnum} lv.{level}\nx{Trait.GetBonus(traitEnum, level, StatsType.Attack)} base Attack but x{Trait.GetBonus(traitEnum, level, StatsType.HP)} damage received",
			TraitsEnum.Regeneration => $"{traitEnum} lv.{level}\nHeals {Trait.GetBonus(traitEnum, level, StatsType.HP, 100)}% of HP at the start of turn",
			TraitsEnum.Second_Wind => $"{traitEnum} lv.{level}\nOn death revives with {Trait.GetBonus(traitEnum, level, StatsType.HP, 100)}% HP",
			TraitsEnum.Speedy => $"{traitEnum} lv.{level}\n+{Trait.GetBonus(traitEnum, level)} movement",
			TraitsEnum.Strong => $"{traitEnum} lv.{level}\nBase Attack increase by x{Trait.GetBonus(traitEnum, level + 1)}",
			TraitsEnum.Tanky => $"{traitEnum} lv.{level}\n+{Trait.GetBonus(traitEnum, level, StatsType.Attack)} Attack and +{Trait.GetBonus(traitEnum, level, StatsType.HP)} HP",
			TraitsEnum.Wealthy => $"{traitEnum} lv.{level}\nDrops another {Trait.GetBonus(traitEnum, level)}g on death",
			_ => string.Empty,
		};
	}
}