using System;
using System.Collections.Generic;
using UnityEngine;
using Utf8Json.Resolvers;

namespace Utf8Json.Formatters
{
      internal class PocoObjectFormatter : IJsonFormatter<object>
    {
        private static ListFormatter<float> listFormater = new ListFormatter<float>();
        private static ListFormatter<string> listStrFormater = new ListFormatter<string>();
        private static ListFormatter<object> listObjectFormater = new ListFormatter<object>();
        private static ArrayFormatter<object> objectArrayFormater = new ArrayFormatter<object>();
        public static DictionaryFormatter<string, float> floatDictFormater = new DictionaryFormatter<string, float>();
        public void Serialize(ref JsonWriter writer, object value, IJsonFormatterResolver formatterResolver)
        {
            switch (value)
            {
                case Dictionary<string, object> dict:
                    PocoResolver.dictFormater.Serialize(ref writer,dict,formatterResolver);
                    break;
                case List<float> list:
                    listFormater.Serialize(ref writer, list, formatterResolver);
                    break;
                case List<string> listStr:
                    listStrFormater.Serialize(ref writer, listStr, formatterResolver);
                    break;
                case string s:
                    NullableStringFormatter.Default.Serialize(ref writer, s, formatterResolver);
                    break;
                case bool b:
                    BooleanFormatter.Default.Serialize(ref writer, b, formatterResolver);
                    break;
                case int i:
                    Int32Formatter.Default.Serialize(ref writer, i, formatterResolver);
                    break;
                case List<object> objList:
                    listObjectFormater.Serialize(ref writer, objList, formatterResolver);
                    break;
                case float f:
                    SingleFormatter.Default.Serialize(ref writer, f, formatterResolver);
                    break;
                case long l:
                    Int64Formatter.Default.Serialize(ref writer, l, formatterResolver);
                    break;
                case null:
                    NullableStringFormatter.Default.Serialize(ref writer, null, formatterResolver);
                    break;
                case float[] fs:
                    SingleArrayFormatter.Default.Serialize(ref writer, fs, formatterResolver);
                    break;
                case Dictionary<string,float> fDict:
                    floatDictFormater.Serialize(ref writer, fDict, formatterResolver);
                    break;
                case object[] objectArray:
                    objectArrayFormater.Serialize(ref writer, objectArray, formatterResolver);
                    break;
                default:
                    Debug.LogError("找不到类型:" + value.GetType());
                    break;
            }
        }

        public object Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            throw new NotImplementedException();
        }
    }
}