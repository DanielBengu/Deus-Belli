using UnityEngine;

public class CharactersButtonScript : MonoBehaviour
{
    public void SelectGod(int god)
	{
        NewGameManager ngManager = GameObject.Find("NewGameManager").GetComponent<NewGameManager>();
        ngManager.SelectGod(god);
    }
}
