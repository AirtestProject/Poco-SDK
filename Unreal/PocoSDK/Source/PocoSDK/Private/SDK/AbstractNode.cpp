// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#include "AbstractNode.h"

namespace Poco
{
	/**
	 * The base class to represent a UI node.
	 */
	AbstractNode::AbstractNode()
	{
		AvailableAttributes = MakeShareable(new FJsonObject);

		// default name
		AvailableAttributes->SetStringField(TEXT("name"), TEXT("<root>"));

		// default type
		AvailableAttributes->SetStringField(TEXT("type"), TEXT("Root"));

		// default visibility
		AvailableAttributes->SetBoolField(TEXT("visible"), true);

		// default positoin
		TArray< TSharedPtr<FJsonValue> > Position;
		Position.Add(MakeShareable(new FJsonValueNumber(0.f)));
		Position.Add(MakeShareable(new FJsonValueNumber(0.f)));
		AvailableAttributes->SetArrayField(TEXT("pos"), Position);

		// default size
		TArray< TSharedPtr<FJsonValue> > Size;
		Size.Add(MakeShareable(new FJsonValueNumber(0.f)));
		Size.Add(MakeShareable(new FJsonValueNumber(0.f)));
		AvailableAttributes->SetArrayField(TEXT("size"), Size);

		// default scale
		TArray< TSharedPtr<FJsonValue> > Scale;
		Scale.Add(MakeShareable(new FJsonValueNumber(1.f)));
		Scale.Add(MakeShareable(new FJsonValueNumber(1.f)));
		AvailableAttributes->SetArrayField(TEXT("scale"), Scale);

		// default anchor point
		TArray< TSharedPtr<FJsonValue> > AnchorPoint;
		AnchorPoint.Add(MakeShareable(new FJsonValueNumber(0.5f)));
		AnchorPoint.Add(MakeShareable(new FJsonValueNumber(0.5f)));
		AvailableAttributes->SetArrayField(TEXT("anchorPoint"), AnchorPoint);

		// default z-orders
		TSharedPtr< FJsonObject > ZOrders = MakeShareable(new FJsonObject);
		ZOrders->SetNumberField(TEXT("local"), 0);
		ZOrders->SetNumberField(TEXT("global"), 0);
		AvailableAttributes->SetObjectField(TEXT("zOrders"), ZOrders);
	}

	bool AbstractNode::GetAttribute(const FString& AttrName, FString& OutString)
	{
		return AvailableAttributes->TryGetStringField(AttrName, OutString);
	}

	bool AbstractNode::GetAttribute(const FString& AttrName, bool& OutBool)
	{
		return AvailableAttributes->TryGetBoolField(AttrName, OutBool);
	}

	bool AbstractNode::GetAttribute(const FString& AttrName, const TArray< TSharedPtr< FJsonValue > >*& OutArray)
	{
		return AvailableAttributes->TryGetArrayField(AttrName, OutArray);
	}

	bool AbstractNode::GetAttribute(const FString& AttrName, const TSharedPtr< FJsonObject >*& OutObject)
	{
		return AvailableAttributes->TryGetObjectField(AttrName, OutObject);
	}

	TSharedPtr<FJsonObject> AbstractNode::EnumerateAttributes()
	{
		TSharedPtr<FJsonObject> Result = MakeShareable(new FJsonObject);

		// set name
		FString Name;
		if (AbstractNode::GetAttribute(TEXT("name"), Name))
		{
			Result->SetStringField("name", Name);
		}

		// set type
		FString Type;
		if (AbstractNode::GetAttribute(TEXT("type"), Type))
		{
			Result->SetStringField(TEXT("type"), Type);
		}

		// set visibility
		bool bVisible;
		if (AbstractNode::GetAttribute(TEXT("visible"), bVisible))
		{
			Result->SetBoolField(TEXT("visible"), bVisible);
		}

		// set position
		const TArray< TSharedPtr< FJsonValue > >* Position;
		if (AbstractNode::GetAttribute(TEXT("pos"), Position))
		{
			Result->SetArrayField(TEXT("pos"), *Position);
		}

		// set size
		const TArray< TSharedPtr< FJsonValue > >* Size;
		if (AbstractNode::GetAttribute(TEXT("size"), Size))
		{
			Result->SetArrayField(TEXT("size"), *Size);
		}

		// set scale
		const TArray< TSharedPtr< FJsonValue > >* Scale;
		if (AbstractNode::GetAttribute(TEXT("scale"), Scale))
		{
			Result->SetArrayField(TEXT("scale"), *Scale);
		}

		// set anchor point
		const TArray< TSharedPtr< FJsonValue > >* AnchorPoint;
		if (AbstractNode::GetAttribute(TEXT("anchorPoint"), AnchorPoint))
		{
			Result->SetArrayField(TEXT("anchorPoint"), *AnchorPoint);
		}

		// set z-orders
		const TSharedPtr< FJsonObject >* ZOrders;
		if (AbstractNode::GetAttribute(TEXT("zOrders"), ZOrders))
		{
			Result->SetObjectField(TEXT("zOrders"), *ZOrders);
		}

		return Result;
	}
}
