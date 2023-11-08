using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressablesManager
{
	static T AddressableLoad<T>(string path)
	{
		AsyncOperationHandle<T> opHandle = Addressables.LoadAssetAsync<T>(path);
		return opHandle.WaitForCompletion();
	}

	static string AddressablePath(string path)
	{
		string result = string.Empty;
		var opHandle = Addressables.LoadResourceLocationsAsync(path);
		for (int i = 0; i < opHandle.WaitForCompletion().Count; i++)
		{
			result += (opHandle.Result[i].InternalId);
		}
		return result;
	}

	public static T LoadResource<T>(TypeOfResource resource, string name)
	{
		string path = $"{resource}/{name}";
		return AddressableLoad<T>(path);
	}

	public static string LoadPath(TypeOfResource resource, string name)
	{
		string path = $"{resource}/{name}";
		return AddressablePath(path);
	}


	public enum TypeOfResource
	{
		Scenes,
		TXT,
		Units,
		Terrains,
		Sprite,
		Prefab
	}
}