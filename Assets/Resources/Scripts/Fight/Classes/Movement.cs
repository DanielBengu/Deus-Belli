using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    readonly float speed = 800;

    //List of steps necessary to go from point A to B
    public List<Transform> movementSteps = new();

    public int MovementTick()
    {
        if (!IsObjectMoving)
            return -1;

        float distanceCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distanceCovered / journeyLength;
        if (fractionOfJourney > 0)
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
                IsObjectMoving = false;
                AnimationPerformer.PerformAnimation(Animation.Idle, objectMovingTransform.gameObject);
                return 1;
            }
        }
        return 0;
    }

    public bool StartObjectMovement(Transform starting, Transform target)
    {
        //Something else is already moving
        if (IsObjectMoving)
            return false;

        IsObjectMoving = true;
        objectMovingTransform = starting;
        startTime = Time.time;
        startingPosition = starting.position;

        //Unit needs to stay at same Y
        targetPosition = target.position;
        targetPosition.y = startingPosition.y;
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
            movementSteps = movementSteps.Skip(1).ToList();
            Unit unitScript = unit.GetComponent<Unit>();

            if (movementSteps.Count == 0)
            {
                IsObjectMoving = true; //Even if not moving we set it true to trigger the movement tick in update
			}
			else
			{
                Tile targetTile = tilesPath.Last().GetComponent<Tile>();

                unitScript.CurrentTile.unitOnTile = null;
                targetTile.unitOnTile = unitScript;
                unitScript.CurrentTile = targetTile;
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
        StartObjectMovement(movingUnit.transform, nextTile);
    }

    public void TeleportUnit(Unit unit, Tile tile)
	{
        unit.transform.position = new(tile.transform.position.x, unit.transform.position.y, tile.transform.position.z);

        unit.CurrentTile.unitOnTile = null;
        tile.unitOnTile = unit;
        unit.CurrentTile = tile;
    }
}