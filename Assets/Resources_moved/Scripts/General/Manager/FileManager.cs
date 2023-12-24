using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static Unit;
using static UnityEditor.FilePathAttribute;
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
	public static List<UnitData> GetUnits(DataSource source, string[] CustomData = null)
	{
		List<UnitData> playerUnits = new();
		switch (source)
		{
			case DataSource.PlayerUnits:
				playerUnits = GetPlayerUnits().ToList();
				break;
			case DataSource.Custom:
				for (int i = 0; i < CustomData.Length; i++)
				{
					try
					{
						/*string[] data = CustomData[i].Split('#');
						GameObject unitObject = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Units, data[0]);
						Unit unitScript = unitObject.GetComponent<Unit>();
						unitScript.Load(unitScript.unitData);
						unitScript.LoadData(data);
						playerUnits.Add(unitScript);*/
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
			Unit unitScript = new();
			unitScript.unitData.ModelName = unit.ModelName;
			unitScript.fightData.sprite = AddressablesManager.LoadResource<Sprite>(AddressablesManager.TypeOfResource.Sprite, unit.PortraitName);
			unitScript.unitData.Name = unit.Name;
			unitScript.unitData.Stats.Attack = unit.Stats.Attack;
			unitScript.unitData.Stats.Hp = unit.Stats.Hp;
			unitScript.unitData.Stats.Range = unit.Stats.Range;
			unitScript.unitData.Stats.Movement = unit.Stats.Movement;
			unitScript.unitData.Traits = unit.Traits;

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

	static UnitData[] GetPlayerUnits()
	{
		string data = File.ReadAllText(PLAYER_UNITS_PATH);
		var unitList = GetDataFromJSON<UnitListData>(data);
		return unitList.unitList;//return ConvertFromUnitJSON(unitList.unitList);
	}
	public static T GetDataFromJSON<T>(string jsonString)
	{
		T unitListData = JsonUtility.FromJson<T>(jsonString);
		return unitListData;
	}
	public static void OverwriteFile(string filePath, string dataLines)
	{
		File.WriteAllText(filePath, dataLines);
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
