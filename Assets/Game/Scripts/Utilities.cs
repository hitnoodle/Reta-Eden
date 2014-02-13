using UnityEngine;
using System.Collections;

public class Utilities 
{
	public static GameObject CreateGameObject(string name)
	{
		return CreateGameObject(name, Vector3.zero, null);
	}

	public static GameObject CreateGameObject(string name, Vector3 position, Transform parent)
	{
		GameObject go = new GameObject();
		go.transform.position = position;
		go.name = name;

		if (parent != null)
			go.transform.parent = parent;

		return go;
	}
}
