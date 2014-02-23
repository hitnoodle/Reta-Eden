using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* Retention Analytics - Clients for Unity3D */
namespace RetaClient 
{
	/* Analytics main class 
		TODO: Pause-resume support
	 */
	public class Reta 
	{
		protected const string DATA_SENT_SUCCESS = "DATA_SENT_SUCCESS";

		public static bool DEBUG_ENABLED = false;

		// Singleton for ease of access and managing resource
		protected static Reta _Instance;
		public static Reta Instance 
		{
			get  
			{ 
				if (_Instance == null) 
				{
					//Create for the first time
					_Instance = new Reta();
				}
				
				return _Instance; 
			}
		}		

		//Object which represents the service
		protected GameObject _GameObject;
		protected RetaController _Controller;

		//Components
		protected Recorder _Recorder; 
		protected Connector _Connector;

		//Temp
		TimedEventDatum _TempTimedEventDatum;

		//Log delegate
		public delegate void OnDebugLog(string log);
		public OnDebugLog onDebugLog = null;

		//Disable when developing
		protected bool _Disable = false;

		//Hidden constructor
		protected Reta() 
		{
			_GameObject = new GameObject();
			_GameObject.transform.position = Vector3.zero;
			_GameObject.name = "_Reta";

			//TODO: Check this shit, leak-prone
			GameObject.DontDestroyOnLoad(_GameObject); 

			_Controller = _GameObject.AddComponent<RetaController>();

			_Recorder = (Recorder)XmlManager.LoadInstanceAsXml("recorder", typeof(Recorder));
			if (_Recorder == null) 
				_Recorder = new Recorder();
			else
				ProcessEvents();

			//_Recorder = new Recorder();

			_Connector = _GameObject.AddComponent<Connector>();
		}

		#region Delegates

		protected void EventSendingSucceed(string result)
		{
			_Connector.onSendingSucceed -= EventSendingSucceed;
			_Connector.onSendingFailed -= EventSendingFailed;

			if (DEBUG_ENABLED && onDebugLog != null) 
				onDebugLog("[Reta] Sending event success: " + result);

			//Check whether result is OK
			if (result == DATA_SENT_SUCCESS)
			{
				_Recorder.DequeueEvent();
				
				//Go again
				ProcessEventData();
			}
		}

		protected void EventSendingFailed(string error)
		{
			_Connector.onSendingSucceed -= EventSendingSucceed;
			_Connector.onSendingFailed -= EventSendingFailed;

			if (Reta.DEBUG_ENABLED && onDebugLog != null)
				onDebugLog("[Reta] Sending failed: " + error);
		}

		protected void TimedEventSendingSucceed(string result)
		{
			_Connector.onSendingSucceed -= TimedEventSendingSucceed;
			_Connector.onSendingFailed -= TimedEventSendingFailed;

			if (DEBUG_ENABLED && onDebugLog != null)
				onDebugLog("[Reta] Sending timed event success: " + result);

			//Check whether result is OK
			if (result == DATA_SENT_SUCCESS)
			{
				_Recorder.DeleteTimedEvent(_TempTimedEventDatum);
				
				//Go again
				ProcessTimedEventData();
			}
		}

		protected void TimedEventSendingFailed(string error)
		{
			_Connector.onSendingSucceed -= TimedEventSendingSucceed;
			_Connector.onSendingFailed -= TimedEventSendingFailed;

			if (DEBUG_ENABLED && onDebugLog != null)
				onDebugLog("[Reta] Sending timed event failed: " + error);
		}

		#endregion

		#region Protected Event Processing

		protected void ProcessEvents()
		{
			ProcessEventData();
			ProcessTimedEventData();
		}

		protected void ProcessEventData()
		{
			EventDatum datum = _Recorder.CurrentEventDatum;

			if (datum != null)
			{
				_Connector.onSendingSucceed += EventSendingSucceed;
				_Connector.onSendingFailed += EventSendingFailed;

				if (DEBUG_ENABLED && onDebugLog != null) 
					onDebugLog("[Reta] Processing event datum " + datum.ToString());

				_Connector.SendData(datum.ToString());
			}
		}

		protected void ProcessTimedEventData()
		{
			TimedEventDatum datum = _Recorder.FinishedTimedEvent;

			if (datum != null)
			{
				_TempTimedEventDatum = datum;

				_Connector.onSendingSucceed += TimedEventSendingSucceed;
				_Connector.onSendingFailed += TimedEventSendingFailed;

				if (DEBUG_ENABLED && onDebugLog != null) 
					onDebugLog("[Reta] Processing timed event datum " + datum.ToString());

				_Connector.SendData(datum.ToString());
			}
		}

		#endregion

		#region Exposed API

		public void SetDebugMode(bool debug)
		{
			DEBUG_ENABLED = debug;
			onDebugLog += _Controller.DebugLog;
		}

		public void Disable()
		{
			_Disable = true;
		}

		public void SetApplicationVersion(string version)
		{
			_Connector.AppVersion = version;
		}

		//default is unique device identifier
		public void SetUserID(string id)
		{
			_Connector.ID = id;
		}

		public void EnableSecureConnection()
		{
			_Connector.UsingSecureChannel = true;
		}

		public void Record(string eventName)
		{
			if (_Disable) return;

			_Recorder.AddEvent(eventName);
			ProcessEventData();
		}

		public void Record(string eventName, List<Parameter> parameters)
		{
			if (_Disable) return;

			_Recorder.AddEvent(eventName, parameters);
			ProcessEventData();
		}
		
		public void Record(string eventName, bool isTimed)
		{
			if (_Disable) return;

			if (isTimed)
			{
				_Recorder.AddTimedEvent(eventName);
			}
			else Record(eventName);
		}
		
		public void Record(string eventName, List<Parameter> parameters, bool isTimed)
		{
			if (_Disable) return;

			if (isTimed)
			{
				_Recorder.AddTimedEvent(eventName, parameters);
			}
			else Record(eventName, parameters);
		}

		public void EndTimedRecord(string eventName)
		{
			if (_Disable) return;

			_Recorder.EndTimedEvent(eventName);
			ProcessTimedEventData();
		}

		public void EndTimedRecord(string eventName, List<Parameter> parameters)
		{
			if (_Disable) return;

			_Recorder.EndTimedEvent(eventName, parameters);
			ProcessTimedEventData();
		}

		#endregion
	}
}
