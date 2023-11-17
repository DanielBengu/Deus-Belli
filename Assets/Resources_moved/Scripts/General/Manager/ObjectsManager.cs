using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class ObjectsManager
{
	public static GameObject[] LoadObjects()
	{
		GameObject[] result = null;
		AsyncOperationHandle<IList<GameObject>> loadOp = Addressables.LoadAssetsAsync<GameObject>("Terrains", null, true);

		// Wait for the operation to complete
		loadOp.WaitForCompletion();

		if (loadOp.Status == AsyncOperationStatus.Succeeded)
			result = loadOp.Result.ToArray();
		else
			Debug.LogError($"Failed to load prefabs. Error: {loadOp.OperationException}");

		Addressables.Release(loadOp);
		return result;
	}

	public static GameObject GetRandomObject(int seed, TypeOfObstacle obstacleType, MapTheme theme)
	{
		GameObject[] objList = LoadObjects();
		//Model3D[] validObjects = model3DArchive.Where(m => m.obstacleType == obstacleType && m.theme == theme).ToArray();
		int index = RandomManager.GetRandomValue(seed, 0, objList.Length);
		return objList[index];
	}

	public static GameObject GetObject(string objectName)
	{
		GameObject[] objList = LoadObjects();
		return objList.Where(m => m.name == objectName).First();
	}

	public struct Model3D
	{
		public GameObject model;
		public TypeOfObstacle obstacleType;
		public MapTheme theme;
		public Model3D(GameObject model, TypeOfObstacle obstacleType, MapTheme theme)
		{
			this.model = model;
			this.obstacleType = obstacleType;
			this.theme = theme;
		}
	}

	public enum TypeOfObstacle
	{
		Terrain,
		SingleTile,
		TwoTimesTwo
	}

	public enum MapTheme
	{
		Plains,
		Snow,
		Hills
	}


}