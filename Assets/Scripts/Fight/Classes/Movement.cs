using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Movement
{
    private Animator animator;
    private string gender;

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

    public bool MovementTick()
    {
        float distanceCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distanceCovered / journeyLength;
        objectMovingTransform.position = Vector3.Lerp(startingPosition, targetPosition, fractionOfJourney);
        //objectMovingTransform.rotation = Quaternion.Lerp(startingRotation, targetRotation, fractionOfJourney);

        if (objectMovingTransform.position == targetPosition && objectMovingTransform.rotation == targetRotation)
        {
            if (movementSteps.Count > 0)
            {
                SetNewMovementStep();
            }
            else
            {
                isObjectMoving = false;
                animator.Play($"{gender} Sword Stance");
                animator = null;
                gender = "";
                return true;
            }
        }

        return false;
    }

    public bool StartObjectMovement(Transform starting, Transform target, float objectSpeed)
    {
        //Something else is already moving
        if (isObjectMoving)
            return false;

        animator = starting.GetComponent<Animator>();
        gender = starting.gameObject.name.Split(' ')[0];
        animator.Play($"{gender} Sword Walk");

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

    public void MoveUnit(Unit unit, Tile targetTile, List<Tile> tilesPath, int movementSpeed)
    {
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

        StartObjectMovement(unit.transform, destination, movementSpeed);
        GameObject.Destroy(destination.gameObject);
    }

    void SetNewMovementStep()
    {
        isObjectMoving = false;
        Transform destination = new GameObject().transform;
        destination.position = movementSteps.First().position;
        objectMovingTransform.rotation = FindCharacterDirection(objectMovingTransform, destination);
        destination.rotation = objectMovingTransform.rotation;

        StartObjectMovement(objectMovingTransform, destination, 800);
        GameObject.Destroy(destination.gameObject);
        movementSteps.RemoveAt(0);
    }

    Quaternion FindCharacterDirection(Transform character, Transform directionTile)
    {
        Quaternion directionRotation = new();

        
        if(character.position.x > directionTile.position.x && character.position.z == directionTile.position.z)         //Moving to the left
            directionRotation = new Quaternion(0, 270, 0, 0);
        else if (character.position.x < directionTile.position.x && character.position.z == directionTile.position.z)   //Moving to the right
            directionRotation = new Quaternion(0, 90, 0, 0);
        else if (character.position.x == directionTile.position.x && character.position.z < directionTile.position.z)   //Moving up
            directionRotation = new Quaternion(0, 0, 0, 0);
        else if (character.position.x == directionTile.position.x && character.position.z > directionTile.position.z)   //Moving down
            directionRotation = new Quaternion(0, 0, 0, 0);

        return directionRotation;
    }
}