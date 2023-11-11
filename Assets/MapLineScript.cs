using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapLineScript : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI textComponent;
	MapEditorManager manager;
	public string map;

	private void Start()
	{
		manager = GameObject.Find("Map Editor").GetComponent<MapEditorManager>();	
	}

	public void OnPointerClick(PointerEventData eventData)
    {
		manager.MapSelected(map);
	}
}
