using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static GeneralManager;

public class CameraManager : MonoBehaviour
{
    public static float movementSpeed = 50f;
    // The minimum and maximum scroll distances
    public float minDistance = 1f;
    public float maxDistance = 10f;

    private static readonly float speedMod = 30.0f;//a speed modifier

	public static float fightscrollSpeed = 25000f;
	public static float rogueScrollSpeed = 3000f;

	Vector3 defaultCameraPosition = new(670, 1070, 73);
    Quaternion defaultCameraRotation = Quaternion.Euler((float)33.3, 0, 0);

	bool isOutOfFocus = false;

	Vector3 cameraPositionOutOfFocus = new(670, 560, 350);
	Quaternion rotationOutOfFocus = Quaternion.Euler(8, 0, 0);
	Vector3 cameraPositionOnFocus = new(735, 930, 700);
	Quaternion rotationOnFocus = Quaternion.Euler(51, 0, 0);

    #region Rogue Camera Setup
    static Vector3 rogueCameraPositionOutOfFocus = new(-343.244995F, -532.328979F, 496.550995F);
    static Quaternion rogueRotationOutOfFocus = Quaternion.Euler(12, 350, 1);

    public Vector3 rogueCameraPositionOnFocus;
    static Quaternion rogueRotationOnFocus = Quaternion.Euler(90, -98, -98); 

    bool isRogueOutOfFocus = false;

    #endregion

    public static void UpdatePositionOrRotation(Transform objectToMove, CurrentSection section, CameraMovementOptions options = null)
    {
        switch (section)
		{
			case CurrentSection.Fight:
                if (Input.GetMouseButton((int)MouseButton.Right))
                    RotateItem(objectToMove);
				else if (Input.GetMouseButton((int)MouseButton.Middle))
				{
					float[] clampValuesCustom = options.ClampValues;
					MoveItemByMouseInput(objectToMove, clampValuesCustom, section);
				}
				break;
			case CurrentSection.Rogue:
                float[] clampValues = GetCameraClamp(options.MapLength);
                MoveItemByMouseInput(objectToMove, clampValues, section);
                break;
			case CurrentSection.Custom:
                if (Input.GetMouseButton((int)MouseButton.Right))
                    RotateItem(objectToMove);
                else if (Input.GetMouseButton((int)MouseButton.Middle))
				{
                    float[] clampValuesCustom = options.GetMovementClamp();
                    MoveItemByMouseInput(objectToMove, clampValuesCustom, section);
                }
                else
                    return;
                break;
			default:
				break;
		}
    }

    static void MoveItemByMouseInput(Transform objectToMove, float[] clampValues, CurrentSection section)
	{
        bool movingX = false;
        bool movingZ = false;

        switch (section)
        {
            case CurrentSection.Fight:
			case CurrentSection.Custom:
                movingX = true;
                movingZ = true;
				break;
            case CurrentSection.Rogue:
                movingX = true;
                break;
            default:
                break;
        }

		Vector3 position = objectToMove.position;

		if (movingX)
        {
			float moveHorizontal = Input.GetAxis("Mouse X");
			position.x += moveHorizontal * movementSpeed;
			position.x = Mathf.Clamp(position.x, clampValues[0], clampValues[1]);
		}

        if (movingZ)
        {
			float moveVertical = Input.GetAxis("Mouse Y");
			position.z += moveVertical * movementSpeed;
			position.z = Mathf.Clamp(position.z, clampValues[2], clampValues[3]);
		}
        
        objectToMove.position = position;
    }
    static void TeleportItemToPosition(Transform objectToMove, float targetX)
    {
        Vector3 position = objectToMove.position;
        position.x = targetX;

        objectToMove.position = position;
    }

    public void SetupCameraToRogueOOF()
    {
        if(isRogueOutOfFocus)
            gameObject.transform.SetPositionAndRotation(rogueCameraPositionOnFocus, rogueRotationOnFocus);
        else
            gameObject.transform.SetPositionAndRotation(rogueCameraPositionOutOfFocus, rogueRotationOutOfFocus);

        isRogueOutOfFocus = !isRogueOutOfFocus;
    }

    public void SetupFightCamera(Transform camera)
    {
        camera.SetPositionAndRotation(cameraPositionOnFocus, rotationOnFocus);
	}

    static void MoveItem(Transform objectToMove, Vector3 direction, float scrollInput, float speed)
    {
		// Calculate the new distance based on the mouse wheel input
		float distance = scrollInput * speed;

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
        float speed = section == CurrentSection.Rogue ? rogueScrollSpeed : fightscrollSpeed;
		MoveItem(objectToMove, direction, scroll, speed);

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

	//Switch between the two camera states: In and Out of focus
	public void CameraFocus(StructureManager structureManager, CurrentSection section){
        Transform cameraStart = transform;
        Transform cameraTarget = GetCameraSwitchFocus(section);
        if(!isOutOfFocus && section == CurrentSection.Fight)
	        structureManager.ClearSelection(true);

        structureManager.actionPerformer.StartAction(ActionPerformed.CameraFocus, cameraStart.gameObject, cameraTarget.gameObject);
        Destroy(cameraTarget.gameObject);
    }

    public Transform GetCameraSwitchFocus(CurrentSection section)
    {
        Transform cameraTarget = new GameObject().transform;

        switch (section)
        {
            case CurrentSection.Fight:
                if (isOutOfFocus)
                    cameraTarget.SetPositionAndRotation(cameraPositionOnFocus, rotationOnFocus);
                else
                    cameraTarget.SetPositionAndRotation(cameraPositionOutOfFocus, rotationOutOfFocus);
                isOutOfFocus = !isOutOfFocus;
                break;
            case CurrentSection.Rogue:
                if (isRogueOutOfFocus)
                    cameraTarget.SetPositionAndRotation(rogueCameraPositionOnFocus, rogueRotationOnFocus);
                else
                    cameraTarget.SetPositionAndRotation(rogueCameraPositionOutOfFocus, rogueRotationOutOfFocus);
                isRogueOutOfFocus = !isRogueOutOfFocus;
                break;
            case CurrentSection.Custom:
                if (isOutOfFocus)
                    cameraTarget.SetPositionAndRotation(cameraPositionOnFocus, rotationOnFocus);
                else
                    cameraTarget.SetPositionAndRotation(cameraPositionOutOfFocus, rotationOutOfFocus);
                isOutOfFocus = !isOutOfFocus;
                break;
            default:
                break;
        }
        
        
        return cameraTarget;
    }

    public bool GetCameraFocusStatus(){
        return isOutOfFocus;
    }

    public void SetCameraFocusStatus(bool status){
        isOutOfFocus = status;
    }
	#endregion
}

public class CameraMovementOptions
{
    float _minMovementValue = -1;
    float _maxMovementValue = -1;

    public int MapLength { get; set; } = -1;
    public Transform Rotator { get; set; } = null;
    public List<Tile> Tiles { get; set; } = null;
    public float[] ClampValues { get; set; }

	#region Constructors
	public CameraMovementOptions()
	{

	}
	public CameraMovementOptions(float minClamp, float maxClamp)
    {
        ClampValues = new float[] { minClamp, maxClamp };
        _minMovementValue = minClamp;
        _maxMovementValue = maxClamp;
    }

    public CameraMovementOptions(float[] clampValues)
    {
        ClampValues = clampValues;
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