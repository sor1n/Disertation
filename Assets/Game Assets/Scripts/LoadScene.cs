using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public bool startGame = false;

	public void LoadByIndex (int indx)
	{
        if(startGame) LevelSettings.settings.BasicLevel(); ;
        SceneManager.LoadScene (indx);
	}
}
