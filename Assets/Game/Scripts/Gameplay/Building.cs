using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Building : MonoBehaviour 
{
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
		float delta = Time.deltaTime;

		HandleMukyas(delta);
		ThrowItems(delta);
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
			else if (_Type == BuildingType.OuterWorld)
			{
				mukya.IncreaseWork(_PrimaryIncrease * delta);
				mukya.IncreaseSocial(_SecondaryIncrease * delta);
			}
		}
	}

	void ThrowItems(float delta)
	{
		if (_Type == BuildingType.Bar)
		{
			
		}
		else if (_Type == BuildingType.CommunicationCenter)
		{
			
		}
		else if (_Type == BuildingType.CityHall)
		{
			
		}
		else if (_Type == BuildingType.House)
		{
			
		}
		else if (_Type == BuildingType.OuterWorld)
		{
			
		}
		else if (_Type == BuildingType.Shop)
		{
			
		}
	}

	#endregion

	#region Resident handling

	public bool IsContainingPosition(Vector2 pos)
	{
		return _Collider == Physics2D.OverlapPoint(pos);
	}

	public float Position()
	{
		return _Transform.position.x;
	}

	public void AddResident(Mukya mukya)
	{
		mukya.None();
		_Residents.Add(mukya);
	}

	public bool RemoveResident(int index)
	{
		if (index >= 0 && index < _Residents.Count)
		{
			Mukya mukya = _Residents[index];
			mukya.UnNone();
			
			_Residents.RemoveAt(index);

			return true;
		}

		return false;
	}

	#endregion
}
