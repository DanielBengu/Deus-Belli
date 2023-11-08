using UnityEngine;

public class CustomCreatorManager : MonoBehaviour
{
	[SerializeField] GameObject mapCanvas;
	[SerializeField] Transform mapObjects;

	[SerializeField] MapEditorManager _mapEditorManager;
	[SerializeField] int mapRows;
	[SerializeField] int mapColumns;

	StructureManager _structureManager;
	public void Start()
	{
		_structureManager = GameObject.Find("StructureManager").GetComponent<StructureManager>();
	}
	public static void BackButton()
	{
        ScenesManager.LoadSceneAsync(ScenesManager.Scenes.MainMenu);
    }

	#region Map Editor
	public void LoadBaseLevel()
	{
		CleanupUnitEditor();

		mapCanvas.SetActive(true);
		Level baseLevel = new();

		baseLevel.StartLevel(0, RogueTileType.Fight, 0, 0, new Ataiku(), mapRows);
		baseLevel.GenerateTerrain(true, mapObjects);
		_structureManager.GenerateFightTiles(baseLevel.tilesDict, null, mapObjects, baseLevel.TopLeftSquarePositionX, baseLevel.YPosition, baseLevel.TopLeftSquarePositionZ, mapRows, mapColumns);
		_mapEditorManager.mapRows = mapRows;
		_mapEditorManager.mapColumns = mapColumns;
	}

	void CleanupMapEditor()
	{
		mapCanvas.SetActive(false);
		for (int i = 0; i < mapObjects.childCount; i++)
		{
			Destroy(mapObjects.GetChild(i).gameObject);
		}
	}

	#endregion

	#region Unit Editor
	public void LoadBaseUnit()
	{
		CleanupMapEditor();
	}

	void CleanupUnitEditor()
	{
		mapCanvas.SetActive(false);
	}

	#endregion

}