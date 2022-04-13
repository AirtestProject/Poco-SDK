#pragma once
#include <string>
#include "cocos2d.h"
#include "sdk/json/writer.h"
#include "sdk/json/stringbuffer.h"

using namespace std;
using namespace cocos2d;
using namespace rapidjson;

enum NodeType {
	N_Unknown = -1,
	N_Control = 50,
	N_Layer = 1,
	N_MenuItem = 2,
	N_Scene = 3,
	N_AtlasNode = 4,
	N_LabelTTF = 5,
	N_SpriteBatchNode = 6,
	N_Sprite = 7,
	N_Guide = 8,
	N_GuideTouchNode = 9,
	N_INode = 10
};


class NeteaseNode {
	Node* _node;
	Size _screenSize;

public:
	NeteaseNode(Node* node);
	~NeteaseNode();

	void getPos(float* pos);

	void getSize(float* data);

	void getScale(float* data);

	void getRotation3D(float* data);

	void getZOrders(float* data);

	void getSkew(float* data);

	void getAnchorPoint(float* pos);

	string getName(const char* nodeType);

	void getChildren(Writer<StringBuffer>& writer);

	void setDumpString(Writer<StringBuffer>& writer);

	void getPayload(string nodeName, int nodeType, Writer<StringBuffer>& writer);

	void getNodeType(int& nodeType);

	string getNodeTypeStr(int nodeType);

	float getScreenWidth() { return _screenSize.width; }

	float getScreenHeight() { return _screenSize.height; }

	void doClick(vector<float>& data) noexcept;

};