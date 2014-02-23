using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
		public string Type;
		public float Position;

		public BuildingData()
		{
			Type = "";
			Position = 0f;
		}

		public BuildingData(string type, float position)
		{
			Type = type;
			Position = position;
		}
	};
	public List<BuildingData> Buildings;

	public class MukyaData
	{
		public string Race;
		public float EnergyStatus;
		public float SocialStatus;
		public float WorkStatus;

		public MukyaData()
		{
			Race = "";
			EnergyStatus = 100f;
			SocialStatus = 100f;
			WorkStatus = 100f;
		}

		public MukyaData(string race, float energys, float socials, float works)
		{
			Race = race;
			EnergyStatus = energys;
			SocialStatus = socials;
			WorkStatus = works;
		}
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
		//Add initial building
		Buildings.Add(new BuildingData("OuterWorld", 100));
		Buildings.Add(new BuildingData("CommunicationCenter", 329));
		Buildings.Add(new BuildingData("CityHall", 739));
		Buildings.Add(new BuildingData("Bar", 1250));
		Buildings.Add(new BuildingData("None", 1750));

		//Adam and Eve
		Mukyas.Add(new MukyaData("Beige", 100, 100, 100));
		Mukyas.Add(new MukyaData("Pink", 100, 100, 100));

		//First money
		Money = 5000;
	}
	
	public void ResetProgress() 
	{
		TimeSaved = DateTime.UtcNow;
		TutorialPlayed = false;

		WorldSize = 1;
		Money = 0;
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
		//See path
		string path = Application.persistentDataPath + "/Profile";

		//Create first if haven't
		if (!File.Exists(path))
		{
			InitialData();
			Save();
		} 
		else
		{
			//Load
			s_Instance = (ProfileManager)XmlManager.LoadInstanceAsXml("Profile", typeof(ProfileManager));
		}
	}
	
	public void Save()
	{
		s_Instance.TimeSaved = DateTime.UtcNow;
		XmlManager.SaveInstanceAsXml("Profile", typeof(ProfileManager), s_Instance);
	}

	#endregion
}
