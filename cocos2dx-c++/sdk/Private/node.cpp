#include<iostream>
#include<regex>
#include "sdk/Public/node.h"
#include <cocos/ui/UIButton.h>
#include <cocos/ui/UITextField.h>
#include <cocos/ui/UIScale9Sprite.h>
#include <cocos/ui/UISlider.h>
using namespace ui;
using namespace cocos2d;
using namespace rapidjson;

bool IsFiniteNumber(double x) {
	return (x <= DBL_MAX && x >= -DBL_MAX);
}

NeteaseNode::NeteaseNode(Node* node)
{
	auto glView = Director::getInstance()->getOpenGLView();
	_screenSize = glView->getFrameSize();
	_node = node;
	_node->retain();
}

NeteaseNode::~NeteaseNode()
{
	try
	{
		_node->release();
	}
	catch (const exception&)
	{
		cout<< "failed";
	}
}

void NeteaseNode::getPos(float* pos)
{
#if COCOS2D_VERSION >= 0x00020100 && COCOS2D_VERSION < 0x00030000
	CCPoint convert_pos = _node->convertToWorldSpaceAR(CCPoint(0, 0));
#else
	Vec2 convert_pos = _node->convertToWorldSpaceAR(Vec2(0, 0));
#endif
	float tem_x = convert_pos.x / _screenSize.width;
	float tem_y = 1 - (convert_pos.y / _screenSize.height);
	if (!IsFiniteNumber(tem_x)) {
		tem_x = 0;
	}
	if (!IsFiniteNumber(tem_y)) {
		tem_y = 0;
	}

	pos[0] = tem_x;
	pos[1] = tem_y;
}

void NeteaseNode::getSize(float* data)
{
	Size size = _node->getContentSize();
	if (!IsFiniteNumber(size.width)) {
		size.width = 0;
	}
	if (!IsFiniteNumber(size.height)) {
		size.height = 0;
	}
	size.width = size.width / _screenSize.width;
	size.height = size.height / _screenSize.height;
	//data[0] = size.width;
	//data[1] = size.height;

	auto glView = Director::getInstance()->getOpenGLView();
	data[0] = size.width * glView->getScaleX();
	data[1] = size.height * glView->getScaleY();
}

void NeteaseNode::getScale(float* data)
{
	float x = _node->getScaleX();
	float y = _node->getScaleY();
	if (!IsFiniteNumber(x)) {
		x = 0;
	}
	if (!IsFiniteNumber(y)) {
		y = 0;
	}
	data[0] = x;
	data[1] = y;
}

void NeteaseNode::getRotation3D(float* data)
{
#if COCOS2D_VERSION >= 0x00020100 && COCOS2D_VERSION < 0x00030000
	float x = _node->getRotationX();
	float y = _node->getRotationY();
	data[0] = 0;
	data[1] = 0;
	data[2] = 0;
#else
	float x = _node->getRotationSkewX();
	float y = _node->getRotationSkewY();
	if (x == y) {
		Vec3 v = _node->getRotation3D();
		data[0] = v.x;
		data[1] = v.y;
		data[2] = v.z;
	}
#endif
}

void NeteaseNode::getZOrders(float* data)
{
#if COCOS2D_VERSION >= 0x00020100 && COCOS2D_VERSION < 0x00030000
	data[0] = _node->getZOrder();
	data[1] = _node->getZOrder();
#else
	data[0] = _node->getGlobalZOrder();
	data[1] = _node->getLocalZOrder();
#endif
}

void NeteaseNode::getSkew(float* data)
{
	data[0] = _node->getSkewX();
	data[1] = _node->getSkewY();
}

void NeteaseNode::getAnchorPoint(float* pos)
{
#if COCOS2D_VERSION >= 0x00020100 && COCOS2D_VERSION < 0x00030000
	CCPoint anchorPoint = _node->getAnchorPoint();
#else
	Vec2 anchorPoint = _node->getAnchorPoint();
#endif

	float tem_x = anchorPoint.x;
	float tem_y = 1 - anchorPoint.y;

	pos[0] = tem_x;
	pos[1] = tem_y;
}

string NeteaseNode::getName(const char* nodeType)
{
#if COCOS2D_VERSION >= 0x00020100 && COCOS2D_VERSION < 0x00030000
	sprintf(name, "<%s>", nodeType);
	return name;
#else
	string name = _node->getName();
	string desc = _node->getDescription();
	if (name.compare("") == 0) {
		name = desc;
	}
	if (name.compare("") == 0) {
		name = "no-name";
	}
	return name;
#endif
}

void NeteaseNode::getChildren(Writer<StringBuffer>& writer){
	int childrenCnt = _node->getChildrenCount();
	CCLOG("childrenCount:%d", childrenCnt);
	if (childrenCnt > 0) {
		writer.Key("children");
		writer.StartArray();

#if COCOS2D_VERSION >= 0x00020100 && COCOS2D_VERSION < 0x00030000
		CCObject* child;
		CCARRAY_FOREACH(_node->getChildren(), child)
		{
			CCNode* sub = (CCNode*)child;
			bool bVisible = sub->isVisible();

			writer.StartObject();

			NeteaseNode childNode(sub);
			childNode.setDumpString(writer);

			writer.EndObject();
		}
#else
		Label* label = dynamic_cast<Label*>(_node);
		for (auto& child : _node->getChildren()) {
			if (label != NULL) {
				Sprite* sprite = dynamic_cast<Sprite*>(child);
				if (sprite != NULL) {
					//not parse Sprite under Label
					continue;
				}
			}
			//parseNodeToXML(doc, element, child);

			writer.StartObject();

			NeteaseNode childNode(child);
			childNode.setDumpString(writer);

			writer.EndObject();
		}
#endif // 
		writer.EndArray();
	}
}

void NeteaseNode::setDumpString(Writer<StringBuffer>& writer)
{
	int nodeType = getNodeType();
	string nodeName = _node->getName();
	if (nodeName == "") {
		nodeName = "no-name";
	}
	writer.Key("name");
	writer.String(nodeName.c_str());

	getPayload(nodeName, nodeType, writer);
	getChildren(writer);
}

void NeteaseNode::getPayload(string nodeName, int nodeType, Writer<StringBuffer>& writer)
{
	writer.Key("payload");
	writer.StartObject();

	writer.Key("screen");
	writer.StartArray();
	writer.Int(this->getScreenWidth());
	writer.Int(this->getScreenHeight());
	//CCLOG("ScreenSize: %d, %d", this->getScreenWidth(), this->getScreenHeight());
	writer.EndArray();

	writer.Key("rotation");

	float rotationX = _node->getRotationX();
	float rotationY = _node->getRotationY();
	CCLOG("rotationX: %f", rotationX);
	CCLOG("rotationY: %f", rotationY);

	if (rotationX == rotationY) {
		writer.Double(_node->getRotation());
	}
	else {
		writer.Double(_node->getRotationX());
	}
	
	writer.Key("tag");
	writer.Int(_node->getTag());
	CCLOG("tag: %d", _node->getTag());

	writer.Key("visible");
	writer.Bool(_node->isVisible());
	CCLOG("visible: %d", _node->isVisible());

#if 0
	writer.Key("desc");

#ifdef COCOS2D_CPP_3X
	writer.String(_node->getDescription().c_str());
#else
	char desc[1024];
	memset(desc, 0, 1024);

	sprintf(desc, "<%d | Tag = %d>", nodeType, _node->getTag());
	writer.String(desc);
#endif
#endif // 0
	writer.Key("type");
	writer.String(this->getNodeTypeStr(nodeType).c_str());
	CCLOG("type: %s", this->getNodeTypeStr(nodeType).c_str());

	writer.Key("name");
	writer.String(nodeName.c_str());
	CCLOG("nodename:: %s", nodeName.c_str());

	float pos[2] = { 0 };
	getPos(pos);
	writer.Key("pos");
	//writer.SetMaxDecimalPlaces(3);
	writer.StartArray();

	writer.Double(pos[0]);
	writer.Double(pos[1]);
	writer.EndArray();
	CCLOG("pos: %f, %f", pos[0], pos[1]);

	float size[2] = { 0 };
	getSize(size);
	writer.Key("size");
	writer.StartArray();
	writer.Double(size[0]);
	writer.Double(size[1]);
	CCLOG("Size: %f, %f", size[0], size[1]);
	writer.EndArray();

	float scale[2] = { 0 };
	getScale(scale);
	writer.Key("scale");
	writer.StartArray();
	writer.Double(scale[0]);
	writer.Double(scale[1]);
	CCLOG("scale: %f, %f", scale[0], scale[1]);
	writer.EndArray();

	float rotation3D[3] = { 0 };
	getRotation3D(rotation3D);
	writer.Key("rotation3D");
	writer.StartObject();
	writer.Key("x");
	writer.Double(rotation3D[0]);
	writer.Key("y");
	writer.Double(rotation3D[1]);
	writer.Key("z");
	writer.Double(rotation3D[2]);
	CCLOG("x: %f, y:%f, z:%f", rotation3D[0], rotation3D[1], rotation3D[2]);
	writer.EndObject();

	float zOrders[2] = { 0 };
	getZOrders(zOrders);
	writer.Key("zOrders");
	writer.StartObject();
	writer.Key("global");
	writer.Double(zOrders[0]);
	writer.Key("local");
	writer.Double(zOrders[1]);
	CCLOG("zOrders: global: %f, local: %f", zOrders[0], zOrders[1]);
	writer.EndObject();

	float skew[2] = { 0 };
	getSkew(skew);
	writer.Key("skew");
	writer.StartArray();
	writer.Double(skew[0]);
	writer.Double(skew[1]);
	CCLOG("skew: %f, %f", skew[0], skew[1]);
	writer.EndArray();

	float anchorPoint[2] = { 0 };
	getAnchorPoint(anchorPoint);
	writer.Key("anchorPoint");
	writer.StartArray();
	writer.Double(anchorPoint[0]);
	writer.Double(anchorPoint[1]);
	CCLOG("anchorPoint: %f, %f", anchorPoint[0], anchorPoint[1]);
	CCLOG("-------------------------------------------");
	writer.EndArray();

	writer.EndObject();
}

int NeteaseNode::getNodeType() {
	int nodeType = -10;
	Layer* layer = dynamic_cast<Layer*>(_node);
	if (layer != nullptr) {
		nodeType = N_Layer;
		return nodeType;
	}

	MenuItem* menuItem = dynamic_cast<MenuItem*>(_node);
	if (menuItem != nullptr) {
		nodeType = N_MenuItem;
		return nodeType;
	}

	Scene* scene = dynamic_cast<Scene*>(_node);
	if (scene != NULL) {
		nodeType = N_Scene;
		return nodeType;
	}

	AtlasNode* atlas = dynamic_cast<AtlasNode*>(_node);
	if (atlas != NULL) {
		nodeType = N_AtlasNode;
		return nodeType;
	}

	LabelTTF* ttf = dynamic_cast<LabelTTF*>(_node);
	if (ttf != NULL) {
		nodeType = N_LabelTTF;
		return nodeType;
	}

	SpriteBatchNode* batch = dynamic_cast<SpriteBatchNode*>(_node);
	if (batch != NULL) {
		nodeType = N_SpriteBatchNode;
		return nodeType;
	}

	Sprite* sprite = dynamic_cast<Sprite*>(_node);
	if (sprite != NULL) {
		nodeType = N_Sprite;
		return nodeType;
	}

	Slider* slider = dynamic_cast<Slider*>(_node);
	if (slider != nullptr) {
		nodeType = N_Slider;
		return nodeType;
	}

	Label* label = dynamic_cast<Label*>(_node);
	if (label != nullptr) {
		nodeType = N_Lable;
		return nodeType;
	}

	ProgressTimer* processTimer = dynamic_cast<ProgressTimer*>(_node);
	if (processTimer != nullptr) {
		nodeType = N_ProgressTimer;
		return nodeType;
	}

	TextField* textField = dynamic_cast<TextField*>(_node);
	if (textField != nullptr) {
		nodeType = N_TextField;
		return nodeType;
	}

	ClippingNode* clippingNode = dynamic_cast<ClippingNode*>(_node);
	if (clippingNode != nullptr) {
		nodeType = N_ClippingNode;
		return nodeType;
	}

	LayerColor* layerColor = dynamic_cast<LayerColor*>(_node);
	if (layerColor != nullptr) {
		nodeType = N_LayerColor;
		return nodeType;
	}

	LabelBMFont* layerBMFont = dynamic_cast<LabelBMFont*>(_node);
	if (layerBMFont != nullptr) {
		nodeType = N_LableBMFont;
		return nodeType;
	}

	Image* image = dynamic_cast<Image*>(_node);
	if (image != nullptr) {
		nodeType = N_Image;
		return nodeType;
	}

	Scale9Sprite* scale9Sprite = dynamic_cast<Scale9Sprite*>(_node);
	if (scale9Sprite != nullptr) {
		nodeType = N_Scale9Sprite;
		return nodeType;
	}

	Button* button = dynamic_cast<Button*>(_node);
	if (button != nullptr) {
		nodeType = N_Button;
		return nodeType;
	}

	nodeType = N_Unknown;
	return nodeType;
}

string NeteaseNode::getNodeTypeStr(int nodeType) 
{
	switch (nodeType) {
	case -1:
		return "Unknown";
	case 1:
		return "Layer";
	case 2:
		return "MenuItem";
	case 3:
		return "Scene";
	case 4:
		return "AtlasNode";
	case 5:
		return "LabelTTF";
	case 6:
		return "SpriteBatchNode";
	case 7:
		return "Sprite";
	case 8:
		return "Guide";
	case 9:
		return "GuideTouchNode";
	case 10:
		return "Slider";
	case 11:
		return "Label";
	case 12:
		return "ProgressTimer";
	case 13:
		return "ClippingNode";
	case 14:
		return "TextField";
	case 15:
		return "LayerColor";
	case 16:
		return "LableBMFont";
	case 17:
		return "Image";
	case 18:
		return "Scale9Sprite";
	case 19:
		return "Button";

	default:
		return "Control";
	}
}