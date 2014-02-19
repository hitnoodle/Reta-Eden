using UnityEngine;
using System.Collections;

public class TitleSceneController : MonoBehaviour 
{
	public float WaitDuration = 2f;

	// Use this for initialization
	void Start() 
	{
		ProfileManager.Instance.Load();

		StartCoroutine(NextScene());
	}

	IEnumerator NextScene()
	{
		yield return new WaitForSeconds(WaitDuration);

		if (ProfileManager.Instance.TutorialPlayed)
			Application.LoadLevel("MainScene");
		else 
			Application.LoadLevel("TutorialScene");
	}
}
