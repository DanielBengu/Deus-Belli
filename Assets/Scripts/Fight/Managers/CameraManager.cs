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

    Vector3 cameraPositionOutOfFocus = new Vector3(670, 300, 640);
    Quaternion rotationOutOfFocus = Quaternion.Euler(0, 0, 0);
    Vector3 cameraPositionOnFocus = new Vector3(670, 690, 650);
    Quaternion rotationOnFocus = Quaternion.Euler(40, 0, 0);
    
    public void UpdatePosition(){
        float moveHorizontal = Input.GetAxis("Mouse X");
        float moveVertical = Input.GetAxis("Mouse Y");

        Vector3 position = transform.position;
        position.x += moveHorizontal * movementSpeed;
        position.z += moveVertical * movementSpeed;

        position.x = Mathf.Clamp(position.x, xMin, xMax);
        position.z = Mathf.Clamp(position.z, yMin, yMax);

        transform.position = position;
    }

    public void ScrollWheel(float scroll){
        // Calculate the new distance based on the mouse wheel input
        distance += -scroll * scrollSpeed;

        // Clamp the distance between the minimum and maximum values
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Get the scroll direction
        int scrollDirection = (int)Mathf.Sign(scroll);

        // Move the camera along the blue axis by the distance
        transform.Translate(Vector3.forward * distance * scrollDirection, Space.Self);
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
            cameraTarget.position = cameraPositionOnFocus;
            cameraTarget.rotation = rotationOnFocus;
            isOutOfFocus = false;
        }else{
            cameraTarget.position = cameraPositionOutOfFocus;
            cameraTarget.rotation = rotationOutOfFocus;
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