using UnityEngine;

public class CharactersButtonScript : MonoBehaviour
{
    public void SelectGod(int god)
	{
        NewGameManager ngManager = GameObject.Find("NewGameCanvas").GetComponent<NewGameManager>();
        ngManager.SelectGod(god);
    }
}
