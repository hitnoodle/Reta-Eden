using UnityEngine;
using System.Collections;

namespace RetaClient 
{
	/* Manage lifecycle of Reta */
	public class RetaController : MonoBehaviour 
	{
		bool isQuitting = false;

		void Start()
		{
			Reta.Instance.Record("[Reta] Session Started");
			Reta.Instance.Record("[Reta] Session Duration", true);
		}

		void OnApplicationPause(bool isPaused)
		{
			if (isPaused)
			{
				Reta.Instance.EndTimedRecord("[Reta] Session Duration");
				Reta.Instance.Record("[Reta] Session Suspended");
			}
			else 
			{
				Reta.Instance.Record("[Reta] Session Resumed");
				Reta.Instance.Record("[Reta] Session Duration", true);
			}	
		}

		void OnApplicationQuit()
		{
			if (!isQuitting)
			{
				Reta.Instance.EndTimedRecord("[Reta] Session Duration");
				Reta.Instance.Record("[Reta] Session Ended");

				Application.CancelQuit();
				StartCoroutine(DelayedQuit());
			}
		}

		IEnumerator DelayedQuit()
		{
			yield return new WaitForSeconds(1);

			isQuitting = true;
			Application.Quit();
		}

		public void DebugLog(string log)
		{
			Debug.Log(log);
		}
	}
}