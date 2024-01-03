using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Build.Pipeline;

public static class Validator
{
	public static bool Validate(object obj)
	{
		return obj.GetType().Name switch
		{
			"TileMapData" => ValidateMap((TileMapData)obj),
			"Unit" => ValidateUnit((Unit)obj),
			_ => true,
		};
	}

	public static bool ValidateMap(TileMapData obj)
	{
		bool isCountCorrect = obj.TileList.DistinctBy(t => t.PositionOnGrid).Count() == obj.Rows * obj.Columns;
		return isCountCorrect;
	}

	public static bool ValidateUnit(Unit unit)
	{
		return IsUnitValuedCorrectly(unit) && IsUnitStatsValid(unit);
	}

	static bool IsUnitValuedCorrectly(Unit unit)
	{
		if (unit == null)
			return false;

		bool isUnitDataValued = unit.UnitData != null;
		bool isUnitFightDataValued = unit.FightData != null;

		return isUnitDataValued && isUnitFightDataValued;
	}

	static bool IsUnitStatsValid(Unit unit)
	{
		bool isHpValid = unit.UnitData.Stats.Hp > 0;
		bool isRangeValid = unit.UnitData.Stats.Range > 0;
		bool isNameValid = unit.UnitData.Name.NullIfEmpty() != null;

		return isHpValid && isRangeValid && isNameValid;
	}
}