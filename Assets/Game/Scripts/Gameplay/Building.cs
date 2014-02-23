using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Building : MonoBehaviour 
{
	#region Constants
	
	public const float START_Y = 280f;
	public const int MAX_RESIDENT = 3;
	public const int PRICE_INFLATION = 2500;

	public static readonly int[] BUILDING_PRICE = 
	{
		5000,		//House
		7500,		//Bar
		10000,		//Shop
	};

	#endregion

	public enum BuildingType
	{
		None,
		Bar,
		CityHall,
		CommunicationCenter,
		House,
		OuterWorld,
		Shop
	}
	public BuildingType _Type = BuildingType.None;
	public float _PrimaryIncrease = 8f;
	public float _SecondaryIncrease = 4f;

	private List<Mukya> _Residents;
	public List<Mukya> Residents
	{
		get { return _Residents; }
	}

	private BoxCollider2D _Collider;
	private Transform _Transform;

	// Use this for initialization
	void Start() 
	{
		_Residents = new List<Mukya>();

		_Collider = GetComponent<BoxCollider2D>();
		_Transform = transform;
	}
	
	// Update is called once per frame
	void Update() 
	{
		//Pause
		if (Utilities.IS_PAUSED) return;

		float delta = Time.deltaTime;
		HandleMukyas(delta);
	}

	#region Behavior handler

	// Again, lazy handling
	void HandleMukyas(float delta)
	{
		foreach(Mukya mukya in _Residents)
		{
			if (_Type == BuildingType.Bar)
			{
				mukya.IncreaseSocial(_PrimaryIncrease * delta);
				mukya.IncreaseEnergy(_SecondaryIncrease * delta);
			}
			else if (_Type == BuildingType.House)
			{
				mukya.IncreaseEnergy(_PrimaryIncrease * delta);
			}
			else if (_Type == BuildingType.OuterWorld || _Type == BuildingType.Shop)
			{
				mukya.IncreaseWork(_PrimaryIncrease * delta);
				mukya.IncreaseSocial(_SecondaryIncrease * delta);
			}
		}
	}

	#endregion

	#region Resident handling

	public bool IsContainingPosition(Vector2 pos)
	{
		if (!_Collider.enabled) return false;

		return _Collider == Physics2D.OverlapPoint(pos);
	}

	public float Position()
	{
		return _Transform.position.x;
	}

	public void MovePosition(float offset)
	{
		Vector3 oldPos = _Transform.position;
		_Transform.position = new Vector3(oldPos.x + offset, oldPos.y, oldPos.z);
	}

	public int TotalResidents()
	{
		return _Residents.Count;
	}

	public bool CanResidenGo()
	{
		return _Residents.Count < MAX_RESIDENT;
	}

	public bool AddResident(Mukya mukya)
	{
		if (_Residents.Count < MAX_RESIDENT)
		{
			mukya.None();
			_Residents.Add(mukya);

			return true;
		}

		return false;
	}

	public bool RemoveResident(int index)
	{
		if (index >= 0 && index < _Residents.Count)
		{
			Mukya mukya = _Residents[index];
			mukya.UnNone();
			mukya.SetPosition(Position());
			
			_Residents.RemoveAt(index);

			return true;
		}

		return false;
	}

	#endregion
}
