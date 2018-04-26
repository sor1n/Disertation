using System.Collections;
using UnityEngine;
using UnityEditor;

public class Utility : EditorWindow
{
	[MenuItem ("Tools/Utility")]
	public static void ShowWindow ()
	{
		EditorWindow.GetWindow (typeof(Utility));
	}

	void OnGUI ()
	{
		EditorGUILayout.LabelField ("Assign Link Wall", EditorStyles.boldLabel);
		GUILayout.BeginVertical ();
		if (GUILayout.Button ("Update links")) LinkObjects ();
		GUILayout.EndVertical ();
	}

	void LinkObjects ()
	{
		GameObject[] selected = Selection.gameObjects;
		GameObject link = null, wall = null, newWall = null;
		for (int i = 0; i < selected.Length; i++) {
			if (selected[i].name.Contains ("Link")) link = selected[i];
			wall = link.GetComponent<LinkScript> ().linkedWall;
			newWall = wall.transform.GetChild (0).gameObject;
			wall.transform.GetChild (0).SetParent (wall.transform.parent);
			link.GetComponent<LinkScript> ().linkedWall = newWall;
			DestroyImmediate (wall.gameObject);
		}
	}
}
