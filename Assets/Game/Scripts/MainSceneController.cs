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
		if (world != null)
		{
			//Randomize season
			int season = Random.Range(0, 3);
			world.Season = (WorldSeason)season;

			//Load world data
			world.Size = 3;
		}
	}
	
	// Update is called once per frame
	void Update() 
	{

	}
}
