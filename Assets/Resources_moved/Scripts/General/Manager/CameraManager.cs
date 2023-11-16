using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static float movementSpeed = 50f;
    // The minimum and maximum scroll distances
    public float minDistance = 1f;
    public float maxDistance = 10f;

    // The current scroll distance
    private float distance = 50f;

    private static float speedMod = 30.0f;//a speed modifier

    public float scrollSpeed = 10f;

    bool isOutOfFocus = false;

    Vector3 defaultCameraPosition = new(670, 1070, 73);
    Quaternion defaultCameraRotation = Quaternion.Euler((float)33.3, 0, 0);

    Vector3 cameraPositionOutOfFocus = new(670, 300, 640);
    Quaternion rotationOutOfFocus = Quaternion.Euler(0, 0, 0);
    Vector3 cameraPositionOnFocus = new(670, 690, 650);
    Quaternion rotationOnFocus = Quaternion.Euler(40, 0, 0);
    
    public static void UpdatePositionOrRotation(Transform objectToMove, GeneralManager.CurrentSection section, CameraMovementOptions options = null)
    {
        switch (section)
		{
			case GeneralManager.CurrentSection.Fight:
                if (Input.GetMouseButton((int)MouseButton.Right))
                RotateItem(objectToMove);
                break;
			case GeneralManager.CurrentSection.Rogue:
                MoveItem(objectToMove, options);
                break;
			case GeneralManager.CurrentSection.Custom:
                if (Input.GetMouseButton((int)MouseButton.Right))
                    RotateItem(objectToMove);
                else if (Input.GetMouseButton((int)MouseButton.Middle))
                    MoveItem(objectToMove, options);
                else
                    return;
                break;
			default:
				break;
		}
    }

    static void MoveItem(Transform objectToMove, CameraMovementOptions options)
	{
        float[] clampValues = options.GetMovementClamp();
        float xMin = clampValues[0];
        float xMax = clampValues[1];

        float moveHorizontalRogue = Input.GetAxis("Mouse X");

        Vector3 position = objectToMove.position;
        position.x += moveHorizontalRogue * movementSpeed;

        position.x = Mathf.Clamp(position.x, xMin, xMax);
        objectToMove.position = position;
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

    public void ScrollWheel(float scroll, Transform rogueSection = null){
        // Calculate the new distance based on the mouse wheel input
        distance += scroll * scrollSpeed;

        // Get the scroll direction
        int scrollDirection = (int)Mathf.Sign(scroll);

        Transform objectToMove = rogueSection != null ? rogueSection : transform;
        Vector3 direction = rogueSection != null ? Vector3.right : Vector3.forward;
        // Move the camera along the blue axis by the distance
        objectToMove.Translate(distance * scrollDirection * 1000 * Time.deltaTime * direction, Space.Self);

        distance = 5f;
    }

    /*
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
    */

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

    public Transform GetCameraPosition(){
        return transform;
    }
}

public class CameraMovementOptions
{
    float _minMovementValue = -1;
    float _maxMovementValue = -1;
    Transform _objectRotator = null;

    #region Constructors
    public CameraMovementOptions(float minClamp, float maxClamp)
    {
        _minMovementValue = minClamp;
        _maxMovementValue = maxClamp;
    }

    public CameraMovementOptions(Transform rotator)
    {
        _objectRotator = rotator;
    }

    public CameraMovementOptions(float minClamp, float maxClamp, Transform rotator)
    {
        _minMovementValue = minClamp;
        _maxMovementValue = maxClamp;
        _objectRotator = rotator;
    }
    #endregion

    #region Setters
    public void SetMovementClamp(float minValue, float maxValue)
    {
        _minMovementValue = minValue;
        _maxMovementValue = maxValue;
    }

    public void SetObjectRotator(Transform rotator)
    {
        _objectRotator = rotator;
    }
    #endregion

    #region Getters
    public float[] GetMovementClamp()
    {
        return new[] { _minMovementValue, _maxMovementValue };
    }

    public Transform GetRotator()
    {
        return _objectRotator;
    }
    #endregion
}