using System;
using System.Collections;
using System.Collections.Generic;

public class ProfileManager
{
	//Instance
	private static ProfileManager s_Instance;
	public static ProfileManager Instance
	{
		get 
		{
			//Create for the first time
			if (s_Instance == null)
				s_Instance = new ProfileManager();
			
			return s_Instance;
		}
	}

	/* Begin Data */

	public DateTime TimeSaved;
	public bool TutorialPlayed;

	public int WorldSize;
	public int Money;
	public int[] Diamonds;

	public class BuildingData
	{
		string Type;
		float Position;
	};
	public List<BuildingData> Buildings;

	public class MukyaData
	{
		string Race;
		float EnergyStatus;
		float SocialStatus;
		float WorkStatus;
	};
	public List<MukyaData> Mukyas;

	/* End Data */

	//Default hidden constructor
	protected ProfileManager() 
	{
		Reset();
		ResetProgress();
	}

	public void InitialData()
	{

	}
	
	public void ResetProgress() 
	{
		TimeSaved = DateTime.UtcNow;
		TutorialPlayed = false;

		WorldSize = 1;
		Money = 100000;
		for(int i=0;i<Diamonds.Length;i++) Diamonds[i] = 0;
	}
	
	public void Reset() 
	{
		Diamonds = new int[MainSceneController.DIAMOND_TYPES];

		if (Buildings != null)
		{
			Buildings.Clear();
			Buildings = null;
		}
		Buildings = new List<BuildingData>();

		if (Mukyas != null)
		{
			Mukyas.Clear();
			Mukyas = null;
		}
		Mukyas = new List<MukyaData>();
	}

	#region Serialization 

	public void Load()
	{

	}
	
	public void Save()
	{

	}

	#endregion
}
