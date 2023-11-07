using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressablesManager
{
	public static Dictionary<TypeOfResource, string> addressablesNames = new()
	{
		{ TypeOfResource.Scenes, "Scenes" },
		{ TypeOfResource.TXT, "TXT" },
		{ TypeOfResource.Units, "Units" },
		{ TypeOfResource.Terrains, "Terrains" },
	};

	static T AddressableLoad<T>(string path)
	{
		AsyncOperationHandle<T> opHandle = Addressables.LoadAssetAsync<T>(path);
		return opHandle.WaitForCompletion();
	}

	public static T LoadResource<T>(TypeOfResource resource, string name)
	{
		string path = $"{addressablesNames[resource]}/{name}";
		return AddressableLoad<T>(path);
	}

	public enum TypeOfResource
	{
		Scenes,
		TXT,
		Units,
		Terrains,
	}
}