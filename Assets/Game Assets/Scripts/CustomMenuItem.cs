using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomMenuItem : MonoBehaviour
{
    public GameObject item;
    public int weight;
    public bool selected;
    
    void Update()
    {
        selected = transform.GetChild(0).GetComponent<Toggle>().isOn;
        weight = (int)transform.GetChild(2).GetComponent<Slider>().value;
        transform.GetChild(1).GetComponent<Text>().text = item.name;
    }
}
