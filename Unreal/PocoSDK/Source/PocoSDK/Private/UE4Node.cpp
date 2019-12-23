// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#include "UE4Node.h"
#include "Blueprint/SlateBlueprintLibrary.h"
#include "Blueprint/WidgetLayoutLibrary.h"
#include "PocoSDK.h"

namespace Poco
{
	AbstractNode* UE4Node::GetParent()
	{
		AbstractNode* Parent = nullptr;

		if (Widget)
		{
			Parent = new UE4Node(Widget->GetParent());
		}

		return Parent;
	}

	TArray<AbstractNode*> UE4Node::GetChildren()
	{
		TArray<UWidget*> Widgets;

		WidgetTree->GetChildWidgets(Widget, Widgets);

		TArray<AbstractNode*> Children;

		for (UWidget* Child : Widgets)
		{
			UPanelWidget* ParentWidget = Child->GetParent();

			// GetChildWidgets() returns all descendants.
			// Check for parent widget to get direct child nodes.
			if (Widget == ParentWidget)
			{
				AbstractNode* ChildNode = new UE4Node(Child);
				Children.Add(ChildNode);
			}
		}

		return Children;
	}

	bool UE4Node::GetAttribute(const FString& AttrName, FString& OutString)
	{
		if (!Widget)
		{
			return false;
		}

		if (AttrName.Equals(TEXT("name"), ESearchCase::IgnoreCase))
		{
			OutString = GetName();
			return true;
		}
		else if (AttrName.Equals(TEXT("type"), ESearchCase::IgnoreCase))
		{
			OutString = GetType();
			return true;
		}

		return false;
	}

	bool UE4Node::GetAttribute(const FString& AttrName, bool& OutBool)
	{
		if (!Widget)
		{
			return false;
		}

		if (AttrName.Equals(TEXT("visible"), ESearchCase::IgnoreCase))
		{
			OutBool = IsVisible();
			return true;
		}

		return false;
	}

	bool UE4Node::GetAttribute(const FString& AttrName, const TArray< TSharedPtr< FJsonValue > >*& OutArray)
	{
		if (!Widget)
		{
			return false;
		}

		if (AttrName.Equals(TEXT("pos"), ESearchCase::IgnoreCase))
		{
			Position = GetPosition();
			OutArray = &Position;
			return true;
		}
		else if (AttrName.Equals(TEXT("size"), ESearchCase::IgnoreCase))
		{
			Size = GetSize();
			OutArray = &Size;
			return true;
		}
		else if (AttrName.Equals(TEXT("scale"), ESearchCase::IgnoreCase))
		{
			Scale = GetScale();
			OutArray = &Scale;
			return true;
		}

		return false;
	}

	TSharedPtr<FJsonObject> UE4Node::EnumerateAttributes()
	{
		TSharedPtr<FJsonObject> Result = AbstractNode::EnumerateAttributes();

		// set name
		FString Name;
		if (GetAttribute(TEXT("name"), Name))
		{
			Result->SetStringField("name", Name);
		}

		// set type
		FString Type;
		if (GetAttribute(TEXT("type"), Type))
		{
			Result->SetStringField("type", Type);
		}

		// set visibility
		bool bVisible;
		if (GetAttribute(TEXT("visible"), bVisible))
		{
			Result->SetBoolField(TEXT("visible"), bVisible);
		}

		// set position
		const TArray< TSharedPtr< FJsonValue > >* PosArray;
		if (GetAttribute(TEXT("pos"), PosArray))
		{
			Result->SetArrayField(TEXT("pos"), *PosArray);
		}

		// set size
		const TArray< TSharedPtr< FJsonValue > >* SizeArray;
		if (GetAttribute(TEXT("size"), SizeArray))
		{
			Result->SetArrayField(TEXT("size"), *SizeArray);
		}

		// set scale
		const TArray< TSharedPtr< FJsonValue > >* ScaleArray;
		if (GetAttribute(TEXT("scale"), ScaleArray))
		{
			Result->SetArrayField(TEXT("scale"), *ScaleArray);
		}

		return Result;
	}

	FString UE4Node::GetName()
	{
		return Widget->GetName();
	}

	FString UE4Node::GetType()
	{
		return Widget->GetClass()->GetDefaultObjectName().ToString();
	}

	bool UE4Node::IsVisible()
	{
		return Widget->IsVisible();
	}

	const TArray< TSharedPtr<FJsonValue> > UE4Node::GetPosition()
	{
		// Gets the last geometry used to Tick the widget. 
		// This data may not exist yet if this call happens prior to the widget having been ticked/painted, or it may be out of date, or a frame behind.
		FGeometry Geometry = Widget->GetCachedGeometry();
		
		// Gets the position on desktop.
		FVector2D AbsolutePosition = Geometry.GetAbsolutePosition();
		
		// The position in the game's viewport, usable for line traces and other uses where you need a coordinate in the space of viewport resolution units.
		FVector2D PositionOnScreen;

		// The position in the space of other widgets in the viewport.
		// Like if you wanted to add another widget to the viewport at the same position in viewport space as this location, this is what you would use.
		FVector2D PositionInViewport;

		USlateBlueprintLibrary::AbsoluteToViewport(Widget->GetWorld(), AbsolutePosition, PositionOnScreen, PositionInViewport);

		FVector2D ViewportSize = UWidgetLayoutLibrary::GetViewportSize(Widget->GetWorld());

		// Gets the center of the widget.
		FVector2D AbsoluteSize = Geometry.GetAbsoluteSize();
		FVector2D CenterPosiiton = PositionOnScreen + 0.5f * AbsoluteSize;

		// Returns a percentage based on compatibility considerations.
		FVector2D PositionPercentage = CenterPosiiton / ViewportSize;

		TArray< TSharedPtr<FJsonValue> > Result;
		Result.Add(MakeShareable(new FJsonValueNumber(PositionPercentage.X)));
		Result.Add(MakeShareable(new FJsonValueNumber(PositionPercentage.Y)));

		return Result;
	}

	const TArray< TSharedPtr<FJsonValue> > UE4Node::GetSize()
	{
		// This data may not exist yet if this call happens prior to the widget having been ticked/painted, or it may be out of date, or a frame behind.
		FGeometry Geometry = Widget->GetCachedGeometry();

		FVector2D AbsoluteSize = Geometry.GetAbsoluteSize();
		FVector2D ViewportSize = UWidgetLayoutLibrary::GetViewportSize(Widget->GetWorld());

		// Returns a percentage based on compatibility considerations.
		FVector2D SizePercentage = AbsoluteSize / ViewportSize;

		TArray< TSharedPtr<FJsonValue> > Result;
		Result.Add(MakeShareable(new FJsonValueNumber(SizePercentage.X)));
		Result.Add(MakeShareable(new FJsonValueNumber(SizePercentage.Y)));

		return Result;
	}

	const TArray< TSharedPtr<FJsonValue> > UE4Node::GetScale()
	{
		float ViewportScale = UWidgetLayoutLibrary::GetViewportScale(Widget->GetWorld());

		TArray< TSharedPtr<FJsonValue> > Result;
		Result.Add(MakeShareable(new FJsonValueNumber(ViewportScale)));
		Result.Add(MakeShareable(new FJsonValueNumber(ViewportScale)));

		return Result;
	}
}
