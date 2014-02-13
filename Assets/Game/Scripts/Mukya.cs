using UnityEngine;
using System.Collections;

// Base sims class
public class Mukya : MonoBehaviour 
{
	#region Constants

	public const float START_Y = 280f;

	private const string DEFAULT_NAME = "Mukya";	
	private const float MAX_STATUS = 100f;

	private const float MIN_MOVE = 100f;

	private const float MIN_SPEED = 50f;
	private const float MAX_SPEED = 100f;
	
	private const float STATUS_DECREASE_MIN = 0.25f; //Decrease status over time
	private const float STATUS_DECREASE_MAX = 0.5f;
	
	private const float PENALTY_THRESHOLD = 10f; //Percentage where multiplier active
	private const float DECREASE_MULTIPLIER = 1.5f; //Multiply decrease status

	#endregion

	#region Shared Mukya Attributes

	public static float START_X = 50f;
	public static float END_X = 974f;

	#endregion

	#region State Handling

	private enum MukyaState
	{
		None,	//Don't do shit (ex: at building)
		Idle,	//Stop -> moving around -> stop
		Moving,	//Moving to certain destination
		Talking	//Talking with another Mukya
	}
	private MukyaState _CurrentState = MukyaState.None;

	public delegate void _OnMoveDone();
	public _OnMoveDone OnMoveDone;

	private float _Speed;
	private float _MoveDestination;

	#endregion

	#region Unique Attributes and Status

	public enum MukyaRace
	{
		None,
		Beige,
		Blue,
		Green,
		Pink,
		Yellow
	}
	public MukyaRace _Race = MukyaRace.None;

	public string _Name = DEFAULT_NAME;
	public float _EnergyStatus = MAX_STATUS;
	public float _SocialStatus = MAX_STATUS;
	public float _WorkStatus = MAX_STATUS;

	#endregion

	//Private access
	private tk2dSprite _Sprite;
	private tk2dSpriteAnimator _Animator;
	private Transform _Transform;

	#region Mono Behavior

	// Use this for initialization
	void Awake() 
	{
		_Sprite = GetComponent<tk2dSprite>();
		_Animator = GetComponent<tk2dSpriteAnimator>();

		_Transform = transform;
	}

	void Start()
	{
		float destination = Random.Range(START_X, END_X);
		while (Mathf.Abs(_Transform.position.x - destination) < MIN_MOVE)
			destination = Random.Range(START_X, END_X);
		
		Move(destination);
		OnMoveDone += Idle;
	}
	
	// Update is called once per frame
	void Update() 
	{
		//Lazy state handling
		if (_CurrentState == MukyaState.None)
		{

		}
		else if (_CurrentState == MukyaState.Idle)
		{

		}
		else if (_CurrentState == MukyaState.Moving)
		{
			Vector3 currentPos = _Transform.position;
			currentPos.x += _Speed * Time.deltaTime;

			_Transform.position = new Vector3(currentPos.x, currentPos.y, currentPos.z);
			if ((_Sprite.FlipX && currentPos.x <= _MoveDestination) 
			    || (!_Sprite.FlipX && currentPos.x >= _MoveDestination))
			{
				_Transform.position = new Vector3(_MoveDestination, currentPos.y, currentPos.z);

				if (OnMoveDone != null)
					OnMoveDone();
			}
		}
		else if (_CurrentState == MukyaState.Talking)
		{
			
		}
	}

	#endregion

	#region State functions

	public void Idle()
	{
		OnMoveDone -= Idle;

		StopAllCoroutines();
		StartCoroutine(MoveAround());

		_Animator.Play("idle");
	}

	IEnumerator MoveAround()
	{
		int wait = Random.Range(1,4);

		yield return new WaitForSeconds(wait);

		float destination = Random.Range(START_X, END_X);
		while (Mathf.Abs(_Transform.position.x - destination) < MIN_MOVE)
			destination = Random.Range(START_X, END_X);

		Move(destination);
		OnMoveDone += Idle;
	}

	public bool Move(float destination)
	{
		//Outside screen
		if (destination < START_X || destination > END_X)
			return false;

		Vector3 currentPos = _Transform.position;
		_Sprite.FlipX = false;

		//Need flip?
		if (currentPos.x > destination)
			_Sprite.FlipX = true;

		//Set speed
		_Speed = Random.Range(MIN_SPEED, MAX_SPEED);
		if (_Sprite.FlipX) 
			_Speed *= -1;

		//Move
		_CurrentState = MukyaState.Moving;
		_MoveDestination = destination;
		_Animator.Play("walk");

		return true;
	}

	#endregion

	#region Status functions

	public float EnergyPercentage() 
	{ 
		return _EnergyStatus / MAX_STATUS; 
	}

	public float SocialPercentage() 
	{ 
		return _SocialStatus / MAX_STATUS; 
	}

	public float WorkPercentage() 
	{ 
		return _WorkStatus / MAX_STATUS; 
	}

	public void IncreaseEnergy(float energy)
	{
		_EnergyStatus += energy;
		if (_EnergyStatus >= MAX_STATUS) _EnergyStatus = MAX_STATUS;
	}

	public void IncreaseSocial(float social)
	{
		_SocialStatus += social;
		if (_SocialStatus >= MAX_STATUS) _SocialStatus = MAX_STATUS;
	}

	public void IncreaseWork(float work)
	{
		_WorkStatus += work;
		if (_WorkStatus >= MAX_STATUS) _WorkStatus = MAX_STATUS;
	}

	#endregion
}
