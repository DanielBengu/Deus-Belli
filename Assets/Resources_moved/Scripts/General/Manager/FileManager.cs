using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static Unit;
public static class FileManager
{
	public const string PLAYER_UNITS_PATH = "Assets\\Resources_moved\\Scripts\\General\\Player Data\\Unit list.json";
	public const string ENCOUNTERS_PATH = "Assets\\Resources_moved\\Config\\Encounters";
	public static EncounterListData GetEncounters(EncounterTypes typeOfEncounter)
	{
		string data = File.ReadAllText($"{ENCOUNTERS_PATH}\\{typeOfEncounter}.json");
		var encounterList = GetDataFromJSON<EncounterListData>(data);
		return encounterList;
	}
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

	public static List<Unit> ConvertFromUnitJSON(IEnumerable<UnitData> unitData)
	{
		List<Unit> units = new();
		foreach (var unit in unitData)
		{
			GameObject unitObject = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Units, unit.ModelName);
			Unit unitScript = unitObject.GetComponent<Unit>();
			unitScript.unitImage = AddressablesManager.LoadResource<Sprite>(AddressablesManager.TypeOfResource.Sprite, unit.PortraitName);
			unitScript.unitName = unit.Name;
			unitScript.attack = unit.Stats.Attack;
			unitScript.hpMax = unit.Stats.Hp;
			unitScript.range = unit.Stats.Range;
			unitScript.movementMax = unit.Stats.Movement;
			unitScript.Traits = ConvertTraitsFromJSON(unit.Traits);
			units.Add(unitScript);
		}
		return units;
	}

	static List<TraitsEnum> ConvertTraitsFromJSON(List<string> traitsList)
	{
		List<TraitsEnum> traits = new();
		foreach (var trait in traitsList)
		{
			// Try parsing the string to an enum
			if (Enum.TryParse(trait, out TraitsEnum parsedEnum))
			{
				traits.Add(parsedEnum);
			}
			else
			{
				Debug.LogWarning($"Could not parse {trait} to Traits");
			}
		}
		return traits;
	}

	static List<Unit> GetPlayerUnits()
	{
		string data = File.ReadAllText(PLAYER_UNITS_PATH);
		var unitList = GetDataFromJSON<UnitListData>(data);
		return ConvertFromUnitJSON(unitList.unitList);
	}
	public static T GetDataFromJSON<T>(string jsonString)
	{
		T unitListData = JsonUtility.FromJson<T>(jsonString);
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

	public enum EncounterTypes
	{
		Generic,
		Agbara,
	}
}
