using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomGame : MonoBehaviour
{
    public List<GameObject> menuButtons = new List<GameObject>(), customMenuButtons = new List<GameObject>();
    public GameObject item, hallwayParent, roomParent, hallwaySlider, roomSlider;

    public bool isButton = false;

    void Start()
    {
        if (!isButton)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            GenerateHallways();
            GenerateRooms();
        }
    }

    void GenerateHallways()
    {
        for (int i = 0; i < LevelSettings.settings.hallways.Count; i++)
        {
            GameObject hallway = Instantiate(item, Vector3.zero, Quaternion.identity, hallwayParent.transform);
            hallway.GetComponent<CustomMenuItem>().item = LevelSettings.settings.hallways[i];
            hallway.GetComponent<RectTransform>().anchoredPosition = -Vector2.up * i * 45;
            CustomGameSettings.custSettings.hallwayItems.Add(hallway);
        }

    }

    void GenerateRooms()
    {
        for (int i = 0; i < LevelSettings.settings.rooms.Count; i++)
        {
            GameObject room = Instantiate(item, Vector3.zero, Quaternion.identity, roomParent.transform);
            room.GetComponent<CustomMenuItem>().item = LevelSettings.settings.rooms[i];
            room.GetComponent<RectTransform>().anchoredPosition = -Vector2.up * i * 45;
            CustomGameSettings.custSettings.roomItems.Add(room);
        }

    }

    public void StartGame()
    {
        CustomGameSettings.custSettings.GenerateHallwaysList();
        CustomGameSettings.custSettings.GenerateRoomsList();
        LevelSettings.settings.hallwaysLevel = CustomGameSettings.custSettings.hallways;
        LevelSettings.settings.roomsLevel = CustomGameSettings.custSettings.rooms;
        LevelSettings.settings.noOfHallways = (int)hallwaySlider.GetComponent<Slider>().value;
        LevelSettings.settings.noOfRooms = (int)roomSlider.GetComponent<Slider>().value;
        SceneManager.LoadScene(1);
    }

    public void EnterMenu()
    {
        foreach (GameObject obj in menuButtons) obj.SetActive(false);
        foreach (GameObject obj in customMenuButtons) obj.SetActive(true);
    }

    public void BackToMainMenu()
    {
        foreach (GameObject obj in menuButtons) obj.SetActive(true);
        foreach (GameObject obj in customMenuButtons) obj.SetActive(false);
    }
}
