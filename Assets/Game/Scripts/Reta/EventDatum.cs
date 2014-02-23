using System;
using System.Collections;
using System.Collections.Generic;

using MiniJSON;

namespace RetaClient
{
	/* Simple key value pair for holding parameter data */
	public class Parameter 
	{
		public string _Key;
		public string Key 
		{
			get { return _Key; } 
		}
		
		public string _Value;
		public string Value 
		{
			set { _Value = value; }
			get { return _Value;  }
		}
		
		public Parameter()
		{
			_Key = "";
			_Value = "";
		}
		
		public Parameter(string key, string value)
		{
			_Key = key;
			_Value = value;
		}

		//JSON formatted string
		public override string ToString ()
		{
			Dictionary<string, string> dict = new Dictionary<string, string>();
			dict.Add(_Key, _Value);

			return Json.Serialize(dict);
		}
	}
	
	/* Class for holding event datum */
	public class EventDatum
	{
		public string _Name;
		public string Name
		{
			get { return _Name; } 
		}

		public List<Parameter> _Parameters;
		public DateTime _Time;
		
		public EventDatum()
		{
			_Name = "";
			_Parameters = null;
			_Time = DateTime.Now.ToUniversalTime();
		}

		public EventDatum(string name)
		{
			_Name = name;
			_Parameters = null;
			_Time = DateTime.Now.ToUniversalTime();
		}

		public EventDatum(string name, List<Parameter> parameters)
		{
			_Name = name;
			_Parameters = parameters;
			_Time = DateTime.Now.ToUniversalTime();
		}

		//JSON formatted string
		public override string ToString()
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();
			dict.Add("Name",_Name);

			if (_Parameters != null) {
				List<string> paramStrings = new List<string>();
				foreach(Parameter param in _Parameters) {
					if (param != null) 
						paramStrings.Add(param.ToString());
				}

				dict.Add("Parameters", paramStrings);
			}

			dict.Add("Time", _Time.ToString());

			return Json.Serialize(dict);
		}
	}

	/* Class for holding timed event datum */
	public class TimedEventDatum : EventDatum
	{
		public DateTime _TimeEnded;
		public TimeSpan _Duration;

		public bool _IsFinished;
		public bool IsFinished 
		{
			get { return _IsFinished; }
		}

		public TimedEventDatum() : base()
		{
			_TimeEnded = _Time;
			_Duration = TimeSpan.Zero;
			_IsFinished = false;
		}

		public TimedEventDatum(string name) : base(name)
		{
			_TimeEnded = _Time;
			_Duration = TimeSpan.Zero;
			_IsFinished = false;
		}

		public TimedEventDatum(string name, List<Parameter> parameters) : base(name, parameters)
		{
			_TimeEnded = _Time;
			_Duration = TimeSpan.Zero;
			_IsFinished = false;
		}

		public void EndEvent()
		{
			_TimeEnded = DateTime.Now.ToUniversalTime();
			_Duration = _TimeEnded - _Time;
			_IsFinished = true;
		}

		public void EndEvent(List<Parameter> parameters)
		{
			_TimeEnded = DateTime.Now.ToUniversalTime();
			_Duration = _TimeEnded - _Time;
			_IsFinished = true;

			if (_Parameters != null)
			{
				int len = parameters.Count;
				List<int> newparams = new List<int>();

				//Update old parameter value
				for (int i=0;i<len;i++)
				{
					bool found = false;
					foreach(Parameter param in _Parameters)
					{
						if (param.Key == parameters[i].Key)
						{
							param.Value = parameters[i].Value;
							found = true;

							break;
						}
					}

					if (!found) newparams.Add(i);
				}

				//Add new parameter
				foreach(int i in newparams)
					_Parameters.Add(parameters[i]);
			}
		}

		//JSON formatted string
		public override string ToString()
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();
			dict.Add("Name",_Name);

			if (_Parameters != null) {
				List<string> paramStrings = new List<string>();
				foreach(Parameter param in _Parameters) {
					if (param != null) 
						paramStrings.Add(param.ToString());
				}

				dict.Add("Parameters", paramStrings);
			}

			dict.Add("Time", _Time.ToString());

			if (_IsFinished)
				_Duration = _TimeEnded - _Time;

			string ms = (int)_Duration.TotalMilliseconds + "ms";
			dict.Add("Duration", ms);
			
			return Json.Serialize(dict);
		}
	}
}
