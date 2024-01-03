using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MovementManager
{
	readonly ActionPerformer actionPerformer;

    private float startTime; // the time when the movement started
    private float journeyLength; // the total distance between the start and end markers
    public bool IsObjectMoving { get; set;}
    private Transform objectMovingTransform;

    Vector3 startingPosition;
    Vector3 targetPosition;

    bool isRotationEnabled;
    Quaternion startingRotation;
    Quaternion targetRotation;

    //List of steps necessary to go from point A to B
    public Queue<Transform> movementSteps = new();

    public MovementManager(ActionPerformer actionPerformer)
    {
        this.actionPerformer = actionPerformer;
    }

    public void MovementTick(float speed, Action callback)
	{
		if (!IsObjectMoving)
			return;

        MoveObject(speed);
        CheckObjectStatus(callback);
	}

    void MoveObject(float speed)
    {
        //Unit didn't move
        if (journeyLength == 0)
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
	}
    void CheckObjectStatus(Action callback)
    {
        // Check if the object has reached the destination
        if (!HasObjectReachedTarget())
            return;

		if (movementSteps.Count > 0)
		{
			SetNewMovementStep(objectMovingTransform);
			return;
		}

        FinishObjectMovement(callback);
	}

    bool HasObjectReachedTarget()
    {
        return Vector3.Distance(objectMovingTransform.position, targetPosition) < 0.01f;
	}

    void FinishObjectMovement(Action callback)
    {
		IsObjectMoving = false;
        actionPerformer.FinishAnimation(objectMovingTransform.gameObject);
		callback();
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

        actionPerformer.PerformAnimation(objectMovingTransform.gameObject, Animation.Move, true);

        Debug.Log("Started movement to " + targetPosition);
        return true;
    }

    public void MoveUnit(Transform unit, List<Transform> tilesPath, bool isFightSection)
    {
        PrepareMovementQueue(tilesPath, isFightSection);

        if (isFightSection && movementSteps.Count > 0)
            UpdateUnitsCurrentTileData(unit, tilesPath);	

        SetNewMovementStep(unit);
    }

    void PrepareMovementQueue(List<Transform> tilesPath, bool skipFirst)
    {
        movementSteps.Clear();

        int skipValue = skipFirst ? 1 : 0;
        foreach (var tile in tilesPath.Skip(skipValue))
        {
            movementSteps.Enqueue(tile);
        }
    }
    void UpdateUnitsCurrentTileData(Transform unit, List<Transform> tilesPath)
    {
		Unit unitScript = unit.GetComponent<Unit>();
		Tile targetTile = tilesPath.Last().GetComponent<Tile>();

		unitScript.Movement.CurrentTile.unitOnTile = null;
		targetTile.unitOnTile = unitScript;
		unitScript.Movement.CurrentTile = targetTile;
	}

    void SetNewMovementStep(Transform movingUnit)
    {
        Transform nextTile;
        IsObjectMoving = false;

        if (movementSteps.Count > 0)
		{
            nextTile = movementSteps.Dequeue();
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