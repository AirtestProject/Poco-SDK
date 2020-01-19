// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "SDK/AbstractDumper.h"
#include "SDK/AbstractNode.h"

namespace Poco
{
	/** Overrides base class AbstractDumper.
	 * Returns the root node of the UI Hierarchy.
	 */
	class UE4Dumper
		: public AbstractDumper
	{
	public:

		/**
		 * Overrides the base class method.
		 * Returns the root node of the current UI hierarchy.
		 *
		 * @return The root node of the current UI hierarchy.
		 */
		virtual AbstractNode* GetRoot() override;
	};

	/**
	 * Overrides base class AbstractNode.
	 * Represents the root node of the UI Hierarchy.
	 */
	class RootNode
		: public AbstractNode
	{
	public:

		/**
		 * Creates and initializes a new instance of the root node.
		 */
		RootNode();

		/**
		 * Overrides the base class method.
		 * Gets an array of root node(s). The name of the method was chosen for coherence purposes.
		 *
		 * @return An array of root node(s).
		 */
		TArray< AbstractNode* > GetChildren() override;

	private:

		/** Holds an array of root node(s). */
		TArray<AbstractNode*> Roots;
	};
}
