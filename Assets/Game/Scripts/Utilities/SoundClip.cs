using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundClip {
	//Sound manager transform for attaching stuffs
	private static Transform Parent = null;
	
	//Name of the sound
	private string m_Name;
	public string Name {
		get { return m_Name; }
	}
	
	//Game object
	private GameObject m_SoundObject;
	public GameObject SoundObject {
		get { return m_SoundObject; }
	}
	
	//Source
	private AudioSource m_Source;
	
	//Volume
	private float m_Volume;
	public float Volume {
		get { return m_Volume; }
		set { 
			m_Volume = value;
			if (m_Source != null) m_Source.volume = m_Volume;
		}
	}
	
	//Loop or not
	private bool m_Loop;
	public bool Loop {
		get { return m_Loop; }
		set {
			m_Loop = value;
			if (m_Source != null) m_Source.loop = m_Loop;
		}
	}
	
	//Constructor
	public SoundClip(string name, string path) {
		//Find parent if haven't
		if (Parent == null) {
			Parent = GameObject.Find("SoundManager").transform;	
		}
		
		//Set name
		m_Name = name;

		//Create gameobject
		m_SoundObject = new GameObject();
		m_SoundObject.name = name;
		m_SoundObject.transform.position = Parent.position;
		m_SoundObject.transform.parent = Parent;
		
		//Create audiosource
		m_Source = m_SoundObject.AddComponent<AudioSource>();
		m_Source.clip = (AudioClip)Resources.Load(path + name, typeof(AudioClip));
		m_Source.playOnAwake = false;
	}
	
	public void Play() { 
		m_Source.Play(); 
	}
	
	public void Pause() {
		m_Source.Pause();
	}
	
	public void Stop(bool cleanup) { 
		//Stop
		m_Source.Stop(); 
		
		//Destroy object
		if (cleanup) Destroy();
	}
	
	//Whether source is playing 
	public bool IsPlaying() {
		return m_Source.isPlaying;
	}
	
	public bool IsExist()
	{
		if(m_Source == null)
			return false;
		else
			return true;
	}
	
	public void Destroy() {
		//Nullify source
		m_Source = null;
		
		//Destroy this gameobject and nullify it
		GameObject.DestroyObject(m_SoundObject);
		m_SoundObject = null;
	}
}