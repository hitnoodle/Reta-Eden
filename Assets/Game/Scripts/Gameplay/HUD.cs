using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class HUD : MonoBehaviour 
{
	private static HUD s_Instance = null;

	#region UI items

	public enum UIState 
	{
		Idle,
		PauseMenu,
		CommCenterMenu,
		CityHallMenu,
		ResidentsMenu,
		BuildMenu
	}
	public UIState _State = UIState.Idle;

	public GameObject Overlay = null;

	public GameObject MenuButton = null;
	public tk2dUIItem MenuButtonItem = null;

	public GameObject PlayerInformation = null;
	public tk2dTextMesh Money = null;
	public tk2dTextMesh[] Diamonds = null;

	public GameObject MukyaInformation = null;
	public tk2dSprite MukyaMini = null;
	public tk2dTextMesh MukyaEnergy = null;
	public tk2dTextMesh MukyaSocial = null;
	public tk2dTextMesh MukyaWork = null;

	public GameObject MultipurposeBox = null;
	public GameObject MultipurposeTitle = null;
	public tk2dTextMesh Title = null;

	public GameObject CommCenterMenu = null;
	public tk2dUIItem ShareButton = null;

	public GameObject CityHallMenu = null;
	public tk2dUIItem RequestResidentButton = null;
	public tk2dUIItem ExchangeCrystalButton = null;

	public GameObject ResidentMenu = null;
	public tk2dSprite[] ResidentIcons;
	public tk2dTextMesh[] ResidentStatus;
	public tk2dTextMesh[] ResidentEmptyStatus;
	public tk2dUIItem[] ResidentOutButtons;

	public GameObject BuildingMenu = null;
	public GameObject[] BuildMenuPages;
	public tk2dUIItem ArrowRight = null;
	public tk2dUIItem ArrowLeft = null;
	public tk2dUIItem BuildButton = null;
	public tk2dTextMesh BuildPrice = null;

	public GameObject PauseCreditText = null;
	public tk2dUIItem PauseResumeButton = null;
	public tk2dUIItem PauseResetButton = null;
	public tk2dTextMesh PauseCounter = null;

	#endregion

	private Camera _Camera = null;
	private BoxCollider2D _ClickBoundary = null;

	private Mukya _CurrentMukya = null;
	private Building _CurrentBuilding = null;

	private bool _CanIdleClick = true;

	private int _BuildMenuPage;
	private BoxCollider[] _BuildMenuBoundaries;

	private int _ResetCounter;
	private bool[] _ResetClicked;

	#region Mono Behavior

	// Use this for initialization
	void Awake() 
	{
		s_Instance = this;

		_Camera = Camera.main;

		DisableOverlay();
	}

	void Update()
	{
		if (_State != UIState.Idle && _State != UIState.PauseMenu)
		{
			//Handling back to Idle code
			bool checkInput = false;
			if (Input.touchCount > 0)
			{
				foreach(Touch t in Input.touches)
				{
					if (t.phase == TouchPhase.Began)
					{
						checkInput = true;
						break;
					}
				}
			}
			else
			{
				checkInput = Input.GetMouseButtonUp(0);
			}

			if (checkInput)
			{
				//Translate to screen space
				Vector3 pos = _Camera.ScreenToWorldPoint(Input.mousePosition);
				Vector2 pos2D = new Vector2(pos.x, pos.y);

				//Special check on building
				if (_State == UIState.BuildMenu)
				{
					Ray ray = _Camera.ScreenPointToRay(Input.mousePosition);
					RaycastHit hit;
					foreach(BoxCollider collider in _BuildMenuBoundaries)
						if (collider.Raycast(ray, out hit, Mathf.Infinity))
							return;
				}

				//Not on touch boundary
				if (_ClickBoundary != Physics2D.OverlapPoint(pos2D))
				{
					ExitState();

					SoundManager.PlaySoundEffectOneShot("button_back");
				}
					
			}

			if (_State == UIState.ResidentsMenu)
				UpdateResidentsStatus();
		}
		else
		{
			if (!_CanIdleClick) _CanIdleClick = true;

			//Update mukya information if shown
			if (_CurrentMukya != null)
			{
				MukyaEnergy.text = _CurrentMukya.EnergyPercentage().ToString("0") + "%";
				MukyaSocial.text = _CurrentMukya.SocialPercentage().ToString("0") + "%";
				MukyaWork.text = _CurrentMukya.WorkPercentage().ToString("0") + "%";
			}
		}
	}

	#endregion

	#region UI handler

	IEnumerator Done()
	{
		yield return new WaitForSeconds(0.1f);

		ExitState();
	}

	private void EnableOverlay()
	{
		MenuButtonItem.OnClick -= PauseMenu;

		MenuButton.SetActive(false);
		PlayerInformation.SetActive(false);

		if (_CurrentMukya != null)
			MukyaInformation.SetActive(false);

		Overlay.SetActive(true);
	}

	public void PauseMenu()
	{
		StartCoroutine(DisplayPauseMenu());
	}

	IEnumerator DisplayPauseMenu()
	{
		yield return new WaitForSeconds(0.1f);

		HUD.ShowPauseMenu();
	}

	private void DisableOverlay()
	{
		MenuButton.SetActive(true);
		PlayerInformation.SetActive(true);

		if (_CurrentMukya != null)
			MukyaInformation.SetActive(true);

		Overlay.SetActive(false);

		MenuButtonItem.OnClick += PauseMenu;
	}

	private void EnterCommunicationMenu()
	{
		MultipurposeBox.SetActive(true);
		MultipurposeTitle.SetActive(true);
		Title.text = "Communication Center";

		CommCenterMenu.SetActive(true);

		_ClickBoundary = CommCenterMenu.GetComponent<BoxCollider2D>();
		ShareButton.OnClick += Share;
	}

	private void Share()
	{
		/*
		StartCoroutine(Done());
		SoundManager.PlaySoundEffectOneShot("button_confirm_again");
		*/
	}

	private void ExitCommunicationMenu()
	{
		ShareButton.OnRelease -= Share;
		_ClickBoundary = null;

		CommCenterMenu.SetActive(false);

		MultipurposeBox.SetActive(false);
		MultipurposeTitle.SetActive(false);
	}

	private void EnterCityHallMenu()
	{
		MultipurposeBox.SetActive(true);
		MultipurposeTitle.SetActive(true);
		Title.text = "City Hall";

		CityHallMenu.SetActive(true);
		
		_ClickBoundary = CityHallMenu.GetComponent<BoxCollider2D>();
		RequestResidentButton.OnClick += RequestResident;
		ExchangeCrystalButton.OnClick += ExchangeCrystal;
	}

	private int CalculateResidentPrice()
	{
		int price = Mukya.MUKYA_PRICE + (Mukya.PRICE_INFLATION * ProfileManager.Instance.WorldSize);
		return price;
	}

	private void RequestResident()
	{
		int price = CalculateResidentPrice();
		bool bought = MainSceneController.SpendMoney(price);
		
		if (bought)
		{
			MainSceneController.RemoveGhost();

			int race = Random.Range(0, 4) + 1;
			MainSceneController.AddNewMukya((Mukya.MukyaRace)race);
			
			StartCoroutine(Done());
			
			SoundManager.PlaySoundEffectOneShot("building_now");
		}
		else
		{
			SoundManager.PlaySoundEffectOneShot("button_error");
		}
	}

	private void ExchangeCrystal()
	{
		MainSceneController.ExchangeMoneyWithCrystal();
		StartCoroutine(Done());

		SoundManager.PlaySoundEffectOneShot("building_now");
	}
	
	private void ExitCityHallMenu()
	{
		RequestResidentButton.OnClick -= RequestResident;
		ExchangeCrystalButton.OnClick -= ExchangeCrystal;
		_ClickBoundary = null;
		
		CityHallMenu.SetActive(false);

		MultipurposeBox.SetActive(false);
		MultipurposeTitle.SetActive(false);
	}

	private void EnterResidentMenu()
	{
		//Don't pause
		Utilities.IS_PAUSED = false;

		MultipurposeBox.SetActive(true);
		MultipurposeTitle.SetActive(true);

		string title = (_CurrentBuilding._Type == Building.BuildingType.OuterWorld) ? "Outer World" : _CurrentBuilding._Type.ToString();
		Title.text = title;
		
		ResidentMenu.SetActive(true);
		
		_ClickBoundary = ResidentMenu.GetComponent<BoxCollider2D>();

		ResetResidentMenu();
		InitializeResidentMenu();
	}

	private void ResetResidentMenu()
	{
		for(int i=0;i<Building.MAX_RESIDENT;i++)
		{
			ResidentOutButtons[i].OnClickUIItem -= OutResident;
			
			ResidentIcons[i].gameObject.SetActive(false);
			ResidentStatus[i].gameObject.SetActive(false);
			ResidentEmptyStatus[i].gameObject.SetActive(false);
			ResidentOutButtons[i].gameObject.SetActive(false);
		}
	}

	private void InitializeResidentMenu()
	{
		//Initiate buttons
		List<Mukya> residents = _CurrentBuilding.Residents;
		int resident_count = residents.Count;
		for(int i=0;i<resident_count;i++)
		{
			Mukya mukya = residents[i];

			string curr_name = "alien" + mukya._Race + "_badge1";
			ResidentIcons[i].SetSprite(curr_name);

			StringBuilder sb = new StringBuilder();
			sb.Append(mukya.EnergyPercentage().ToString("0"));
			sb.Append(" ");
			sb.Append(mukya.SocialPercentage().ToString("0"));
			sb.Append(" ");
			sb.Append(mukya.WorkPercentage().ToString("0"));
			ResidentStatus[i].text = sb.ToString();

			ResidentOutButtons[i].OnClickUIItem += OutResident;

			ResidentIcons[i].gameObject.SetActive(true);
			ResidentStatus[i].gameObject.SetActive(true);
			ResidentOutButtons[i].gameObject.SetActive(true);
		}
		
		//Empty list
		int empty = Building.MAX_RESIDENT - resident_count;
		if (empty > 0)
		{
			for(int i=resident_count;i<Building.MAX_RESIDENT;i++)
				ResidentEmptyStatus[i].gameObject.SetActive(true);
		}
	}

	private void UpdateResidentsStatus()
	{
		List<Mukya> residents = _CurrentBuilding.Residents;
		int resident_count = residents.Count;
		for(int i=0;i<resident_count;i++)
		{
			Mukya mukya = residents[i];
			
			StringBuilder sb = new StringBuilder();
			sb.Append(mukya.EnergyPercentage().ToString("0"));
			sb.Append(" ");
			sb.Append(mukya.SocialPercentage().ToString("0"));
			sb.Append(" ");
			sb.Append(mukya.WorkPercentage().ToString("0"));
			ResidentStatus[i].text = sb.ToString();
		}
	}

	private void OutResident(tk2dUIItem outButton)
	{
		string index_string = outButton.name.Substring(0,1);
		int index = System.Int32.Parse(index_string) - 1;
		_CurrentBuilding.RemoveResident(index);

		ResetResidentMenu();
		InitializeResidentMenu();

		SoundManager.PlaySoundEffectOneShot("button_confirm_again");
	}
	
	private void ExitResidentMenu()
	{
		_ClickBoundary = null;

		ResetResidentMenu();
		ResidentMenu.SetActive(false);
		
		MultipurposeBox.SetActive(false);
		MultipurposeTitle.SetActive(false);
	}

	private void EnterBuildingMenu()
	{
		MultipurposeBox.SetActive(true);
		MultipurposeTitle.SetActive(true);
		Title.text = "Expand World";
		
		BuildingMenu.SetActive(true);

		_ClickBoundary = BuildingMenu.GetComponent<BoxCollider2D>();
		_BuildMenuBoundaries = new BoxCollider[2];
		_BuildMenuBoundaries[0] = ArrowLeft.GetComponent<BoxCollider>();
		_BuildMenuBoundaries[1] = ArrowRight.GetComponent<BoxCollider>();

		BuildButton.OnClick += Build;

		ShowBuildPage(_BuildMenuPage);
	}

	private int CalculateBuildPrice(int index)
	{
		int price = Building.BUILDING_PRICE[index] + (Building.PRICE_INFLATION * ProfileManager.Instance.WorldSize);
		return price;
	}

	private void Build()
	{
		int price = CalculateBuildPrice(_BuildMenuPage);
		bool bought = MainSceneController.SpendMoney(price);

		if (bought)
		{
			Building.BuildingType type = Building.BuildingType.None;

			if (_BuildMenuPage == 0)
				type = Building.BuildingType.House;
			else if (_BuildMenuPage == 1)
				type = Building.BuildingType.Bar;
			else if (_BuildMenuPage == 2)
				type = Building.BuildingType.Shop;

			MainSceneController.BuildNewBuilding(type);

			StartCoroutine(Done());

			SoundManager.PlaySoundEffectOneShot("building_now");
		}		
		else
		{
			SoundManager.PlaySoundEffectOneShot("button_error");
		}
	}

	private void NextPage()
	{
		ArrowLeft.OnClick -= PrevPage;
		ArrowRight.OnClick -= NextPage;

		ShowBuildPage(_BuildMenuPage + 1);

		SoundManager.PlaySoundEffectOneShot("button_confirm");
	}

	private void PrevPage()
	{
		ArrowLeft.OnClick -= PrevPage;
		ArrowRight.OnClick -= NextPage;

		ShowBuildPage(_BuildMenuPage - 1);

		SoundManager.PlaySoundEffectOneShot("button_confirm");
	}

	private void ShowBuildPage(int index)
	{
		int len = BuildMenuPages.Length;

		if (index < 0 || index >= len)
			return;

		for(int i=0;i<len;i++)
		{
			if (i==index)
				BuildMenuPages[i].SetActive(true);
			else
				BuildMenuPages[i].SetActive(false);

			BuildPrice.text = CalculateBuildPrice(index).ToString();
		}

		if (index == 0)
		{
			ArrowLeft.gameObject.SetActive(false);
			ArrowRight.gameObject.SetActive(true);

			ArrowRight.OnClick += NextPage;
		}
		else if (index == len - 1)
		{
			ArrowLeft.gameObject.SetActive(true);
			ArrowRight.gameObject.SetActive(false);

			ArrowLeft.OnClick += PrevPage;
		}
		else
		{
			ArrowLeft.gameObject.SetActive(true);
			ArrowRight.gameObject.SetActive(true);

			ArrowLeft.OnClick += PrevPage;
			ArrowRight.OnClick += NextPage;
		}

		_BuildMenuPage = index;
	}

	private void ExitBuildingMenu()
	{
		BuildButton.OnClick -= Build;
		ArrowLeft.OnClick -= PrevPage;
		ArrowRight.OnClick -= NextPage;

		_ClickBoundary = null;
		_BuildMenuBoundaries = null;
		
		BuildingMenu.SetActive(false);
		
		MultipurposeBox.SetActive(false);
		MultipurposeTitle.SetActive(false);
	}

	private void EnterPauseMenu()
	{
		PauseCreditText.SetActive(true);
		PauseResumeButton.gameObject.SetActive(true);
		PauseResetButton.gameObject.SetActive(true);

		_ResetCounter = 3;

		_ResetClicked = new bool[_ResetCounter];
		for(int i=0;i<_ResetCounter;i++) _ResetClicked[i] = false;

		PauseCounter.text = _ResetCounter + "x!";

		PauseResumeButton.OnClick += ResumeGame;
		PauseResetButton.OnClick += ResetGame;
	}

	private void ResetGame()
	{
		int times = 0;
		foreach(bool b in _ResetClicked) 
			if (b) times++;

		if (times == 2)
		{
			MainSceneController.ResetGame();
			
			StartCoroutine(Done());
			
			SoundManager.PlaySoundEffectOneShot("button_back");
		}
		else
		{
			_ResetClicked[times] = true;

			_ResetCounter--;
			PauseCounter.text = _ResetCounter + "x!";
		}
	}

	private void ResumeGame()
	{
		StartCoroutine(Done());
		
		SoundManager.PlaySoundEffectOneShot("button_back");
	}

	private void ExitPauseMenu()
	{
		PauseResumeButton.OnClick -= ResumeGame;
		PauseResetButton.OnClick -= ResetGame;

		_ResetClicked = null;

		PauseCreditText.SetActive(false);
		PauseResumeButton.gameObject.SetActive(false);
		PauseResetButton.gameObject.SetActive(false);
	}

	private void EnterState()
	{
		Utilities.IS_PAUSED = true;
		EnableOverlay();

		if (_State == UIState.CommCenterMenu)
			EnterCommunicationMenu();
		else if (_State == UIState.CityHallMenu)
			EnterCityHallMenu();
		else if (_State == UIState.ResidentsMenu)
			EnterResidentMenu();
		else if (_State == UIState.BuildMenu)
			EnterBuildingMenu();
		else if (_State == UIState.PauseMenu)
			EnterPauseMenu();

		SoundManager.PlaySoundEffectOneShot("button_confirm");
	}

	private void ExitState()
	{
		if (_State == UIState.CommCenterMenu)
			ExitCommunicationMenu();
		else if (_State == UIState.CityHallMenu)
			ExitCityHallMenu();
		else if (_State == UIState.ResidentsMenu)
			ExitResidentMenu();
		else if (_State == UIState.BuildMenu)
			ExitBuildingMenu();
		else if (_State == UIState.PauseMenu)
			ExitPauseMenu();

		DisableOverlay();
		Utilities.IS_PAUSED = false;
		_CanIdleClick = false;

		//Back to idle
		_State = UIState.Idle;
	}

	#endregion

	#region Static functions

	public static bool CanIdleClick()
	{
		if (s_Instance == null) return false;

		return s_Instance._CanIdleClick;
	}

	public static void ShowMukyaInformation(Mukya mukya)
	{
		if (s_Instance == null) return;

		s_Instance._CurrentMukya = mukya;
		s_Instance.MukyaInformation.SetActive(true);

		string curr_name = "alien" + mukya._Race + "_badge1";
		s_Instance.MukyaMini.SetSprite(curr_name);
	}

	public static void HideMukyaInformation()
	{
		if (s_Instance == null) return;
		
		s_Instance._CurrentMukya = null;
		s_Instance.MukyaInformation.SetActive(false);
	}

	public static void SetMoney(int money)
	{
		if (s_Instance == null) return;

		s_Instance.Money.text = money.ToString();
	}

	public static void SetDiamond(int index, int diamond)
	{
		if (s_Instance == null) return;

		if (index >= 0 && index < 4)
		{
			s_Instance.Diamonds[index].text = diamond.ToString();
		}
	}

	public static void ShowPauseMenu()
	{
		if (s_Instance == null) return;

		s_Instance._State = UIState.PauseMenu;
		s_Instance.EnterState();
	}

	public static void ShowCommunicationCenterMenu()
	{
		if (s_Instance == null) return;

		s_Instance._State = UIState.CommCenterMenu;
		s_Instance.EnterState();
	}

	public static void ShowCityHallMenu()
	{
		if (s_Instance == null) return;

		s_Instance._State = UIState.CityHallMenu;
		s_Instance.EnterState();
	}

	public static void ShowResidentsMenu(Building building)
	{
		if (s_Instance == null) return;

		s_Instance._CurrentBuilding = building;

		s_Instance._State = UIState.ResidentsMenu;
		s_Instance.EnterState();
	}
	
	public static void ShowBuildMenu()
	{
		if (s_Instance == null) return;

		s_Instance._State = UIState.BuildMenu;
		s_Instance.EnterState();
	}


	#endregion
}
