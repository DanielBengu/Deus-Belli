using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionPerformer
{
    public StructureManager structureManager;
    public SpriteManager spriteManager;

    public Pathfinding pathfinding;
    public Movement movement;

    public Unit enemyInQueueForAnimation;

    public bool IsUnitNotIdle(Unit unit)
	{
        return false;
    }

    public void StartAction(ActionPerformed action, GameObject source, GameObject target)
    {
        switch (action)
        {
            case ActionPerformed.Attack:
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
            case ActionPerformed.Default:
                break;
        }
    }

    public void SetupFightMovement(Unit source, Tile targetTile)
    {
        RemoveMovementFromUnit(source, targetTile.tentativeCost);
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
        List<Tile> tilesPath = pathfinding.FindPathToDestination(targetTile);
        movement.MoveUnit(unit.transform, tilesPath.Select(t => t.transform).ToList(), true);
    }

    public void SetupAttack(Unit attacker, Unit defender)
    {
        attacker.HasPerformedMainAction = true;
        Attack(attacker, defender);
    }

    void Attack(Unit attacker, Unit defender)
    {
        enemyInQueueForAnimation = defender;
        defender.transform.LookAt(attacker.CurrentTile.transform, Vector3.up);
        attacker.transform.LookAt(defender.CurrentTile.transform, Vector3.up);

        AnimationPerformer.PerformAnimation(Animation.Attack, attacker.gameObject);
        
        defender.hpCurrent -= attacker.attack;
    }

    void KillUnit(Unit unitToKill)
    {
        structureManager.gameData.unitsOnField.Remove(unitToKill);
        Object.Destroy(unitToKill.gameObject);
    }

    void RemoveMovementFromUnit(Unit unit, float movementToRemove)
    {
        unit.movementCurrent -= movementToRemove;
        if (unit.movementCurrent < 0) unit.movementCurrent = 0;
    }

    public void StartTakeDamageAnimation()
	{
        if (enemyInQueueForAnimation.hpCurrent <= 0)
            KillUnit(enemyInQueueForAnimation);
        else
            AnimationPerformer.PerformAnimation(Animation.TakeDamage, enemyInQueueForAnimation.gameObject);
    }
}