using System.Linq;
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

	public void LoadCustomMap(string map, Transform parent, bool isPreview)
	{
		string[] mapData = map.Split('#')[0].Split(';');
		string[] mapLayout = map.Split('#')[1].Split(';');

		int mapRows = int.Parse(mapData[0]);
		int mapColumns = int.Parse(mapData[1]);

		Level level = new();
		level.StartLevel(0, RogueTileType.Fight, 0, 0, new Ataiku(), mapRows);
		level.GenerateTerrain(true, parent, mapLayout);
		int xPositionForMap = isPreview ? 800 : level.TopLeftSquarePositionX;
		_structureManager.GenerateFightTiles(level.tilesDict, null, mapObjects, xPositionForMap, level.YPosition, level.TopLeftSquarePositionZ, mapRows, mapColumns);
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