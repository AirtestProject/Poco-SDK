// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#include "AbstractDumper.h"

namespace Poco
{
	TSharedPtr<FJsonObject> AbstractDumper::DumpHierarchy()
	{
		return DumpHierarchyImplementation(GetRoot(), true);
	}

	TSharedPtr<FJsonObject> AbstractDumper::DumpHierarchy(bool bOnlyVisibleNode)
	{
		return DumpHierarchyImplementation(GetRoot(), bOnlyVisibleNode);
	}

	TSharedPtr<FJsonObject> AbstractDumper::DumpHierarchyImplementation(AbstractNode* Node, bool bOnlyVisibleNode)
	{
		if (!Node)
		{
			return nullptr;
		}

		TSharedPtr<FJsonObject> Payload = Node->EnumerateAttributes();
		TSharedPtr<FJsonObject> Result = MakeShareable(new FJsonObject);
		
		// set the name field
		FString Name;
		Node->GetAttribute("name", Name);
		Result->SetStringField("name", Name);

		// set the payload field
		Result->SetObjectField("payload", Payload);

		TArray<AbstractNode*> Children = Node->GetChildren();

		TArray< TSharedPtr<FJsonValue> > ChildrenJson;

		for (AbstractNode* Child : Children)
		{
			bool bVisible;
			Child->GetAttribute("visible", bVisible);

			if (!bOnlyVisibleNode || bVisible)
			{
				// Add child nodes recursively
				TSharedPtr<FJsonValue> ChildJson = MakeShareable(new FJsonValueObject(DumpHierarchyImplementation(Child, bOnlyVisibleNode)));
				ChildrenJson.Add(ChildJson);
			}
		}

		if (Children.Num() > 0)
		{
			Result->SetArrayField("children", ChildrenJson);
		}

		return Result;
	}
}
