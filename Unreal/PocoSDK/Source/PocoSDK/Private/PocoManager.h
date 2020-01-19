// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "HAL/Runnable.h"
#include "Misc/TickableObject.h"
#include "Sockets.h"
#include "Dom/JsonObject.h"
#include "Blueprint/UserWidget.h"

namespace Poco
{

	class FPocoWorker;

	/**
	 * Implements a class that manages a collection of worker threads.
	 */
	class FPocoManager
	{
	public:

		/** Creates and initializes a new instance of Poco manager. */
		FPocoManager();

		/** Destructor. */
		~FPocoManager();

		/**
		 * Called every frame to clean up finished worker threads.
		 *
		 * @param DeltaTime Game time passed since the last call.
		 */
		void Tick(float DeltaTime);

		/**
		 * Handles the Tcp connection.
		 *
		 * The connection handling process includes request parsing, command execution, and sending back the response.
		 *
		 * @param InSocket The client socket.
		 */
		void HandleConnection(FSocket* InSocket);

	private:

		/** Holds the tickable game object. */
		FTickableObject* TickableObject;

		/** Holds an array of worker threads. */
		TArray< TSharedPtr<FPocoWorker> > WorkerPool;
	};

	/**
	 * Implements a runnable that handles incoming TCP connections.
	 */
	class FPocoWorker
		: public FRunnable
	{
	public:

		/**
		 * Creates and initializes a new instance of the Poco worker thread from the client socket.
		 *
		 * @param Socket The client socket.
		 */
		FPocoWorker(FSocket* Socket);

		/**
		 * Runs the worker thread.
		 *
		 * This is where all per object thread work is done. This is only called if the initialization was successful.
		 *
		 * @return The exit code of the worker thread.
		 */
		virtual uint32 Run() override;

		/**
		 * Exits the worker thread.
		 *
		 * Called in the context of the aggregating thread to perform any cleanup.
		 */
		virtual void Exit() override;

		/** Creates a thread to perform the task asynchronously. */
		void Start();

		/**
		 * Overrides the base class method.
		 * Stops the runnable object. This is called if a thread is requested to terminate early.
		 */
		virtual void Stop() override;

		/**
		 * Checks whether it is safe to delete the worker thread.
		 *
		 * @return true if it is safe to delete the worker thread, false otherwise.
		 */
		bool SafeToDelete();

	private:

		/** Holds a flag indicating whether the worker has started its work. */
		bool Started;

		/** Holds a flag indicating whether the worker has had its work done. */
		bool Ended;

		/** Holds the client socket. */
		FSocket* Socket;

		/** Holds the response to be sent back to Poco. */
		TSharedPtr<FJsonObject> Response;

		/** Holds the id field of the jsonrpc 2.0 request. */
		FString Id;

		/**
		 * Parses the request and executes the command.
		 *
		 * @return true upon success, false otherwise.
		 */
		bool HandleRequest();

		/**
		 * Sends the response back to Poco.
		 *
		 * @return true upon success, false otherwise.
		 */
		bool SendResponse();

		/** Holds a flag indicating that the thread is stopping. */
		bool Stopping;

		/** Holds the SDK version number as a string. */
		FString SDKVersion = TEXT("1.0");
	};
}
