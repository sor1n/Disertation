using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkScript : MonoBehaviour
{
	public bool canLinkRoom, canLinkHallway;
	private bool isParentRoom = false;
	public GameObject linkedWall;

	void Awake ()
	{
		if (transform.parent.name.Contains ("Room")) isParentRoom = true;
	}
	/*
	void OnTriggerEnter (Collider col)
	{
		if (linkedWall != null && !isParentRoom) Destroy (linkedWall);
		Destroy (gameObject);
	}

	void OnTriggerStay (Collider col)
	{
		if (linkedWall != null && !isParentRoom) Destroy (linkedWall);
		Destroy (gameObject);
	}*/
}
