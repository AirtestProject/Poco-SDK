// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#include "PocoSDK.h"

DEFINE_LOG_CATEGORY(PocoLog);

#define LOCTEXT_NAMESPACE "FPocoSDKModule"

void FPocoSDKModule::StartupModule()
{
	// This code will execute after your module is loaded into memory; the exact timing is specified in the .uplugin file per-module

	UE_LOG(PocoLog, Warning, TEXT("Poco SDK module loaded."));

	/** Creates a new instance of Poco Tcp server. */
	PocoServer = new Poco::FTcpServer(FIPv4Endpoint(Address, Port));
}

void FPocoSDKModule::ShutdownModule()
{
	// This function may be called during shutdown to clean up your module. For modules that support dynamic reloading,
	// we call this function before unloading the module.
}

#undef LOCTEXT_NAMESPACE
	
IMPLEMENT_MODULE(FPocoSDKModule, PocoSDK)
