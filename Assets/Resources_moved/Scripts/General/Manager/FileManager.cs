using Assets.Resources_moved.Scripts.General.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public static class FileManager
{
	public const string PLAYER_UNITS_PATH = "Assets\\Resources_moved\\Config\\Player Data\\Unit list.json";
	public const string MAPS_PATH = "Assets\\Resources_moved\\Config\\Maps\\Generic";
	public const string ENCOUNTERS_PATH = "Assets\\Resources_moved\\Config\\Encounters";
	public const string GODS_PATH = "Assets\\Resources_moved\\Config\\Gods";
	public const string SAVEDATA_PATH = "Assets\\Resources_moved\\Config\\Player Data\\SaveData.json";
	public const string FAILSAFE_PATH = "Assets\\Resources_moved\\Config\\FailsafeUnit.json";

	public static List<UnitData> GetUnits(DataSource source, string[] CustomData = null)
	{
		List<UnitData> playerUnits = new();
		switch (source)
		{
			case DataSource.PlayerUnits:
				playerUnits = GetFileFromJSON<UnitListData>(PLAYER_UNITS_PATH).unitList.ToList();
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
				map = GetFileFromJSON<TileMapData>(files[i]);
				map.FillTiles();
				map.TileList = map.TileList.OrderBy(t => t.PositionOnGrid).ToList();
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
	public static T GetFileFromJSON<T>(string path)
	{
		string data = File.ReadAllText(path);
		T obj = JsonUtility.FromJson<T>(data);
		return obj;
	}

	public static void SaveFileToJSON(object data, string path)
	{
		string textData = JsonUtility.ToJson(data);
		File.WriteAllText(path, textData);
	}

	public static UnitData LoadFailsafeUnit()
	{
		return GetFileFromJSON<UnitData>(FAILSAFE_PATH);
	}

	public enum DataSource
	{
		PlayerUnits,
		Custom,
		SaveData
	}

	public enum EncounterTypes
	{
		Generic,
		Agbara,
	}
}
