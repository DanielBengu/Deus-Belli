using UnityEngine;
public class InitManager : MonoBehaviour
{
    void Start()
    {
        ScenesManager.LoadSceneAsync(ScenesManager.Scenes.MainMenu);
    }
}