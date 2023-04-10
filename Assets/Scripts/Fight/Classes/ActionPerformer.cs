using System.Collections.Generic;
using UnityEngine;

public class ActionPerformer
{
    public StructureManager structureManager;
    public SpriteManager spriteManager;

    public Pathfinding pathfinding;
    public Movement movement;

    public void StartAction(ActionPerformed action, Unit source, Tile target)
    {
        switch (action)
        {
            case ActionPerformed.Attack:
                SetupAttack(source, target.unitOnTile);
                break;
            case ActionPerformed.Movement:
                SetupMovement(source, target);
                break;
            case ActionPerformed.Default:
                break;
        }
    }

    public void SetupMovement(Unit source, Tile targetTile)
    {
        RemoveMovementFromUnit(source, targetTile.tentativeCost);
        MoveUnit(source, targetTile, false);
        //actionInQueue = ActionPerformed.Movement;
        spriteManager.ClearMapTilesSprite();
    }

    public void MoveUnit(Unit unit, Tile targetTile, bool addToSelectedMapTiles)
    {
        List<Tile> tilesPath = pathfinding.FindPathToDestination(targetTile);
        if (addToSelectedMapTiles)
            structureManager.selectedTiles = tilesPath;
        movement.MoveUnit(unit, targetTile, tilesPath);
    }

    public void SetupAttack(Unit attacker, Unit defender)
    {
        attacker.HasPerformedMainAction = true;
        Attack(attacker, defender);
    }

    void Attack(Unit attacker, Unit defender)
    {
        defender.transform.LookAt(attacker.CurrentTile.transform, Vector3.up);
        attacker.transform.LookAt(defender.CurrentTile.transform, Vector3.up);

        AnimationPerformer.PerformAnimation(Animation.Attack, attacker);
        AnimationPerformer.PerformAnimation(Animation.TakeDamage, defender);

        defender.hpCurrent -= attacker.attack;
        if (defender.hpCurrent <= 0)
            KillUnit(defender);
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
}