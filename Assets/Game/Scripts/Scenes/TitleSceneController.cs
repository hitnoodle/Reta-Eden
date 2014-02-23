using UnityEngine;
using System.Collections;
using RetaClient;

public class TitleSceneController : MonoBehaviour 
{
	public float WaitDuration = 2f;

	// Use this for initialization
	void Start() 
	{
		ProfileManager.Instance.Load();

		Reta.Instance.SetApplicationVersion("0.1");
		//Reta.Instance.SetDebugMode(true);
		//Reta.Instance.Disable();

		StartCoroutine(NextScene());
	}

	IEnumerator NextScene()
	{
		yield return new WaitForSeconds(WaitDuration);

		Reta.Instance.ProcessEvents();

		if (ProfileManager.Instance.TutorialPlayed)
			Application.LoadLevel("MainScene");
		else 
			Application.LoadLevel("TutorialScene");
	}
}
