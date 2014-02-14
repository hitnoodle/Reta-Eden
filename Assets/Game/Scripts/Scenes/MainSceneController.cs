using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainSceneController : MonoBehaviour 
{
	//Building information
	public List<Building> Buildings;
	private Building _SelectedBuilding = null;

	//Mukyas information
	private List<Mukya> Mukyas;
	private Mukya _SelectedMukya = null;

	//Main Camera
	private Camera _Camera;

	// Ongoing Mukya to its building
	private Hashtable _OngoingResidents;

	// Use this for initialization
	void Awake() 
	{
		//Create arrays
		Mukyas = new List<Mukya>();
		_OngoingResidents = new Hashtable();

		//Set camera
		_Camera = Camera.main;

		//Load scene from data
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
		GameObject mukyasObject = Utilities.CreateGameObject("Mukyas", new Vector3(0, Mukya.START_Y, -2), null);
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
				Mukyas.Add(mukya);

				Transform mukyaTransform = mukyaObject.transform;
				mukyaTransform.name = mukya._Name;
				mukyaTransform.parent = mukyasTransform;
				
				float x = Random.Range(Mukya.START_X, Mukya.END_X);
				mukyaTransform.localPosition = new Vector3(x, 0, 0);
			}
		}
	}
	
	// Update is called once per frame
	void Update() 
	{
		if (Input.GetMouseButtonUp(0))
		{
			Vector3 pos = _Camera.ScreenToWorldPoint(Input.mousePosition);
			Vector2 pos2D = new Vector2(pos.x, pos.y);

			if (_SelectedMukya != null)
			{
				_SelectedBuilding = GetSelectedBuilding(pos);
				if (_SelectedBuilding != null)
				{
					if (_OngoingResidents.ContainsKey(_SelectedMukya))
					{
						_SelectedMukya.OnMoveDone -= AddOngoingResident;
						_OngoingResidents.Remove(_SelectedMukya);
					}

					_SelectedMukya.Move(_SelectedBuilding.Position());
					_SelectedMukya.OnMoveDone += AddOngoingResident;

					_OngoingResidents.Add(_SelectedMukya, _SelectedBuilding);
				}
			}

			_SelectedMukya = GetSelectedMukya(pos2D);
		}

		if (_SelectedMukya != null)
			Debug.Log(_SelectedMukya._Name + " " + _SelectedMukya.EnergyPercentage() + " " + _SelectedMukya.SocialPercentage() + " " + _SelectedMukya.WorkPercentage());
	}

	private void AddOngoingResident(Mukya mukya)
	{
		mukya.OnMoveDone -= AddOngoingResident;

		Building building = (Building)_OngoingResidents[mukya];
		building.AddResident(mukya);

		_OngoingResidents.Remove(mukya);
	}

	private Building GetSelectedBuilding(Vector2 pos)
	{
		foreach(Building building in Buildings)
		{
			if (building.IsContainingPosition(pos))
				return building;
		}
		
		return null;
	}

	private Mukya GetSelectedMukya(Vector2 pos)
	{
		foreach(Mukya mukya in Mukyas)
		{
			if (mukya.IsContainingPosition(pos))
				return mukya;
		}

		return null;
	}
}
