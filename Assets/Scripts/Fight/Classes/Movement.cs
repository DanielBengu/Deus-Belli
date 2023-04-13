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
                AnimationPerformer.PerformAnimation(Animation.Idle, objectMovingTransform.gameObject);
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

        AnimationPerformer.PerformAnimation(Animation.Move, objectMovingTransform.gameObject);

        Debug.Log("Started movement to " + targetPosition);
        return true;
    }

    public void MoveUnit(Transform unit, List<Transform> tilesPath, bool isFightSection)
    {
        movementSteps = tilesPath.ToList();

        if (isFightSection)
		{
            Unit unitScript = unit.GetComponent<Unit>();

            if (movementSteps.Count == 0)
            {
                isObjectMoving = true; //Even if not moving we set it true to trigger the movement tick in update
                return;
            }

            Tile targetTile = tilesPath.Last().GetComponent<Tile>();

            unitScript.CurrentTile.unitOnTile = null;
            targetTile.unitOnTile = unitScript;
            unitScript.CurrentTile = targetTile;
        }

        SetNewMovementStep(unit);
    }

    void SetNewMovementStep(Transform movingUnit)
    {
        isObjectMoving = false;

        Transform nextTile = movementSteps.First().transform;
        movingUnit.transform.LookAt(nextTile, Vector3.up);
        movementSteps.RemoveAt(0);

        StartObjectMovement(movingUnit.transform, nextTile);
    }
}