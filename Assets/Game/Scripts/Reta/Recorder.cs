using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RetaClient 
{
	/* Module for managing recorded data 
		- Nice to have: Save and load to local data
	 */
	public class Recorder 
	{
		public List<EventDatum> _EventData;
		public List<TimedEventDatum> _TimedEventData;

		public Recorder() 
		{
			_EventData = new List<EventDatum>();
			_TimedEventData = new List<TimedEventDatum>();
		}

		#region Serializer

		void Save()
		{
			XmlManager.SaveInstanceAsXml("recorder", typeof(Recorder), this);
		}

		void Load()
		{

		}

		#endregion

		#region Getter

		//Will return the first event datum in queue
		public EventDatum CurrentEventDatum 
		{
			get 
			{ 
				if (_EventData.Count > 0)
					return _EventData[0]; 
				else 
					return null;
			}
		}

		//Will return the first finished found timed event
		public TimedEventDatum FinishedTimedEvent
		{
			get
			{
				foreach(TimedEventDatum datum in _TimedEventData)
				{
					if (datum.IsFinished)
						return datum;
				}
				
				return null;
			}
		}

		#endregion

		#region Insertion

		public void AddEvent(string eventName)
		{
			EventDatum datum = new EventDatum(eventName);
			_EventData.Add(datum);

			Save();
		}

		public void AddEvent(string eventName, List<Parameter> parameters)
		{
			EventDatum datum = new EventDatum(eventName, parameters);
			_EventData.Add(datum);

			Save();
		}

		public void AddTimedEvent(string eventName)
		{
			TimedEventDatum datum = new TimedEventDatum(eventName);
			_TimedEventData.Add(datum);

			Save();
		}

		public void AddTimedEvent(string eventName, List<Parameter> parameters)
		{
			TimedEventDatum datum = new TimedEventDatum(eventName, parameters);
			_TimedEventData.Add(datum);

			Save();
		}

		#endregion

		#region Edit

		public bool EndTimedEvent(string eventName)
		{
			//Search for the record indicating event beginning
			foreach(TimedEventDatum datum in _TimedEventData)
			{
				if (datum.Name == eventName)
				{
					//Update duration
					datum.EndEvent();

					Save();

					return true;
				}
			}

			return false;
		}

		public bool EndTimedEvent(string eventName, List<Parameter> parameters)
		{
			//Search for the record indicating event beginning
			foreach(TimedEventDatum datum in _TimedEventData)
			{
				if (datum.Name == eventName)
				{
					//Update duration
					datum.EndEvent(parameters);

					Save();

					return true;
				}
			}
			
			return false;
		}

		#endregion

		#region Delete

		public void DequeueEvent()
		{
			if (_EventData.Count > 0)
			{
				_EventData.RemoveAt(0);
				Save();
			}
		}

		public void DeleteTimedEvent(TimedEventDatum datum)
		{
			_TimedEventData.Remove(datum);
			Save();
		}

		#endregion
	}
}
