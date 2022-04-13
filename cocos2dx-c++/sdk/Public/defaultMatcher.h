#pragma once
#include <iostream>
#include <map>
#include<regex>
#include "sdk/Public/node.h"
#include "sdk/json/document.h"
#include "cocos2d.h"
using namespace std;
using namespace cocos2d;
using namespace rapidjson;

struct comparators {
	string regexStr;
    
};

class IMatcher {

public:
	IMatcher();
	~IMatcher();

	void match(rapidjson::Value& cond, Scene* node) noexcept;
};

class EqualizationComparator {

public:
	EqualizationComparator();
	~EqualizationComparator();

	static bool compare(string l, string r) noexcept;
};

class RegexpComparator {

public:
	RegexpComparator();
	virtual ~RegexpComparator();

	static bool compare(string origin, string pattern);
};

class DefaultMatcher : public IMatcher{
	
public:
	DefaultMatcher();
	~DefaultMatcher();
	static map<string, EqualizationComparator> equalMap;
	static map<string, RegexpComparator> regexMap;
	static bool match(rapidjson::Value& cond, Node* node);

};