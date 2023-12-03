using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class PocoListenerUtils
{
    public static void SubscribePocoListeners(RPCParser rpc, PocoListenersBase listeners)
    {
        var methods = listeners.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance);

        var uniqueListeners = new HashSet<string>();

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<PocoMethodAttribute>();

            if (attribute != null)
            {
                rpc.addListener(listeners, attribute.Name, method);

                if (uniqueListeners.Add(attribute.Name) == false)
                {
                    Debug.LogError($"Attempt to add non-unique Poco listener: `{attribute.Name}`, " +
                                   $"please check attributes of listeners at `{listeners.GetType().Name}`");
                }
            }
        }
    }

    public static object HandleInvocation(
        Dictionary<string, (object instance, MethodInfo method)> listeners,
        Dictionary<string, object> data)
    {
        if (listeners == null)
        {
            throw new ArgumentNullException(
                nameof(listeners),
                "To use `poco.invoke()`, please assign object " +
                $"of class derived from {nameof(PocoListenersBase)} " +
                $"to field at `{nameof(PocoManager)}`");
        }

        var paramsObject = (JObject)data["params"];

        var listener = paramsObject["listener"].ToObject<string>();

        if (listeners.TryGetValue(listener, out var listenerPair) == false)
        {
            throw new NotImplementedException(
                $"Listener method for `{listener}` " +
                $"marked with `{nameof(PocoMethodAttribute)}` was not found " +
                $"at `{listeners.GetType().Name}`");
        }

        var (instance, method) = listenerPair;

        var args = GetInvocationArgs(paramsObject, method);

        var result = method.Invoke(instance, args);

        return result;
    }

    private static object[] GetInvocationArgs(JObject paramsObject, MethodInfo method)
    {
        var parameters = method.GetParameters();

        if (paramsObject.TryGetValue("data", out var data) == false)
        {
            if (parameters.Length > 0)
            {
                throw new ArgumentException(
                    $"Signature mismatch of method `{method}`: " +
                    "expected 0 arguments in listener, " +
                    $"received {parameters.Length} arguments");
            }

            return Array.Empty<object>();
        }

        var args = new List<object>();

        var remainingArgNames = new HashSet<string>(data.ToObject<Dictionary<string, object>>().Keys);

        foreach (var parameter in parameters)
        {
            var parameterName = parameter.Name;

            var argToken = data[parameterName];

            if (argToken == null)
            {
                throw new ArgumentException(
                    $"Signature mismatch of method `{method}`: " +
                    $"excess parameter `{parameterName}` in listener");
            }

            try
            {
                var argValue = argToken.ToObject(parameter.ParameterType);
                args.Add(argValue);
                remainingArgNames.Remove(parameterName);
            }
            catch (JsonReaderException exception)
            {
                throw new ArgumentException(
                    $"Signature mismatch of method `{method}`: " +
                    $"parameter `{parameterName}` type mismatch: " +
                    $"tried to parse received value `{argToken}`, " +
                    $"with type `{parameter.ParameterType.Name}` at listener",
                    exception);
            }
        }

        if (remainingArgNames.Count > 0)
        {
            throw new ArgumentException(
                $"Signature mismatch of method `{method}`: " +
                $"missing parameters in listener: `{string.Join(", ", remainingArgNames)}`");
        }

        return args.ToArray();
    }
}
