using UnityEngine;
using System.Collections;

public class MainSceneController : MonoBehaviour 
{
	public bool _Create = false;

	// Use this for initialization
	void Awake() 
	{
		if (_Create)
			CreateScene();
	}

	void CreateScene()
	{
		//Create world
		GameObject worldObject = Utilities.CreateGameObject("World");
		World world = worldObject.AddComponent<World>();

		//Randomize season
		int season = Random.Range(0, 3);
		world.Season = (WorldSeason)season;
		
		//Load world data
		world.Size = 3;

		//Set camera drag
		CameraDrag drag = GetComponent<CameraDrag>();
		if (drag != null) 
		{
			float offset = (float)Screen.width * 768f / (float)Screen.height;
			drag.MaxX = world.Width - offset;
		}

		//Create mukyas
		GameObject mukyasObject = Utilities.CreateGameObject("Mukyas", new Vector3(0, Mukya.START_Y, -1), null);
		Transform mukyasTransform = mukyasObject.transform;

		Mukya.START_X = 25f;
		Mukya.END_X = world.Width - 25f;

		//Load mukyas data
		foreach(Mukya.MukyaRace race in System.Enum.GetValues(typeof(Mukya.MukyaRace)))
		{
			if (race != Mukya.MukyaRace.None)
			{
				string raceString = race.ToString();
				
				GameObject mukyaObject = (GameObject)Instantiate(Resources.Load("Prefabs/Mukya_" + raceString));
				Mukya mukya = mukyaObject.GetComponent<Mukya>();

				Transform mukyaTransform = mukyaObject.transform;

				mukyaTransform.name = mukya._Name;
				mukyaTransform.parent = mukyasTransform;
				
				float x = Random.Range(Mukya.START_X, Mukya.END_X);
				mukyaTransform.position = new Vector3(x, mukyasTransform.position.y, mukyaTransform.position.z);
			}
		}
	}
	
	// Update is called once per frame
	void Update() 
	{

	}
}
