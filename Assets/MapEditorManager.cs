using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapEditorManager : MonoBehaviour
{
	static readonly string CUSTOM_MAP_ARCHIVE = "CustomMapArchive";

	[SerializeField] Image carousel0;
	[SerializeField] Image carousel1;
	[SerializeField] Image carousel2;
	[SerializeField] Image carousel3;
	[SerializeField] Image carousel4;
	[SerializeField] Image carousel5;
	[SerializeField] Transform tileParents;

	[SerializeField] GameObject mapLoadPanel;
	[SerializeField] GameObject mapLoadContentViewPort;
	[SerializeField] GameObject mapLoadViewPortLine;

	string mapArchivePath;

	Image currentCarousel;
	GameObject itemSelected;

	public int mapColumns;
	public int mapRows;

	bool isMapLoadActive = false;

	List<TextMeshProUGUI> mapsList = new();

	private void Start()
	{
		currentCarousel = carousel0;
		mapArchivePath = AddressablesManager.LoadPath(AddressablesManager.TypeOfResource.TXT, CUSTOM_MAP_ARCHIVE);
	}

	public void ItemFromCarouselSelected(int item)
	{
		if (isMapLoadActive)
			return;

		//First we reset back the selection
		currentCarousel.color = new Color(255, 255, 255);

		switch (item)
		{
			case 0:
				currentCarousel = carousel0;
				itemSelected = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Terrains, "Grass1");
				break;
			case 1:
				currentCarousel = carousel1;
				itemSelected = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Terrains, "PineTree");
				break;
			case 2:
				currentCarousel = carousel2;
				itemSelected = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Terrains, "GrassPath");
				break;
			case 3:
				currentCarousel = carousel3;
				itemSelected = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Terrains, "Mountain");
				break;
			case 4:
				currentCarousel = carousel4;
				itemSelected = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Terrains, "Tree");
				break;
			case 5:
				currentCarousel = carousel5;
				itemSelected = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Terrains, "BirchTree");
				break;
		}
		currentCarousel.color = new Color(170, 170, 170);
	}

	public void ChangeTile(Tile tile)
	{
		if (isMapLoadActive)
			return;

		GameObject newTile = Instantiate(itemSelected, tileParents);
		newTile.name = tile.name;
		newTile.transform.SetPositionAndRotation(tile.transform.position, tile.transform.rotation);
		Tile tileScript = newTile.GetComponent<Tile>();
		tileScript.tileNumber = tile.tileNumber;
		tileScript.isEdit = tile.isEdit;
		tileScript.modelName = itemSelected.name;
		Destroy(tile.gameObject);
	}

	public void SaveMap()
	{
		if (isMapLoadActive)
			return;

		try
		{
			using (StreamWriter writer = File.AppendText(mapArchivePath))
			{
				writer.Write($"{mapRows};{mapColumns};");
				for (int i = 0; i < tileParents.childCount; i++)
				{
					Tile tile = GameObject.Find($"Terrain_{i}").GetComponent<Tile>();
					writer.Write(tile.modelName + ";");
				}
				writer.WriteLine();
			}
			Debug.Log("Map saved succesfully.");
		}
		catch (IOException e)
		{
			Debug.LogError("Error while saving custom map: " + e.Message);
		}
	}

	public void LoadMap()
	{
		if (isMapLoadActive)
			return;

		isMapLoadActive = true;
		mapLoadPanel.SetActive(true);
		StartupLoad();
	}

	#region Load Map Section

	void StartupLoad()
	{
		string[] maps = File.ReadAllLines(mapArchivePath);
		for (int i = 0; i < maps.Length; i++)
		{
			Vector3 spawnPosition = new(90, -20 * (i + 1), 0);
			GameObject line = Instantiate(mapLoadViewPortLine, mapLoadContentViewPort.transform);
			line.transform.localPosition = spawnPosition;
			line.name = "MapLine " + i;

			TextMeshProUGUI text = line.GetComponent<TextMeshProUGUI>();
			text.text = "Map " + (i + 1);

			mapsList.Add(text);
		}
	}

	#endregion
}
