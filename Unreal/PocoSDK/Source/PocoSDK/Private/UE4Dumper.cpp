// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#include "UE4Dumper.h"
#include "Blueprint/UserWidget.h"
#include "UObject/UObjectIterator.h"
#include "Components/PanelWidget.h"
#include "UE4Node.h"
#include "PocoSDK.h"

namespace Poco
{
	AbstractNode* UE4Dumper::GetRoot()
	{
		return new RootNode();
	}

	RootNode::RootNode()
	{
		for (TObjectIterator<UUserWidget> Itr; Itr; ++Itr)
		{
			UUserWidget* UserWidget = *Itr;

			if (UserWidget && UserWidget->GetIsVisible() && UserWidget->WidgetTree)
			{
				// Call GetRootWidget() on the first non-null user widget.
				UWidget* RootWidget = UserWidget->GetRootWidget();

				if (RootWidget != nullptr)
				{
					UE4Node* Root = new UE4Node(RootWidget);

					Roots.Add((AbstractNode*)Root);

					return;
				}
			}
		}
	}

	TArray<AbstractNode*> RootNode::GetChildren()
	{
		return Roots;
	}
}
