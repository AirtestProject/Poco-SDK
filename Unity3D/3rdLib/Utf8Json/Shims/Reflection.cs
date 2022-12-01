﻿#if !(UNITY_METRO || UNITY_WSA)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace System.Reflection
{
#if !NET_4_6

    internal class TypeInfo
    {
        readonly Type type;

        public TypeInfo(Type type)
        {
            this.type = type;
        }

        public string Name
        {
            get
            {
                return type.Name;
            }
        }

        public TypeAttributes Attributes
        {
            get
            {
                return type.Attributes;
            }
        }

        public bool IsClass
        {
            get
            {
                return type.IsClass;
            }
        }

        public bool IsPublic
        {
            get
            {
                return type.IsPublic;
            }
        }

        public bool IsInterface
        {
            get
            {
                return type.IsInterface;
            }
        }

        public bool IsAbstract
        {
            get
            {
                return type.IsAbstract;
            }
        }

        public bool IsArray
        {
            get
            {
                return type.IsArray;
            }
        }

        public bool IsValueType
        {
            get
            {
                return type.IsValueType;
            }
        }

        public bool IsNestedPublic
        {
            get
            {
                return type.IsNestedPublic;
            }
        }

        public IEnumerable<ConstructorInfo> DeclaredConstructors
        {
            get
            {
                return type.GetConstructors().AsEnumerable();
            }
        }

        public bool IsGenericType
        {
            get
            {
                return type.IsGenericType;
            }
        }

        public Type GetGenericTypeDefinition()
        {
            return type.GetGenericTypeDefinition();
        }

        public Type AsType()
        {
            return type;
        }

        public MethodInfo GetDeclaredMethod(string name)
        {
            return type.GetMethod(name);
        }

        public IEnumerable<MethodInfo> GetDeclaredMethods(string name)
        {
            return type.GetMethods().Where(x => x.Name == name);
        }

        public Type[] GenericTypeArguments
        {
            get
            {
                return type.GetGenericArguments();
            }
        }

        public bool IsEnum
        {
            get
            {
                return type.IsEnum;
            }
        }

        public bool IsConstructedGenericType()
        {
            return type.IsGenericType && !type.IsGenericTypeDefinition;
        }

        public Type[] ImplementedInterfaces
        {
            get
            {
                return type.GetInterfaces();
            }
        }

        public MethodInfo[] GetRuntimeMethods()
        {
            return type.GetMethods();
        }

        public bool IsAssignableFrom(TypeInfo c)
        {
            return type.IsAssignableFrom(c.AsType());
        }

        public PropertyInfo GetDeclaredProperty(string name)
        {
            return type.GetProperty(name);
        }

        public T GetCustomAttribute<T>(bool inherit = true)
            where T : Attribute
        {
            return type.GetCustomAttributes(inherit).OfType<T>().FirstOrDefault();
        }
        public IEnumerable<T> GetCustomAttributes<T>(bool inherit = true)
            where T : Attribute
        {
            return type.GetCustomAttributes(inherit).OfType<T>();
        }
    }

#endif

    internal static class ReflectionExtensions
    {

#if !NET_4_6

        static readonly BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                                           | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.SetField | BindingFlags.SetProperty
                                           | BindingFlags.InvokeMethod;

        public static TypeInfo GetTypeInfo(this Type type)
        {
            return new TypeInfo(type);
        }

        public static TypeInfo CreateTypeInfo(this TypeBuilder type)
        {
            return new TypeInfo(type.CreateType());
        }

        public static MethodInfo GetRuntimeMethod(this Type type, string name, Type[] types)
        {
            return type.GetMethod(name, types); // no use flags.
        }

        public static MethodInfo GetRuntimeMethod(this Type type, string name)
        {
            return type.GetMethod(name); // no use flags.
        }

        public static MethodInfo[] GetRuntimeMethods(this Type type)
        {
            return type.GetMethods(flags);
        }

        public static PropertyInfo GetRuntimeProperty(this Type type, string name)
        {
            return type.GetProperty(name, flags);
        }

        public static PropertyInfo[] GetRuntimeProperties(this Type type)
        {
            return type.GetProperties(flags);
        }

        public static FieldInfo GetRuntimeField(this Type type, string name)
        {
            return type.GetField(name, flags);
        }

        public static FieldInfo[] GetRuntimeFields(this Type type)
        {
            return type.GetFields(flags);
        }

        public static T GetCustomAttribute<T>(this FieldInfo type, bool inherit)
            where T : Attribute
        {
            return type.GetCustomAttributes(inherit).OfType<T>().FirstOrDefault();
        }

        public static T GetCustomAttribute<T>(this PropertyInfo type, bool inherit)
            where T : Attribute
        {
            return type.GetCustomAttributes(inherit).OfType<T>().FirstOrDefault();
        }

        public static T GetCustomAttribute<T>(this ConstructorInfo type, bool inherit)
            where T : Attribute
        {
            return type.GetCustomAttributes(inherit).OfType<T>().FirstOrDefault();
        }

#else

        public static bool IsConstructedGenericType(this TypeInfo type)
        {
            return type.IsConstructedGenericType;
        }

#endif
    }

#if !NET_4_6
    internal static class CustomAttributeExtensions
    {
        public static T GetCustomAttribute<T>(MemberInfo memberInfo, bool inherit)
        {
            return (T)memberInfo.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        }
    }

#endif
}


#endif