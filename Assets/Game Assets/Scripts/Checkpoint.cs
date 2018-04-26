using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
	public bool reached = false;

	void OnTriggerEnter (Collider col)
	{
		if (col.tag.Contains ("Player")) reached = true;
	}
}
