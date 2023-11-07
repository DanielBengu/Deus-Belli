using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomCreatorManager : MonoBehaviour
{
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
		Level baseLevel = new();
		baseLevel.StartLevel(0, RogueTileType.Fight, 0, 0, new Ataiku());
		Transform objectsParent = GameObject.Find("Map Objects").transform;
		baseLevel.GenerateTerrain(true, objectsParent);
		_structureManager.GenerateFightTiles(baseLevel.tilesDict, null, objectsParent, baseLevel.TopLeftSquarePositionX, baseLevel.YPosition, baseLevel.TopLeftSquarePositionZ, 10, 10);
	}

	public void LoadBaseUnit()
	{

	}
}