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

	[SerializeField] GameObject mainCanvas;
	[SerializeField] GameObject mapCreatorSection;
	[SerializeField] GameObject mapLoadPanel;
	[SerializeField] GameObject mapLoadContentViewPort;
	[SerializeField] GameObject mapLoadViewPortLine;
	[SerializeField] Button editMapButton;
	[SerializeField] GameObject insertNamePanel;
	[SerializeField] Transform mapLoadPreviewParent;

	CustomCreatorManager manager;

	string mapArchivePath;

	Image currentCarousel;
	GameObject itemSelected;

	public string mapSelected = string.Empty;
	public int mapColumns;
	public int mapRows;

	bool isMapLoadActive = false;
	bool isAskingForInput = false;
	List<TextMeshProUGUI> mapsList = new();

	void Start()
	{
		currentCarousel = carousel0;
		mapArchivePath = AddressablesManager.LoadPath(AddressablesManager.TypeOfResource.TXT, CUSTOM_MAP_ARCHIVE);
		manager = GameObject.Find("Manager").GetComponent<CustomCreatorManager>();
	}
	void Update()
	{
		if (!isMapLoadActive)
			return;

		manager.cameraManager.UpdatePositionOrRotation(mapLoadPreviewParent, GeneralManager.CurrentSection.Custom);
	}

	public bool IsStandby(CustomSection section)
	{
		return section switch
		{
			CustomSection.Initial => isMapLoadActive || isAskingForInput,
			CustomSection.Edit_Custom_Map => isMapLoadActive || isAskingForInput,
			CustomSection.Load_Custom_Map => isAskingForInput,
			CustomSection.Ask_For_Input => false,
			CustomSection.Edit_Custom_Unit => isAskingForInput,
			_ => false,
		};
	}

	public void ItemFromCarouselSelected(int item)
	{
		if (IsStandby(CustomSection.Edit_Custom_Map))
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
		if (IsStandby(CustomSection.Edit_Custom_Map))
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

	public void SetActiveTrueSection(CustomSection section)
	{
		mapCreatorSection.SetActive(false);
		mapLoadPanel.SetActive(false);
		insertNamePanel.SetActive(false);

		switch (section)
		{
			case CustomSection.Initial:
				break;
			case CustomSection.Edit_Custom_Map:
				mapCreatorSection.SetActive(true);
				break;
			case CustomSection.Load_Custom_Map:
				mapLoadPanel.SetActive(true);
				break;
			case CustomSection.Ask_For_Input:
				insertNamePanel.SetActive(false);
				break;
			case CustomSection.Edit_Custom_Unit:
				break;
			default:
				break;
		}
	}

	public void SaveMap()
	{
		if (IsStandby(CustomSection.Edit_Custom_Map))
			return;

		try
		{
			using (StreamWriter writer = File.AppendText(mapArchivePath))
			{
				writer.Write($"{mapRows};{mapColumns}#");
				for (int i = 0; i < tileParents.childCount; i++)
				{
					Tile tile = GameObject.Find($"Terrain_{i}").GetComponent<Tile>();
					writer.Write(tile.modelName);
					if (i != tileParents.childCount - 1)
						writer.Write(";");
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
		if (IsStandby(CustomSection.Edit_Custom_Map))
			return;

		ClearMap(tileParents);
		isMapLoadActive = true;
		SetActiveTrueSection(CustomSection.Load_Custom_Map);
		StartupLoad();
	}

	public void BackFromLoadMap(bool loadBaseLevel)
	{
		if (IsStandby(CustomSection.Load_Custom_Map))
			return;

		mapSelected = string.Empty;
		ClearMap(mapLoadPreviewParent);
		SetActiveTrueSection(CustomSection.Edit_Custom_Map);
		isMapLoadActive = false;
		mapLoadPanel.SetActive(false);
		if(loadBaseLevel)
			manager.LoadBaseLevel();
	}

	public void EditCustomMap()
	{
		if (IsStandby(CustomSection.Load_Custom_Map))
			return;

		LoadCustomMap(false);
		BackFromLoadMap(false);
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

			MapLineScript lineScript = line.GetComponent<MapLineScript>();
			lineScript.map = maps[i];

			mapsList.Add(text);
		}
	}

	public void MapSelected(string map)
	{
		editMapButton.interactable = true;
		mapSelected = map;
		LoadCustomMap(true);
	}

	public void LoadCustomMap(bool isPreview)
	{
		Transform parent = isPreview ? mapLoadPreviewParent : tileParents;
		ClearMap(parent);
		manager.LoadCustomMap(mapSelected, parent, isPreview);
	}

	void ClearMap(Transform parent)
	{
		for (int i = 0; i < parent.childCount; i++)
			Destroy(parent.GetChild(i).gameObject);
	}

	public void AskForName()
	{
		if (IsStandby(CustomSection.Load_Custom_Map))
			return;

		SetActiveTrueSection(CustomSection.Ask_For_Input);
	}

	#endregion

	public enum CustomSection
	{
		Initial,
		Edit_Custom_Map,
		Load_Custom_Map,
		Ask_For_Input,
		Edit_Custom_Unit,
	}
}
