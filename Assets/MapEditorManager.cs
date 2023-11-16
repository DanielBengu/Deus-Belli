using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UI;

public class MapEditorManager : MonoBehaviour
{
	static readonly string CUSTOM_MAP_ARCHIVE = "CustomMapArchive";
	Image[] carouselList = null;
	GameObject[] objectsList;
	Image currentCarousel;

	[SerializeField] Transform tileParents;

	[SerializeField] GameObject mainCanvas;
	[SerializeField] GameObject mapCreatorSection;
	[SerializeField] GameObject mapLoadPanel;
	[SerializeField] GameObject mapLoadContentViewPort;
	[SerializeField] GameObject mapLoadViewPortLine;
	[SerializeField] Button editMapButton;
	[SerializeField] GameObject insertNamePanel;
	[SerializeField] Transform mapLoadPreviewParent;
	[SerializeField] Transform mainCamera;
	[SerializeField] GameObject carouselItems;

	CustomCreatorManager manager;
	public Transform rotator;

	string mapArchivePath;
	GameObject itemSelected;

	public string mapSelected = string.Empty;
	public int mapColumns;
	public int mapRows;

	public CustomSection currentSection;
	List<TextMeshProUGUI> mapsList = new();

	void Start()
	{
		mapArchivePath = AddressablesManager.LoadPath(AddressablesManager.TypeOfResource.TXT, CUSTOM_MAP_ARCHIVE);
		manager = GameObject.Find("Manager").GetComponent<CustomCreatorManager>();
		currentSection = CustomSection.Initial;
	}
	void Update()
	{
		if (!(currentSection == CustomSection.Edit_Custom_Map || currentSection == CustomSection.Load_Custom_Map))
			return;
		if(currentSection == CustomSection.Edit_Custom_Map)
			CameraManager.UpdatePositionOrRotation(rotator, GeneralManager.CurrentSection.Custom, new(200f, 1200f));
	}

	//We set the objects from which the carousel fills
	void SetObjects()
	{
		AsyncOperationHandle<IList<GameObject>> loadOp = Addressables.LoadAssetsAsync<GameObject>("Terrains", null, true);

		// Wait for the operation to complete
		loadOp.WaitForCompletion();

		if (loadOp.Status == AsyncOperationStatus.Succeeded)
			objectsList = loadOp.Result.ToArray();
		else
			Debug.LogError($"Failed to load prefabs. Error: {loadOp.OperationException}");

		Addressables.Release(loadOp);
	}

	public void SetCarousel()
	{
		SetObjects();

		int carouselCount = carouselItems.transform.childCount;
		carouselList = new Image[carouselCount];
		for (int i = 0; i < carouselCount; i++)
		{
			if (objectsList.Length <= i)
				break;
			Image carouseilImage = carouselItems.transform.GetChild(i).GetComponent<Image>();
			carouseilImage.sprite = AddressablesManager.LoadResource<Sprite>(AddressablesManager.TypeOfResource.Sprite, objectsList[i].name);
			carouselList[i] = carouseilImage;
		}

		currentCarousel = carouselList[0];
		itemSelected = objectsList[0];
	}

	public bool IsStandby(CustomSection section)
	{
		return section switch
		{
			CustomSection.Initial => currentSection == CustomSection.Load_Custom_Map || currentSection == CustomSection.Ask_For_Input,
			CustomSection.Edit_Custom_Map => currentSection == CustomSection.Load_Custom_Map || currentSection == CustomSection.Ask_For_Input,
			CustomSection.Load_Custom_Map => currentSection == CustomSection.Ask_For_Input,
			CustomSection.Ask_For_Input => false,
			CustomSection.Edit_Custom_Unit => currentSection == CustomSection.Ask_For_Input,
			_ => false,
		};
	}

	public void ItemFromCarouselSelected(int item)
	{
		if (IsStandby(CustomSection.Edit_Custom_Map))
			return;

		itemSelected = ObjectsManager.GetObject(carouselList[item].sprite.name);
	}

	public void ChangeTile(Tile tile)
	{
		if (IsStandby(CustomSection.Edit_Custom_Map) || Input.GetMouseButton((int)MouseButton.Middle))
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
				//The first part indicates the name of the custom map
				writer.Write($"Map1-");
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
		currentSection = CustomSection.Load_Custom_Map;
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
		currentSection = CustomSection.Edit_Custom_Map;
		mapLoadPanel.SetActive(false);
		if(loadBaseLevel)
			manager.LoadBaseLevel();
	}

	public void EditCustomMap()
	{
		if (IsStandby(CustomSection.Load_Custom_Map))
			return;
		currentSection = CustomSection.Edit_Custom_Map;
		LoadCustomMap(false);
		BackFromLoadMap(false);
	}

	#region Load Map Section

	void StartupLoad()
	{
		string[] maps = File.ReadAllLines(mapArchivePath);
		for (int i = 0; i < maps.Length; i++)
		{
			string mapName = maps[i].Split('-')[0];
			Vector3 spawnPosition = new(90, -20 * (i + 1), 0);
			GameObject line = Instantiate(mapLoadViewPortLine, mapLoadContentViewPort.transform);
			line.transform.localPosition = spawnPosition;
			line.name = mapName;

			TextMeshProUGUI text = line.GetComponent<TextMeshProUGUI>();
			text.text = mapName;

			MapLineScript lineScript = line.GetComponent<MapLineScript>();
			lineScript.map = maps[i];

			mapsList.Add(text);
		}
	}

	public void MapSelected(string map)
	{
		editMapButton.interactable = true;
		mapSelected = map;
		LoadCustomMap(false);
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

		currentSection = CustomSection.Ask_For_Input;
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
