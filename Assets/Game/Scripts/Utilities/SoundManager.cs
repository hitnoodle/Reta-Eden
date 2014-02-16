using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* Manager class for handling sounds. Currently
 * only implement simple functions. 
 * */
public class SoundManager : MonoBehaviour {
	
	/* Static attributes */
	
	//Resource path
	private const string BGM_PATH = "Sounds/BGM/";
	private const string SFX_PATH = "Sounds/SFX/";

	private const int MAX_ACTIVE_SFX = 25;
	private const int MAX_ACTIVE_SFX_PER_SOUND = 10;
	
	//Singleton
	private static SoundManager m_Instance = null;
	
	/* Attributes */
	
	//Mute
	private static bool m_GlobalMuteBGM = false;
	public static bool GlobalMuteBGM
	{
		get { return m_GlobalMuteBGM; }
		set { 
			if (value && IsBackgroundMusicPlaying()) 
				StopBackgroundMusic(true);
			m_GlobalMuteBGM = value;
		}
	}
	
	private static bool m_GlobalMuteSFX = false;
	public static bool GlobalMuteSFX
	{
		get { return m_GlobalMuteSFX; }
		set { 
			if (value) 
				StopAllSoundEffects();
			m_GlobalMuteSFX = value;
		}
	}
	
	//Background music
	private SoundClip m_BackgroundMusic = null;
	
	private float m_BackgroundMusicVolume = 1.0f;
	public static float BackgroundMusicVolume {
		get { return m_Instance.m_BackgroundMusicVolume; }
		set { 
			m_Instance.m_BackgroundMusicVolume = value;
			if (m_Instance.m_BackgroundMusic != null) 
				m_Instance.m_BackgroundMusic.Volume = m_Instance.m_BackgroundMusicVolume;
		}
	}
	
	private bool isFade = false;
	private float m_FadeSpeed = 0;
	private float m_FadeVolume = 0;
	
	public delegate void OnFadeBackgroundMusicIn();
	private OnFadeBackgroundMusicIn m_OnFadeBackgroundMusicIn;
	
	public static OnFadeBackgroundMusicIn onFadeBackgroundMusicIn {
		get { return m_Instance.m_OnFadeBackgroundMusicIn; }
		set { m_Instance.m_OnFadeBackgroundMusicIn = value;}
	}
	
	public delegate void OnFadeBackgroundMusicOut();
	private OnFadeBackgroundMusicOut m_OnFadeBackgroundMusicOut;
	
	public static OnFadeBackgroundMusicOut onFadeBackgroundMusicOut {
		get { return m_Instance.m_OnFadeBackgroundMusicOut; }
		set { m_Instance.m_OnFadeBackgroundMusicOut = value;}
	}
	
	//Sound effects
	private List<string> m_Sounds = new List<string>();
	
	private Dictionary<string, List<SoundClip>> m_SoundEffectPool = new Dictionary<string, List<SoundClip>>();
	private Dictionary<string, List<SoundClip>> m_SoundEffectActive = new Dictionary<string, List<SoundClip>>();
	
	private List<SoundClip> m_SoundEffectOneShot = new List<SoundClip>();
	private List<SoundClip> m_SoundEffectToDestroy = new List<SoundClip>();
	
	private float m_SoundEffectVolume = 1.0f;
	public static float SoundEffectVolume {
		get { return m_Instance.m_SoundEffectVolume; }
		set {
			m_Instance.m_SoundEffectVolume = value;
			
			//For all sounds
			foreach(string s in m_Instance.m_Sounds) {		
				//Get pool
				List<SoundClip> clips = m_Instance.m_SoundEffectPool[s];
				List<SoundClip> clips_active = m_Instance.m_SoundEffectActive[s];
				
				//Change all volume in pool
				if (clips != null) {
					foreach(SoundClip clip in clips) {
						clip.Volume = m_Instance.m_SoundEffectVolume;
					}
				}		
			
				//Change all volume in active 
				if (clips_active != null) {
					foreach(SoundClip clip in clips_active) {
						clip.Volume = m_Instance.m_SoundEffectVolume;
					}
				}
			}
		}
	}
	
	private int m_ActiveSound = 0;
	
	/* Protected mono methods */
	
	protected void Awake() {
		//Set instance
		m_Instance = this;
	}
	
	protected void OnDestroy() {
		//Remove
		RemoveAllResources();
		
		//Nullify singleton
		m_Instance = null;
	}
	
	protected void Update() {
		//Update fading background music
		if (isFade) {
			//Change background music volume
			BackgroundMusicVolume = BackgroundMusicVolume + (m_FadeSpeed * Time.deltaTime);
			//Debug.Log("Sound manager now fading.");
			
			//Stop fading if already in or out
			if (m_FadeSpeed > 0 && BackgroundMusicVolume >= m_FadeVolume) {
				//Debug.Log("Sound manager fade in done.");
				
				//Clamp
				BackgroundMusicVolume = m_FadeVolume;
				
				//Reset
				isFade = false;
				m_FadeSpeed = 0;

				//Run delegate
				if (m_OnFadeBackgroundMusicIn != null) m_OnFadeBackgroundMusicIn();
			} else if (m_FadeSpeed < 0 && BackgroundMusicVolume <= m_FadeVolume) {
				//Debug.Log("Sound manager fade out done.");
				
				//Clamp
				BackgroundMusicVolume = m_FadeVolume;
				
				//Reset
				isFade = false;
				m_FadeSpeed = 0;

				//Run delegate
				if (m_OnFadeBackgroundMusicOut != null) m_OnFadeBackgroundMusicOut();
			}
		}
		
		//Update sound effect active
		foreach(KeyValuePair<string, List<SoundClip>> kv in m_SoundEffectActive) {
			List<SoundClip> list = kv.Value;
			foreach(SoundClip s in list) {
				if (!s.IsPlaying()) {
					//Put it back into pool
					m_SoundEffectPool[kv.Key].Add(s);
					m_SoundEffectToDestroy.Add(s);
				}
			}
			
			foreach(SoundClip s in m_SoundEffectToDestroy) {
				//Delete and make it inactive
				list.Remove(s);
				s.SoundObject.SetActive(false);
				m_ActiveSound--;
			}
			
			m_SoundEffectToDestroy.Clear();
		}
		
		//Update single shot sound effect
		foreach(SoundClip s in m_SoundEffectOneShot) {
			if (!s.IsPlaying()) {
				m_SoundEffectToDestroy.Add(s);
			}
		}
		
		foreach(SoundClip s in m_SoundEffectToDestroy) {
			m_SoundEffectOneShot.Remove(s);
			s.Destroy();
			m_ActiveSound--;
		}
		
		m_SoundEffectToDestroy.Clear();
	}
	
	/* Private methods */

	private bool IsSoundExists(string sfx) {
		//Search in sound list
		foreach(string s in m_Sounds) {
			if (s == sfx) return true;
		}
		
		//Don't find anything
		return false;
	}
	
	/* Public static methods */
	
	#region Background Musics
	
	//Play background music 
	public static bool PlayBackgroundMusic(string name, bool loop) {
		//Don't do shit if mute
		if (GlobalMuteBGM) return false;
		
		//Check BGM
		if (m_Instance.m_BackgroundMusic != null && m_Instance.m_BackgroundMusic.Name != name) {
			//Stop and clean
			StopBackgroundMusic(true);
		}
		
		//Create new bgm if needed
		if (m_Instance.m_BackgroundMusic == null) {
			//Create
			SoundClip s = new SoundClip(name, BGM_PATH);
			m_Instance.m_BackgroundMusic = s;
		}
		
		//Set attributes
		m_Instance.m_BackgroundMusic.Loop = loop;
		m_Instance.m_BackgroundMusic.Volume = m_Instance.m_BackgroundMusicVolume;
		
		//Play
		m_Instance.m_BackgroundMusic.Play();
		
		//Return
		return true;
	}
	
	//Check whether background music is currently playing
	public static bool IsBackgroundMusicPlaying() {
		//Don't do shit if mute
		if (GlobalMuteBGM) return false;
		
		if (m_Instance.m_BackgroundMusic != null) {
			return m_Instance.m_BackgroundMusic.IsPlaying();
		}
		
		return false;
	}
	
	//Resume playing background music
	public static bool ResumeBackgroundMusic() {
		//Don't do shit if mute
		if (GlobalMuteBGM) return false;
		
		if (m_Instance.m_BackgroundMusic != null) {
			m_Instance.m_BackgroundMusic.Play();
			return true;
		}
		
		return false;
	}
	
	//Pause background music
	public static bool PauseBackgroundMusic() {
		//Don't do shit if mute
		if (GlobalMuteBGM) return false;
		
		if (m_Instance.m_BackgroundMusic != null) {
			m_Instance.m_BackgroundMusic.Pause();
			return true;
		}
		
		return false;
	}
	
	//Stop background music and cleanup if needed
	public static bool StopBackgroundMusic(bool cleanup) {
		if (m_Instance.m_BackgroundMusic != null) {
			m_Instance.m_BackgroundMusic.Stop(cleanup);
			m_Instance.m_BackgroundMusic = null;
			return true;
		}

		return false;
	}
	
	public static bool FadeBackgroundMusicIn(float time) {
		return FadeBackgroundMusicIn(time, 1.0f);
	}
	
	public static bool FadeBackgroundMusicIn(float time, float volume) {
		return FadeBackgroundMusic(time, m_Instance.m_BackgroundMusicVolume, volume);
	}
	
	public static bool FadeBackgroundMusicOut(float time) {
		return FadeBackgroundMusicOut(time, 0f);
	}
	
	public static bool FadeBackgroundMusicOut(float time, float volume) {
		return FadeBackgroundMusic(time, m_Instance.m_BackgroundMusicVolume, volume);
	}
	
	public static bool FadeBackgroundMusic(float time, float _from, float _to) {
		//Debug.Log("BGM will be faded from " + _from + " to " + _to + " in " + time + " seconds.");
		
		//Don't do shit if mute
		if (GlobalMuteBGM) return false;
		
		if (m_Instance.m_BackgroundMusic != null) {
			m_Instance.m_FadeVolume = _to;
			m_Instance.m_FadeSpeed = (_to - _from) / time;
			m_Instance.isFade = true;
		}
		
		return m_Instance.isFade;
	}
	
	#endregion
	
	#region Sound efffects
	
	//Play and destroy
	public static bool PlaySoundEffectOneShot(string name) {
		return PlaySoundEffectOneShot(name, false, SoundEffectVolume);
	}
	
	//Play with loop, must stop manually
	public static bool PlaySoundEffectOneShot(string name, bool loop, float volume) {
		//Don't do shit if mute
		if (GlobalMuteSFX) return false;
		
		//Check total count, play nothing if > max
		if (m_Instance.m_ActiveSound >= MAX_ACTIVE_SFX) return false;
		
		//Check first if looping, don't create again if already there
		if (loop) {
			foreach(SoundClip clip in m_Instance.m_SoundEffectOneShot) {
				if (clip.Name == name) {
					//Play again
					clip.Play();
					
					//Return here
					return true;
				}
			}
		}
		
		//Create new sound clip
		SoundClip s = new SoundClip(name, SFX_PATH);
		s.Volume = volume;
		s.Loop = loop;
		
		//Add to list and play
		m_Instance.m_SoundEffectOneShot.Add(s);
		s.Play();
		
		//Add number
		m_Instance.m_ActiveSound++;
		
		return true;
	}
	
	//Get latest sound clip oneshot
	public static SoundClip GetSoundEffectOneShot(string name) {
		int len = m_Instance.m_SoundEffectOneShot.Count - 1;
		for(int i=len;i>=0;i--) {
			SoundClip s = m_Instance.m_SoundEffectOneShot[i];
			if (s.Name == name) return s;
		}
		
		return null;
	}
	
	//Default is play the effect and put it back into pool
	public static bool PlaySoundEffect(string name) {
		//Don't do shit if mute
		if (GlobalMuteSFX) return false;
		
		//Check total count, play nothing if > max
		if (m_Instance.m_ActiveSound >= MAX_ACTIVE_SFX) return false;
		
		//Check first and preload if needed
		if (!m_Instance.IsSoundExists(name)) PreloadSoundEffect(name, 1);
		
		//Get list from pool
		List<SoundClip> clips = m_Instance.m_SoundEffectPool[name];
		List<SoundClip> clips_active = m_Instance.m_SoundEffectActive[name];
		
		//Check total count per active
		if (clips_active.Count >= MAX_ACTIVE_SFX_PER_SOUND) return false;
		
		//Add another if needed
		if (clips.Count == 0) PreloadSoundEffect(name, 1);
		
		//Get the last one
		SoundClip clip = clips[clips.Count - 1];
		clips.Remove(clip);
		
		//Set to active
		clip.SoundObject.SetActive(true);
		clips_active.Add(clip);
		
		//Play
		clip.Volume = SoundEffectVolume;
		clip.Play();
		
		m_Instance.m_ActiveSound++;
		return true;
	}
	
	//Prefabricate sound effect object at certain number
	public static bool PreloadSoundEffect(string name, int objectCount) {
		//Don't do shit if mute
		if (GlobalMuteSFX) return false;
		
		//Check
		if (!m_Instance.IsSoundExists(name)) {
			//Create entry in pool
			m_Instance.m_SoundEffectPool.Add(name, new List<SoundClip>());
			m_Instance.m_SoundEffectActive.Add(name, new List<SoundClip>());
			
			//Add to sounds
			m_Instance.m_Sounds.Add(name);	
		}
		
		//Get pool
		List<SoundClip> pool = m_Instance.m_SoundEffectPool[name];
		
		//Add new soundclip
		for(int i=0;i<objectCount;i++) {
			//Create
			SoundClip s = new SoundClip(name, SFX_PATH);
			pool.Add(s);
		
			//Make all inactive
			s.SoundObject.SetActive(false);
		}
		
		return true;
	}
	
	//Using pause, destroy will take place on update
	public static void StopSoundEffect(string name) {
		//Stop effect in pool
		foreach(KeyValuePair<string, List<SoundClip>> kv in m_Instance.m_SoundEffectActive) {
			if (kv.Key == name) {
				List<SoundClip> list = kv.Value;
				foreach(SoundClip s in list) s.Pause();
			}
		}
		
		//Stop effect one shot
		foreach(SoundClip s in m_Instance.m_SoundEffectOneShot) 
			if (s.Name == name) s.Pause();
	}
	
	public static void StopAllSoundEffects() {
		//Stop all effect in pool
		foreach(KeyValuePair<string, List<SoundClip>> kv in m_Instance.m_SoundEffectActive) {
			List<SoundClip> list = kv.Value;
			foreach(SoundClip s in list) s.Pause();
		}
		
		//Stop all effect one shot
		foreach(SoundClip s in m_Instance.m_SoundEffectOneShot) s.Pause();
	}
	
	#endregion
	
	#region Resources
	
	//Removing all sound resources
	public static void RemoveAllResources() {
		//BGM
		RemoveBackgroundMusic();
		
		//SFX
		RemoveSoundEffects();
	}
	
	//Only remove bgm resource
	public static void RemoveBackgroundMusic() {
		if (m_Instance.m_BackgroundMusic != null) {
			m_Instance.m_BackgroundMusic.Destroy();
			m_Instance.m_BackgroundMusic = null;
		}
	}
	
	//Only remove sfx resources
	public static void RemoveSoundEffects() {
		//Delete all sound clip from pool
		if (m_Instance.m_SoundEffectPool != null) {
			foreach(KeyValuePair<string, List<SoundClip>> k in m_Instance.m_SoundEffectPool) {
				foreach(SoundClip s in k.Value) {
					s.Destroy();
				}
			}
			m_Instance.m_SoundEffectPool.Clear();
			m_Instance.m_SoundEffectPool = null;
		}
		
		//Delete all sound clip from active
		if (m_Instance.m_SoundEffectActive != null) {
			foreach(KeyValuePair<string, List<SoundClip>> k in m_Instance.m_SoundEffectActive) {
				foreach(SoundClip s in k.Value) {
					s.Destroy();
				}
			}
			m_Instance.m_SoundEffectActive.Clear();
			m_Instance.m_SoundEffectActive = null;
		}
		
		//Delete all strings
		if (m_Instance.m_Sounds != null) {
			m_Instance.m_Sounds.Clear();
			m_Instance.m_Sounds = null;
		}
	}
	
	#endregion
}
