using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomCreatorManager : MonoBehaviour
{
	[SerializeField] GameObject mapCanvas;
	[SerializeField] Transform mapObjects;

	StructureManager _structureManager;
	public void Start()
	{
		_structureManager = GameObject.Find("StructureManager").GetComponent<StructureManager>();
	}
	public static void BackButton()
	{
        ScenesManager.LoadSceneAsync(ScenesManager.Scenes.MainMenu);
    }

    public void LoadBaseLevel()
	{
		CleanupUnitEditor();

		mapCanvas.SetActive(true);
		Level baseLevel = new();

		baseLevel.StartLevel(0, RogueTileType.Fight, 0, 0, new Ataiku());
		baseLevel.GenerateTerrain(true, mapObjects);
		_structureManager.GenerateFightTiles(baseLevel.tilesDict, null, mapObjects, baseLevel.TopLeftSquarePositionX, baseLevel.YPosition, baseLevel.TopLeftSquarePositionZ, 10, 10);
	}

	void CleanupMapEditor()
	{
		mapCanvas.SetActive(false);
		for (int i = 0; i < mapObjects.childCount; i++)
		{
			Destroy(mapObjects.GetChild(i).gameObject);
		}
	}

	public void LoadBaseUnit()
	{
		CleanupMapEditor();
	}

	void CleanupUnitEditor()
	{
		mapCanvas.SetActive(false);
	}
}