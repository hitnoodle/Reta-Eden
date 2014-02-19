using UnityEngine;
using System.Collections;

public class TutorialSceneController : MonoBehaviour 
{
	private readonly string[] TUTORIAL_TITLES = 
	{
		"Welcome",
		"Apocalypse",
		"New Home",
		"Venture",
		"Gathering",
		"Expand",
		"You're not Alone",
	};

	public tk2dTextMesh Title;
	public tk2dTextMesh PageNumber;

	public GameObject[] Pages;

	public tk2dUIItem ArrowLeft;
	public tk2dUIItem ArrowRight;

	private int _PageIndex = 0;
	private bool _OnceMore = false;

	// Use this for initialization
	void Start() 
	{
		ShowPage(_PageIndex);

		StartCoroutine(StartBackgroundMusic());
	}

	IEnumerator StartBackgroundMusic()
	{
		yield return new WaitForSeconds(0.1f);

		SoundManager.BackgroundMusicVolume = 0.5f;
		SoundManager.PlayBackgroundMusic("hometown", true);
	}

	void OnceMore()
	{
		ArrowRight.OnClick -= OnceMore;
		ArrowRight.OnClick += MainScene;

		PageNumber.text = "Next once more to play!";
		_OnceMore = true;

		SoundManager.PlaySoundEffectOneShot("button_confirm_again");
	}

	void MainScene()
	{
		ProfileManager.Instance.TutorialPlayed = true;
		ProfileManager.Instance.Save();

		Application.LoadLevel("MainScene");

		SoundManager.PlaySoundEffectOneShot("button_confirm_again");
	}

	void NextPage()
	{
		ArrowLeft.OnClick -= PrevPage;
		ArrowRight.OnClick -= NextPage;
		
		ShowPage(_PageIndex + 1);
		
		SoundManager.PlaySoundEffectOneShot("button_confirm");
	}

	void PrevPage()
	{
		if (_PageIndex == Pages.Length - 1)
		{
			if (_OnceMore)
				ArrowRight.OnClick -= MainScene;
			else
				ArrowRight.OnClick -= OnceMore;

			_OnceMore = false;
		}

		ArrowLeft.OnClick -= PrevPage;
		ArrowRight.OnClick -= NextPage;
		
		ShowPage(_PageIndex - 1);
		
		SoundManager.PlaySoundEffectOneShot("button_confirm");
	}

	void ShowPage(int index)
	{
		int len = Pages.Length;
		
		if (index < 0 || index >= len)
			return;
		
		for(int i=0;i<len;i++)
			Pages[i].SetActive(false);

		StartCoroutine(MakePageActive(index));
		ShowTexts(index);
		
		if (index == 0)
		{
			ArrowLeft.gameObject.SetActive(false);
			ArrowRight.gameObject.SetActive(true);
			
			ArrowRight.OnClick += NextPage;
		}
		else if (index == len - 1)
		{
			ArrowRight.OnClick += OnceMore;
			ArrowLeft.OnClick += PrevPage;
		}
		else
		{
			ArrowLeft.gameObject.SetActive(true);
			ArrowRight.gameObject.SetActive(true);
			
			ArrowLeft.OnClick += PrevPage;
			ArrowRight.OnClick += NextPage;
		}
		
		_PageIndex = index;
	}

	IEnumerator MakePageActive(int index)
	{
		yield return new WaitForSeconds(0.1f);

		Pages[index].SetActive(true);
	}

	void ShowTexts(int page)
	{
		Title.text = TUTORIAL_TITLES[page];
		PageNumber.text = page + 1 + "/" + Pages.Length;
	}
}
