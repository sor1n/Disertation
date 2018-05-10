using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGameSettings : MonoBehaviour
{
    public static CustomGameSettings custSettings;
    public List<GameObject> hallways = new List<GameObject>(), rooms = new List<GameObject>(), hallwayItems = new List<GameObject>(), roomItems = new List<GameObject>();

    void Awake()
    {
        custSettings = this;
    }

    public void GenerateHallwaysList()
    {
        foreach (GameObject obj in hallwayItems)
        {
            CustomMenuItem it = obj.GetComponent<CustomMenuItem>();
            if (it.selected)
                for (int i = 0; i < it.weight; i++)
                    hallways.Add(it.item);
        }
        Debug.Log(hallwayItems.Count + " " + hallways.Count);
    }

    public void GenerateRoomsList()
    {
        foreach (GameObject obj in roomItems)
        {
            CustomMenuItem it = obj.GetComponent<CustomMenuItem>();
            if (it.selected)
                for (int i = 0; i < it.weight; i++)
                    rooms.Add(it.item);
        }
        Debug.Log(roomItems.Count + " " + rooms.Count);
    }
}
