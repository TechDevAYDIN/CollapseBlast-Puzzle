using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class LvlButon : MonoBehaviour
{
    private int LvlNum;
    // Start is called before the first frame update
    void Start()
    {
        LvlNum = Int16.Parse(GetComponentInChildren<TextMeshProUGUI>().text);
    }
    public void OnClicked()
    {
        PlayerPrefs.SetInt("isPlayerSelectLevel", LvlNum);
        SceneManager.LoadScene(1);
    }
}
