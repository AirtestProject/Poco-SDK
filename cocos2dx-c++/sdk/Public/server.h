#include "sdk/clsocket/PassiveSocket.h"
#include <string>
//#include "2d/CCRenderTexture.h"
using namespace std;


#define MAX_PACKET 4096
#define SERVER_PORT 18888

struct Header_T {
	int len;
};

typedef struct NET_DATA {
	Header_T	dataHeader;
	char		dataStr[MAX_PACKET];
}NetData;

struct SOCKET_INFO {
	string		ip;
	int				port;
	int				socket;

    string		production;

	void*			sender_thread;
};


typedef struct ACTION_DATA {
	CActiveSocket*	pClient;
	//IGGQATimeJob*	pScreen;
}DATA;



void JsonDataSend(CActiveSocket* pClient, string data);
bool ServerRun();
void* do_action(void* data);
void standartDump(const char* idStr, CActiveSocket* pClient);
void unknownFunction(CActiveSocket* pClient, string id_s, string clientIp, string funcName);