using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class FightInput
{
	readonly FightManager _fightManager;
	readonly StructureManager _structureManager;

	public FightInput(FightManager fm, StructureManager sm)
    {
		_fightManager = fm;
		_structureManager = sm;
	}
    public void ManageClick(ObjectClickedEnum objectClicked, GameObject reference)
	{
		if (_fightManager.IsSetup)
		{
			ManageSetupInput(objectClicked, reference);
			return;
		}

		switch (objectClicked)
		{
			case ObjectClickedEnum.EmptyTile:
				ManageClick_EmptyTileSelected(reference.GetComponent<Tile>());
				_structureManager.ClearShowcase(_fightManager.leftUnitShowcaseParent);
				_structureManager.SetInfoPanel(false, _fightManager.UnitSelected);
				break;

			case ObjectClickedEnum.UnitTile:
				var unitSelected = reference.GetComponent<Unit>();
				ManageClick_UnitSelected(unitSelected);
				break;

			case ObjectClickedEnum.RightClickOnField:
				_fightManager.ResetGameState(true);
				break;

			case ObjectClickedEnum.Default:
				break;
		}
	}

	public void ManageClick_EmptyTileSelected(Tile tileSelected)
	{
		Unit unitSelected = _fightManager.UnitSelected;

		if (!unitSelected)
		{
			EmptyActionTileClick(tileSelected, true);
			return;
		}

		//If tile clicked is in range
		if (unitSelected.Movement.PossibleMovements.Contains(tileSelected) && unitSelected.FightData.currentStats.FACTION == FightManager.USER_FACTION)
		{
			Click_TileInsideRange(tileSelected);
			return;
		}

		//User clicked outside the range
		EmptyActionTileClick(tileSelected, true);
	}

	public void ManageClick_UnitSelected(Unit unit)
	{
		if (unit.FightData.currentStats.FACTION == FightManager.USER_FACTION)
			ManagePlayerClick(unit);
		else
			ManageEnemyClick(unit);
	}

	void ManageEnemyClick(Unit unit)
	{
		if(IsOutsidePlayerAttackRange(unit))
		{
			EmptyActionTileClick(unit.Movement.CurrentTile, true);
			_structureManager.SetInfoPanel(true, unit);
			_fightManager.HandleShowcase(unit, false, true, Animation.ShowcaseIdle);
			return;
		}

		Tile tileToMoveTo = _structureManager.CheapestTileToMoveTo(_fightManager.PossibleAttacks, _fightManager.UnitSelected, unit);
		//User wants to attack an enemy, we confirm the action and the path to take
		if (!_fightManager.ShowingPathToTile || _fightManager.ShowingPathToTile.data.PositionOnGrid != unit.Movement.CurrentTile.data.PositionOnGrid)
		{
			AskForMovementConfirmation(unit.Movement.CurrentTile, tileToMoveTo);
			_structureManager.SetAttackPanel(true, _fightManager.UnitSelected, unit);
			_fightManager.HandleShowcase(unit, false, false, Animation.ShowcaseAttack);
			return;
		}

		//User confirmed the action
		_fightManager.QueueAttack(_fightManager.UnitSelected, unit, tileToMoveTo);
		_structureManager.SetInfoPanel(false, unit);
		_fightManager.ClearShowcase();
	}

	void ManagePlayerClick(Unit unit)
	{
		//We selected an allied unit that has already performed his action this turn
		if (HasUnitAlreadyPerformedAction())
		{
			EmptyActionTileClick(unit.Movement.CurrentTile, false);
			_structureManager.SetInfoPanel(true, unit);
			_fightManager.HandleShowcase(unit, true, true, Animation.ShowcaseIdle);
			return;
		}

		//We selected an allied unit that can still perform an action
		_fightManager.ResetGameState(false);
		PossibleActionsForUnit(unit);
		_structureManager.SetInfoPanel(true, unit);
		_fightManager.HandleShowcase(unit, true, true, Animation.ShowcaseIdle);
	}

	void EmptyActionTileClick(Tile currentTile, bool resetGameState)
	{
		_fightManager.ResetGameState(resetGameState);
        if (currentTile.unitOnTile != null)
		{
			_fightManager.UnitSelected = currentTile.unitOnTile;
			PossibleActionsForUnit(currentTile.unitOnTile);
			_fightManager.HandleShowcase(currentTile.unitOnTile, true, true, Animation.ShowcaseIdle);
		}
	}

	void PossibleActionsForUnit(Unit currentUnit)
	{
		List<Tile> possibleMovements = _structureManager.GeneratePossibleMovementForUnit(currentUnit, true);
		_fightManager.PossibleAttacks = _fightManager.StructureManager.GetPossibleAttacksForUnit(currentUnit, !currentUnit.Movement.HasPerformedMainAction, possibleMovements);
	}

	public bool HasUnitAlreadyPerformedAction()
	{
		if (!_fightManager.UnitSelected)
			return false;
		
		return _fightManager.UnitSelected.Movement.HasPerformedMainAction;
	}

	public bool IsOutsidePlayerAttackRange(Unit unit)
	{
		if (!_fightManager.UnitSelected)
			return true;

		bool isLastUnitSelectedAlly = _fightManager.UnitSelected.FightData.currentStats.FACTION == FightManager.USER_FACTION;
		bool isUnitOnAttackRange = IsAttackPossible(_fightManager.UnitSelected, unit);

		return !(isLastUnitSelectedAlly && isUnitOnAttackRange);
	}

	void Click_TileInsideRange(Tile tileSelected)
	{
		if (_fightManager.ShowingPathToTile && _fightManager.ShowingPathToTile.data.PositionOnGrid == tileSelected.data.PositionOnGrid)
		{
			_structureManager.MoveUnit(_fightManager.UnitSelected, tileSelected, false);
			_fightManager.ResetGameState(true, false);
			return;
		}
		AskForMovementConfirmation(tileSelected);
	}

	public bool IsAttackPossible(Unit attacker, Unit defender)
	{
		return _structureManager.IsAttackPossible(attacker, defender);
	}

	public void AskForMovementConfirmation(Tile destinationTile, Tile tileToMoveTo = null)
	{
		PossibleActionsForUnit(_fightManager.UnitSelected);
		Tile tileToReach = tileToMoveTo != null ? tileToMoveTo : destinationTile;
		List<Tile> path = _structureManager.FindPathToDestination(tileToReach, false, _fightManager.UnitSelected.Movement.CurrentTile.data.PositionOnGrid);
		List<Tile> attackTiles = destinationTile.ToList();
		_structureManager.SelectTiles(path, false, TileType.Selected);
		_structureManager.SelectTiles(attackTiles, false, TileType.Enemy);
		_fightManager.ShowingPathToTile = destinationTile;
	}


	#region Setup

	void ManageSetupInput(ObjectClickedEnum oc, GameObject reference) 
	{
		switch (oc)
		{
			case ObjectClickedEnum.UnitTile:
				Setup_UnitSelected(reference.GetComponent<Unit>());
				break;
			case ObjectClickedEnum.EmptyTile:
				Setup_EmptyTileSelected(reference.GetComponent<Tile>());
				break;
		}
	}

	void Setup_UnitSelected(Unit unitSelected)
	{
		_fightManager.UnitSelected = unitSelected;
		_fightManager.ResetGameState(false);
		_fightManager.SetupUnitPosition();
		_structureManager.SelectTiles(unitSelected.Movement.CurrentTile, false, TileType.Selected);
		_structureManager.SetInfoPanel(true, unitSelected);
		if (unitSelected.FightData.currentStats.FACTION != FightManager.USER_FACTION)
		{
			PossibleActionsForUnit(unitSelected);
			_fightManager.HandleShowcase(unitSelected, false, true, Animation.ShowcaseIdle);
			_fightManager.UnitSelected = null;
		}
		else
		{
			_fightManager.HandleShowcase(unitSelected, true, true, Animation.ShowcaseIdle);
		}

	}
	bool Setup_IsTeleportValid(Tile tileSelected)
	{
		bool isTileSelectedEmpty = tileSelected.unitOnTile == null;
		bool isTileSelectedPassable = tileSelected.data.ValidForMovement;
		bool isTileSelectedInsideSetupRange = _fightManager.SetupTiles.Contains(tileSelected.data.PositionOnGrid);

		return isTileSelectedEmpty && isTileSelectedPassable && isTileSelectedInsideSetupRange;
	}
	void Setup_EmptyTileSelected(Tile tileSelected)
	{
		//User wants to move a unit
		if (_fightManager.UnitSelected)
		{
			Setup_MoveUnit(tileSelected);
			return;
		}

		Setup_EmptyClick(tileSelected);
	}
	void Setup_MoveUnit(Tile tileSelected)
	{
		//User clicked on an allowed tile to teleport the unit
		if (Setup_IsTeleportValid(tileSelected))
			_structureManager.MoveUnit(_fightManager.UnitSelected, tileSelected, true);
		_fightManager.ResetGameState(true);
		_fightManager.SetupUnitPosition();
		return;
	}
	void Setup_EmptyClick(Tile tileSelected)
	{
		_fightManager.ResetGameState(true);
		_structureManager.ClearSelection(true);
		_fightManager.ClearShowcase();
		_structureManager.SelectTiles(tileSelected, true, TileType.Selected);
	}

	#endregion

	public void ManageInputs()
	{
		if (!_fightManager.IsGameInStandby && Input.anyKeyDown)
			ManageKeysDown();
	}

	//Method that manages the press of a key (only for the frame it is clicked)
	void ManageKeysDown()
	{
		/*if (Input.GetMouseButtonDown((int)MouseButton.Middle))
            ResetGameState(true);*/
	}

	public enum ObjectClickedEnum
	{
		EmptyTile,
		UnitTile,
		RightClickOnField,
		Default,
	}
}
