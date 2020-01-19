// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Sockets.h"

namespace Poco
{
	/**
	 * Creates a string from a byte array.
	 * Reference: https://wiki.unrealengine.com/TCP_Socket_Listener,_Receive_Binary_Data_From_an_IP/Port_Into_UE4,_%28Full_Code_Sample%29
	 *
	 * @param BinaryArray The byte array to be converted into a string.
	 * @return The string created from the byte array.
	 */
	FString StringFromBinaryArray(TArray<uint8> BinaryArray);
	
	/**
	 * Converts binary data into a json-formatted string.
	 *
	 * @param InSocket The client socket.
	 * @param OutString The payload as a json-formatted string.
	 * @return true upon success, false otherwise.
	 */
	bool GetJsonString(FSocket* InSocket, FString& OutString);
}
