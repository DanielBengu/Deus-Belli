using System.Collections.Generic;
[System.Serializable]
public class UnitListData
{
	public UnitData[] unitList;
}

[System.Serializable]
public class UnitData
{
	public string Name;
	public string ModelName;
	public string PortraitName;
	public Stats Stats = new();
	public bool RandomizedTraits;
	public List<Traits> Traits;
	public int Faction;
	public string AttackType;
}

[System.Serializable]
public class Stats
{
	public int Hp;
	public int Armor;
	public int Ward;
	public int Movement;
	public int Attack;
	public int Range;
}

[System.Serializable]
public class Traits
{
	public string Name;
	public int Level;
}