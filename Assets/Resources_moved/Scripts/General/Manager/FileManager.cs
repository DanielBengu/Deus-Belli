using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public static class FileManager
{
	public const string PLAYER_UNITS_PATH = "Assets\\Resources_moved\\Scripts\\General\\Player Data\\Unit list.json";
	public const string MAPS_PATH = "Assets\\Resources_moved\\Config\\Maps\\Generic";
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

	static UnitData[] GetPlayerUnits()
	{
		string data = File.ReadAllText(PLAYER_UNITS_PATH);
		var unitList = GetDataFromJSON<UnitListData>(data);
		return unitList.unitList;
	}

	public static TileMapData GetRandomGenericMap(int seed)
	{
		string[] files = Directory.GetFiles(MAPS_PATH, $"*.json");
		files = files.OrderBy(f => RandomManager.GetRandomValue(seed, 0, 12345678)).ToArray();
		TileMapData map = new();
		bool foundValidMap = false;
		for (int i = 0;i < files.Length; i++)
		{
			try
			{
				string mapData = File.ReadAllText(files[i]);
				map = GetDataFromJSON<TileMapData>(mapData);
				bool isMapValid = Validator.Validate(map);
				if (isMapValid)
				{
					foundValidMap = true;
					break;
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"Error while opening the file {files[i]}: {e.Message}");
			}
		}
		if (!foundValidMap)
			map = null;
		return map;
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
