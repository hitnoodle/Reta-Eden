using UnityEngine;
using System.Collections;

public enum WorldSeason 
{
	Green,
	Ice,
	Candy
};

public class World : MonoBehaviour 
{
	public const int TILE_WIDTH = 70;
	public const int TILE_PER_ADDITION = 7;

	public WorldSeason Season = WorldSeason.Green;
	public int Size = 1;
	public int InitialTiles = 29;

	public int Width
	{
		get { return (InitialTiles + ((Size - 1) * TILE_PER_ADDITION)) * TILE_WIDTH; }
	}
	
	private Transform _Transform;

	private Transform _BackgroundsTransform;
	private tk2dSprite[] _Backgrounds;

	private Transform _FloorsTransform;

	void Start() 
	{
		_Transform = transform;

		CreateWorld();
	}

	void CreateWorld()
	{
		//Season 
		string season = Season.ToString();

		//Create Background
		GameObject backgroundsObject = (GameObject)Instantiate(Resources.Load("Prefabs/Backgrounds_" + season));
		backgroundsObject.name = "Backgrounds";

		_BackgroundsTransform = backgroundsObject.transform;
		_BackgroundsTransform.parent = _Transform;

		_Backgrounds = backgroundsObject.GetComponentsInChildren<tk2dSprite>();

		//Create Floors
		GameObject floorsObject = Utilities.CreateGameObject("Floors", Vector3.zero, _Transform);
		_FloorsTransform = floorsObject.transform;

		//Initial tiles
		for (int i=0;i<InitialTiles;i++)
		{
			GameObject tilesObject = (GameObject)Instantiate(Resources.Load("Prefabs/FloorTiles_" + season));
			tilesObject.name = "FloorTiles";

			Transform tilesTransform = tilesObject.transform;
			tilesTransform.parent = _FloorsTransform;
			tilesTransform.position = new Vector3(0 + (i * TILE_WIDTH), 0, 0);
		}

		if (Size > 1)
		{
			int newSize = Size;
			Size = 1;

			for(int i=1;i<newSize;i++)
				IncreaseWorldSize();
		}
	}

	void CreateAdditions(int addSize)
	{
		int startX = (InitialTiles * TILE_WIDTH) + ((Size - 2) * TILE_PER_ADDITION * TILE_WIDTH);
		int addition = addSize * TILE_PER_ADDITION;

		//Season 
		string season = Season.ToString();

		//Add more tiles
		for (int i=0;i<addition;i++)
		{
			GameObject tilesObject = (GameObject)Instantiate(Resources.Load("Prefabs/FloorTiles_" + season));
			tilesObject.name = "FloorTiles";
			
			Transform tilesTransform = tilesObject.transform;
			tilesTransform.parent = _FloorsTransform;
			tilesTransform.position = new Vector3(startX + (i * TILE_WIDTH), 0, 0);
		}

		//Create more background if needed
		int more_background = (int)Mathf.Ceil(((float)Width - 2048f) / 1024f);
		if (more_background > 0)
		{
			for(int i=0;i<more_background;i++)
			{
				GameObject newBg;
				
				if (i%2==0)
					newBg = (GameObject)Instantiate(_Backgrounds[0].gameObject);
				else
					newBg = (GameObject)Instantiate(_Backgrounds[1].gameObject);
				
				newBg.name = "Background" + (3 + i);
				
				Transform newBgTransform = newBg.transform;
				newBgTransform.parent = _BackgroundsTransform;
				newBgTransform.position = new Vector3(2048 + (i * 1024), 768, 1);
			}
		}
	}

	public void IncreaseWorldSize()
	{
		Size += 1;
		CreateAdditions(1);
	}
}
