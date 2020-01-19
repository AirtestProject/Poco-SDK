// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Modules/ModuleManager.h"
#include "TcpServer.h"
#include "IPAddress.h"

DECLARE_LOG_CATEGORY_EXTERN(PocoLog, Log, All);

class FPocoSDKModule : public IModuleInterface
{
public:

	/** IModuleInterface implementation */
	virtual void StartupModule() override;
	virtual void ShutdownModule() override;

private:

	/** Holds the Tcp server for Poco services. */
	Poco::FTcpServer* PocoServer;

	/** Holds the default IP address. */
	FIPv4Address Address = FIPv4Address(0, 0, 0, 0);

	/** Holds the default port number. */
	uint16 Port = 5001;
};
