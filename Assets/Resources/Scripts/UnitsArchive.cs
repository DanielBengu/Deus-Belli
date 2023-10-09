using UnityEngine;

public static class UnitsArchive
{
	public enum Units
	{
		Male_A,
		Male_B,
		Male_C,
		Female_A,
		Female_B,
		Female_C
	}

	public static GameObject GetUnit(Units unit)
	{
		string unitPrefabName = unit.ToString().Replace('_', ' ');
		return Resources.Load<GameObject>($"Prefabs/Units/{unitPrefabName}");
	}
}
