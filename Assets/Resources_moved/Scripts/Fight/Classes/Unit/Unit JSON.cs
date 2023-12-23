using System.Collections.Generic;
using System.Linq;
using static Unit;

[System.Serializable]
public class UnitListData
{
	public UnitData[] unitList;

	public static UnitData[] ConvertToJSON(IEnumerable<Unit> unitList)
	{
		UnitData[] result = new UnitData[unitList.Count()];

		for (int i = 0; i < unitList.Count(); i++)
		{
			Unit currentUnit = unitList.ElementAt(i);
			result[i] = new UnitData()
			{
				ModelName = currentUnit.name,
				Name = currentUnit.unitName,
				PortraitName = currentUnit.unitImage.name,
				Stats = new()
				{
					Attack = currentUnit.attack,
					Movement = (int)currentUnit.movementMax,
					Hp = currentUnit.hpMax,
					Range = currentUnit.range
				},
				Traits = currentUnit.Traits.Select(t => t.ToString()).ToList()
			};
		}
		return result;
	}
}

[System.Serializable]
public class UnitData
{
	public string Name;
	public string ModelName;
	public string PortraitName;
	public Stats Stats;
	public bool RandomizedTraits;
	public List<string> Traits = new();
}

[System.Serializable]
public class Stats
{
	public int Hp;
	public int Movement;
	public int Attack;
	public int Range;
}