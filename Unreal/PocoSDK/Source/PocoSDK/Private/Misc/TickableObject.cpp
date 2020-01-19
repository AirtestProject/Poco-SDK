// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#include "TickableObject.h"
#include "PocoManager.h"

namespace Poco
{
	void FTickableObject::Tick(float DeltaTime)
	{
		if (Parent != nullptr)
		{
			Parent->Tick(DeltaTime);
		}
	}
}
