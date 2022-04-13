#include "cocos2d.h"
#include "sdk/Public/server.h"
#include "sdk/Public/dump.h"
#include "sdk/json/stringbuffer.h"
#include "sdk/json/document.h"
#include "sdk/json/writer.h"
using namespace std;

void JsonDataSend(CActiveSocket* pClient, string data) {
	NetData sendData;
	memset(sendData.dataStr, 0, MAX_PACKET);

	int dumpStrLen = data.length();
	int totalStrLen = dumpStrLen + 4;
	if (totalStrLen > MAX_PACKET) {
		int strOffset = 0;
		int willSendedLen = totalStrLen;
		while (willSendedLen > 0) {
			int sendLen = 0;
			if (willSendedLen <= MAX_PACKET) {
				sendLen = willSendedLen;
			}
			else {
				sendLen = MAX_PACKET % willSendedLen;
			}

			if (strOffset == 0) {
				int firstStrLen = sendLen - 4;
				std::string sendStr = data.substr(strOffset, firstStrLen);
				sprintf(sendData.dataStr, "%s", sendStr.c_str());
				sendData.dataHeader.len = dumpStrLen;
				pClient->Send((unsigned char *)&sendData, sendLen);
				strOffset += firstStrLen;
				willSendedLen -= sendLen;
			}
			else {
				std::string sendStr = data.substr(strOffset, sendLen);
				pClient->Send((unsigned char *)sendStr.c_str(), sendLen);
				strOffset += sendLen;
				willSendedLen -= sendLen;
			}
		}
	}
	else {
		memcpy(sendData.dataStr, data.c_str(), data.length());
		sendData.dataHeader.len = data.length();
		pClient->Send((unsigned char *)&sendData, sendData.dataHeader.len + 4);
	}
}

bool ServerRun()
// 这是接下整个sdk的初始化入口，用于接收前端的指令需求，然后创建线程回调do_action
{
	CPassiveSocket socket;
	string host_ip = "localhost";

	socket.Initialize();

	socket.Listen(host_ip.c_str(), SERVER_PORT);

	while (true) {
		socket.Select();

		DATA* actionData = new DATA;
		actionData->pClient = socket.Accept();
		if (actionData->pClient == NULL) {
			continue;
		}
#ifdef _WIN32
		// LPTHREAD_START_ROUTINE是windows自带的回调函数机制，不过在.net4中被遗弃
		CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)do_action, (void*)actionData, 0, NULL);
#else
		CCLOG("enter create other thread");
		pthread_t thread = 0;
		pthread_create(&thread, NULL, do_action, (void*)actionData);
#endif

#ifdef WIN32
		if (10038 == GetLastError()) {
			CCLOG("enter create win32 error");
			break;
		}
#else
		if (9 == errno) {
			CCLOG("enter create other thread");
			break;
		}
#endif
	}

	socket.Close();

	return true;
}



void* do_action(void* data)
{
	DATA* pData = (DATA*)data;
	CActiveSocket* pClient = pData->pClient;

	while (true) {
		//NetData recvData;
		int numBytes = pClient->Receive(2048);
		if (numBytes <= 0) {
			pClient->Close();
			delete pClient;
			delete data;
			pClient = nullptr;
			data = nullptr;
			return NULL;
		}
		char tmpRes[2048 + 1];
		memset(tmpRes, 0, 2048 + 1);
		memcpy(tmpRes, pClient->GetData(), numBytes);

		Header_T* recvHeader = (Header_T*)tmpRes;
		int recvLen = recvHeader->len;
		if (recvLen > 100000) {
			pClient->Close();
			delete pClient;
			delete data;
			return NULL;
		}

		char recvDataStr[2048] = { 0 };
		memcpy(recvDataStr, tmpRes + 4, recvLen);
		
		rapidjson::Document document;
		document.Parse(recvDataStr);

		if (!document.HasMember("id") || !document.HasMember("method")) {
			pClient->Close();
			delete pClient;
			delete data;
			return NULL;
		}

		rapidjson::Value& id_v = document["id"];
		rapidjson::Type vType = id_v.GetType();

		char idStr[512] = { 0 };
		if (vType == rapidjson::Type::kStringType) {
			sprintf(idStr, "%s", id_v.GetString());
		}
		else if (vType == rapidjson::Type::kNumberType) {
			sprintf(idStr, "%d", id_v.GetInt());
		}
		else {
			sprintf(idStr, "%s", "");
		}

		rapidjson::Value& s = document["method"];
		const char* methodStr = s.GetString();

		if (strcmp(methodStr, "Dump") == 0) {
			standartDump(idStr, pClient);
		}
		else {
		    unknownFunction(pClient, idStr, "255.255.255.0", methodStr);
		}
	}
}
		
void standartDump(const char* idStr, CActiveSocket* pClient) {
	int bytesReceived = 0;
	int nparsed = -1;

	string dumpStr = "";

	dumpStr = Dump(idStr);

	JsonDataSend(pClient, dumpStr);
}

void unknownFunction(CActiveSocket* pClient, string id_s, string clientIp, string funcName) {
	rapidjson::StringBuffer buffer;
	rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);

	writer.StartObject();

	writer.Key("jsonrpc");
	writer.Double(2.0);
	writer.Key("id");
	writer.String(id_s.c_str());

	writer.Key("error");
	writer.StartObject();
	writer.Key("message");

	char msg[1024] = { 0 };
	sprintf(msg, "No such rpc method \"%s\", reqid: %s, client:%s\"", funcName.c_str(), id_s.c_str(), clientIp.c_str());

	writer.Key(msg);
	writer.EndObject();

	writer.EndObject();

	JsonDataSend(pClient, buffer.GetString());
}


