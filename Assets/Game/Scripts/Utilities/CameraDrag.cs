using UnityEngine;
using System.Collections;

public class CameraDrag : MonoBehaviour 
{
	public float MinX = 0;
	public float MaxX = 0;
	public float Speed = 400f;
	public float _SmoothTime = 2f;

	private Camera _Camera;
	private Transform _CameraTransform;

	private bool _IsDragging = false;
	private bool _UnderInertia = false;
	private float _Time = 0.0f;

	private Vector3 _TouchOrigin;
	private Vector3 _LastDragPosition;
	private float _CurrentSpeed;

#if UNITY_ANDROID
	private float _TouchBeganTime;
	private float _TouchDragTime = 0.15f;
#endif

	// Use this for initialization
	void Start() 
	{
		_Camera = GetComponent<Camera>();

		if (_Camera == null)
			_Camera = Camera.main;

		_CameraTransform = _Camera.transform;
	}

	void MoveCamera(float deltaX)
	{
		Vector3 move = new Vector3(deltaX * Speed, 0, 0);

		_CameraTransform.Translate(move, Space.Self);
		
		Vector3 currentPos = _CameraTransform.position;
		if (currentPos.x >= MaxX) 
		{
			currentPos.x = MaxX;
			_CameraTransform.position = currentPos;
		}
		else if (currentPos.x <= MinX) 
		{
			currentPos.x = MinX;
			_CameraTransform.position = currentPos;
		}
	}

	// Update is called once per frame
	void Update() 
	{
		//Pause
		if (Utilities.IS_PAUSED) return;

		if(_UnderInertia && _Time <= _SmoothTime)
		{
			MoveCamera(_CurrentSpeed * -1);
			_CurrentSpeed = Mathf.Lerp(_CurrentSpeed, 0, _Time / _SmoothTime);

			_Time += Time.deltaTime;
		}
		else
		{
			_UnderInertia = false;
			_Time = 0.0f;
		}

		if (Input.touchCount > 0)
		{
			foreach(Touch t in Input.touches)
			{
				if (t.phase == TouchPhase.Began)
				{
					_TouchOrigin = Input.mousePosition;
					_LastDragPosition = _TouchOrigin;
					
					_TouchBeganTime = Time.time;
				}
				else if (t.phase == TouchPhase.Moved)
				{
					if (!_IsDragging)
					{
						if (Time.time - _TouchBeganTime > _TouchDragTime)
						{
							_TouchOrigin = Input.mousePosition;
							_LastDragPosition = _TouchOrigin;

							_IsDragging = true;
							_UnderInertia = false;
						}
					}
					else
					{
						Vector3 pos = _Camera.ScreenToViewportPoint(Input.mousePosition - _LastDragPosition);
						
						_CurrentSpeed = pos.x;
						MoveCamera(_CurrentSpeed * -1);
						
						_LastDragPosition = t.position;
					}
				}
				else if (t.phase == TouchPhase.Ended)
				{
					if (_IsDragging)
					{
						_IsDragging = false;
						_UnderInertia = true;
					}
				}
			}
		}
		else
		{
			if(Input.GetMouseButtonDown(0))
			{
				_TouchOrigin = Input.mousePosition;
				_LastDragPosition = _TouchOrigin;
				
				_IsDragging = true;
				_UnderInertia = false;
			}
			
			if (!Input.GetMouseButton(0))
			{
				_IsDragging = false;
				_UnderInertia = true;
			}
			
			if (_IsDragging)
			{
				Vector3 pos = _Camera.ScreenToViewportPoint(Input.mousePosition - _LastDragPosition);
				
				_CurrentSpeed = pos.x;
				MoveCamera(_CurrentSpeed * -1);
				
				_LastDragPosition = Input.mousePosition;
			}
		}
	}

	public bool IsMoving()
	{
		bool move = false;

		if (Input.touchCount > 0)
		{
			move = _IsDragging;
		}
		else
		{
			move = _CurrentSpeed != 0f || _Time != 0f; 
		}

		return move;
	}
}
