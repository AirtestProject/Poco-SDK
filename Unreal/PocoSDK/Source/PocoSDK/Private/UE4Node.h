// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "SDK/AbstractNode.h"
#include "Components/Widget.h"
#include "Blueprint/WidgetTree.h"

namespace Poco
{
	class UE4Node
		: AbstractNode
	{
	public:

		/**
		 * Creates and initializes a new instance of the UI node.
		 *
		 * @param Name The name of the widget to create a node for.
		 */
		UE4Node(UWidget* W)
			: Widget(W)
		{}

		/** Destructor. */
		~UE4Node()
		{}

		/**
		 * Overrides the base class method.
		 * Gets the parent of the widget.
		 *
		 * @return A pointer to the parent widget on success, nullptr otherwise.
		 */
		virtual AbstractNode* GetParent() override;

		/**
		 * Overrides the base class method.
		 * Gets the children of the given node.
		 *
		 * @return An array of child nodes.
		 */
		virtual TArray<AbstractNode*> GetChildren();

		/**
		 * Overloads the base class method.
		 * Gets the value of the attribute as a string.
		 *
		 * @param AttrName The name of the attribute.
		 * @param OutString The value of the attribute as a string.
		 * @return true on success, false otherwise.
		 */
		virtual bool GetAttribute(const FString& AttrName, FString& OutString) override;

		/**
		 * Overloads the base class method.
		 * Gets the value of the attribute as a boolean.
		 *
		 * @param AttrName The name of the attribute.
		 * @param OutBool The value of the attribute as a boolean.
		 * @return true on success, false otherwise.
		 */
		virtual bool GetAttribute(const FString& AttrName, bool& OutBool) override;

		/**
		 * Overloads the base class method.
		 * Gets the value of the attribute as an array.
		 *
		 * @param AttrName The name of the attribute.
		 * @param OutArray The value of the attribute as an array.
		 * @return true on success, false otherwise.
		 */
		virtual bool GetAttribute(const FString& AttrName, const TArray< TSharedPtr< FJsonValue > > *&OutArray) override;

		/**
		 * Overloads the base class method.
		 * Gets the value of the attribute as an object.
		 *
		 * @param AttrName The name of the attribute.
		 * @param OutObject The value of the attribute as an object.
		 * @return true on success, false otherwise.
		 */
		virtual bool GetAttribute(const FString& AttrName, const TSharedPtr< FJsonObject >*& OutObject) override;

		/**
		 * Overrides the base class method.
		 * Gets available attributes and their values.
		 *
		 * @return Available attributes and their values as a json object.
		 */
		virtual TSharedPtr<FJsonObject> EnumerateAttributes() override;

	private:

		/** Holds the widget. */
		UWidget* Widget;

		/** Holds the position of the widget as a json value array. */
		TArray< TSharedPtr<FJsonValue> > PositionJson;

		/** Holds the size of the widget as a json value array. */
		TArray< TSharedPtr<FJsonValue> > SizeJson;

		/** Holds the scale of the widget as a json value array. */
		TArray< TSharedPtr<FJsonValue> > ScaleJson;

		/** Holds the z-orders of the widget as a json value object. */
		TSharedPtr<FJsonObject> ZOrderJson = MakeShareable(new FJsonObject);;

		/**
		 * Gets the name of the widget.
		 *
		 * @param OutString The name of the widget.
		 * @return true on success, false otherwise.
		 */
		bool GetName(FString& OutString);

		/**
		 * Gets the type of the widget.
		 *
		 * @param OutString The type of the widget.
		 * @return true on success, false otherwise.
		 */
		bool GetType(FString& OutString);

		/**
		 * Gets the current visibility of the widget.
		 *
		 * @return true if the widget is Visible, HitTestInvisible or SelfHitTestInvisible, false otherwise
		 */
		bool IsVisible();
		
		/**
		 * Gets the position of the widget.
		 *
		 * @param OutArray The position of the widget as a json value array.
		 * @return true on success, false otherwise.
		 */
		bool GetPosition(const TArray< TSharedPtr< FJsonValue > >*& OutArray);

		/**
		 * Gets the size of the widget.
		 *
		 * @param OutArray The size of the widget  .
		 * @return true on success, false otherwise.
		 */
		bool GetSize(const TArray< TSharedPtr< FJsonValue > >*& OutArray);

		/**
		 * Gets the scale of the widget.
		 *
		 * @param OutArray The scale of the widget as a json value array.
		 * @return true on success, false otherwise.
		 */
		bool GetScale(const TArray< TSharedPtr< FJsonValue > >*& OutArray);

		/**
		 * Gets the z-order on the slot.
		 *
		 * @param OutObject The name of the widget.
		 * @return true on success, false otherwise.
		 */
		bool GetZOrder(const TSharedPtr< FJsonObject >*& OutObject);

		/**
		 * Gets the widget text.
		 *
		 * @param OutString The text of the widget.
		 * @return true on success, false otherwise.
		 */
		bool GetText(FString& OutString);

		/**
		 * Gets the widget tree.
		 *
		 * @return A pointer to the widget tree.
		 */
		UWidgetTree* GetWidgetTree();
	};
}
