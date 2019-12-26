// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "SDK/AbstractNode.h"
#include "Components/Widget.h"
#include "Blueprint/WidgetTree.h"
#include "UObject/UObjectIterator.h"

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
		UE4Node(FString Name)
			:NodeName(Name)
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

		/** Holds the name of the widget. */
		FString NodeName;

		/** Holds the position of the widget as a json value array. */
		TArray< TSharedPtr<FJsonValue> > PositionJson;

		/** Holds the size of the widget as a json value array. */
		TArray< TSharedPtr<FJsonValue> > SizeJson;

		/** Holds the scale of the widget as a json value array. */
		TArray< TSharedPtr<FJsonValue> > ScaleJson;

		/** Holds the z-order of the widget. */
		TSharedPtr<FJsonObject> ZOrders = MakeShareable(new FJsonObject);

		/**
		 * Gets the widget from the given name.
		 *
		 * @param Name the name of the widget.
		 * @return The widget with the given name.
		 */
		UWidget* GetWidget(const FString& Name);

		/**
		 * Gets the widget tree.
		 *
		 * return A pointer to the widget tree.
		 */
		UWidgetTree* GetWidgetTree();

		/**
		 * Gets the name of the widget.
		 *
		 * @return The name of the widget.
		 */
		FString GetName();

		/**
		 * Gets the type of the widget.
		 *
		 * @return The type of the widget.
		 */
		FString GetType();

		/**
		 * Gets the current visibility of the widget.
		 *
		 * @return true if the widget is Visible, HitTestInvisible or SelfHitTestInvisible, false otherwise
		 */
		bool IsVisible();
		
		/**
		 * Gets the position of the widget.
		 *
		 * @return The position of the widget as a json value array.
		 */
		bool GetPosition(const TArray< TSharedPtr< FJsonValue > >*& OutArray);

		/**
		 * Gets the size of the widget.
		 *
		 * @return The size of the widget as a json value array.
		 */
		bool GetSize(const TArray< TSharedPtr< FJsonValue > >*& OutArray);

		/**
		 * Gets the scale of the widget.
		 *
		 * @return The scale of the widget as a json value array.
		 */
		bool GetScale(const TArray< TSharedPtr< FJsonValue > >*& OutArray);

		/**
		 * Gets the z-order on the slot.
		 *
		 * @return The z-order on the slot
		 */
		const TSharedPtr< FJsonObject > GetZOrder();

		/**
		 * Gets the widget text.
		 *
		 * @return The widget text.
		 */
		bool GetText(FString& OutString);
	};
}
