using System.Collections.Generic;
using UnityEngine;
using static GeneralManager;

public static class SaveManager
{
	public static void SaveGameProgress(RunData runData)
	{
		SaveData saveData = new()
		{
			Religion = runData.religion,
			God = runData.god,
			Ascension = runData.difficulty,
			Gold = runData.gold,
			Seed = runData.masterSeed,
			CurrentRow = runData.currentRow,
			CurrentPositionInRow = runData.currentPositionInRow,
			UnitList = new() { unitList = runData.unitList.ToArray() },
		};

		FileManager.SaveFileToJSON(saveData, FileManager.SAVEDATA_PATH);
	}
}