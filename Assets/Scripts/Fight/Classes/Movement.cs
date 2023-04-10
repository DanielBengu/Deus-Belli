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

    Vector3 targetPosition;

    readonly float speed = 800;

    //List of steps necessary to go from point A to B
    public List<Transform> movementSteps = new();

    public bool MovementTick()
    {
        float distanceCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distanceCovered / journeyLength;
        objectMovingTransform.position = Vector3.Lerp(startingPosition, targetPosition, fractionOfJourney);

        //Unit arrived at destination
        if (objectMovingTransform.position == targetPosition)
        {
            if (movementSteps.Count > 0)
            {
                SetNewMovementStep(objectMovingTransform);
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

    public bool StartObjectMovement(Transform starting, Transform target)
    {
        //Something else is already moving
        if (isObjectMoving)
            return false;

        isObjectMoving = true;
        objectMovingTransform = starting;
        startTime = Time.time;
        startingPosition = starting.position;
        targetPosition = target.position;
        journeyLength = Vector3.Distance(startingPosition, targetPosition);

        AnimationPerformer.PerformAnimation(Animation.Move, objectMovingTransform.gameObject.GetComponent<Unit>());

        Debug.Log("Started movement to " + targetPosition);
        return true;
    }

    public void MoveUnit(Unit unit, Tile targetTile, List<Tile> tilesPath)
    {
        //The first of tilesPath it's the starting tile so we skip it
        movementSteps = tilesPath.Skip(1).Select(t => t.transform).ToList();

        if(movementSteps.Count == 0)
        {
            isObjectMoving = true; //Even if not moving we set it true to trigger the movement tick in update
            return;
        }

        unit.CurrentTile.unitOnTile = null;
        targetTile.unitOnTile = unit;
        unit.CurrentTile = targetTile;

        SetNewMovementStep(unit.transform);
    }

    void SetNewMovementStep(Transform movingUnit)
    {
        isObjectMoving = false;

        Transform nextTile = movementSteps.First().transform;
        movingUnit.transform.LookAt(nextTile, Vector3.up);
        movementSteps.RemoveAt(0);

        StartObjectMovement(movingUnit.transform, nextTile);
    }

    public static float FindCharacterDirection(Transform character, Vector3 directionTile)
    {
        float rotationY = 0;

        if (character.position.x > directionTile.x && character.position.z == directionTile.z)         //Moving to the left
            rotationY = 270f;
        else if (character.position.x < directionTile.x && character.position.z == directionTile.z)   //Moving to the right
            rotationY = 90f;
        else if (character.position.x == directionTile.x && character.position.z < directionTile.z)   //Moving up
            rotationY = 0f;
        else if (character.position.x == directionTile.x && character.position.z > directionTile.z)   //Moving down
            rotationY = 180f;

        return rotationY;
    }
}