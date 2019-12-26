// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Dom/JsonObject.h"

namespace Poco
{
	class AbstractNode
	{
	public:

		/**
		 * Creates and initializes a new instance of the abstract node.
		 */
		AbstractNode();

		/** Destructor. */
		virtual ~AbstractNode()
		{}

		/**
		 * Virtual that can be overridden by the inheriting class.
		 * Gets the parent of the given node.
		 *
		 * @return The parent of the given node as a json object.
		 */
		virtual AbstractNode* GetParent() { return nullptr; }

		/**
		 * Virtual that can be overridden by the inheriting class.
		 * Gets an array of child nodes of the given node.
		 *
		 * @return An array of child nodes.
		 */
		virtual TArray<AbstractNode*> GetChildren()
		{
			TArray<AbstractNode*> Children;

			return Children;
		}

		/**
		 * Gets available attributes and their values.
		 *
		 * @return Available attributes and their values as a json object.
		 */
		virtual TSharedPtr<FJsonObject> EnumerateAttributes();

		/**
		 * Gets the value of the attribute as a string.
		 *
		 * @param AttrName The name of the attribute.
		 * @param OutString The value of the attribute as a string.
		 */
		virtual bool GetAttribute(const FString& AttrName, FString& OutString);

		/**
		 * Gets the value of the attribute as a boolean.
		 *
		 * @param AttrName The name of the attribute.
		 * @param OutBool The value of the attribute as a boolean.
		 */
		virtual bool GetAttribute(const FString& AttrName, bool& OutBool);

		/**
		 * Gets the value of the attribute as an array.
		 *
		 * @param AttrName The name of the attribute.
		 * @param OutArray The value of the attribute as an array.
		 */
		virtual bool GetAttribute(const FString& AttrName, const TArray< TSharedPtr< FJsonValue > >*& OutArray);

		/**
		 * Gets the value of the attribute as an object.
		 *
		 * @param AttrName The name of the attribute.
		 * @param OutObject The value of the attribute as an object.
		 */
		virtual bool GetAttribute(const FString& AttrName, const TSharedPtr< FJsonObject >*& OutObject);

	private:

		/** Holds available attributes of the given node. */
		TSharedPtr< FJsonObject > AvailableAttributes;
	};
}
