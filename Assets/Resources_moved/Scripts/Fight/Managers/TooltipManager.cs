using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static Unit;

public class TooltipManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] Transform tooltipParent;
	[SerializeField] GameObject tooltipPrefab;
	GameObject currentTooltip;

	// Calculate an offset to position the tooltip above the cursor
	[SerializeField]
	Vector3 offset = new(0f, 105f, 0f);

	public string header;
	public string text;

	public void OnPointerEnter(PointerEventData eventData)
	{
		// Instantiate the tooltip prefab and position it near the cursor
		currentTooltip = Instantiate(tooltipPrefab, Input.mousePosition + offset, tooltipParent.transform.rotation, tooltipParent);

		currentTooltip.transform.position = Input.mousePosition + offset;
		currentTooltip.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = header;
		currentTooltip.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = text;
	}

	void Update()
	{
		if(currentTooltip != null)
			currentTooltip.transform.position = Input.mousePosition + offset;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		// Destroy the tooltip when the cursor leaves the image
		if (currentTooltip != null)
		{
			Destroy(currentTooltip);
			currentTooltip = null;
		}
	}
}