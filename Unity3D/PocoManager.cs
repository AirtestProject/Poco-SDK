using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System;
using MiniJSON;
using TcpServer;
using Poco;

public class PocoManager : MonoBehaviour
{
	private bool mRunning;
	public AsyncTcpServer server = null;
	private RPCParser rpc = null;
	private SimpleProtocolFilter prot = null;
	private UnityDumper dumper = new UnityDumper ();
	private List<TcpClientState> inbox = new List<TcpClientState> ();
	private object Lock = new object ();

	void Awake ()
	{
		prot = new SimpleProtocolFilter ();
		rpc = new RPCParser ();
		rpc.addRpcMethod ("Add", Add);
		rpc.addRpcMethod ("Screen", Screen);
		rpc.addRpcMethod ("Dump", Dump);

		mRunning = true;

		server = new AsyncTcpServer (5001);
		server.Encoding = Encoding.UTF8;
		server.DatagramReceived += 
			new EventHandler<TcpDatagramReceivedEventArgs<byte[]>> (server_Received);
		server.Start ();
		Debug.Log ("Tcp server started");
	}

	private void server_Received (object sender, TcpDatagramReceivedEventArgs<byte[]> e)
	{
		Debug.Log (string.Format ("Client : {0} --> {1}", 
			e.Client.TcpClient.Client.RemoteEndPoint.ToString (), e.Datagram.Length));
		lock (Lock) {
			inbox.Add (e.Client);
		}
	}

	static object Add (List<object> param)
	{
		int first = Convert.ToInt32 (param [0]);
		int second = Convert.ToInt32 (param [1]);
		return first + second;
	}

	private object Dump (List<object> param)
	{
		return dumper.dumpHierarchy ();
	}

	static object Screen (List<object> param)
	{
		Application.CaptureScreenshot ("~/Desktop/screen.png");
		return true;
	}

	public void stopListening ()
	{
		mRunning = false;
		server.Stop ();
	}

	void Update ()
	{
		List<TcpClientState> toProcess;
		lock (Lock) {
			toProcess = new List<TcpClientState> (inbox);
			inbox.Clear ();
		}
		if (toProcess != null) {
			foreach (TcpClientState client in toProcess) {
				List<string> msgs = client.Prot.swap_msgs ();
				msgs.ForEach (delegate(string msg) {
					Debug.Log (msg);
					string response = rpc.HandleMessage (msg);
					Debug.Log (response);
					byte[] bytes = prot.pack (response);
					server.Send (client.TcpClient, bytes);
				});
			}
		}
	}

	void OnApplicationQuit ()
	{ // stop listening thread
		stopListening ();
	}
}


public class RPCParser
{
	public delegate object RpcMethod (List<object>param);

	protected Dictionary<string, RpcMethod> RPCHandler = new Dictionary<string, RpcMethod> ();

	public string HandleMessage (string json)
	{
		var data = Json.Deserialize (json) as Dictionary<string,object>;
		if (data.ContainsKey ("method")) {
			string method = data ["method"].ToString ();
			Debug.Log ("method:" + method);
			List<object> param = null;
			if (data.ContainsKey ("params")) {
				param = data ["params"] as List<object>;              
			}

			Debug.Log ("param:" + param);
	
			object idAction = null;
			if (data.ContainsKey ("id")) {
				// if it have id, it is a request
				idAction = data ["id"];   
			}

			Debug.Log ("idAction:" + idAction);

			string response = null;
			object result = null;
			try {
				result = RPCHandler [method] (param);
			} catch (Exception e) {
				// return error response
				Debug.Log (e);
				response = formatResponseError (idAction, null, e);
				return response;
			}

			// return result response
			response = formatResponse (idAction, result);
			return response;

		} else {
			// do not handle response
			Debug.Log ("ignore message without method");
			return null;
		}
	}

	// Call a method in the server
	public string formatRequest (string method, object idAction, List<object> param = null)
	{
		Dictionary<string,object> data = new Dictionary<string, object> ();
		data ["jsonrpc"] = "2.0";
		data ["method"] = method;
		if (param != null)
			data ["params"] = Json.Serialize (param);
		// if idAction is null, it is a notification
		if (idAction != null)
			data ["id"] = idAction;
		return Json.Serialize (data);
	}

	// Send a response from a request the server made to this client
	public string formatResponse (object idAction, object result)
	{
		Dictionary<string,object> rpc = new Dictionary<string, object> ();
		rpc ["jsonrpc"] = "2.0";
		rpc ["id"] = idAction;
		rpc ["result"] = result;
		return Json.Serialize (rpc);
	}

	// Send a error to the server from a request it made to this client
	public string formatResponseError (object idAction, IDictionary<string,object> data, Exception e)
	{
		Dictionary<string,object> rpc = new Dictionary<string, object> ();
		rpc ["jsonrpc"] = "2.0";
		rpc ["id"] = idAction;

		Dictionary<string, object> errorDefinition = new Dictionary<string, object> ();
		errorDefinition ["code"] = 1;
		errorDefinition ["message"] = e.ToString ();

		if (data != null)
			errorDefinition ["data"] = data;

		rpc ["error"] = errorDefinition;
		return Json.Serialize (rpc);
	}

	public void addRpcMethod (string name, RpcMethod method)
	{
		RPCHandler [name] = method;
	}
}
