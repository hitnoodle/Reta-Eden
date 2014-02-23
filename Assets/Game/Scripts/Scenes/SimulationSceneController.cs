using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetaClient;

//Actions:
// Tutorial, Buy New Building, Exchange Crystal, Share Progress
// Request Resident, Moving Resident in and out
public class SimulationSceneController : MonoBehaviour 
{
	int level = 0;
	string player = "Player3";

	// Use this for initialization
	void Start () 
	{
		Reta.Instance.SetApplicationVersion("0.1");
		Reta.Instance.SetUserID(player);
		Reta.Instance.SetDebugMode(true);
		//Reta.Instance.Disable();

		//StartCoroutine(Tutorial());
		StartCoroutine(Game());
	}

	IEnumerator Tutorial()
	{
		yield return new WaitForSeconds(1);

		Reta.Instance.Record("Tutorial Duration",true);

		float wait = Random.Range(4f, 20f);
		yield return new WaitForSeconds(wait);

		Reta.Instance.EndTimedRecord("Tutorial Duration");
		
		List<Parameter> parameterLevel = new List<Parameter>();
		parameterLevel.Add(new Parameter("Level", "1"));
		
		Reta.Instance.Record("Level Duration", parameterLevel, true);
		
		List<Parameter> parameterProgression = new List<Parameter>();
		parameterProgression.Add(new Parameter("Increase", "5"));
		
		Reta.Instance.Record("Game Progression", parameterProgression); 

		yield return new WaitForSeconds(3);

		StartCoroutine(Game());
	}

	IEnumerator Game()
	{
		while(true)
		{
			int mukya = Random.Range(1, 5);
			for(int i=0;i<mukya;i++)
			{
				List<Parameter> parameters = new List<Parameter>();
				parameters.Add(new Parameter("Feature", "Moving Resident In"));
				
				Reta.Instance.Record("Game Feature Consumed", parameters);

				float wait = Random.Range(1f, 3f);
				yield return new WaitForSeconds(wait);
			}

			int crystal = Random.Range(1, 5);
			for(int i=0;i<crystal;i++)
			{
				List<Parameter> parameters = new List<Parameter>();
				parameters.Add(new Parameter("Feature", "Touching Crystal"));
				
				Reta.Instance.Record("Game Feature Consumed", parameters);
				
				float wait = Random.Range(1f, 4f);
				yield return new WaitForSeconds(wait);
			}

			for(int i=0;i<mukya;i++)
			{
				List<Parameter> parameters = new List<Parameter>();
				parameters.Add(new Parameter("Feature", "Moving Resident Out"));
				
				Reta.Instance.Record("Game Feature Consumed", parameters);
				
				float wait = Random.Range(1f, 3f);
				yield return new WaitForSeconds(wait);
			}

			int percent = Random.Range(0,100);
			if (percent >= 0 && percent < 45)
			{
				List<Parameter> parameters = new List<Parameter>();
				parameters.Add(new Parameter("Feature", "Exchanging Crystal"));
				
				Reta.Instance.Record("Game Feature Consumed", parameters);
			}
			else if (percent >= 45 && percent < 70)
			{
				List<Parameter> parameters = new List<Parameter>();
				parameters.Add(new Parameter("Feature", "Requesting Resident"));
				
				Reta.Instance.Record("Game Feature Consumed", parameters);
			}
			else if (percent >= 70)
			{
				Reta.Instance.EndTimedRecord("Level Duration");
				
				List<Parameter> parameterLevel = new List<Parameter>();
				parameterLevel.Add(new Parameter("Level", level.ToString()));
				
				Reta.Instance.Record("Level Duration", parameterLevel, true);
				
				List<Parameter> parameterProgression = new List<Parameter>();
				parameterProgression.Add(new Parameter("Increase", "5"));
				
				Reta.Instance.Record("Game Progression", parameterProgression); 

				List<Parameter> parameterFeature = new List<Parameter>();
				parameterFeature.Add(new Parameter("Feature", "Building New Building"));
				
				Reta.Instance.Record("Game Feature Consumed", parameterFeature);

				level++;
				Debug.Log(player + " " + level);
			}

			float waitagain = Random.Range(3f, 8f);
			yield return new WaitForSeconds(waitagain);

			percent = Random.Range(0,100);
			if (percent > 80)
			{
				List<Parameter> parameters = new List<Parameter>();
				parameters.Add(new Parameter("Feature", "Sharing Progress"));
				
				Reta.Instance.Record("Social Feature Consumed", parameters);
			}

			float loop = Random.Range(5f, 10f);
			yield return new WaitForSeconds(loop);
		}
	}
}
