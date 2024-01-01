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
	public List<string> Traits;
	public int Faction;
}

[System.Serializable]
public class Stats
{
	public int Hp;
	public int Movement;
	public int Attack;
	public int Range;
}