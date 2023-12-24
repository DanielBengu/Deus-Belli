using System.Collections.Generic;
using UnityEngine;
using static GeneralManager;

public static class SaveManager
{
	public static void SaveGameProgress(int gold, int currentRow, int currentPositionInRow, int gameStatus, List<UnitData> unitList)
	{
		PlayerPrefs.SetInt(GOLD, gold);
		PlayerPrefs.SetInt(CURRENT_ROW, currentRow);
		PlayerPrefs.SetInt(CURRENT_POSITION_IN_ROW, currentPositionInRow);
		PlayerPrefs.SetInt(GAME_STATUS, gameStatus);
		SaveUnits(unitList);
	}
	public static void SaveUnits(List<UnitData> unitsList)
	{
		UnitListData unitListData = new() { unitList = unitsList.ToArray() };
		string unitJSON =  JsonUtility.ToJson(unitListData);

		FileManager.OverwriteFile(FileManager.PLAYER_UNITS_PATH, unitJSON);
	}
}