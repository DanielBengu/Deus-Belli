using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class InitManager : MonoBehaviour
{
    void Start()
    {
        ScenesManager.LoadSceneAsync(ScenesManager.Scenes.MainMenu);
    }
}