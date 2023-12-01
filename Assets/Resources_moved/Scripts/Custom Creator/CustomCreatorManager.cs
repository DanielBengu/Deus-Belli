using System.Linq;
using UnityEngine;

public class CustomCreatorManager : MonoBehaviour
{
	[SerializeField] GameObject mapCanvas;
	public Transform mapObjects;

	[SerializeField] MapEditorManager _mapEditorManager;
	[SerializeField] int mapRows;
	[SerializeField] int mapColumns;

	public CameraManager cameraManager;
	StructureManager _structureManager;
	public void Start()
	{
		_structureManager = GameObject.Find("StructureManager").GetComponent<StructureManager>();
	}
	public void BackButton()
	{
		if (_mapEditorManager.IsStandby(MapEditorManager.CustomSection.Initial))
			return;
        ScenesManager.LoadSceneAsync(ScenesManager.Scenes.MainMenu);
    }

	#region Map Editor
	public void LoadBaseLevel()
	{
		if (mapObjects.childCount > 0)
			return;

		CleanupUnitEditor();

		mapCanvas.SetActive(true);
		Level baseLevel = new();

		baseLevel.StartLevel(0, RogueTileType.Fight, 0, 0, new Ataiku(), mapRows);
		baseLevel.GenerateTerrain(true, mapObjects);
		var tiles = _structureManager.GenerateFightTiles(baseLevel.tilesDict, null, baseLevel.TopLeftSquarePositionX, baseLevel.YPosition, baseLevel.TopLeftSquarePositionZ, mapRows, mapColumns);
		_mapEditorManager.mapRows = mapRows;
		_mapEditorManager.mapColumns = mapColumns;
		_mapEditorManager.currentSection = MapEditorManager.CustomSection.Edit_Custom_Map;
		Transform topLeftTile = tiles.Values.First(t => t.tileNumber == 0).transform;
		Transform bottomRightTile = tiles.Values.First(t => t.tileNumber == (mapRows * mapColumns) - 1).transform;
		//We find the exact center of this new board
		_mapEditorManager.rotator.position = new((topLeftTile.position.x + bottomRightTile.position.x) / 2, topLeftTile.position.y, (topLeftTile.position.z + bottomRightTile.position.z) / 2);
		mapObjects.parent = _mapEditorManager.rotator;
		_mapEditorManager.SetCarousel();
	}

	void CleanupMapEditor()
	{
		mapCanvas.SetActive(false);
		for (int i = 0; i < mapObjects.childCount; i++)
			Destroy(mapObjects.GetChild(i).gameObject);
	}

	public void LoadCustomMap(string map, Transform parent, bool isPreview)
	{
		string mapWithoutName = map.Split('-')[1];
		string[] mapData = mapWithoutName.Split('#')[0].Split(';');
		string[] mapLayout = mapWithoutName.Split('#')[1].Split(';');

		int mapRows = int.Parse(mapData[0]);
		int mapColumns = int.Parse(mapData[1]);

		Level level = new();
		level.StartLevel(0, RogueTileType.Fight, 0, 0, new Ataiku(), mapRows);
		level.GenerateTerrain(true, parent, mapLayout);
		int xPositionForMap = isPreview ? 800 : level.TopLeftSquarePositionX;
		_structureManager.GenerateFightTiles(level.tilesDict, null, xPositionForMap, level.YPosition, level.TopLeftSquarePositionZ, mapRows, mapColumns);
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