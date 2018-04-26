using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SprayPaint : MonoBehaviour
{
	Texture2D tex;
	Camera camera;
	Color brushColor;
	float brushSize;
	public GameObject brush, paint;

	void Start ()
	{
		camera = gameObject.GetComponent<Camera> ();
		int width = camera.pixelWidth;
		int height = camera.pixelHeight;
		tex = new Texture2D (width, height, TextureFormat.RGB24, false);
		brushColor = Color.red;
		brushSize = 1.0f;
		paint = new GameObject ("Paint");
	}

	void Update ()
	{
		if (Input.GetMouseButton (0)) {
			Draw ();
		}
	}

	void Draw ()
	{
		Vector3 uvWorldPosition = Vector3.zero, normal = Vector3.zero;	
		Transform hitObj = null;
		if (HitTestUVPosition (ref uvWorldPosition, ref normal, ref hitObj)) {
			GameObject brushObj = Instantiate (brush); //Paint a brush
			Vector3 vals = uvWorldPosition.normalized;

			brushObj.GetComponent<SpriteRenderer> ().color = brushColor;//Set the brush color
			brushColor.a = brushSize * 2.0f; // Brushes have alpha to have a merging effect when painted over.
			brushObj.transform.localPosition = uvWorldPosition + (normal * 0.01f); //The position of the brush (in the UVMap)
			brushObj.transform.localScale = Vector3.one * brushSize;//The size of the brush
			brushObj.transform.localRotation = Quaternion.Euler (new Vector3 (normal.y, normal.x, 0) * 90.0f);
			brushObj.transform.parent = hitObj;
		}	
	}

	bool HitTestUVPosition (ref Vector3 uvWorldPosition, ref Vector3 normal, ref Transform hitObj)
	{
		RaycastHit hit;
		Vector3 cursorPos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0.0f);
		Ray cursorRay = camera.ScreenPointToRay (cursorPos);
		if (Physics.Raycast (cursorRay, out hit, 2, ~((1 << 8) | (1 << 13)))) {
			Collider collider = hit.collider as Collider;
			if (collider == null || hit.transform.tag == "Player") return false;	
			normal = hit.normal;
			uvWorldPosition = hit.point;
			hitObj = hit.transform;
			return true;
		} else return false;
	}

	void MergeTexture ()
	{ 
		
	}
}
