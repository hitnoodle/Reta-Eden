using UnityEngine;
using System.Collections;

public class TitleSceneController : MonoBehaviour 
{
	public float WaitDuration = 2f;

	// Use this for initialization
	void Start() 
	{
		StartCoroutine(GoToMain());
	}

	IEnumerator GoToMain()
	{
		yield return new WaitForSeconds(WaitDuration);

		Application.LoadLevel("MainScene");
	}
}
