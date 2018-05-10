using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropdownMenu : MonoBehaviour
{
    private bool open = false;
    public GameObject items;

    public void Clicked()
    {
        open = !open;
        gameObject.transform.Rotate(Vector3.forward * 90 * (open?-1:1));
        items.SetActive(open);
    }
}
