using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainSceneController : MonoBehaviour 
{
	//Building information
	public List<Building> Buildings;

	//Mukyas information
	private List<Mukya> Mukyas;
	private Mukya _SelectedMukya = null;

	//Main Camera
	private Camera _Camera;
	private CameraDrag _CameraDrag;

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
		_CameraDrag = _Camera.GetComponent<CameraDrag>();

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
				mukyaTransform.name = "Mukya" + mukya._Race.ToString();
				mukyaTransform.parent = mukyasTransform;
				
				float x = Random.Range(Mukya.START_X, Mukya.END_X);
				mukyaTransform.localPosition = new Vector3(x, 0, 0);
			}
		}
	}
	
	// Update is called once per frame
	void Update() 
	{
		//Pause
		if (Utilities.IS_PAUSED) return;

		//When released
		if (Input.GetMouseButtonUp(0) && !_CameraDrag.IsMoving())
		{
			//Translate to screen space
			Vector3 pos = _Camera.ScreenToWorldPoint(Input.mousePosition);
			Vector2 pos2D = new Vector2(pos.x, pos.y);

			//Touching mukyas?
			Mukya touchedMukya = GetSelectedMukya(pos2D);

			//Check other
			if (touchedMukya == null)
			{
				//Touching building?
				Building touchedBuilding = GetSelectedBuilding(pos);
				if (touchedBuilding != null)
				{
					//Make mukya go to building if selected before
					if (_SelectedMukya != null)
					{
						//Only if it's these buildings of course
						if (touchedBuilding._Type == Building.BuildingType.Bar ||
						    touchedBuilding._Type == Building.BuildingType.House ||
						    touchedBuilding._Type == Building.BuildingType.OuterWorld ||
						    touchedBuilding._Type == Building.BuildingType.Shop)
						{
							//Cancel if already moving
							if (_OngoingResidents.ContainsKey(_SelectedMukya))
							{
								_SelectedMukya.OnMoveDone -= AddOngoingResident;
								_OngoingResidents.Remove(_SelectedMukya);
							}
							
							_SelectedMukya.Move(touchedBuilding.Position());
							_SelectedMukya.OnMoveDone += AddOngoingResident;
							
							_OngoingResidents.Add(_SelectedMukya, touchedBuilding);
						}
						else
						{
							//Enable HUD
							ShowBuildingUI(touchedBuilding);
						}
					}
					else
					{
						//Just touching the building, enable HUD
						ShowBuildingUI(touchedBuilding);
					}
				}
			}

			//Continue
			_SelectedMukya = touchedMukya;

			if (_SelectedMukya != null)
				HUD.ShowMukyaInformation(_SelectedMukya);
			else
				HUD.HideMukyaInformation();
		}
	}

	#region Resident handling

	private void AddOngoingResident(Mukya mukya)
	{
		mukya.OnMoveDone -= AddOngoingResident;

		Building building = (Building)_OngoingResidents[mukya];

		if (building.CanResidenGo()) 
			building.AddResident(mukya);
		else 
			mukya.Idle(mukya);

		_OngoingResidents.Remove(mukya);
	}

	#endregion

	#region Touch handling

	private void ShowBuildingUI(Building building)
	{
		if (building._Type == Building.BuildingType.Bar ||
		    building._Type == Building.BuildingType.House ||
		    building._Type == Building.BuildingType.OuterWorld ||
		    building._Type == Building.BuildingType.Shop)
		{
			HUD.ShowResidentsMenu(building);
		}
		else if (building._Type == Building.BuildingType.CommunicationCenter)
		{
			HUD.ShowCommunicationCenterMenu();
		}
		else if (building._Type == Building.BuildingType.CityHall)
		{
			HUD.ShowCityHallMenu();
		}
		else if (building._Type == Building.BuildingType.None)
		{
			HUD.ShowBuildMenu();
		}
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

	#endregion
}
