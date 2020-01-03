// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#include "Converter.h"

namespace Poco
{
	/**
	 * Creates a string from a byte array.
	 * Reference: https://wiki.unrealengine.com/TCP_Socket_Listener,_Receive_Binary_Data_From_an_IP/Port_Into_UE4,_%28Full_Code_Sample%29
	 *
	 * @param BinaryArray The byte array to be converted into a string.
	 * @return The string created from the byte array.
	 */
	FString StringFromBinaryArray(TArray<uint8> BinaryArray)
	{
		// Add 0 termination. Even if the string is already 0-terminated, it doesn't change the results.
		BinaryArray.Add(0); 

		// Create a string from a byte array. The string is expected to be 0 terminated (i.e. a byte set to 0).
		return FString(UTF8_TO_TCHAR(reinterpret_cast<const char*>(BinaryArray.GetData())));
	}

	bool GetJsonString(FSocket* InSocket, FString& OutString)
	{

		if (!InSocket)
		{
			return false;
		}

		TArray<uint8> Head;
		TArray<uint8> Payload;

		uint32 PayloadLength = 0;
		uint32 Size;
		uint32 HeadBytes = 4;

		InSocket->Wait(ESocketWaitConditions::WaitForRead, FTimespan::FromHours(1));

		while (InSocket->HasPendingData(Size))
		{
			if (Size >= HeadBytes)
			{
				Head.Init(0, HeadBytes);
				int32 Read = 0;
				InSocket->Recv(Head.GetData(), HeadBytes, Read);
				PayloadLength = *((uint32*)Head.GetData());
				break;
			}
		}

		while (InSocket->HasPendingData(Size))
		{
			Payload.Init(0, FMath::Min(PayloadLength, 65507u));
			int32 PayloadRead = 0;
			InSocket->Recv(Payload.GetData(), PayloadLength, PayloadRead);
		}

		if (Payload.Num() <= 0)
		{
			return false;
		}

		OutString = StringFromBinaryArray(Payload);

		return true;
	}
}
