using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetaClient;

public class MainSceneController : MonoBehaviour 
{
	public const int MAX_PLAYER_DIAMOND = 99;
	public const int DIAMOND_TYPES = 4;
	public const int MAX_DIAMOND_SPAWNED = 20;

	public const int MIN_DIAMOND_SPAWNER_TIME = 3;
	public const int MAX_DIAMOND_SPAWNER_TIME = 12;

	public const int MAX_MUKYA_PER_LEVEL = 2;

	private static MainSceneController s_Instance;

	//World information
	private World _World;

	//Building information
	private List<Building> _Buildings;
	private Building _BuildingNone;
	private Building _BuildingCityHall;

	private Transform _BuildingsTransform;

	//Mukyas information
	private List<Mukya> _Mukyas;
	private Mukya _SelectedMukya;

	private Transform _MukyasTransform;

	//Diamond information
	private List<Diamond> _Diamonds;
	private List<Diamond> _DiamondsActive;

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

	private int[] _PlayerDiamonds;

	//Main Camera
	private Camera _Camera;
	private CameraDrag _CameraDrag;

	// Ongoing Mukya to its building
	private Hashtable _OngoingResidents;

	private Vector2 _TouchOrigin;
	private float _TouchTime;
	private float _TouchDetectTime = 0.08f;
	private bool _IsTouched = false;
	private bool _IsTouchedMove = false;

	// Use this for initialization
	void Awake() 
	{
		ProfileManager.Instance.Load();

		s_Instance = this;

		//Create arrays
		_Buildings = new List<Building>();

		_Mukyas = new List<Mukya>();

		_Diamonds = new List<Diamond>();
		_DiamondsActive = new List<Diamond>();

		_PlayerDiamonds = new int[DIAMOND_TYPES];
		_OngoingResidents = new Hashtable();

		//Set camera
		_Camera = Camera.main;
		_CameraDrag = _Camera.GetComponent<CameraDrag>();

		//Load scene from data
		CreateScene();

		//Default value
		Money = ProfileManager.Instance.Money;
		for(int i=0;i<DIAMOND_TYPES;i++) 
			SetDiamond(i, ProfileManager.Instance.Diamonds[i]);

		//Spawn diamonds
		StartCoroutine(SpawnDiamonds());

		//Background music
		StartCoroutine(StartBackgroundMusic());
	}

	void CreateScene()
	{
		//Create world
		GameObject worldObject = Utilities.CreateGameObject("World");
		_World = worldObject.AddComponent<World>();

		//Randomize season
		int season = Random.Range(0, 3);
		_World.Season = (WorldSeason)season;
		
		//Load world data
		_World.Size = ProfileManager.Instance.WorldSize;

		//Set camera drag
		float offset = (float)Screen.width * 768f / (float)Screen.height;
		_CameraDrag.MaxX = _World.Width - offset;

		//Create buildings
		GameObject buildingsObject = Utilities.CreateGameObject("Buildings", new Vector3(0, Building.START_Y, -1), null);
		_BuildingsTransform = buildingsObject.transform;

		List<ProfileManager.BuildingData> b_data = ProfileManager.Instance.Buildings;
		foreach(ProfileManager.BuildingData b_datum in b_data)
		{
			GameObject buildingObject = (GameObject)Instantiate(Resources.Load("Prefabs/Building_" + b_datum.Type));
			buildingObject.name = "Building" + b_datum.Type;
			
			Transform buildingTransform = buildingObject.transform;
			buildingTransform.parent = _BuildingsTransform;
			buildingTransform.localPosition = new Vector3(b_datum.Position, 0, 0);
			
			Building building = buildingObject.GetComponent<Building>();
			_Buildings.Add(building);

			if (b_datum.Type == "CityHall") 
				_BuildingCityHall = building;
			else if (b_datum.Type == "None")
				_BuildingNone = building;
		}

		//Create mukyas
		GameObject mukyasObject = Utilities.CreateGameObject("Mukyas", new Vector3(0, Mukya.START_Y, -2), null);
		_MukyasTransform = mukyasObject.transform;

		Mukya.START_X = 25f;
		Mukya.END_X = _World.Width - 25f;

		//Load mukyas data
		List<ProfileManager.MukyaData> m_data = ProfileManager.Instance.Mukyas;
		foreach(ProfileManager.MukyaData m_datum in m_data)
		{
			float x = Random.Range(Mukya.START_X, Mukya.END_X);
			MainSceneController.AddNewMukya(m_datum.Race, m_datum.EnergyStatus, m_datum.SocialStatus, m_datum.WorkStatus, x);
		}

		//Create Diamonds
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
				_Diamonds.Add(diamond);

				Transform diamondTransform = diamondObject.transform;
				diamondTransform.name = "Diamond" + type_string;
				diamondTransform.parent = diamondsTransform;

				diamondObject.SetActive(false);
			}
		}

		//Shuffle diamond arrays first
		System.Random r = new System.Random();
		int n = _Diamonds.Count;  
		while (n > 1) {  
			n--;  
			int k = r.Next(n + 1);  
			Diamond value = _Diamonds[k];  
			_Diamonds[k] = _Diamonds[n];  
			_Diamonds[n] = value;  
		} 
	}

	IEnumerator StartBackgroundMusic()
	{
		yield return new WaitForSeconds(0.1f);

		if (_World.Season == WorldSeason.Candy)
		{
			SoundManager.BackgroundMusicVolume = 0.5f;
			SoundManager.PlayBackgroundMusic("baby_on_board", true);
		}
		else if (_World.Season == WorldSeason.Ice)
		{
			SoundManager.BackgroundMusicVolume = 0.5f;
			SoundManager.PlayBackgroundMusic("hometown", true);
		}
		else if (_World.Season == WorldSeason.Green)
		{
			SoundManager.BackgroundMusicVolume = 0.4f;
			SoundManager.PlayBackgroundMusic("finish_line", true);
		}
	}

	IEnumerator ResetGameCoroutine()
	{
		yield return new WaitForSeconds(0.15f);

		ProfileManager.Instance.Reset();
		ProfileManager.Instance.ResetProgress();
		ProfileManager.Instance.InitialData();
		ProfileManager.Instance.Save();

		Application.LoadLevel("TitleScene");
	}

	IEnumerator SaveGameCoroutine()
	{
		yield return new WaitForSeconds(0.15f);

		MainSceneController.SaveGame();
	}

	IEnumerator RemoveMukyaCoroutine(Mukya mukya)
	{
		yield return new WaitForSeconds(0.1f);

		if (mukya == _SelectedMukya) 
		{
			_SelectedMukya = null;
			HUD.HideMukyaInformation();
		}

		_Mukyas.Remove(mukya);
		MainSceneController.SaveGame();

		DestroyObject(mukya.gameObject);
	}

	IEnumerator SpawnDiamonds()
	{
		int wait = Random.Range(MIN_DIAMOND_SPAWNER_TIME, MAX_DIAMOND_SPAWNER_TIME);
		wait = 0;

		while(true)
		{
			yield return new WaitForSeconds(wait);

			//Spawn diamonds at buildings
			foreach(Building b in _Buildings)
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
						int dec_timer = times * (b.TotalResidents() - 1);
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

		if (_DiamondsActive.Count < MAX_DIAMOND_SPAWNED)
		{
			Diamond d = _Diamonds[0];
			_Diamonds.RemoveAt(0);

			d.Spawn(b.Position());
			d.OnSpawnDone += UnSpawnDiamond;

			_DiamondsActive.Add(d);

			SoundManager.PlaySoundEffectOneShot("crystal_spawned");
		}
	}

	void UnSpawnDiamond(Diamond d)
	{
		d.OnSpawnDone -= UnSpawnDiamond;

		d.gameObject.SetActive(false);

		_DiamondsActive.Remove(d);
		_Diamonds.Add(d);
	}
	
	// Update is called once per frame
	void Update() 
	{
		//Pause
		if (Utilities.IS_PAUSED) return;

		/*
		if (_SelectedMukya != null)
		{
			Debug.Log(_SelectedMukya.Position() 
			          + " " + _SelectedMukya._Race.ToString()
			          + " " + _SelectedMukya.Destination());
		}
		*/

		bool checkInput = false;

		if (Input.touchCount > 0)
		{
			foreach(Touch t in Input.touches)
			{
				if (!_IsTouched)
				{
					if (t.phase == TouchPhase.Began)
					{
						_TouchOrigin = t.position;
						_TouchTime = 0f;
					}
					else if (t.phase == TouchPhase.Stationary)
					{
						if (!_IsTouchedMove)
						{
							_TouchTime += Time.deltaTime;
							if (_TouchTime >= _TouchDetectTime)
							{
								_IsTouched = true;
								checkInput = true;
								break;
							}
						}
					}
					else if (t.phase == TouchPhase.Moved)
					{
						if (!_IsTouchedMove && Vector2.Distance(_TouchOrigin, t.position) > 10f)
							_IsTouchedMove = true;
					}
					else if (t.phase == TouchPhase.Ended)
					{
						if (!_IsTouchedMove)
							checkInput = true;

						_IsTouched = false;
						_IsTouchedMove = false;
					}
				}
				else
				{
					if (t.phase == TouchPhase.Ended)
					{
						_IsTouched = false;
						_IsTouchedMove = false;
					}
				}
			}
		}
		else
		{
			checkInput = Input.GetMouseButtonUp(0) && !_CameraDrag.IsMoving();
		}

		//When released
		if (checkInput)
		{
			//Translate to screen space
			Vector3 pos = _Camera.ScreenToWorldPoint(Input.mousePosition);
			Vector2 pos2D = new Vector2(pos.x, pos.y);

			Diamond touchedDiamond = GetSelectedDiamond(pos2D);
			if (touchedDiamond != null)
			{
				int index = (int)touchedDiamond._Type;
				SetDiamond(index, _PlayerDiamonds[index] + 1);
				UnSpawnDiamond(touchedDiamond);

				//Begin analytics
				
				List<Parameter> parameters = new List<Parameter>();
				parameters.Add(new Parameter("Feature", "Touching Crystal"));
				
				Reta.Instance.Record("Game Feature Consumed", parameters);
				
				//End analytics

				SoundManager.PlaySoundEffectOneShot("touched_crystal");

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
						    touchedBuilding._Type == Building.BuildingType.Shop ||
						    touchedBuilding._Type == Building.BuildingType.OuterWorld)
						{
							//Cancel if already moving
							if (_OngoingResidents.ContainsKey(_SelectedMukya))
							{
								_SelectedMukya.OnMoveDone -= AddOngoingResident;
								_OngoingResidents.Remove(_SelectedMukya);
							}

							bool moving = _SelectedMukya.Move(touchedBuilding.Position());
							if (moving)
							{
								_SelectedMukya.StopAllCoroutines();
								_SelectedMukya.OnMoveDone += AddOngoingResident;
								
								_OngoingResidents.Add(_SelectedMukya, touchedBuilding);

								/*
								Debug.Log(_SelectedMukya.Position() 
								          + " " + _SelectedMukya._Race.ToString()
								          + " " + touchedBuilding.Position()
								          + " " + touchedBuilding._Type.ToString());
								*/

								//Begin analytics
								
								List<Parameter> parameters = new List<Parameter>();
								parameters.Add(new Parameter("Feature", "Moving Resident In"));
								
								Reta.Instance.Record("Game Feature Consumed", parameters);
								
								//End analytics

								SoundManager.PlaySoundEffectOneShot("meow2");
							}
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
			{
				HUD.ShowMukyaInformation(_SelectedMukya);

				SoundManager.PlaySoundEffectOneShot("meow");
			}
			else
				HUD.HideMukyaInformation();
		}
	}

	private void SetDiamond(int index, int value)
	{
		int newValue = value;
		if (newValue < 0) newValue = 0;
		else if (newValue > MAX_PLAYER_DIAMOND) newValue = MAX_PLAYER_DIAMOND;

		_PlayerDiamonds[index] = newValue;
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
		mukya.OnMoveDone -= AddOngoingResident;

		if (mukya == _SelectedMukya)
		{
			_SelectedMukya = null;
			HUD.HideMukyaInformation();
		}

		Building building = (Building)_OngoingResidents[mukya];

		if (building.CanResidenGo()) 
			building.AddResident(mukya);
		else 
			mukya.Idle(mukya);

		_OngoingResidents.Remove(mukya);
	}

	private Diamond GetSelectedDiamond(Vector2 pos)
	{
		foreach(Diamond diamond in _DiamondsActive)
		{
			if (diamond.IsContainingPosition(pos))
				return diamond;
		}
		
		return null;
	}

	private Building GetSelectedBuilding(Vector2 pos)
	{
		foreach(Building building in _Buildings)
		{
			if (building.IsContainingPosition(pos))
				return building;
		}
		
		return null;
	}

	private Mukya GetSelectedMukya(Vector2 pos)
	{
		foreach(Mukya mukya in _Mukyas)
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
			oldMoney += s_Instance._PlayerDiamonds[i] * Diamond.DIAMOND_PRICE[i];
			s_Instance.SetDiamond(i,0);
		}

		s_Instance.Money = oldMoney;
	}

	public static bool AddNewMukya(Mukya.MukyaRace race)
	{
		if (s_Instance == null) return false;

		return AddNewMukya(race, s_Instance._BuildingCityHall.Position());
	}

	public static bool AddNewMukya(Mukya.MukyaRace race, float position)
	{
		string raceString = race.ToString();
		ProfileManager.Instance.Mukyas.Add(new ProfileManager.MukyaData(raceString, 100f, 100f, 100f));

		return AddNewMukya(raceString, 100f, 100f, 100f, position);
	}

	public static bool AddNewMukya(string race, float m_energy, float m_social, float m_work, float position)
	{
		if (s_Instance == null) return false;
		//if (s_Instance.Mukyas.Count >= (s_Instance._World.Width * MAX_MUKYA_PER_LEVEL)) return false;

		GameObject mukyaObject = (GameObject)Instantiate(Resources.Load("Prefabs/Mukya_" + race));
		
		Mukya mukya = mukyaObject.GetComponent<Mukya>();
		mukya._EnergyStatus = m_energy;
		mukya._SocialStatus = m_social;
		mukya._WorkStatus = m_work;
		s_Instance._Mukyas.Add(mukya);
		
		Transform mukyaTransform = mukyaObject.transform;
		mukyaTransform.name = "Mukya" + mukya._Race.ToString();
		mukyaTransform.parent = s_Instance._MukyasTransform;
		
		mukyaTransform.localPosition = new Vector3(position, 0, 0);
		
		return true;
	}

	public static void RemoveMukya(Mukya mukya)
	{
		if (s_Instance == null) return;
	
		if (mukya == null) 
			return;
		else
			s_Instance.StartCoroutine(s_Instance.RemoveMukyaCoroutine(mukya));
	}

	public static bool RemoveGhost()
	{
		if (s_Instance == null) return false;

		foreach(Mukya mukya in s_Instance._Mukyas)
		{
			if (mukya._Race == Mukya.MukyaRace.Ghost)
			{
				s_Instance.StartCoroutine(s_Instance.RemoveMukyaCoroutine(mukya));
				return true;
			}
		}

		return false;
	}

	public static bool SpendMoney(int money)
	{
		if (s_Instance == null) return false;

		int oldMoney = s_Instance.Money - money;
		if (oldMoney < 0) return false;

		s_Instance.Money = oldMoney;

		return true;
	}

	public static void BuildNewBuilding(Building.BuildingType type)
	{
		if (s_Instance == null) return;
		if (type == Building.BuildingType.None) return;

		s_Instance._World.IncreaseWorldSize();

		GameObject buildingObject = (GameObject)Instantiate(Resources.Load("Prefabs/Building_" + type.ToString()));
		buildingObject.name = "Building" + type.ToString();

		Transform buildingTransform = buildingObject.transform;
		buildingTransform.parent = s_Instance._BuildingsTransform;
		buildingTransform.localPosition = new Vector3(s_Instance._BuildingNone.Position(), 0, 0);

		Building building = buildingObject.GetComponent<Building>();
		s_Instance._Buildings.Add(building);

		s_Instance._BuildingNone.MovePosition(World.TILE_WIDTH * World.TILE_PER_ADDITION);

		//Update camera drag
		float offset = (float)Screen.width * 768f / (float)Screen.height;
		s_Instance._CameraDrag.MaxX = s_Instance._World.Width - offset;

		//Update mukya end
		Mukya.END_X = s_Instance._World.Width - 25f;

		//Begin analytics

		Reta.Instance.EndTimedRecord("Level Duration");

		List<Parameter> parameterLevel = new List<Parameter>();
		parameterLevel.Add(new Parameter("Level", s_Instance._World.Size.ToString()));

		Reta.Instance.Record("Level Duration", parameterLevel, true);
		
		List<Parameter> parameterProgression = new List<Parameter>();
		parameterProgression.Add(new Parameter("Increase", "5"));

		Reta.Instance.Record("Game Progression", parameterProgression); 

		//End analytics

		//Save
		s_Instance.StartCoroutine(s_Instance.SaveGameCoroutine());
	}

	public static void SaveGame()
	{
		if (s_Instance == null) return;

		//Save attributes
		ProfileManager.Instance.Money = s_Instance._Money;
		ProfileManager.Instance.Diamonds = s_Instance._PlayerDiamonds;
		ProfileManager.Instance.WorldSize = s_Instance._World.Size;

		//Save building
		ProfileManager.Instance.Buildings.Clear();
		ProfileManager.Instance.Buildings = new List<ProfileManager.BuildingData>();
		foreach(Building b in s_Instance._Buildings)
			ProfileManager.Instance.Buildings.Add(new ProfileManager.BuildingData(b._Type.ToString(), b.Position()));

		//Save mukya
		ProfileManager.Instance.Mukyas.Clear();
		ProfileManager.Instance.Mukyas = new List<ProfileManager.MukyaData>();
		foreach(Mukya m in s_Instance._Mukyas)
			ProfileManager.Instance.Mukyas.Add(new ProfileManager.MukyaData(m._Race.ToString(), m._EnergyStatus, m._SocialStatus, m._WorkStatus));

		//Save
		ProfileManager.Instance.Save();
	}

	public static void ResetGame()
	{
		if (s_Instance == null) return;

		s_Instance.StartCoroutine(s_Instance.ResetGameCoroutine());
	}
}
