using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TDA.BlastTest;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Singleton
    {
        get
        {
            if (singleton == null)
                singleton = FindObjectOfType(typeof(GameManager)) as GameManager;

            return singleton;
        }
        set
        {
            singleton = value;
        }
    }
    private static GameManager singleton;
    #endregion
    public bool canTouch = true;

    public GameObject LevelFailedCanvas;
    public GameObject LevelPassedCanvas;
    
    public void LevelFailed()
    {
        LevelFailedCanvas.SetActive(true);
    }
    public void LevelPassed()
    {
        LevelPassedCanvas.SetActive(true);
        if (LevelManager.Instance.level.num >= PlayerPrefs.GetInt("CurrentLevel", 1))
        {
            PlayerPrefs.SetInt("CurrentLevel", LevelManager.Instance.level.num + 1);
        }
    }

    public void NextLevelBTN()
    {
        print(LevelManager.Instance.level.num);
        PlayerPrefs.SetInt("isPlayerSelectLevel", LevelManager.Instance.level.num + 1);
        SceneManager.LoadScene(1);
    }
    public void TryLevelBTN()
    {
        PlayerPrefs.SetInt("isPlayerSelectLevel", LevelManager.Instance.level.num);
        SceneManager.LoadScene(1);
    }
    public void HomeBTN()
    {
        SceneManager.LoadScene(0);
    }
}
