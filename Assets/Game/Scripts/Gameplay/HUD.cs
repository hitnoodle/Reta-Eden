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

	#endregion

	private Camera _Camera = null;
	private BoxCollider2D _ClickBoundary = null;

	private Mukya _CurrentMukya = null;
	private Building _CurrentBuilding = null;

	private bool _CanIdleClick = true;

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
		if (_State != UIState.Idle)
		{
			//Handling back to Idle code
			if (Input.GetMouseButtonUp(0))
			{
				//Translate to screen space
				Vector3 pos = _Camera.ScreenToWorldPoint(Input.mousePosition);
				Vector2 pos2D = new Vector2(pos.x, pos.y);

				//Not on touch boundary
				if (_ClickBoundary != Physics2D.OverlapPoint(pos2D))
					ExitState();
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

	private void RequestResident()
	{

	}

	private void ExchangeCrystal()
	{
		MainSceneController.ExchangeMoneyWithCrystal();
		StartCoroutine(Done());
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
		Title.text = _CurrentBuilding._Type.ToString();
		
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
	}
	
	private void ExitResidentMenu()
	{
		_ClickBoundary = null;

		ResetResidentMenu();
		ResidentMenu.SetActive(false);
		
		MultipurposeBox.SetActive(false);
		MultipurposeTitle.SetActive(false);
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
	}

	private void ExitState()
	{
		if (_State == UIState.CommCenterMenu)
			ExitCommunicationMenu();
		else if (_State == UIState.CityHallMenu)
			ExitCityHallMenu();
		else if (_State == UIState.ResidentsMenu)
			ExitResidentMenu();

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
	}


	#endregion
}
