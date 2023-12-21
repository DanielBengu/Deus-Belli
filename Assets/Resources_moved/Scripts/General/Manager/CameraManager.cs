using Unity.VisualScripting;
using UnityEngine;
using static GeneralManager;

public class CameraManager : MonoBehaviour
{
    public static float movementSpeed = 50f;
    // The minimum and maximum scroll distances
    public float minDistance = 1f;
    public float maxDistance = 10f;

    private static readonly float speedMod = 30.0f;//a speed modifier
	private static readonly float fightSpeedMod = 300.0f;//a speed modifier

	public static float scrollSpeed = 25000f;

    Vector3 defaultCameraPosition = new(670, 1070, 73);
    Quaternion defaultCameraRotation = Quaternion.Euler((float)33.3, 0, 0);
    public static void UpdatePositionOrRotation(Transform objectToMove, CurrentSection section, CameraMovementOptions options = null)
    {
        switch (section)
		{
			case CurrentSection.Fight:
                if (Input.GetMouseButton((int)MouseButton.Right))
                RotateItem(objectToMove);
                break;
			case CurrentSection.Rogue:
                float[] clampValues = GetCameraClamp(options.MapLength);
                MoveItemByMouseInput(objectToMove, clampValues[0], clampValues[1]);
                break;
			case CurrentSection.Custom:
                if (Input.GetMouseButton((int)MouseButton.Right))
                    RotateItem(objectToMove);
                else if (Input.GetMouseButton((int)MouseButton.Middle))
				{
                    float[] clampValuesCustom = options.GetMovementClamp();
                    MoveItemByMouseInput(objectToMove, clampValuesCustom[0], clampValuesCustom[1]);
                }
                else
                    return;
                break;
			default:
				break;
		}
    }

    static void MoveItemByMouseInput(Transform objectToMove, float xMin, float xMax)
	{
        float moveHorizontalRogue = Input.GetAxis("Mouse X");

        Vector3 position = objectToMove.position;
        position.x += moveHorizontalRogue * movementSpeed;

        position.x = Mathf.Clamp(position.x, xMin, xMax);
        objectToMove.position = position;
    }
    static void TeleportItemToPosition(Transform objectToMove, float targetX)
    {
        Vector3 position = objectToMove.position;
        position.x = targetX;

        objectToMove.position = position;
    }

    static void MoveItem(Transform objectToMove, Vector3 direction, float scrollInput)
    {
		// Calculate the new distance based on the mouse wheel input
		float distance = scrollInput * scrollSpeed;

		// Move the camera along the blue axis by the distance
		objectToMove.Translate(distance * Time.deltaTime * direction);
	}

    static void RotateItem(Transform objectToMove)
	{
        float moveHorizontalCustom = Input.GetAxis("Mouse X");
        objectToMove.Rotate(Vector3.up, moveHorizontalCustom * 20 * speedMod * Time.deltaTime, Space.World);
    }

    public void ResetCamera()
	{
        transform.SetPositionAndRotation(defaultCameraPosition, defaultCameraRotation);
    }

    public void HandleScroll(float scroll, Transform objectToMove, CurrentSection section, int mapLength)
	{
		Vector3 direction = section == CurrentSection.Rogue ? Vector3.right : Vector3.forward;
		MoveItem(objectToMove, direction, scroll);

        if(section == CurrentSection.Rogue)
        {
			float[] clampValues = GetCameraClamp(mapLength);
			Vector3 objectPosition = objectToMove.position;
			objectPosition.x = Mathf.Clamp(objectPosition.x, clampValues[0], clampValues[1]);
		}
	}

    static float[] GetCameraClamp(int mapLength)
    {
        float maxX = mapLength * -275;
        return new[] { maxX, -400f };
    }

    static float[] GetMovableObjectClamp(int mapLength)
    {
        float maxX = mapLength * -275;
        return new[] { maxX, -400f };
    }

    public void SetupRogueCamera(Transform objectToSetup, int mapLength)
	{
        float[] xClamp = GetMovableObjectClamp(mapLength);
        TeleportItemToPosition(objectToSetup, xClamp[1]);
    }

	#region Switch Camera Focus (legacy)
	/*
	
	bool isOutOfFocus = false;

	Vector3 cameraPositionOutOfFocus = new(670, 300, 640);
	Quaternion rotationOutOfFocus = Quaternion.Euler(0, 0, 0);
	Vector3 cameraPositionOnFocus = new(670, 690, 650);
	Quaternion rotationOnFocus = Quaternion.Euler(40, 0, 0);

	//Switch between the two camera states: In and Out of focus
	public void CameraFocus(){
    Transform cameraStart = cameraManager.GetCameraPosition();
    Transform cameraTarget = cameraManager.GetCameraSwitchFocus();
    if(IsCameraFocused){
	    structureManager.ClearSelectedTiles();
	    structureManager.SetInfoPanel(false, UnitSelected, infoPanel);
    }
    structureManager.StartObjectMovement(cameraStart, cameraTarget, 2500);
    GameObject.Destroy(cameraTarget.gameObject);
    }

        public Transform GetCameraSwitchFocus(){
        Transform cameraTarget = new GameObject().transform;
        if(isOutOfFocus){
            cameraTarget.SetPositionAndRotation(cameraPositionOnFocus, rotationOnFocus);
            isOutOfFocus = false;
        }else{
            cameraTarget.SetPositionAndRotation(cameraPositionOutOfFocus, rotationOutOfFocus);
            isOutOfFocus = true;
        }
        return cameraTarget;
    }

    public bool GetCameraFocusStatus(){
        return isOutOfFocus;
    }

    public void SetCameraFocusStatus(bool status){
        isOutOfFocus = status;
    }

    */

	#endregion
}

public class CameraMovementOptions
{
    float _minMovementValue = -1;
    float _maxMovementValue = -1;

    public int MapLength { get; set; } = -1;
    public Transform Rotator { get; set; } = null;

	#region Constructors
	public CameraMovementOptions()
	{

	}
	public CameraMovementOptions(float minClamp, float maxClamp)
    {
        _minMovementValue = minClamp;
        _maxMovementValue = maxClamp;
    }

    public CameraMovementOptions(float minClamp, float maxClamp, Transform rotator)
    {
        _minMovementValue = minClamp;
        _maxMovementValue = maxClamp;
        Rotator = rotator;
    }
    #endregion

    #region Setters
    public void SetMovementClamp(float minValue, float maxValue)
    {
        _minMovementValue = minValue;
        _maxMovementValue = maxValue;
    }
    #endregion

    #region Getters
    public float[] GetMovementClamp()
    {
        return new[] { _minMovementValue, _maxMovementValue };
    }

    public Transform GetRotator()
    {
        return Rotator;
    }
    #endregion
}