// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"

namespace Poco
{
	class FJsonParser
	{
	public:

		/** 
		 * Gets the method field of a jsonrpc 2.0 request.
		 * Returns false if the field does not exist or cannot be converted.
		 *
		 * @param InString The jsonrpc 2.0 request.
		 * @param OutString The field's value as a string. 
		 * @return false if the field does not exist or cannot be converted, true otherwise.
		 */
		static bool GetMethod(const FString& InString, FString& OutString);

		/**
		 * Gets the id field of a jsonrpc 2.0 request.
		 * Returns false if the field does not exist or cannot be converted.
		 *
		 * @param InString The jsonrpc 2.0 request.
		 * @param OutString The field's value as a string.
		 * @return false if the field does not exist or cannot be converted, true otherwise.
		 */
		static bool GetId(const FString& Json, FString& OutString);

		/** 
		 * Gets the boolean parameter field of a jsonrpc 2.0 request.
		 * Returns false if the field does not exist or cannot be converted.
		 *
		 * @param Json The jsonrpc 2.0 request.
		 * @param OutBool The field's value as a boolean.
		 * @return false if the field does not exist or cannot be converted, true otherwise.
		 */
		static bool GetBoolParam(const FString& Json, bool& OutBool);

	private:
		/**
		 * Gets the field with the specified name Key in a Json-formatted string.
		 * Returns false if the field does not exist or cannot be converted.
		 *
		 * @param Json The Json-formatted string.
		 * @param Key The name of the field to get.
		 * @param OutString The fields value as a string.
		 * @return false if the field does not exist or cannot be converted, true otherwise.
		 */
		static bool GetStringByKey(const FString& Json, const FString& Key, FString& OutString);
	};
}
