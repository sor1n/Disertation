using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSettings : MonoBehaviour
{
	public static LevelSettings settings;

	public List<GameObject> hallways = new List<GameObject> (), rooms = new List<GameObject> (), hallwaysLevel = new List<GameObject> (), roomsLevel = new List<GameObject> ();
	public int noOfHallways = 20, noOfRooms = 40, floors = 1, enemies = 1;

	void Awake ()
	{
		if (!settings) {
			DontDestroyOnLoad (gameObject);
			settings = this;
		} else if (settings != this) {
			Destroy (gameObject);
		}
	}

    public void BasicLevel()
    {
        for (int i = 0; i < hallways.Count - 2; i++) hallwaysLevel.Add(hallways[i]);
        foreach(GameObject obj in rooms) roomsLevel.Add(obj);
    }
}
