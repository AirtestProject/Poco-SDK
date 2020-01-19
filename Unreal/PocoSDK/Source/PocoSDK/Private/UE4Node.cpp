// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#include "UE4Node.h"
#include "Blueprint/SlateBlueprintLibrary.h"
#include "Blueprint/WidgetLayoutLibrary.h"
#include "Components/CanvasPanelSlot.h"
#include "Components/MultiLineEditableText.h"
#include "Components/MultiLineEditableTextBox.h"
#include "Components/RichTextBlock.h"
#include "Components/TextBlock.h"
#include "Components/TextWidgetTypes.h"
#include "UObject/UObjectBase.h"
#include "UObject/UObjectIterator.h"

namespace Poco
{
	AbstractNode* UE4Node::GetParent()
	{
		AbstractNode* Parent = nullptr;

		if (Widget)
		{
			if (UPanelWidget* ParentWidget = Widget->GetParent())
			{
				Parent = new UE4Node(Widget->GetParent());
			}
			else
			{
				if (UWidgetTree* ParentWidgetTree = Cast<UWidgetTree>(Widget->GetOuter()))
				{
					if (UUserWidget* ParentUserWidget = Cast<UUserWidget>(ParentWidgetTree->GetOuter()))
					{
						Parent = new UE4Node(ParentUserWidget);
					}
				}
			}
		}

		return Parent;
	}

	UWidgetTree* UE4Node::GetWidgetTree()
	{
		if (Widget)
		{
			if (UUserWidget* UserWidget = Cast<UUserWidget>(Widget))
			{
				if (UserWidget->WidgetTree)
				{
					return UserWidget->WidgetTree;
				}
			}
			else
			{
				if (UWidgetTree* ParentWidgetTree = Cast<UWidgetTree>(Widget->GetOuter()))
				{
					if (UUserWidget* ParentUserWidget = Cast<UUserWidget>(ParentWidgetTree->GetOuter()))
					{
						if (ParentUserWidget->WidgetTree)
						{
							return ParentUserWidget->WidgetTree;
						}
					}
				}
			}
		}

		return nullptr;
	}

	TArray<AbstractNode*> UE4Node::GetChildren()
	{
		TArray<AbstractNode*> Children;

		if (Widget)
		{
			TArray<UWidget*> Widgets;

			if (UUserWidget* UserWidget = Cast<UUserWidget>(Widget))
			{
				GetWidgetTree()->GetAllWidgets(Widgets);
			}
			else
			{
				GetWidgetTree()->GetChildWidgets(Widget, Widgets);
			}

			for (UWidget* Child : Widgets)
			{
				if (Child)
				{
					UPanelWidget* ParentWidget = Child->GetParent();

					if (!ParentWidget || Widget == ParentWidget)
					{
						AbstractNode* ChildNode = new UE4Node(Child);
						Children.Add(ChildNode);
					}
				}
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
			return GetName(OutString);
		}
		else if (AttrName.Equals(TEXT("type"), ESearchCase::IgnoreCase))
		{
			return GetType(OutString);
		}
		else if (AttrName.Equals(TEXT("text"), ESearchCase::IgnoreCase))
		{
			return GetText(OutString);
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
			return GetPosition(OutArray);
		}
		else if (AttrName.Equals(TEXT("size"), ESearchCase::IgnoreCase))
		{
		
			return GetSize(OutArray);
		}
		else if (AttrName.Equals(TEXT("scale"), ESearchCase::IgnoreCase))
		{
			return GetScale(OutArray);
		}

		return false;
	}

	bool UE4Node::GetAttribute(const FString& AttrName, const TSharedPtr< FJsonObject >*& OutObject)
	{
		if (!Widget)
		{
			return false;
		}

		if (AttrName.Equals(TEXT("zOrders"), ESearchCase::IgnoreCase))
		{
			return GetZOrder(OutObject);
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

		// set z-orders
		const TSharedPtr< FJsonObject >* ZOrdersObject;
		if (GetAttribute(TEXT("zOrders"), ZOrdersObject))
		{
			Result->SetObjectField(TEXT("zOrders"), *ZOrdersObject);
		}

		// set text
		FString Text;
		if (GetAttribute(TEXT("text"), Text))
		{
			Result->SetStringField("text", Text);
		}

		return Result;
	}

	bool UE4Node::GetName(FString& OutString)
	{
		if (!Widget)
		{
			return false;
		}

		OutString = Widget->GetName();

		return true;
	}

	bool UE4Node::GetType(FString& OutString)
	{
		if (Widget && Widget->GetClass())
		{
			OutString = Widget->GetClass()->GetDefaultObjectName().ToString();

			return true;
		}

		return false;
	}

	bool UE4Node::IsVisible()
	{
		if (!Widget)
		{
			return false;
		}

		bool bVisible = ((Widget->Visibility == ESlateVisibility::Visible)
			|| (Widget->Visibility == ESlateVisibility::HitTestInvisible)
			|| (Widget->Visibility == ESlateVisibility::SelfHitTestInvisible));

		return bVisible;
	}

	bool UE4Node::GetPosition(const TArray< TSharedPtr< FJsonValue > >*& OutArray)
	{
		if (!Widget)
		{
			return false;
		}

		// Gets the last geometry used to Tick the widget. 
		// This data may not exist yet if this call happens prior to the widget having been ticked/painted, or it may be out of date, or a frame behind.
		const FGeometry Geometry = Widget->GetCachedGeometry();

		FVector2D Position = Geometry.GetAbsolutePosition();

		// The position in the game's viewport, usable for line traces and other uses where you need a coordinate in the space of viewport resolution units.
		FVector2D PositionOnScreen;

		// The position in the space of other widgets in the viewport.
		// Like if you wanted to add another widget to the viewport at the same position in viewport space as this location, this is what you would use.
		FVector2D PositionInViewport;

		USlateBlueprintLibrary::AbsoluteToViewport(Widget->GetWorld(), Position, PositionOnScreen, PositionInViewport);

		// Returns a percentage based on compatibility considerations.
		FVector2D Size = Geometry.GetAbsoluteSize();
		FVector2D Center = PositionOnScreen + 0.5f * Size;
		const FVector2D ViewportSize = UWidgetLayoutLibrary::GetViewportSize(Widget->GetWorld());
		FVector2D Percentage = Center / ViewportSize;

		PositionJson.Add(MakeShareable(new FJsonValueNumber(Percentage.X)));
		PositionJson.Add(MakeShareable(new FJsonValueNumber(Percentage.Y)));

		OutArray = &PositionJson;
		
		return true;
	}

	bool UE4Node::GetSize(const TArray< TSharedPtr< FJsonValue > >*& OutArray)
	{
		if (!Widget)
		{
			return false;
		}

		FGeometry Geometry = Widget->GetCachedGeometry();

		FVector2D AbsoluteSize = Geometry.GetAbsoluteSize();
		FVector2D ViewportSize = UWidgetLayoutLibrary::GetViewportSize(Widget->GetWorld());
		FVector2D SizePercentage = AbsoluteSize / ViewportSize;

		SizeJson.Add(MakeShareable(new FJsonValueNumber(SizePercentage.X)));
		SizeJson.Add(MakeShareable(new FJsonValueNumber(SizePercentage.Y)));

		OutArray = &SizeJson;

		return true;
	}

	bool UE4Node::GetScale(const TArray< TSharedPtr< FJsonValue > >*& OutArray)
	{
		if (!Widget)
		{
			return false;
		}

		float ViewportScale = UWidgetLayoutLibrary::GetViewportScale(Widget->GetWorld());

		ScaleJson.Add(MakeShareable(new FJsonValueNumber(ViewportScale)));
		ScaleJson.Add(MakeShareable(new FJsonValueNumber(ViewportScale)));

		OutArray = &ScaleJson;

		return true;
	}

	bool UE4Node::GetZOrder(const TSharedPtr< FJsonObject >*& OutObject)
	{
		if (!Widget)
		{
			return false;
		}
		
		if (UCanvasPanelSlot* Slot = Cast<UCanvasPanelSlot>(Widget->Slot))
		{
			int32 ZOrder = Slot->GetZOrder();

			ZOrderJson->SetNumberField(TEXT("local"), ZOrder);
			ZOrderJson->SetNumberField(TEXT("global"), ZOrder);

			OutObject = &ZOrderJson;

			return true;
		}
		
		return false;
	}

	bool UE4Node::GetText(FString& OutString)
	{
		if (!Widget)
		{
			return false;
		}

		if (UMultiLineEditableText* MultiLineEditableText = Cast<UMultiLineEditableText>(Widget))
		{
			OutString = MultiLineEditableText->GetText().ToString();
			return true;
		}

		if (UMultiLineEditableTextBox* MultiLineEditableTextBox = Cast<UMultiLineEditableTextBox>(Widget))
		{
			OutString = MultiLineEditableTextBox->GetText().ToString();
			return true;
		}

		if (URichTextBlock* RichTextBlock = Cast<URichTextBlock>(Widget))
		{
			OutString = RichTextBlock->GetText().ToString();
			return true;
		}

		if (UTextBlock* TextBlock = Cast<UTextBlock>(Widget))
		{
			OutString = TextBlock->GetText().ToString();
			return true;
		}

		return false;
	}
}
