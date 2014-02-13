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
	private const int TILE_WIDTH = 70;

	public WorldSeason Season = WorldSeason.Green;
	public int Size = 1;
	public int InitialTiles = 20;
	public int TilePerAddition = 10;

	public int Width
	{
		get 
		{
			return (20 + ((Size - 1) * TilePerAddition)) * TILE_WIDTH;
		}
	}

	private Transform _Transform;

	// Use this for initialization
	void Start() 
	{
		//Cache transform
		_Transform = transform;

		//Create current world
		CreateWorld();
	}

	void CreateWorld()
	{
		//Season 
		string season = Season.ToString();

		//Create Background
		GameObject backgroundsObject = (GameObject)Instantiate(Resources.Load("Prefabs/Backgrounds_" + season));
		backgroundsObject.name = "Backgrounds";
		backgroundsObject.transform.parent = _Transform;

		//Create Floors
		GameObject floorsObject = Utilities.CreateGameObject("Floors", Vector3.zero, _Transform);
		Transform floorsTransform = floorsObject.transform;

		//Initial tiles
		for (int i=0;i<InitialTiles;i++)
		{
			GameObject tilesObject = (GameObject)Instantiate(Resources.Load("Prefabs/FloorTiles_" + season));
			tilesObject.name = "FloorTiles";

			Transform tilesTransform = tilesObject.transform;
			tilesTransform.parent = floorsTransform;
			tilesTransform.position = new Vector3(0 + (i * TILE_WIDTH), 0, 0);
		}

		if (Size > 1)
		{
			int addition = (Size - 1) * TilePerAddition;
			int startX = InitialTiles * TILE_WIDTH;
			for (int i=0;i<addition;i++)
			{
				GameObject tilesObject = (GameObject)Instantiate(Resources.Load("Prefabs/FloorTiles_" + season));
				tilesObject.name = "FloorTiles";
				
				Transform tilesTransform = tilesObject.transform;
				tilesTransform.parent = floorsTransform;
				tilesTransform.position = new Vector3(startX + (i * TILE_WIDTH), 0, 0);
			}
		}
	}
}
