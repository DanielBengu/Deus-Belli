using System.Linq;
using UnityEngine;

public static class ObjectsManager
{
    const string basePathForFightObjects = "Prefabs/Fight/Objects";
	public static readonly Model3D[] model3DArchive = new Model3D[5]
	{
		new(Resources.Load<GameObject>($"{basePathForFightObjects}/Grass1"), TypeOfObstacle.Terrain, MapTheme.Plains),
		new(Resources.Load<GameObject>($"{basePathForFightObjects}/Grass2"), TypeOfObstacle.Terrain, MapTheme.Plains),
		new(Resources.Load<GameObject>($"{basePathForFightObjects}/Mountain"), TypeOfObstacle.SingleTile, MapTheme.Plains),
		new(Resources.Load<GameObject>($"{basePathForFightObjects}/Tree"), TypeOfObstacle.SingleTile, MapTheme.Plains),
		new(Resources.Load<GameObject>($"{basePathForFightObjects}/BirchTree"), TypeOfObstacle.SingleTile, MapTheme.Plains),
	};

	public static GameObject GetRandomObject(int seed, TypeOfObstacle obstacleType, MapTheme theme)
	{
		Model3D[] validObjects = model3DArchive.Where(m => m.obstacleType == obstacleType && m.theme == theme).ToArray();
		int index = RandomManager.GetRandomValue(seed, 0, validObjects.Length);
		return validObjects[index].model;
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