// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Tickable.h"

namespace Poco {
	class FPocoManager;

	/**
	 * Implements a tickable object that listens for incoming TCP connections.
	 */
	class FTickableObject
		: FTickableGameObject
	{
	public:

		/**
		* Creates and initializes a new instance from the owner.
		*
		* @param Parent The parent class that owns this tickable object.
		*/
		FTickableObject(FPocoManager* Manager)
			:Parent(Manager) {}

		/**
		 * Overrides the Tick() method in the base class FTickableObjectBase.
		 *
		 * @param DeltaTime Game time passed since the last call.
		 */
		virtual void Tick(float DeltaTime) override;

		/**
		 * Used to determine whether an object is ready to be ticked.
		 *
		 * @return true if object is ready to be ticked, false otherwise.
		 */
		virtual bool IsTickable() const override { return true; }

		/**
		 * Used to determine if an object should be ticked when the game is paused.
		 *
		 * @return true if it should be ticked when paused, false otherwise.
		 */
		virtual bool IsTickableWhenPaused() const override { return true; }

		/**
		 * Used to determine whether the object should be ticked in the editor.
		 *
		 * @return true if this tickable object can be ticked in the editor.
		 */
		virtual bool IsTickableInEditor() const override { return true; }

		/**
		 * Returns the stat id to use for this tickable.
		 *
		 * @return The stat id to use for this tickable.
		 **/
		virtual TStatId GetStatId() const override
		{
			RETURN_QUICK_DECLARE_CYCLE_STAT(FPocoManager, STATGROUP_Tickables);
		}

	private:

		/** Holds the owner of this tickable object. */
		FPocoManager* Parent;
	};
}
