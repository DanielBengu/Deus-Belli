using System.Collections.Generic;
using static Unit;

//All traits in game
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
	Swift_Attack,
	Plunderer__s_Fortune,
	Poisoned_Tip,
}

public enum TraitAttributes
{
	OnlyPlayerObtainable, //Only Player units can obtain this trait
    OnlyEnemyObtainable, //Only Enemy units can obtain this trait
    MustBeMelee, //Requires a melee (range = 1) unit
    MustBeRanged, //Requires a range (range > 1) unit
    Specific, //Made for specific situation (event/boss fight), cannot be obtained otherwise
    Purchasable, //Can be purchased in shop
    RandomFightTrait //Can be randomly generated during fights or in events
}

[System.Serializable]
public class Trait
{
    public string Name;
    public int MaxLevel;
    public List<string> Attributes;
}

[System.Serializable]
public class TraitList
{
    public List<Trait> Traits;
}

public struct TraitStruct
{
	public TraitsEnum traitEnum;
	public string name;
	public int level;
	public bool enabled;
	public List<TraitAttributes> attributes;

	public TraitStruct(TraitsEnum traitEnum, string name, int level, bool enabled)
	{
		this.traitEnum = traitEnum;
		this.name = name;
		this.level = level;
		this.enabled = enabled;
		attributes = GetAttributesOfTrait(traitEnum);
	}

	public static List<TraitAttributes> GetAttributesOfTrait(TraitsEnum traitEnum)
	{
		return traitEnum switch
		{
			TraitsEnum.Floaty => new() { TraitAttributes.Purchasable, TraitAttributes.RandomFightTrait},
			TraitsEnum.Healthy => new() { TraitAttributes.Purchasable, TraitAttributes.RandomFightTrait},
            TraitsEnum.Magic_Defence => new() { TraitAttributes.Purchasable, TraitAttributes.RandomFightTrait},
            TraitsEnum.Overload => new() { TraitAttributes.Purchasable },
            TraitsEnum.Regeneration => new() { TraitAttributes.Purchasable },
            TraitsEnum.Second_Wind => new() { TraitAttributes.Purchasable },
            TraitsEnum.Speedy => new() { TraitAttributes.Purchasable, TraitAttributes.RandomFightTrait },
            TraitsEnum.Strong => new() { TraitAttributes.Purchasable, TraitAttributes.RandomFightTrait },
            TraitsEnum.Tanky => new() { TraitAttributes.Purchasable, TraitAttributes.RandomFightTrait },
            TraitsEnum.Wealthy => new() { TraitAttributes.Purchasable, TraitAttributes.RandomFightTrait, TraitAttributes.OnlyEnemyObtainable },
			TraitsEnum.Swift_Attack => new() { TraitAttributes.Purchasable, TraitAttributes.RandomFightTrait },
            TraitsEnum.Plunderer__s_Fortune => new() { TraitAttributes.Specific },
			TraitsEnum.Poisoned_Tip => new() { TraitAttributes.MustBeRanged, TraitAttributes.Purchasable, TraitAttributes.OnlyPlayerObtainable, TraitAttributes.OnlyEnemyObtainable },
			_ => new(),
		};
	}

	public static int GetMaxLevelOfTrait(TraitsEnum traitEnum)
	{
        return traitEnum switch
        {
            TraitsEnum.Floaty => 1,
            TraitsEnum.Healthy => 3,
            TraitsEnum.Magic_Defence => 4,
            TraitsEnum.Overload => 1,
            TraitsEnum.Regeneration => 3,
            TraitsEnum.Second_Wind => 1,
            TraitsEnum.Speedy => 5,
            TraitsEnum.Strong => 3,
            TraitsEnum.Tanky => 3,
            TraitsEnum.Wealthy => 5,
            TraitsEnum.Swift_Attack => 1,
            TraitsEnum.Plunderer__s_Fortune => 1,
			TraitsEnum.Poisoned_Tip => 3,
            _ => new(),
        };
    }

	public static bool ValidateAttributesForUnit(UnitData unit, List<TraitAttributes> attributes)
	{
		bool valid = true;

		foreach (var attribute in attributes)
		{
			switch (attribute)
			{
				case TraitAttributes.OnlyPlayerObtainable:
					valid = unit.Faction == FightManager.USER_FACTION;
					break;
				case TraitAttributes.OnlyEnemyObtainable:
                    valid = unit.Faction != FightManager.USER_FACTION;
                    break;
				case TraitAttributes.MustBeMelee:
					valid = unit.Stats.Range == 1;
					break;
				case TraitAttributes.MustBeRanged:
					valid = unit.Stats.Range > 1;
					break;
			}

			if (!valid)
				break;
		}

		return valid;
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
			TraitsEnum.Plunderer__s_Fortune => GetPlundererBonus(statType, level, baseValue),
			TraitsEnum.Poisoned_Tip => GetPoisonedTipBonus(statType, level, baseValue),
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

	static int GetPoisonedTipBonus(StatsType statsType, int level, int baseValue)
	{
		if (statsType == StatsType.Attack)
			return baseValue * GetPoisonedTipBonus(StatsType.HP, level, baseValue) / 100;
		else if (statsType == StatsType.HP)
			return 10 * level;
		else if (statsType == StatsType.Timer)
			return 3;
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

	static int GetPlundererBonus(StatsType statType, int level, int baseValue)
	{
		if (statType == StatsType.Gold)
			return 400 - (level * 100);
		else if (statType == StatsType.Attack)
			return baseValue / GetPlundererBonus(StatsType.Gold, level, baseValue);
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
	const string POISON = "<color=green>POISON</color>";
	public static string GetTraitHeader(TraitsEnum traitEnum,  int level)
	{
		return $"<color=red><b>{GetConvertedText(traitEnum.ToString())}</b></color> lv.{level}\n";
	}
	public static string GetTraitText(TraitsEnum traitEnum, int level)
	{
		return traitEnum switch
		{
			TraitsEnum.Floaty => $"User ignores terrain condition",
			TraitsEnum.Healthy => $"Base {HP} x{TraitStruct.GetBonus(traitEnum, level + 1)}",
			TraitsEnum.Magic_Defence => $"Increase {WARD} by {TraitStruct.GetBonus(traitEnum, level, StatsType.Ward, 100)}%",
			TraitsEnum.Overload => $"x{TraitStruct.GetBonus(traitEnum, level, StatsType.Attack)} base {ATTACK} but x{TraitStruct.GetBonus(traitEnum, level, StatsType.HP)} damage received",
			TraitsEnum.Regeneration => $"Heals {TraitStruct.GetBonus(traitEnum, level, StatsType.HP, 100)}% of {HP} at the start of turn",
			TraitsEnum.Second_Wind => $"On death revives with {TraitStruct.GetBonus(traitEnum, level, StatsType.HP, 100)}% {HP}",
			TraitsEnum.Speedy => $"+{TraitStruct.GetBonus(traitEnum, level)} {MOVEMENT}",
			TraitsEnum.Strong => $"Base {ATTACK} increase by x{TraitStruct.GetBonus(traitEnum, level + 1)}",
			TraitsEnum.Tanky => $"+{TraitStruct.GetBonus(traitEnum, level, StatsType.Armor)} {ARMOR} and +{TraitStruct.GetBonus(traitEnum, level, StatsType.HP)} {HP}",
			TraitsEnum.Wealthy => $"Drops another {TraitStruct.GetBonus(traitEnum, level)}g on death",
			TraitsEnum.Swift_Attack => $"Unit can't be retaliated during attacks",
			TraitsEnum.Plunderer__s_Fortune => $"Unit gets 1 {ATTACK} for every {TraitStruct.GetBonus(traitEnum, level, StatsType.Gold)}g from player",
			TraitsEnum.Poisoned_Tip => $"Ranged attacks {POISON} the enemy, making it take {TraitStruct.GetBonus(traitEnum, level, StatsType.HP)}% of {HP} as damage",
			_ => string.Empty,
		};
	}

	//Converts symbols in respective character
	// _ is space, __ is single quote (')
	public static string GetConvertedText(string traitName)
	{
		traitName = traitName.Replace("__", "\'");
		traitName = traitName.Replace('_', ' ');
		return traitName;
	}
}