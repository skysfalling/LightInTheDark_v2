using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuSceneManager : MonoBehaviour
{

    [Header("Cutscenes")]
    public string introCutscene;

    [Header("Tutorial")]
    public string level_1_1;
    public string level_1_2;
    public string level_1_3;

    [Header("Levels")]
    public string level_2;
    public string level_3;


    public void StartGame()
    {
        SceneManager.LoadScene(introCutscene);
    }

    public void LoadLevel(int level)
    {
        if (level == 2)
        {
            SceneManager.LoadScene(level_2);

        }

        if (level == 3)
        {
            SceneManager.LoadScene(level_3);

        }
    }

    public void RestartLevelFromSavePoint()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}

