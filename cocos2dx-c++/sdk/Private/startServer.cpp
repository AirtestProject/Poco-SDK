#include "AppDelegate.h"
//#include "sdk/Public/startServer.h"
#include "sdk/Public/server.h"
#include <iostream>

void *aServerRun(void* data) {
	ServerRun();
	return nullptr;
}

void ServerStart()
{
	auto* res = "";
#ifdef _WIN32
	CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)aServerRun, (void*)res, 0, NULL);
#else
	pthread_t thread = 0;
	pthread_create(&thread, NULL, aServerRun, (void*)res);
#endif
}

