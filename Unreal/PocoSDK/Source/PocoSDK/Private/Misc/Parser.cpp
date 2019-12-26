// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#include "Parser.h"
#include "Dom/JsonObject.h"
#include "Serialization/JsonTypes.h"
#include "Serialization/JsonReader.h"
#include "Serialization/JsonSerializer.h"

namespace Poco
{
	bool FJsonParser::GetMethod(const FString& InputString, FString& OutString)
	{
		FString Key = TEXT("method");
		return GetStringByKey(InputString, Key, OutString);
	}

	bool FJsonParser::GetId(const FString& InputString, FString& OutString)
	{
		FString Key = TEXT("id");
		return GetStringByKey(InputString, Key, OutString);
	}

	bool FJsonParser::GetBoolParam(const FString& InputString, bool& OutBool)
	{
		TSharedRef< TJsonReader<> > Reader = TJsonReaderFactory<>::Create(InputString);

		TSharedPtr<FJsonObject> Object;

		if (FJsonSerializer::Deserialize(Reader, Object) && Object.IsValid())
		{
			FString Key = TEXT("params");
			const TArray< TSharedPtr<FJsonValue> >* OutArray;

			if (Object->TryGetArrayField(Key, OutArray) && OutArray->Num() > 0 && (*OutArray)[0]->Type == EJson::Boolean)
			{
				OutBool = (*OutArray)[0]->AsBool();
				return true;
			}
		}
		return false;
	}

	bool FJsonParser::GetStringByKey(const FString& InputString, const FString& Key, FString& OutString)
	{
		TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(InputString);

		TSharedPtr<FJsonObject> Object;

		if (FJsonSerializer::Deserialize(Reader, Object) && Object.IsValid())
		{
			const TSharedPtr<FJsonValue>* Value = Object->Values.Find(Key);
			if (Value && (*Value)->Type == EJson::String)
			{
				OutString = (*Value)->AsString();
				return true;
			}
		}
		return false;
	}
}
