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
	public GameObject brush;

	void Start ()
	{
		camera = gameObject.GetComponent<Camera> ();
		int width = camera.pixelWidth;
		int height = camera.pixelHeight;
		tex = new Texture2D (width, height, TextureFormat.RGB24, false);
		brushColor = Color.red;
		brushSize = 1.0f;
	}

	void Update ()
	{
		if (Input.GetMouseButton (0)) {
			Draw ();
		}
	}

	void Draw ()
	{
		Vector3 uvWorldPosition = Vector3.zero;		
		if (HitTestUVPosition (ref uvWorldPosition)) {
			/*GameObject brushObj = Instantiate (brush); //Paint a brush
			Vector3 vals = uvWorldPosition.normalized;
			brushObj.GetComponent<SpriteRenderer> ().color = new Color (Mathf.Abs (vals.x), Mathf.Abs (vals.y), Mathf.Abs (vals.z)); //Set the brush color
			brushColor.a = brushSize * 2.0f; // Brushes have alpha to have a merging effect when painted over.
			brushObj.transform.localPosition = uvWorldPosition; //The position of the brush (in the UVMap)
			brushObj.transform.localScale = Vector3.one * brushSize;//The size of the brush*/
		}	
	}

	bool HitTestUVPosition (ref Vector3 uvWorldPosition)
	{
		RaycastHit hit;
		Vector3 cursorPos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0.0f);
		Ray cursorRay = camera.ScreenPointToRay (cursorPos);
		if (Physics.Raycast (cursorRay, out hit, 2, ~((1 << 8) | (1 << 13)))) {
			Collider collider = hit.collider as Collider;
			if (collider == null || hit.transform.tag == "Player") return false;	
			Texture2D tex = (Texture2D)collider.GetComponent<Renderer> ().material.mainTexture;
			Color C = tex.GetPixelBilinear (hit.textureCoord.x, hit.textureCoord.y);
			Debug.Log (C);
			//Debug.Log (hit.textureCoord + " " + hit.textureCoord2 + " " + hit.barycentricCoordinate);
			uvWorldPosition = hit.point;
			return true;
		} else return false;
	}

	void MergeTexture ()
	{ 
		
	}
}
