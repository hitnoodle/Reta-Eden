using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainSceneController : MonoBehaviour 
{
	public const int MAX_PLAYER_DIAMOND = 99;
	public const int DIAMOND_TYPES = 4;
	public const int MAX_DIAMOND_SPAWNED = 20;

	public const int MIN_DIAMOND_SPAWNER_TIME = 3;
	public const int MAX_DIAMOND_SPAWNER_TIME = 12;

	private static MainSceneController s_Instance;

	//Building information
	public List<Building> Buildings;

	//Mukyas information
	private List<Mukya> Mukyas;
	private Mukya _SelectedMukya = null;

	//Diamond information
	private List<Diamond> Diamonds;
	private List<Diamond> DiamondsActive;

	//Player information
	private int _Money;
	public int Money
	{
		get { return _Money; }
		set
		{
			_Money = value;
			if (_Money < 0) _Money = 0;

			HUD.SetMoney(_Money);
		}
	}

	private int[] _Diamonds;

	//Main Camera
	private Camera _Camera;
	private CameraDrag _CameraDrag;

	// Ongoing Mukya to its building
	private Hashtable _OngoingResidents;

	// Use this for initialization
	void Awake() 
	{
		s_Instance = this;

		//Create arrays
		Mukyas = new List<Mukya>();

		Diamonds = new List<Diamond>();
		DiamondsActive = new List<Diamond>();

		_Diamonds = new int[DIAMOND_TYPES];
		_OngoingResidents = new Hashtable();

		//Set camera
		_Camera = Camera.main;
		_CameraDrag = _Camera.GetComponent<CameraDrag>();

		//Load scene from data
		CreateScene();

		//Default value
		Money = 100000;
		for(int i=0;i<DIAMOND_TYPES;i++) SetDiamond(i, 9);

		//Spawn diamonds
		StartCoroutine(SpawnDiamonds());
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

		//CreateDiamonds
		GameObject diamondsObject = Utilities.CreateGameObject("Diamonds", new Vector3(0, Diamond.START_Y, -3), null);
		Transform diamondsTransform = diamondsObject.transform;

		for(int i=0;i<DIAMOND_TYPES;i++)
		{
			int num = Diamond.DIAMOND_SPAWN_RATES[i] * MAX_DIAMOND_SPAWNED / 100;
			Diamond.DiamondType type = (Diamond.DiamondType)i;
			string type_string = type.ToString();

			for(int j=0;j<num;j++)
			{
				GameObject diamondObject = (GameObject)Instantiate(Resources.Load("Prefabs/Diamond_" + type_string));

				Diamond diamond = diamondObject.GetComponent<Diamond>();
				Diamonds.Add(diamond);

				Transform diamondTransform = diamondObject.transform;
				diamondTransform.name = "Diamond" + type_string;
				diamondTransform.parent = diamondsTransform;

				diamondObject.SetActive(false);
			}
		}

		//Shuffle diamond arrays first
		System.Random r = new System.Random();
		int n = Diamonds.Count;  
		while (n > 1) {  
			n--;  
			int k = r.Next(n + 1);  
			Diamond value = Diamonds[k];  
			Diamonds[k] = Diamonds[n];  
			Diamonds[n] = value;  
		}  
	}

	IEnumerator SpawnDiamonds()
	{
		int wait = Random.Range(MIN_DIAMOND_SPAWNER_TIME, MAX_DIAMOND_SPAWNER_TIME);
		wait = 0;

		while(true)
		{
			yield return new WaitForSeconds(wait);

			//Spawn diamonds at buildings
			foreach(Building b in Buildings)
			{
				if (b._Type == Building.BuildingType.Bar ||
				    b._Type == Building.BuildingType.House ||
				    b._Type == Building.BuildingType.OuterWorld ||
				    b._Type == Building.BuildingType.Shop)
				{
					if (b.TotalResidents() > 0)
					{
						//Decrease timer according to building residents
						int times = MAX_DIAMOND_SPAWNER_TIME - MIN_DIAMOND_SPAWNER_TIME / Building.MAX_RESIDENT;
						int dec_timer = times * b.TotalResidents();
						int spawn = Random.Range(MIN_DIAMOND_SPAWNER_TIME, MAX_DIAMOND_SPAWNER_TIME - dec_timer);

						StartCoroutine(SpawnDiamond(b, spawn));
					}
				}
			}

			wait = Random.Range(MIN_DIAMOND_SPAWNER_TIME, MAX_DIAMOND_SPAWNER_TIME);
		}
	}

	IEnumerator SpawnDiamond(Building b, int delay)
	{
		yield return new WaitForSeconds(delay);

		//Hack when pause
		while (Utilities.IS_PAUSED)
			yield return new WaitForSeconds(delay);

		if (DiamondsActive.Count < MAX_DIAMOND_SPAWNED)
		{
			Diamond d = Diamonds[0];
			Diamonds.RemoveAt(0);

			d.Spawn(b.Position());
			d.OnSpawnDone += UnSpawnDiamond;

			DiamondsActive.Add(d);
		}
	}

	void UnSpawnDiamond(Diamond d)
	{
		d.OnSpawnDone -= UnSpawnDiamond;

		d.gameObject.SetActive(false);

		DiamondsActive.Remove(d);
		Diamonds.Add(d);
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

			Diamond touchedDiamond = GetSelectedDiamond(pos2D);
			if (touchedDiamond != null)
			{
				int index = (int)touchedDiamond._Type;
				SetDiamond(index, _Diamonds[index] + 1);
				UnSpawnDiamond(touchedDiamond);

				return;
			}

			//Touching mukyas?
			Mukya touchedMukya = GetSelectedMukya(pos2D);

			//Check other
			if (touchedMukya == null)
			{
				//Touching building?
				Building touchedBuilding = GetSelectedBuilding(pos);
				if (touchedBuilding != null && HUD.CanIdleClick())
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

	private void SetDiamond(int index, int value)
	{
		int newValue = value;
		if (newValue < 0) newValue = 0;
		else if (newValue > MAX_PLAYER_DIAMOND) newValue = MAX_PLAYER_DIAMOND;

		_Diamonds[index] = newValue;
		HUD.SetDiamond(index, newValue);
	}

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

	private void AddOngoingResident(Mukya mukya)
	{
		if (mukya == _SelectedMukya)
		{
			_SelectedMukya = null;
			HUD.HideMukyaInformation();
		}
			
		mukya.OnMoveDone -= AddOngoingResident;

		Building building = (Building)_OngoingResidents[mukya];

		if (building.CanResidenGo()) 
			building.AddResident(mukya);
		else 
			mukya.Idle(mukya);

		_OngoingResidents.Remove(mukya);
	}

	private Diamond GetSelectedDiamond(Vector2 pos)
	{
		foreach(Diamond diamond in DiamondsActive)
		{
			if (diamond.IsContainingPosition(pos))
				return diamond;
		}
		
		return null;
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

	public static void ExchangeMoneyWithCrystal()
	{
		if (s_Instance == null) return;

		int oldMoney = s_Instance.Money;
		for(int i=0;i<DIAMOND_TYPES;i++)
		{
			oldMoney += s_Instance._Diamonds[i] * Diamond.DIAMOND_PRICE[i];
			s_Instance.SetDiamond(i,0);
		}

		s_Instance.Money = oldMoney;
	}

	public static void DecreaseMoney(int money)
	{
		if (s_Instance == null) return;

		int oldMoney = s_Instance.Money - money;
		s_Instance.Money = oldMoney;
	}
}
