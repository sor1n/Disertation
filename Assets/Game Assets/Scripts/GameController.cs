using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEditor;
using UnityStandardAssets.Characters.FirstPerson;

public class GameController : MonoBehaviour
{
	private List<GameObject> spawnedHallways = new List<GameObject> (), spawnedRooms = new List<GameObject> (), arrangedRooms = new List<GameObject> ();
	private int maxInsTime = 200, inst = 0, insTimer = 0;

	private List<GameObject> hallways = new List<GameObject> (), rooms = new List<GameObject> (), generatedStructures = new List<GameObject> ();
	public GameObject entrance, startRoom, enemy, end, locker, gameOverTxt, gameWonTxt, player, instructions, jumpCheckpoint, doorCheckpoint, building, thumbnail;

	private int noOfHallways, noOfRooms, floors, enemies;
	public bool gameOver = false, gameWon = false;

	private bool finishedGeneration = false;

    void Awake()
    {
        noOfHallways = LevelSettings.settings.noOfHallways;
        noOfRooms = LevelSettings.settings.noOfRooms;
        floors = LevelSettings.settings.floors;
        enemies = LevelSettings.settings.enemies;
        hallways = LevelSettings.settings.hallwaysLevel;
        rooms = LevelSettings.settings.roomsLevel;
    }

	void Start ()
	{
		player = GameObject.FindGameObjectWithTag ("Player");
                
		if (floors > 0) {
			building = new GameObject ("Building");

			for (int f = 1; f <= floors; f++) {
				Debug.Log ("Generating floor " + f);
				GameObject floor = new GameObject ("Floor_" + f);
				floor.transform.parent = building.transform;

				GameObject strRm = Instantiate (startRoom, Vector3.up * 3.2f * (f - 1), Quaternion.identity);
				if (floors > 1) {
					if (f < floors) {
						if (f % 2 == 0) Destroy (strRm.transform.GetChild (0).gameObject);
						else Destroy (strRm.transform.GetChild (1).gameObject);
						Destroy (strRm.transform.GetChild (2).gameObject);
					} else {
						Destroy (strRm.transform.GetChild (0).gameObject);
						Destroy (strRm.transform.GetChild (1).gameObject);						
					}
					if (f > 1) Destroy (strRm.transform.GetChild (3).gameObject);
				} else {
					Destroy (strRm.transform.GetChild (0).gameObject);
					Destroy (strRm.transform.GetChild (1).gameObject);					
				}
				spawnedHallways.Add (strRm);
				strRm.transform.parent = floor.transform;
				arrangedRooms.Add (strRm);
				generatedStructures.Add (strRm);
				for (int i = 1; i <= noOfHallways; i++) {
					spawnedHallways.Add (Instantiate (hallways[Random.Range (0, hallways.Count)], Vector3.zero, Quaternion.identity));
					spawnedHallways[i].transform.parent = floor.transform;
				}
				ImprovedGeneration (0, 0, 1, spawnedHallways);		
				for (int i = 0; i < noOfRooms; i++) {
					spawnedRooms.Add (Instantiate (rooms[Random.Range (0, rooms.Count)], Vector3.zero, Quaternion.identity));
					spawnedRooms[i].transform.parent = floor.transform;
				}
				ImprovedGeneration (0, 1, 0, spawnedRooms);	
				spawnedHallways.Clear ();
				spawnedRooms.Clear ();
				arrangedRooms.Clear ();
			}
			DestroyRemainingLinks ();
			generatedStructures.Sort ((x, y) => Vector3.Distance (x.transform.position, Vector3.zero).CompareTo (Vector3.Distance (y.transform.position, Vector3.zero)));

			GenerateLockers ();
			SpawnEnding ();
			SpawnEnemies ();
		}
		finishedGeneration = true;
		thumbnail.GetComponent<Image> ().sprite = CreateSprite (rooms[0]);
	}

	void SpawnEnemies ()
	{
		for (int i = generatedStructures.Count - 1; i > generatedStructures.Count - enemies - 1; i--) {
			Debug.Log ("Spawned enemy in " + generatedStructures[i].name);
			Instantiate (enemy, generatedStructures[i].transform.position + Vector3.up, Quaternion.identity);
		}		
	}

	void SpawnEnding ()
	{
		//Instantiate (end, RandomPoint (generatedStructures[Random.Range (1, generatedStructures.Count)].transform.position, 5f), Quaternion.identity);				
		GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
		GameObject candidate = null;
		float maxDist = 0;
		for (int i = 0; i < walls.Length; i++) {
			float d = Vector3.Distance (walls[i].transform.position, Vector3.zero);
			if (d > maxDist) {
				maxDist = d;
				candidate = walls[i];
			}
		}
		GameObject ent = Instantiate (end, candidate.transform.position - Vector3.up * 1.5f, candidate.transform.rotation);
		ent.transform.parent = candidate.transform.parent;
		Destroy (candidate.gameObject);
	}

	void CreateNavMesh (GameObject building)
	{
		NavMeshSurface surface = building.gameObject.AddComponent (typeof(NavMeshSurface)) as NavMeshSurface;
		surface.collectObjects = CollectObjects.Children;
		surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
		surface.layerMask = 1 << LayerMask.NameToLayer ("Walkable");
		surface.BuildNavMesh ();		
	}

	void GenerateLockers ()
	{
		/*
		GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
		for (int i = 0; i < walls.Length; i++) {
			GameObject ent = Instantiate (locker, walls[i].transform.position - Vector3.up * 1.5f - walls[i].transform.forward * 0.2f, Quaternion.identity);
			ent.transform.localRotation = Quaternion.Euler (new Vector3 (0, walls[i].transform.parent.localRotation.y, 0));
		}*/
		
	}

	void DestroyRemainingLinks ()
	{
		GameObject[] links = GameObject.FindGameObjectsWithTag ("Link");
		for (int j = 0; j < links.Length; j++) {
			int con = 0;
			bool collWithDoor = false;
			Collider[] colls = Physics.OverlapBox (links[j].transform.position + Vector3.up, links[j].GetComponent<Collider> ().bounds.extents);
			for (int i = 0; i < colls.Length; i++) {
				if (colls[i].transform.name.Contains ("Door")) collWithDoor = true;
				if (!colls[i].transform.parent.name.Equals (links[j].transform.parent.name) && colls[i].transform.name.Contains ("Wall") && generatedStructures.Contains (colls[i].transform.parent.gameObject)) con++;
			}
			if (con > 0) {
				GameObject wall = links[j].GetComponent<LinkScript> ().linkedWall;
				if (wall != null) {
					if (links[j].transform.parent.name.Contains ("Room") && !links[j].transform.parent.name.Contains ("StartingRoom") && !collWithDoor) {						
						GameObject ent = Instantiate (entrance, wall.transform.position - Vector3.up * 1.5f, wall.transform.rotation);
						ent.transform.parent = wall.transform.parent;
					}
					DestroyObjectAndCollider (wall.gameObject);
				}
			}
			DestroyObjectAndCollider (links[j].gameObject);
		}
	}

	void ImprovedGeneration (int strucType, int prevStrucType, int start, List<GameObject> currentStructure)
	{
		for (int j = start; j < currentStructure.Count; j++) {			
			currentStructure[j].name += ("_" + j);
			Transform structTransform = currentStructure[j].transform;

			for (int i = 0; i < arrangedRooms.Count; i++) if (getViableLinksCount (arrangedRooms[i], prevStrucType) <= 0) arrangedRooms.Remove (arrangedRooms[i]);
			List<GameObject> roomsList = new List<GameObject> (arrangedRooms), curViableLocations = getViableLinks (currentStructure[j], strucType), prevViableLocations;

			while (roomsList.Count > 0) {
				GameObject connectingStructure = roomsList[Random.Range (0, roomsList.Count)];
				prevViableLocations = getViableLinks (connectingStructure, prevStrucType);

				if (prevViableLocations.Count <= 0) {
					Debug.Log ("<color=#de1220ff>Room has insufficient links. Skipping room " + connectingStructure.name + "</color>");
					roomsList.Remove (connectingStructure);
					continue;
				}
				// Set values for links
				GameObject currentLink = curViableLocations[Random.Range (0, curViableLocations.Count)];
				GameObject connectingLink = prevViableLocations[Random.Range (0, prevViableLocations.Count)];

				Vector3 linkPosFor = Vector3.Scale (currentLink.transform.localPosition, currentLink.transform.forward);
				Vector3 linkPosRig = Vector3.Scale (currentLink.transform.localPosition, currentLink.transform.right);

				float dist = Vector3.Distance (linkPosFor, structTransform.localPosition);

				// Move structure to the connecting link position
				structTransform.position = connectingLink.transform.position;

				// Get he amount it needs to rotate to be opposite of the target link
				int y = (int)Mathf.Round ((180 + (connectingLink.transform.localEulerAngles.y + connectingStructure.transform.localEulerAngles.y) - currentLink.transform.localEulerAngles.y) / 90.0f) * 90;
				if (y >= 360) y -= 360;

				// Roate the structure by y
				structTransform.localRotation = Quaternion.Euler (new Vector3 (0, y, 0));

				// Move the structure so it sticks and aligns to the existing structure
				structTransform.position += currentLink.transform.forward * (dist * 2 - 1) - currentLink.transform.forward * dist - currentLink.transform.right * (linkPosRig.x + linkPosRig.y + linkPosRig.z);

				Collider[] col = Physics.OverlapBox (structTransform.position, currentStructure[j].GetComponent<Collider> ().bounds.extents - new Vector3 (0.2f, 0.2f, 0.2f), Quaternion.identity, 1 << LayerMask.NameToLayer ("Structure"));

				if (col.Length > 1) {
					roomsList.Remove (connectingStructure);
					structTransform.position = Vector3.zero;
					structTransform.localRotation = Quaternion.Euler (Vector3.zero);
					continue;
				}

				// Destroy the collider for the wall, since it is no longer needed (good for performance)
				Destroy (currentStructure[j].GetComponent<Rigidbody> ());
				Debug.Log ("<color=#00eb20ff>Successfully placed " + currentStructure[j] + " in a viable location, next to " + connectingStructure.name + "</color>");

				DestroyWalls (currentLink, connectingLink, prevStrucType);

				// Add the structure to a list containing all the parts generated and arranged
				arrangedRooms.Add (currentStructure[j]);
				generatedStructures.Add (currentStructure[j]);
				break;
			}
			if (roomsList.Count <= 0) {
				Debug.Log ("<color=#de1220ff>No available places left to put " + currentStructure[j] + ". Destroying object</color>");
				DestroyObjectAndCollider (currentStructure[j].gameObject);	
			}
		}
	}

	void FixedUpdate ()
	{
		if (finishedGeneration) {
			finishedGeneration = false;
			StartCoroutine ("Wait", 0.1f);
		}
	}

	IEnumerator Wait (float t)
	{
		yield return new WaitForSeconds (t);
		CreateNavMesh (building);
	}

	void Update ()
	{
		if (Input.GetKey (KeyCode.Escape)) SceneManager.LoadScene (0);
		if (player != null && player.transform.position.y < -5 && player.layer == 0) gameOver = true;
		if (gameOver || gameWon) {
			inst = -1;
			if (gameOver) gameOverTxt.SetActive (true);
			else gameWonTxt.SetActive (true);
			if (Input.GetKeyDown (KeyCode.R)) SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
		}

		if (instructions != null) {
			switch (inst) {
				case 0:
					maxInsTime = 70;
					instructions.GetComponent<Text> ().text = "Move the mouse to look around the room";
					if (Input.GetAxis ("Mouse X") != 0 || Input.GetAxis ("Mouse Y") != 0) insTimer++;
					break;
				case 1:
					maxInsTime = 150;
					instructions.GetComponent<Text> ().text = "Use the W A S D keys to move FORWARD, LEFT, BACKWARD and RIGHT. \nYou can sprint by holding SHIFT as you move.";
					if (Input.GetAxis ("Horizontal") != 0 || Input.GetAxis ("Vertical") != 0) insTimer++;
					break;
				case 2:
					instructions.GetComponent<Text> ().text = "Press the SPACEBAR key to jump over the obstacle";
					if (jumpCheckpoint.GetComponent<Checkpoint> ().reached) inst++;
					break;
				case 3:
					instructions.GetComponent<Text> ().text = "Press the E key to interract with the door and open it";
					if (doorCheckpoint.GetComponent<Checkpoint> ().reached) inst++;
					break;
				case 4:
					instructions.GetComponent<Text> ().text = "Go through the door to win the game and end the tutorial";
					break;
				case 5:
					instructions.GetComponent<Text> ().text = "Avoid the monster by hiding in lockers.\n Press the E key to go into the locker";
					break;
				default:
					instructions.GetComponent<Text> ().text = "";
					break;
			}
			if (insTimer >= maxInsTime) {
				inst++;
				insTimer = 0;
			}
		}
	}

	private Texture2D GetAssetThumbanil (GameObject obj)
	{
		return AssetPreview.GetAssetPreview (obj);
	}

	private Sprite CreateSprite (GameObject obj)
	{
		Texture2D asset = GetAssetThumbanil (obj);
		byte[] bytes = asset.EncodeToPNG ();
		Object.Destroy (asset);
		//AssetDatabase.CreateAsset (bytes, "Assets/" + obj.name + ".png");
		File.WriteAllBytes (AssetDatabase.GetAssetPath (0) + "/Ass.png", bytes);

		return Sprite.Create (asset, new Rect (0, 0, asset.width, asset.height), Vector2.zero);
	}

	private void DestroyWalls (GameObject currentLink, GameObject connectingLink, int prevStrucType)
	{
		// Get the walls that the links are attached to; replace the wall with an entrance if the prev structure was a room
		GameObject wall = currentLink.GetComponent<LinkScript> ().linkedWall;
		GameObject prevWall = connectingLink.GetComponent<LinkScript> ().linkedWall;
		if (prevStrucType == 1) {
			if (wall != null) {						
				GameObject ent = Instantiate (entrance, wall.transform.position - Vector3.up * 1.5f, wall.transform.rotation);
				ent.transform.parent = wall.transform.parent;
			}
		}

		// Destroy the links that attach the two structures and the walls that go between them
		if (wall != null) DestroyObjectAndCollider (wall.gameObject);
		if (prevWall != null) DestroyObjectAndCollider (prevWall.gameObject);
		DestroyObjectAndCollider (currentLink.gameObject);
		DestroyObjectAndCollider (connectingLink.gameObject);

	}

	private void DestroyObjectAndCollider (GameObject obj)
	{
		Destroy (obj.GetComponent<Collider> ());
		Destroy (obj.gameObject);		
	}

	private int getViableLinksCount (GameObject room, int linkType)
	{
		return getViableLinks (room, linkType).Count;
	}

	private List<GameObject> getViableLinks (GameObject room, int linkType)
	{
		List<GameObject> links = new List<GameObject> ();
		foreach (Transform t in room.GetComponentsInChildren<Transform>()) {
			if (t.name.Contains ("Link")) {
				switch (linkType) {
					case 0:
						if (t.GetComponent<LinkScript> ().canLinkHallway) links.Add (t.gameObject);
						break;
					case 1:
						if (t.GetComponent<LinkScript> ().canLinkRoom) links.Add (t.gameObject);
						break;
				}
			}
		}
		return links;
	}

	// Provided by Unity
	public Vector3 RandomPoint (Vector3 center, float range)
	{
		for (int i = 0; i < 30; i++) {
			Vector3 randomPoint = center + Random.insideUnitSphere * range;
			NavMeshHit hit;
			if (NavMesh.SamplePosition (randomPoint, out hit, 1.0f, NavMesh.AllAreas)) {
				return hit.position;
			}
		}
		return center;
	}
}
