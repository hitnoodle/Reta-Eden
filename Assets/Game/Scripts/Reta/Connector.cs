using UnityEngine;
using System.Collections;

namespace RetaClient
{
	/* Modules for managing connection I.E: sending/receiving data 
		TODO: Testing, Secure channel (HTTPS)
	 */
	public class Connector : MonoBehaviour
	{
		//Server url
		//protected const string _URL = "http://localhost:8080/connector";
		protected const string _URL = "http://retaserver.appspot.com/connector";

		protected string _ID = "";
		public string ID
		{
			set { _ID = value; }
		}

		protected string _AppVersion = "UNDEFINED";
		public string AppVersion
		{
			set { _AppVersion = value; }
		}

		protected bool _UsingSecureChannel = false;
		public bool UsingSecureChannel
		{
			set { _UsingSecureChannel = value; }
		}

		//Delegates
		public delegate void OnSendingSucceed(string result);
		public OnSendingSucceed onSendingSucceed = null;

		public delegate void OnSendingFailed(string error);
		public OnSendingFailed onSendingFailed = null;

		protected void Awake()
		{
			if (_ID == "")
				_ID = SystemInfo.deviceUniqueIdentifier;
		}

		//Data is sent using HTTP GET
		public void SendData(string data)
		{
			WWWForm formData = new WWWForm();
			formData.AddField("userid", _ID);
			formData.AddField("appversion", _AppVersion);
			formData.AddField("data", data);

			if (Reta.DEBUG_ENABLED && Reta.Instance.onDebugLog != null)
				Reta.Instance.onDebugLog("[Reta] Sending Data " + data + " to " + _ID);

			//Create URL
			StartCoroutine(SendingData(formData));
		}

		public IEnumerator SendingData(WWWForm data)
		{
			WWW web = new WWW(_URL, data);

			yield return web;

			if (!string.IsNullOrEmpty(web.error))
			{
				if (onSendingFailed != null) onSendingFailed(web.error);
			}
			else
			{
				if (onSendingSucceed != null) onSendingSucceed(web.text);
			}

			yield return null;
		}
	}
}
