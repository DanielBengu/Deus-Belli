using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ActionPerformer
{
    public StructureManager structureManager;
    public SpriteManager spriteManager;

    public Pathfinding pathfinding;
    public MovementManager movement;

    public Unit unitInQueueForAnimation;
    public Unit sourceOfAction;
    ActionPerformed actionPerformed;

    public ActionPerformer(StructureManager structureManager, SpriteManager spriteManager)
    {
        this.structureManager = structureManager;
        this.spriteManager = spriteManager;
        movement = new(this);
    }

    public void StartAction(ActionPerformed action, GameObject source, GameObject target)
    {
        actionPerformed = action;
        switch (action)
        {
            case ActionPerformed.SimpleAttack:
            case ActionPerformed.RetaliationAttack:
                var unitScriptAttack = source.GetComponent<Unit>();
                Tile tileOfTarget = target.GetComponent<Tile>();
				Unit targetUnit = tileOfTarget != null ? tileOfTarget.unitOnTile : target.GetComponent<Unit>();
                SetupAttack(unitScriptAttack, targetUnit);
                break;
            case ActionPerformed.FightMovement:
                var unitScriptMovement = source.GetComponent<Unit>();
                var targetTile = target.GetComponent<Tile>();
                SetupFightMovement(unitScriptMovement, targetTile);
                break;
            case ActionPerformed.RogueMovement:
                SetupRogueMovement(source.transform, target.transform);
                break;
            case ActionPerformed.FightTeleport:
                var unitScriptTeleport = source.GetComponent<Unit>();
                var targetTileTeleport = target.GetComponent<Tile>();
                SetupFightTeleport(unitScriptTeleport, targetTileTeleport);
                break;
            case ActionPerformed.CameraFocus:
				SetupCameraMovement(source.transform, target.transform);
                break;
			case ActionPerformed.Default:
                break;
        }
    }

    public void SetupFightMovement(Unit source, Tile targetTile)
    {
		Debug.Log($"UNIT {source.UnitData.Name} MOVING FROM TILE {source.Movement.CurrentTile.data.PositionOnGrid} TO TILE {targetTile.data.PositionOnGrid}");
		source.FightData.RemoveMovement((int)targetTile.tentativeCost);
        MoveUnit(source, targetTile);
    }

    public void SetupFightTeleport(Unit source, Tile targetTile)
	{
        movement.TeleportUnit(source, targetTile);
	}

    public void SetupRogueMovement(Transform source, Transform targetTile)
    {
        movement.MoveUnit(source.transform, new List<Transform>() { targetTile }, false);
    }

    public void MoveUnit(Unit unit, Tile targetTile)
    {
        List<Tile> tilesPath = pathfinding.FindPathToDestination(targetTile, out float cost, unit.Movement.CurrentTile.data.PositionOnGrid);
        movement.MoveUnit(unit.transform, tilesPath.Select(t => t.transform).ToList(), true);
    }

    public void SetupAttack(Unit attacker, Unit defender)
    {
        attacker.Movement.HasPerformedMainAction = true;
        Attack(attacker, defender);
    }

	public void SetupCameraMovement(Transform start, Transform target)
	{
		movement.StartObjectMovement(start, target, true);
	}

	void Attack(Unit attacker, Unit defender)
    {
        unitInQueueForAnimation = defender;
        sourceOfAction = attacker;

        defender.transform.LookAt(attacker.Movement.CurrentTile.transform, Vector3.up);
        attacker.transform.LookAt(defender.Movement.CurrentTile.transform, Vector3.up);

		PerformAnimation(attacker.gameObject, Animation.Attack, true);
    }

    public void PerformAnimation(GameObject target, Animation animation, bool countTowardsObjectsAnimating)
    {
        if(countTowardsObjectsAnimating)
            structureManager.ObjectsAnimating.Add(new(target, animation));
		AnimationPerformer.PerformAnimation(animation, target);
	}

    public void FinishAnimation(GameObject target)
    {
        structureManager.ObjectsAnimating.RemoveAll(o => o.Item1 == target);
		AnimationPerformer.PerformAnimation(Animation.Idle, target);
	}

    public void ReceiveAttack()
    {
		unitInQueueForAnimation.FightData.TakeDamage(sourceOfAction.FightData.currentStats.ATTACK, unitInQueueForAnimation.FightData.currentStats.ATTACK_TYPE);
        StartTakeDamageAnimation();

        if (unitInQueueForAnimation.FightData.IsDead()) { 
            ClearState(); 
            return; 
        }

		//For simple attacks we need to retaliate
		if (actionPerformed != ActionPerformed.SimpleAttack)
        {
			ClearState();
		}
	}

	public void StartTakeDamageAnimation()
	{
        Animation animation = unitInQueueForAnimation.FightData.currentStats.CURRENT_HP <= 0 ? Animation.Die : Animation.TakeDamage;

		PerformAnimation(unitInQueueForAnimation.gameObject, animation, true);
    }

    public void ManageRetaliation()
    {
        if(IsRetaliationValid())
		    StartAction(ActionPerformed.RetaliationAttack, unitInQueueForAnimation.gameObject, sourceOfAction.gameObject);
    }

    bool IsRetaliationValid()
    {
		bool doesAttackerHaveAntiRetaliation = sourceOfAction != null && sourceOfAction.FightData.ContainsTrait(TraitsEnum.Swift_Attack, out _);
		bool isActionSimpleAttack = actionPerformed == ActionPerformed.SimpleAttack;

		return isActionSimpleAttack && !doesAttackerHaveAntiRetaliation;
	}

    void ClearState()
    {
		unitInQueueForAnimation = null;
        sourceOfAction = null;
        actionPerformed = ActionPerformed.Default;
	}
}