using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public static class ScenesManager
{
	public static void LoadSceneAsync(Scenes scene)
	{
        // Load the scene asynchronously by its Addressable address.
        AsyncOperationHandle<SceneInstance> sceneLoadHandle = Addressables.LoadSceneAsync($"Scenes/{scene}", LoadSceneMode.Single);

        // Use the completed handle to get the loaded scene.
        sceneLoadHandle.Completed += operationHandle =>
        {
            if (operationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                SceneInstance sceneInstance = operationHandle.Result;
                SceneManager.SetActiveScene(sceneInstance.Scene);
            }
            else
            {
                Debug.LogError($"Failed to load the scene {scene}: {operationHandle.OperationException}");
            }
        };
    }

    public enum Scenes
	{
        MainMenu,
        Fight,
        CustomCreator
	}
}
