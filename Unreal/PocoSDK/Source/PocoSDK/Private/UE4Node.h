// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "SDK/AbstractNode.h"
#include "Components/Widget.h"
#include "Blueprint/UserWidget.h"
#include "Blueprint/WidgetTree.h"
#include "UObject/UObjectIterator.h"

namespace Poco
{
	class UE4Node
		: AbstractNode
	{
	public:

		/**
		 * Creates and initializes a new instance of the UE4 node.
		 *
		 * @param Widget The widget to create a node for.
		 */
		UE4Node(UWidget* Widget) 
			: Widget(Widget)
			, WidgetTree(((UUserWidget*)Widget)->WidgetTree)
		{}

		/** Destructor. */
		~UE4Node() {}

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
		 */
		virtual bool GetAttribute(const FString& AttrName, FString& OutString) override;

		/**
		 * Overloads the base class method.
		 * Gets the value of the attribute as a boolean.
		 *
		 * @param AttrName The name of the attribute.
		 * @param OutString The value of the attribute as a boolean.
		 */
		virtual bool GetAttribute(const FString& AttrName, bool& OutBool) override;

		/**
		 * Overloads the base class method.
		 * Gets the value of the attribute as an array.
		 *
		 * @param AttrName The name of the attribute.
		 * @param OutArray The value of the attribute as an array.
		 */
		virtual bool GetAttribute(const FString& AttrName, const TArray< TSharedPtr< FJsonValue > > *&OutArray) override;

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

		/** Holds the widget tree. */
		UWidgetTree* WidgetTree;

		/** Holds the position of the widget as a json value array. */
		TArray< TSharedPtr<FJsonValue> > Position;

		/** Holds the size of the widget as a json value array. */
		TArray< TSharedPtr<FJsonValue> > Size;

		/** Holds the scale of the widget as a json value array. */
		TArray< TSharedPtr<FJsonValue> > Scale;

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
		const TArray< TSharedPtr<FJsonValue> > GetPosition();

		/**
		 * Gets the size of the widget.
		 *
		 * @return The size of the widget as a json value array.
		 */
		const TArray< TSharedPtr<FJsonValue> > GetSize();

		/**
		 * Gets the scale of the widget.
		 *
		 * @return The scale of the widget as a json value array.
		 */
		const TArray< TSharedPtr<FJsonValue> > GetScale();
	};
}