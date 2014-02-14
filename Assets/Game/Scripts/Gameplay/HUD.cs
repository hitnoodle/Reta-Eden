using UnityEngine;
using System.Collections;

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

	#endregion

	private Camera _Camera = null;
	private BoxCollider2D _ClickBoundary = null;

	private Mukya _CurrentMukya = null;
	private Building _CurrentBuilding = null;

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
		}
		else
		{
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
		MultipurposeBox.SetActive(true);
		MultipurposeTitle.SetActive(true);
		Title.text = _CurrentBuilding._Type.ToString();
		
		ResidentMenu.SetActive(true);
		
		_ClickBoundary = ResidentMenu.GetComponent<BoxCollider2D>();
	}
	
	private void ExitResidentMenu()
	{
		_ClickBoundary = null;
		
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

		//Back to idle
		_State = UIState.Idle;
	}

	#endregion

	#region Static functions

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
