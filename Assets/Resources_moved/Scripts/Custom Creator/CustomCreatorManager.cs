using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomCreatorManager : MonoBehaviour
{
    public static void BackButton()
	{
        ScenesManager.LoadSceneAsync(ScenesManager.Scenes.MainMenu);
    }
}