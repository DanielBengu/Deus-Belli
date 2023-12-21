[System.Serializable]
public class UnitListData
{
	public UnitData[] unitList;
}

[System.Serializable]
public class UnitData
{
	public string Name;
	public string model_name;
	public string portrait_name;
	public Stats stats;
}

[System.Serializable]
public class Stats
{
	public int hp;
	public int movement;
	public int attack;
	public int range;
}