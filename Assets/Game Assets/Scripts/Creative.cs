using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class Creative : MonoBehaviour
{
	public List<GameObject> structures = new List<GameObject> ();
	public GameObject info;
	private GameObject connectingLink, currentLink, connectingStructure, currentStructure, player;
	private int structId = 0, link_mask;

	void Start ()
	{
		link_mask = LayerMask.GetMask ("Link");
		player = GameObject.FindGameObjectWithTag ("Player");
	}

	RaycastHit hit;
	Transform link;

	void Update ()
	{
		Physics.SphereCast (player.transform.position - player.transform.forward * 2, 3f, player.transform.GetChild (0).forward, out hit, 3f, link_mask);
		if (hit.collider != null) {
			if (connectingLink != null && connectingLink != hit.collider.transform.gameObject) connectingLink.GetComponent<Renderer> ().material.color = Color.white;
			connectingLink = hit.collider.transform.gameObject;
			connectingLink.GetComponent<Renderer> ().material.color = Color.green;
			Collider[] colls = Physics.OverlapBox (connectingLink.transform.position, connectingLink.GetComponent<Collider> ().bounds.extents * 2);
			for (int i = 0; i < colls.Length; i++) if (!(colls[i].name.Contains ("Wall") || colls[i].name.Contains ("Link") || colls[i].name.Contains ("Platform"))) Debug.Log (colls[i].name);
		}
		if (Input.GetAxis ("Mouse ScrollWheel") != 0f) {
			structId += 1 * (int)Mathf.Sign (Input.GetAxis ("Mouse ScrollWheel"));
			if (structId >= structures.Capacity) structId = 0;
			else if (structId < 0) structId = structures.Capacity - 1;
		}
		if (Input.GetMouseButtonDown (0) && connectingLink != null) {
			connectingStructure = connectingLink.transform.parent.gameObject;
			currentStructure = Instantiate (structures[structId], Vector3.zero, Quaternion.identity);
			List<GameObject> links = getViableLinks (currentStructure, 0);
			currentLink = links[0];
			ImprovedGeneration ();
		}
		info.GetComponent<Text> ().text = structures[structId].name;
	}

	void OnDrawGizmos ()
	{
		if (connectingLink != null) {
			Gizmos.color = new Color (1, 1, 0, .5f);
			Gizmos.DrawCube (connectingLink.transform.position, connectingLink.GetComponent<Collider> ().bounds.size * 2);
		}
	}

	public void ImprovedGeneration ()
	{
		Vector3 linkPosFor = Vector3.Scale (currentLink.transform.localPosition, currentLink.transform.forward);
		Vector3 linkPosRig = Vector3.Scale (currentLink.transform.localPosition, currentLink.transform.right);
		Vector3 strucPos = currentStructure.transform.localPosition;
		float dist = Vector3.Distance (linkPosFor, strucPos);
		float align = linkPosRig.x + linkPosRig.y + linkPosRig.z;

		// Move structure to the connecting link position
		currentStructure.transform.position = connectingLink.transform.position;

		// Get he amount it needs to rotate to be opposite of the target link
		int y = (int)Mathf.Round ((180 + (connectingLink.transform.localEulerAngles.y + connectingStructure.transform.localEulerAngles.y) - currentLink.transform.localEulerAngles.y) / 90.0f) * 90;
		if (y >= 360) y -= 360;

		// Roate the structure by y
		currentStructure.transform.localRotation = Quaternion.Euler (new Vector3 (0, y, 0));

		// Move the structure so it sticks and aligns to the existing structure
		currentStructure.transform.position += currentLink.transform.forward * (dist * 2 - 1) - currentLink.transform.forward * dist - currentLink.transform.right * align;

		Destroy (currentLink);
		Destroy (connectingLink);
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
}
