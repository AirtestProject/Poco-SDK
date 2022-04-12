#include<fstream>
#include "sdk/Public/dump.h"
#include "sdk/Public/node.h"
#include "sdk/json/stringbuffer.h"
#include "sdk/json/writer.h"
using namespace std;

string Dump(const char* id) {
	Director* currentDirector = Director::getInstance();
	Scene* currentScene = currentDirector->getRunningScene();

	if (currentScene == NULL)
		return "";

	rapidjson::StringBuffer buffer;
	rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);

	writer.StartObject();

	writer.Key("jsonrpc");
	writer.Double(2.0);
	writer.Key("id");
	writer.String(id);
	writer.Key("result");
	writer.StartObject();

	auto rootName = currentScene->getName();
	writer.Key("name");
	writer.String(rootName.c_str());

	NeteaseNode scene_node(currentScene);
	scene_node.getPayload(rootName, 3, writer);
	scene_node.getChildren(writer);

	writer.EndObject();
	writer.EndObject();

	string dumpString = buffer.GetString();
	
	return dumpString;
}


