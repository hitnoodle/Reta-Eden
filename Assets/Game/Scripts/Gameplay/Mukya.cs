using UnityEngine;
using System.Collections;

// Base sims class
public class Mukya : MonoBehaviour 
{
	#region Constants

	public const float START_Y = 280f;

	private const float MAX_STATUS = 100f;
	private const float MIN_MOVE = 100f;

	private const float MIN_SPEED = 50f;
	private const float MAX_SPEED = 100f;
	
	private const float STATUS_DECREASE_MIN = 0.5f; //Decrease status over time
	private const float STATUS_DECREASE_MAX = 1f;
	
	private const float PENALTY_THRESHOLD = 10f; //Percentage where multiplier active
	private const float DECREASE_MULTIPLIER = 1.5f; //Multiply decrease status

	public const int MUKYA_PRICE = 1000;
	public const int PRICE_INFLATION = 500;

	#endregion

	#region Shared Mukya Attributes

	public static float START_X = 50f;
	public static float END_X = 974f;

	#endregion

	#region Unique Attributes and Status

	public enum MukyaRace
	{
		Ghost,
		Beige,
		Blue,
		Green,
		Pink,
		Yellow
	}
	public MukyaRace _Race = MukyaRace.Ghost;

	public float _EnergyStatus = MAX_STATUS;
	public float _SocialStatus = MAX_STATUS;
	public float _WorkStatus = MAX_STATUS;

	#endregion

	//Private access
	private tk2dSprite _Sprite;
	private tk2dSpriteAnimator _Animator;
	private BoxCollider2D _Collider;
	private Transform _Transform;

	private bool _IsMoving;
	private float _Speed;
	private float _MoveDestination;
	
	public delegate void _OnMoveDone(Mukya mukya);
	public _OnMoveDone OnMoveDone;

	private bool _Penalty;
	private float[] _StatusDecrease;

	private bool _IsDead;

	#region Mono Behavior

	// Use this for initialization
	void Awake() 
	{
		_Sprite = GetComponent<tk2dSprite>();
		_Animator = GetComponent<tk2dSpriteAnimator>();
		_Collider = GetComponent<BoxCollider2D>();
		_Transform = transform;

		_IsMoving = false;
		_Speed = 0f;

		_Penalty = false;
		_StatusDecrease = new float[3];
		for(int i=0;i<_StatusDecrease.Length;i++)
			_StatusDecrease[i] = Random.Range(STATUS_DECREASE_MIN, STATUS_DECREASE_MAX);

		_IsDead = false;
	}

	void Start()
	{
		MoveAround();
	}
	
	// Update is called once per frame
	void Update() 
	{
		//Pause
		if (Utilities.IS_PAUSED) 
		{
			//Pause animation if needed
			if (!_Animator.Paused) _Animator.Pause();

			return;
		}

		//Resume animation if needed
		if (_Animator.Paused) _Animator.Resume();

		//Always decrease status
		if (_Race != MukyaRace.Ghost) DecreaseStatus(Time.deltaTime);

		//Move, baby
		if (_IsMoving)
		{
			Vector3 currentPos = _Transform.position;
			_Transform.position = new Vector3(currentPos.x + (_Speed * Time.deltaTime), currentPos.y, currentPos.z);

			bool doneMove = false;
			if (!_Sprite.FlipX)
			{
				if (currentPos.x >= _MoveDestination) doneMove = true;
			}
			else
			{
				if (currentPos.x <= _MoveDestination) doneMove = true;
			}

			if (doneMove)
			{
				_IsMoving = false;
				_Transform.position = new Vector3(_MoveDestination, currentPos.y, currentPos.z);
				
				if (OnMoveDone != null)
					OnMoveDone(this);
			}
		}
	}

	#endregion


	public bool IsContainingPosition(Vector2 pos)
	{
		if (!_Collider.enabled) return false;
		if (_Race == MukyaRace.Ghost) return false;

		return _Collider == Physics2D.OverlapPoint(pos);
	}

	public float Position()
	{
		return _Transform.position.x;
	}

	public void SetPosition(float pos)
	{
		Vector3 oldPos =_Transform.position;
		_Transform.position = new Vector3(pos, oldPos.y, oldPos.z);
	}

	public float Destination()
	{
		return _MoveDestination;
	}

	#region State functions

	IEnumerator Wait()
	{
		int wait = Random.Range(1,4);
	
		yield return new WaitForSeconds(wait);

		MoveAround();
	}

	void MoveAround()
	{
		float destination = Random.Range(START_X, END_X);
		while (Mathf.Abs(_Transform.position.x - destination) < MIN_MOVE)
			destination = Random.Range(START_X, END_X);
		
		Move(destination);
		OnMoveDone += Idle;
	}
	
	public void Idle(Mukya mukya)
	{
		OnMoveDone -= Idle;

		_Animator.Play("idle");

		StartCoroutine(Wait());
	}

	public void None()
	{
		_Sprite.renderer.enabled = false;
		_Collider.enabled = false;
	}

	public void UnNone()
	{
		_Sprite.renderer.enabled = true;
		_Collider.enabled = true;

		_IsMoving = false;
		Idle(this);
	}

	public bool Move(float destination)
	{
		//Outside screen
		if (destination < START_X || destination > END_X) return false;

		//Face left if the destination is on the left, duh
		_Sprite.FlipX = (Position() > destination);

		//Set speed and where to go
		_Speed = Random.Range(MIN_SPEED, MAX_SPEED);
		if (_Sprite.FlipX) _Speed *= -1;

		//Move
		_IsMoving = true;
		_MoveDestination = destination;
		_Animator.Play("walk");

		return true;
	}

	#endregion

	#region Status functions

	void DecreaseStatus(float deltaTime)
	{
		_EnergyStatus -= _StatusDecrease[0] * deltaTime;
		if (_EnergyStatus <= 0) _EnergyStatus = 0;

		_SocialStatus -= _StatusDecrease[1] * deltaTime;
		if (_SocialStatus <= 0) _SocialStatus = 0;

		_WorkStatus -= _StatusDecrease[2] * deltaTime;
		if (_WorkStatus <= 0) _WorkStatus = 0;

		if (EnergyPercentage() <= PENALTY_THRESHOLD || SocialPercentage() <= PENALTY_THRESHOLD || WorkPercentage() <= PENALTY_THRESHOLD)
		{
			//Penalty
			if (!_Penalty)
			{
				for(int i=0;i<_StatusDecrease.Length;i++)
					_StatusDecrease[i] *= DECREASE_MULTIPLIER;

				_Penalty = true;
			}
			else
			{
				//Dead.. fucking dead
				if (!_IsDead)
				{
					if (_EnergyStatus == 0 && _SocialStatus == 0 && _WorkStatus == 0)
					{
						gameObject.SetActive(false);

						MainSceneController.AddNewMukya(MukyaRace.Ghost, Position());
						MainSceneController.RemoveMukya(this);
						
						SoundManager.PlaySoundEffectOneShot("meow2");
						
						_IsDead = true;
					}
				}
			}
		}
		else
		{
			//Reset decrease
			if (_Penalty) 
			{
				for(int i=0;i<_StatusDecrease.Length;i++)
					_StatusDecrease[i] = Random.Range(STATUS_DECREASE_MIN, STATUS_DECREASE_MAX);

				_Penalty = false;
			}
		}
	}

	public float EnergyPercentage() 
	{ 
		return _EnergyStatus / MAX_STATUS * 100f; 
	}

	public float SocialPercentage() 
	{ 
		return _SocialStatus / MAX_STATUS * 100f; 
	}

	public float WorkPercentage() 
	{ 
		return _WorkStatus / MAX_STATUS * 100f; 
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
