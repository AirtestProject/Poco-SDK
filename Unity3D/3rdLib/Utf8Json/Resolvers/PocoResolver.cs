using System;
using System.Collections.Generic;
using UnityEngine;
using Utf8Json.Formatters;

namespace Utf8Json.Resolvers
{
    public class PocoResolver : global::Utf8Json.IJsonFormatterResolver
    {
        public static readonly IJsonFormatterResolver Instance = new PocoResolver();

        public static DictionaryFormatter<string, object> dictFormater = new DictionaryFormatter<string, object>();
        private static readonly Dictionary<Type, object> formatterMap = new Dictionary<Type, object>()
        {
            // Primitive
            {typeof(Dictionary<string, object>), dictFormater },
            {typeof(object), new PocoObjectFormatter()}
        };
        public IJsonFormatter<T> GetFormatter<T>()
        {
            Type t = typeof(T);
            if (formatterMap.TryGetValue(t, out object formater))
            {
                return (IJsonFormatter<T>)formater;
            }
            else
            {
                throw new Exception("找不到类型" + t.Name);
            }
        }
    }

  
}