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
    //Quaternion startingRotation;

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
                AnimationPerformer.PerformAnimation(Animation.Idle, objectMovingTransform.gameObject.GetComponent<Unit>());
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

        isObjectMoving = true;
        objectMovingTransform = starting;
        startTime = Time.time;
        startingPosition = starting.position;
        //startingRotation = starting.rotation;
        targetPosition = target.position;
        targetRotation = target.rotation;
        speed = objectSpeed;
        journeyLength = Vector3.Distance(startingPosition, targetPosition);

        AnimationPerformer.PerformAnimation(Animation.Move, objectMovingTransform.gameObject.GetComponent<Unit>());

        Debug.Log("Started movement to " + targetPosition);
        return true;
    }

    public void MoveUnit(Unit unit, Tile targetTile, List<Tile> tilesPath, int movementSpeed)
    {
        //The first of tilesPath it's the starting tile so we skip it
        movementSteps = tilesPath.Skip(1).Select(t => t.transform).ToList();

        if(movementSteps.Count == 0)
        {
            isObjectMoving = true; //Even if not moving we set it true to trigger the movement tick in update
            return;
        }

        unit.CurrentTile.unitOnTile = null;

        Transform destination = new GameObject().transform;
        destination.SetPositionAndRotation(movementSteps.First().position, unit.transform.rotation);
        movementSteps.RemoveAt(0);

        targetTile.unitOnTile = unit;
        unit.CurrentTile = targetTile;

        StartObjectMovement(unit.transform, destination, movementSpeed);
        Object.Destroy(destination.gameObject);
    }

    void SetNewMovementStep()
    {
        isObjectMoving = false;
        Transform destination = new GameObject().transform;
        destination.position = movementSteps.First().position;

        Quaternion rotation = objectMovingTransform.rotation;
        rotation.y = FindCharacterDirection(objectMovingTransform, destination);
        objectMovingTransform.rotation = rotation;

        destination.rotation = objectMovingTransform.rotation;

        StartObjectMovement(objectMovingTransform, destination, 800);
        Object.Destroy(destination.gameObject);
        movementSteps.RemoveAt(0);
    }

    public static float FindCharacterDirection(Transform character, Transform directionTile)
    {
        float rotationY = 0;


        if (character.position.x > directionTile.position.x && character.position.z == directionTile.position.z)         //Moving to the left
            rotationY = 270f;
        else if (character.position.x < directionTile.position.x && character.position.z == directionTile.position.z)   //Moving to the right
            rotationY = 90f;
        else if (character.position.x == directionTile.position.x && character.position.z < directionTile.position.z)   //Moving up
            rotationY = 0f;
        else if (character.position.x == directionTile.position.x && character.position.z > directionTile.position.z)   //Moving down
            rotationY = 180f;

        return rotationY;
    }
}