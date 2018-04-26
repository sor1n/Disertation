using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndScript : MonoBehaviour {
	private GameObject player, gameController;

	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player");
		gameController = GameObject.FindGameObjectWithTag ("GameController");		
	}

	void OnTriggerEnter (Collider col)
	{
		if (col.transform.tag.Contains ("Player")) {
			gameController.GetComponent<GameController> ().gameWon = true;
			player.GetComponent<CharacterController> ().enabled = false;
		}
	}
}
