using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Movement
{
    private float startTime; // the time when the movement started
    private float journeyLength; // the total distance between the start and end markers
    public bool isObjectMoving = false;
    private Transform objectMovingTransform;

    Vector3 startingPosition;
    Quaternion startingRotation;

    Vector3 targetPosition;
    Quaternion targetRotation;

    [SerializeField]
    float speed;

    //List of steps necessary to go from point A to B
    public List<Transform> movementSteps = new();

    public void SetupNewMovement(){

    }

    public bool MovementTick(){
        float distanceCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distanceCovered / journeyLength;
        objectMovingTransform.position = Vector3.Lerp(startingPosition,targetPosition, fractionOfJourney);
        objectMovingTransform.rotation = Quaternion.Lerp(startingRotation, targetRotation, fractionOfJourney);

        if(objectMovingTransform.position == targetPosition && objectMovingTransform.rotation == targetRotation){
            if(movementSteps.Count > 0){
                isObjectMoving = false;
                Transform destination = new GameObject().transform;
                destination.position = movementSteps.First().position;
                destination.rotation = objectMovingTransform.rotation;

                StartObjectMovement(objectMovingTransform, destination, 800);
                GameObject.Destroy(destination.gameObject);
                movementSteps.RemoveAt(0);
            }else{
                isObjectMoving = false;
                return true;
            }
        }

        return false;
    }

    public bool StartObjectMovement(Transform starting, Transform target, float objectSpeed){
        //Something else is already moving
        if(isObjectMoving)
            return false;

        isObjectMoving = true;
        objectMovingTransform = starting;
        startTime = Time.time;
        startingPosition = starting.position;
        startingRotation = starting.rotation;
        targetPosition = target.position;
        targetRotation = target.rotation;
        speed = objectSpeed;
        journeyLength = Vector3.Distance(startingPosition, targetPosition);
        Debug.Log("Started movement to " + targetPosition);
        return true;
    }

    public void MoveUnit(Unit unit, Tile targetTile, List<Tile> tilesPath){
        foreach (var tile in tilesPath.Skip(1))
        {
            movementSteps.Add(tile.transform);
        }


        unit.currentTile.unitOnTile = null;

        Transform destination = new GameObject().transform;
        destination.position = movementSteps.First().position;
        destination.rotation = unit.transform.rotation;
        movementSteps.RemoveAt(0);

        targetTile.unitOnTile = unit.gameObject;
        unit.currentTile = targetTile;

        StartObjectMovement(unit.transform, destination, 800);
        GameObject.Destroy(destination.gameObject);
    }
}