using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
	private NavMeshAgent agent;
	private GameObject player, gameController;
	private Vector3 dest = Vector3.zero, targetPos;
	private AudioSource source;
	public AudioClip walk, chase;

	void Start ()
	{
		agent = GetComponent<NavMeshAgent> ();
		player = GameObject.FindGameObjectWithTag ("Player");
		gameController = GameObject.FindGameObjectWithTag ("GameController");
		source = GetComponent<AudioSource> ();
	}

	void Update ()
	{
		GetComponent<NavMeshAgent> ().enabled = !player.GetComponent<FirstPersonController> ().IsCreative ();

		if (agent != null && agent.enabled) {
			if (!agent.hasPath) targetPos = gameController.GetComponent<GameController> ().RandomPoint (transform.position, 20f);
			if (!player.GetComponent<FirstPersonController> ().IsHiding () && Vector3.Distance (player.transform.position, transform.position) <= 10f) targetPos = player.transform.position;
			/*else if ((agent.path.status == NavMeshPathStatus.PathComplete && agent.remainingDistance < 1) ||
			         agent.path.status == NavMeshPathStatus.PathInvalid ||
			         agent.path.status == NavMeshPathStatus.PathPartial ||
			         !agent.hasPath) targetPos = gameController.GetComponent<GameController> ().RandomPoint (transform.position, 20f);	*/
			agent.destination = targetPos;
		}

		if (targetPos == player.transform.position && source.clip != chase) source.clip = chase;
		else if (targetPos != player.transform.position && source.clip != walk) source.clip = walk;
		if (!source.isPlaying) source.Play ();
	}

	void OnTriggerEnter (Collider col)
	{
		if (col.transform.tag.Contains ("Player")) {
			gameController.GetComponent<GameController> ().gameOver = true;
			player.GetComponent<CharacterController> ().enabled = false;
		}
	}
}
