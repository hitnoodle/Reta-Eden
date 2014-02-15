using UnityEngine;
using System.Collections;

public class Diamond : MonoBehaviour 
{
	#region Constants
	
	public const float START_Y = 280f;

	public const int MIN_SPAWN_TIME = 3;
	public const int MAX_SPAWN_TIME = 9;

	public static readonly int[] DIAMOND_SPAWN_RATES = 
	{
		55,
		20,
		20,
		5
	};

	public static readonly int[] DIAMOND_PRICE = 
	{
		50,
		150,
		150,
		1000
	};

	#endregion

	public enum DiamondType
	{
		Blue,
		Green,
		Red,
		Yellow
	};
	public DiamondType _Type = DiamondType.Blue;

	public delegate void _OnSpawnDone(Diamond diamond);
	public _OnSpawnDone OnSpawnDone;

	//Private access
	private Transform _Transform;
	private BoxCollider2D _Collider;
	private int _WaitDuration = 0;

	// Use this for initialization
	void Start () 
	{
		_Transform = transform;
		_Collider = GetComponent<BoxCollider2D>();
	}

	IEnumerator Wait()
	{
		yield return new WaitForSeconds(_WaitDuration);

		if (OnSpawnDone != null)
			OnSpawnDone(this);
	}

	public void Spawn(float buildingPosition)
	{
		_Transform = transform;
		_Collider = GetComponent<BoxCollider2D>();

		gameObject.SetActive(true);

		float pos = buildingPosition + ((Random.Range(10,30) - 20) * 30);
		_Transform.localPosition = new Vector3(pos, 0, 0);

		_WaitDuration= Random.Range(MIN_SPAWN_TIME, MAX_SPAWN_TIME);
		StartCoroutine(Wait());
	}
	
	public bool IsContainingPosition(Vector2 pos)
	{
		if (!_Collider.enabled) return false;
		
		return _Collider == Physics2D.OverlapPoint(pos);
	}
}
