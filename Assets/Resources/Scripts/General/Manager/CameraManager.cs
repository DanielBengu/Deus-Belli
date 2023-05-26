using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public float movementSpeed;
    public float xMin;
    public float xMax;
    public float yMin;
    public float yMax;
    // The minimum and maximum scroll distances
    public float minDistance = 1f;
    public float maxDistance = 10f;

    // The current scroll distance
    private float distance = 5f;

    public float scrollSpeed;

    bool isOutOfFocus = false;

    Vector3 defaultCameraPosition = new(670, 1070, 73);
    Quaternion defaultCameraRotation = Quaternion.Euler((float)33.3, 0, 0);

    Vector3 cameraPositionOutOfFocus = new(670, 300, 640);
    Quaternion rotationOutOfFocus = Quaternion.Euler(0, 0, 0);
    Vector3 cameraPositionOnFocus = new(670, 690, 650);
    Quaternion rotationOnFocus = Quaternion.Euler(40, 0, 0);
    
    public void UpdatePosition(Transform rogueInstance){
        float moveHorizontal = Input.GetAxis("Mouse X");
        //float moveVertical = Input.GetAxis("Mouse Y");

        Vector3 position = rogueInstance.position;
        position.x += moveHorizontal * movementSpeed;
        //position.z += moveVertical * movementSpeed;

        /*position.x = Mathf.Clamp(position.x, xMin, xMax);
        position.z = Mathf.Clamp(position.z, yMin, yMax);*/

        rogueInstance.position = position;
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