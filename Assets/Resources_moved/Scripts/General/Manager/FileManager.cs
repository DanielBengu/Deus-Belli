using System.Collections.Generic;
using System.IO;
using UnityEngine;
public static class FileManager
{
	public const string PLAYER_UNITS_PATH = "Assets\\Resources_moved\\Scripts\\General\\Player Data\\Unit list.json";
	public static List<Unit> GetUnits(DataSource source, string[] CustomData = null)
	{
		List<Unit> playerUnits = new();
		switch (source)
		{
			case DataSource.PlayerUnits:
				playerUnits = GetPlayerUnits();
				break;
			case DataSource.Custom:
				for (int i = 0; i < CustomData.Length; i++)
				{
					try
					{
						string[] data = CustomData[i].Split('#');
						GameObject unitObject = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Units, data[0]);
						Unit unitScript = unitObject.GetComponent<Unit>();
						unitScript.Load(unitScript);
						unitScript.LoadData(data);
						playerUnits.Add(unitScript);
					}
					catch
					{
					}
				}
				break;
			default:
				return null;
		}
		return playerUnits;
	}

	public static List<Unit> ConvertFromUnitJSON(UnitListData unitListData)
	{
		List<Unit> units = new List<Unit>();
		foreach (var unitData in unitListData.unitList)
		{
			GameObject unitObject = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Units, unitData.model_name);
			Unit unit = unitObject.GetComponent<Unit>();
			unit.unitImage = AddressablesManager.LoadResource<Sprite>(AddressablesManager.TypeOfResource.Sprite, unitData.portrait_name);
			unit.unitName = unitData.Name;
			unit.attack = unitData.stats.attack;
			unit.hpMax = unitData.stats.hp;
			unit.range = unitData.stats.range;
			unit.movementMax = unitData.stats.movement;
			units.Add(unit);
		}
		return units;
	}

	static List<Unit> GetPlayerUnits()
	{
		string data = File.ReadAllText(PLAYER_UNITS_PATH);
		var unitList = CreateFromJSON(data);
		return ConvertFromUnitJSON(unitList);
	}

	public static UnitListData CreateFromJSON(string jsonString)
	{
		UnitListData unitListData = JsonUtility.FromJson<UnitListData>(jsonString);
		return unitListData;
	}

	public static void OverwriteFile(string filePath, string[] dataLines)
	{
		File.WriteAllLines(filePath, dataLines);
	}

	public static void AppendFile(string filePath, string[] dataLines)
	{
		File.AppendAllLines(filePath, dataLines);
	}

	public enum DataSource
	{
		PlayerUnits,
		Custom,
	}
}
