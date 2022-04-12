
#include "sdk/Public/defaultMatcher.h"

void IMatcher::match(rapidjson::Value& cond, Scene* node) noexcept{
	cout <<"nothing" <<endl;
}

IMatcher::IMatcher()
{
}

IMatcher::~IMatcher()
{
}

EqualizationComparator::EqualizationComparator()
{
}

EqualizationComparator::~EqualizationComparator()
{
}

bool EqualizationComparator::compare(string l, string r) noexcept
{
	return l == r;
}

RegexpComparator::RegexpComparator()
{
}

RegexpComparator::~RegexpComparator()
{
}

bool RegexpComparator::compare(string origin, string pattern)
{
	if (origin == "" || pattern == "")
	    return false;
	auto result = regex_match(origin, regex(pattern));
	return result;
}

DefaultMatcher::DefaultMatcher()
{
	/*equalMap.insert(std::pair<string, EqualizationComparator>("attr=", EqualizationComparator()));
	regexMap.insert({ "attr.*=", RegexpComparator() });*/
}

DefaultMatcher::~DefaultMatcher()
{

}

bool DefaultMatcher::match(rapidjson::Value& cond, Node* node)
{	
	//先取第一个元素判断
	//rapidjson::Value& op = cond[0];
	//const char* opStr = op.GetString();
	//if (strcmp(opStr, "and") == 0) {
	//	//判断第二部分是数组还是string
	//	rapidjson::Value& args = cond[1];
	//	rapidjson::Type vType = args.GetType();
	//	if (vType == rapidjson::Type::kArrayType) {
	//		if (!DefaultMatcher::match(args, node))
	//			return false;
	//		return true;
	//	}
	//}
	//if (strcmp(opStr, "or") == 0) {
	//	//判断第二部分是数组还是string
	//	rapidjson::Value& args = cond[1];
	//	rapidjson::Type vType = args.GetType();
	//	if (vType == rapidjson::Type::kArrayType) {
	//		if (DefaultMatcher::match(args, node))
	//			return true;
	//		return false;
	//	}
	//}
	//rapidjson::Value& args = cond[1];
	//if (comparators.find(opStr) != comparators.end()) {
	//	rapidjson::Value& args0 = args[0];
	//	rapidjson::Value& args1 = args[1];
	//	//string attribute = args0.GetString();
	//	string value = args1.GetString();
	//	string targetValue = node->getName();
	//	return EqualizationComparator().compare(targetValue, value);
	//}
	return false;
}
