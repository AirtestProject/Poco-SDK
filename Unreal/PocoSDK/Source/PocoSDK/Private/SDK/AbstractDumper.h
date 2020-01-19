// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Dom/JsonObject.h"
#include "AbstractNode.h"

namespace Poco
{
	class AbstractDumper
	{
	public:

		/**
		 * Virtual that can be overridden by the inheriting class.
		 * Returns the root node of the current UI hierarchy.
		 *
		 * @return The root node of the current UI hierarchy.
		 */
		virtual AbstractNode* GetRoot() { return nullptr; }

		/**
		 * Creates a json dump of the current UI hierarchy.
		 *
		 * @return A json dump of the current UI hierarchy.
		 */
		TSharedPtr<FJsonObject> DumpHierarchy();

		/**
		 * Creates a json dump of the current UI hierarchy.
		 *
		 * @param bOnlyVisibleNode A flag indicating whether the node is the only visible node in the subtree rooted at it.
		 * @return A json dump of the current UI hierarchy.
		 */
		TSharedPtr<FJsonObject> DumpHierarchy(bool bOnlyVisibleNode);

	private:

		/**
		 * Creates a json dump of the current UI tree rooted at the given node.
		 *
		 * @param Node A node in the current UI hierarchy.
		 * @param bOnlyVisibleNode A flag indicating whether the node is the only visible node in the subtree rooted at it.
		 * @return A json dump of the current UI tree rooted at the given node.
		 */
		TSharedPtr<FJsonObject> DumpHierarchyImplementation(AbstractNode* Node, bool bOnlyVisibleNode);
	};
}
