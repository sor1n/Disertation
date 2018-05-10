using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{        
    public void UpdateText(GameObject obj)
    {
        obj.GetComponent<Text>().text = gameObject.GetComponent<Slider>().value.ToString();
    }
}
