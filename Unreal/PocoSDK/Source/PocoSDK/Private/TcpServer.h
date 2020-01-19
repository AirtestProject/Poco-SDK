// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "HAL/Runnable.h"
#include "IPAddress.h"
#include "SocketSubsystem.h"
#include "Interfaces/IPv4/IPv4Endpoint.h"
#include "HAL/RunnableThread.h"
#include "Common/TcpSocketBuilder.h"
#include "PocoManager.h"

namespace Poco
{
	/**
	 * Implements a runnable that listens for incoming TCP connections.
	 */
	class FTcpServer
		: public FRunnable
	{
	public:

		/**
		 * Creates and initializes a new instance from the specfied IP endpoint.
		 *
		 * @param LocalEndpoint The local IP endpoint to listen on.
		 * @param SleepTime The maximum time to wait for a pending connection inside the polling loop (default = 1 second).
		 */
		FTcpServer(const FIPv4Endpoint& LocalEndpoint, const FTimespan& InSleepTime = FTimespan::FromSeconds(1))
			: DeleteSocket(true)
			, Endpoint(LocalEndpoint)
			, SleepTime(InSleepTime)
			, Socket(nullptr)
			, Stopping(false)
		{		
			Thread = FRunnableThread::Create(this, TEXT("FTcpListener"), 8 * 1024, TPri_Normal);
		}

		/** Destructor. */
		~FTcpServer()
		{
			if (Thread != nullptr)
			{
				Thread->Kill(true);
				delete Thread;
			}

			if (DeleteSocket && (Socket != nullptr))
			{
				ISocketSubsystem::Get(PLATFORM_SOCKETSUBSYSTEM)->DestroySocket(Socket);
				Socket = nullptr;
			}
		}

	public:

		/**
		 * Gets the listener's local IP endpoint.
		 *
		 * @return IP endpoint.
		 */
		const FIPv4Endpoint& GetLocalEndpoint() const
		{
			return Endpoint;
		}

		/**
		 * Gets the listener's network socket.
		 *
		 * @return Network socket.
		 */
		FSocket* GetSocket() const
		{
			return Socket;
		}

		/**
		 * Checks whether the listener is listening for incoming connection.
		 *
		 * @return true if it is listening, false otherwise.
		 */
		bool IsActive() const
		{
			return ((Socket != nullptr) && !Stopping);
		}

	public:

		// FRunnable interface

		virtual bool Init() override
		{
			if (Socket == nullptr)
			{
				Socket = FTcpSocketBuilder(TEXT("FTcpListener server"))
					.AsReusable()
					.BoundToEndpoint(Endpoint)
					.Listening(8)
					.WithSendBufferSize(2 * 1024 * 1024);
			}

			// Creates and initializes a new instance of Poco manager.
			PocoManager = new FPocoManager();

			return (Socket != nullptr);
		}

		virtual uint32 Run() override
		{
			TSharedRef<FInternetAddr> RemoteAddress = ISocketSubsystem::Get(PLATFORM_SOCKETSUBSYSTEM)->CreateInternetAddr();

			const bool bHasZeroSleepTime = (SleepTime == FTimespan::Zero());

			while (!Stopping)
			{
				bool Pending = false;

				// handle incoming connections
				if (Socket->WaitForPendingConnection(Pending, SleepTime))
				{
					if (Pending)
					{
						FSocket* ConnectionSocket = Socket->Accept(*RemoteAddress, TEXT("FTcpListener client"));

						if (PocoManager)
						{
							// handles client connections
							PocoManager->HandleConnection(ConnectionSocket);
						}
					}
					else if (bHasZeroSleepTime)
					{
						FPlatformProcess::Sleep(0.f);
					}
				}
				else
				{
					FPlatformProcess::Sleep(SleepTime.GetSeconds());
				}
			}

			return 0;
		}

		virtual void Stop() override
		{
			Stopping = true;
		}

		virtual void Exit() override {}

	private:

		/** Holds a flag indicating whether the socket should be deleted in the destroctor. */
		bool DeleteSocket;

		/** Holds the server endpoint. */
		FIPv4Endpoint Endpoint;

		/** Holds the time to sleep between checking for pending connections. */
		FTimespan SleepTime;

		/** Holds the server socket. */
		FSocket* Socket;

		/** Holds a flag indicating that the thread is stopping. */
		bool Stopping;

		/** Holds the thread object. */
		FRunnableThread* Thread;

		/** Holds a single instance of PocoManager. */
		FPocoManager* PocoManager;
	};
}
