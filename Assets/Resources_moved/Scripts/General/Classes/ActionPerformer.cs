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

    public Unit enemyInQueueForAnimation;

    public ActionPerformer(StructureManager structureManager, SpriteManager spriteManager)
    {
        this.structureManager = structureManager;
        this.spriteManager = spriteManager;
        movement = new(this);
    }

    public void StartAction(ActionPerformed action, GameObject source, GameObject target)
    {
        switch (action)
        {
            case ActionPerformed.SimpleAttack:
                var unitScriptAttack = source.GetComponent<Unit>();
                var targetUnit = target.GetComponent<Tile>().unitOnTile;
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
        RemoveMovementFromUnit(source, (int)targetTile.tentativeCost);
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
        enemyInQueueForAnimation = defender;
        defender.transform.LookAt(attacker.Movement.CurrentTile.transform, Vector3.up);
        attacker.transform.LookAt(defender.Movement.CurrentTile.transform, Vector3.up);

		defender.FightData.currentHp -= attacker.UnitData.Stats.Attack;

		PerformAnimation(attacker.gameObject, Animation.Attack);
    }

    public void PerformAnimation(GameObject target, Animation animation)
    {
        structureManager.ObjectsAnimating.Add(new(target, animation));
		AnimationPerformer.PerformAnimation(animation, target);
	}

    public void FinishAnimation(GameObject target)
    {
        structureManager.ObjectsAnimating.RemoveAll(o => o.Item1 == target);
		AnimationPerformer.PerformAnimation(Animation.Idle, target);
	}

    void RemoveMovementFromUnit(Unit unit, int movementToRemove)
    {
        unit.FightData.currentMovement -= movementToRemove;
        if (unit.FightData.currentMovement < 0) unit.FightData.currentMovement = 0;
    }

    public void StartTakeDamageAnimation()
	{
        Animation animation = enemyInQueueForAnimation.FightData.currentHp <= 0 ? Animation.Die : Animation.TakeDamage;

        PerformAnimation(enemyInQueueForAnimation.gameObject, animation);
    }
}