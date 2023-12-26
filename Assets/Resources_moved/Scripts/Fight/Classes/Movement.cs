using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Movement
{
    private float startTime; // the time when the movement started
    private float journeyLength; // the total distance between the start and end markers
    public bool IsObjectMoving
	{
        get;
        set;
	}
    private Transform objectMovingTransform;

    Vector3 startingPosition;
    Vector3 targetPosition;

    bool isRotationEnabled;
    Quaternion startingRotation;
    Quaternion targetRotation;

    //List of steps necessary to go from point A to B
    public List<Transform> movementSteps = new();

	public void MovementTick(float speed, Action callback)
	{
		if (!IsObjectMoving)
			return;

		float distanceCovered = (Time.time - startTime) * speed;
		float fractionOfJourney = distanceCovered / journeyLength;

		// Ensure fractionOfJourney stays within [0, 1] range
		fractionOfJourney = Mathf.Clamp01(fractionOfJourney);

		// Lerp position continuously until reaching the target
		objectMovingTransform.position = Vector3.Lerp(startingPosition, targetPosition, fractionOfJourney);

        if (isRotationEnabled)
		    // Interpolate the rotation using Quaternion.Lerp
		    objectMovingTransform.rotation = Quaternion.Lerp(startingRotation, targetRotation, fractionOfJourney);


		// Check if the object has reached the destination
		if (Vector3.Distance(objectMovingTransform.position, targetPosition) < 0.1f)
		{
			if (movementSteps.Count > 0)
			{
				SetNewMovementStep(objectMovingTransform);
				return;
			}

			IsObjectMoving = false;
            AnimationPerformer.PerformAnimation(Animation.Idle, objectMovingTransform.gameObject);
			callback();
		}
	}

	public bool StartObjectMovement(Transform starting, Transform target, bool rotationEnabled)
    {
        //Something else is already moving
        if (IsObjectMoving)
            return false;

        IsObjectMoving = true;
        objectMovingTransform = starting;
        startTime = Time.time;
        startingPosition = starting.position;

        targetPosition = target.position;
        journeyLength = Vector3.Distance(startingPosition, targetPosition);

        isRotationEnabled = rotationEnabled;
        if(isRotationEnabled)
        {
            startingRotation = starting.rotation;
            targetRotation = target.rotation;
        }

        AnimationPerformer.PerformAnimation(Animation.Move, objectMovingTransform.gameObject);

        Debug.Log("Started movement to " + targetPosition);
        return true;
    }

    public void MoveUnit(Transform unit, List<Transform> tilesPath, bool isFightSection)
    {
        movementSteps = tilesPath.ToList();

        if (isFightSection)
		{
            movementSteps = movementSteps.Skip(1).ToList();
            Unit unitScript = unit.GetComponent<Unit>();

            if (movementSteps.Count == 0)
            {
                IsObjectMoving = true; //Even if not moving we set it true to trigger the movement tick in update
			}
			else
			{
                Tile targetTile = tilesPath.Last().GetComponent<Tile>();

                unitScript.Movement.CurrentTile.unitOnTile = null;
                targetTile.unitOnTile = unitScript;
                unitScript.Movement.CurrentTile = targetTile;
            }
        }

        SetNewMovementStep(unit);
    }

    void SetNewMovementStep(Transform movingUnit)
    {
        Transform nextTile;
        IsObjectMoving = false;

        if (movementSteps.Count > 0)
		{
            nextTile = movementSteps.First().transform;
            movementSteps.RemoveAt(0);
        }
        else
		{
            nextTile = movingUnit;
        }
            
        movingUnit.transform.LookAt(nextTile, Vector3.up);
        StartObjectMovement(movingUnit.transform, nextTile, false);
    }

    public void TeleportUnit(Unit unit, Tile tile)
	{
        unit.transform.position = new(tile.transform.position.x, unit.transform.position.y, tile.transform.position.z);

        unit.Movement.CurrentTile.unitOnTile = null;
        tile.unitOnTile = unit;
        unit.Movement.CurrentTile = tile;
    }
}