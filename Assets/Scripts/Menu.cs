using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject settings_menu;
    [SerializeField] private Button start;
    [SerializeField] private Button settings;
    [SerializeField] private Button exit;
    [SerializeField] private TMP_Dropdown screenMode_dropdown;

    private void Start()
    {
        screenMode_dropdown.value = PlayerPrefs.GetInt("ScreenMode");
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            menu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0;
        }
    }

    public void Start_Game()
    {
        menu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
    }

    public void Exit_Game()
    {
        Application.Quit();
    }

    public void Open_Settings()
    {
        menu.SetActive(false);
        settings_menu.SetActive(true);
    }

    public void Close_Settings()
    {
        menu.SetActive(true);
        settings_menu.SetActive(false);
    }

    public void Screen_Modes(int value)
    {
        if (value == 0)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            PlayerPrefs.SetInt("ScreenMode", 0);
        }
        if (value == 1)
        { 
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerPrefs.SetInt("ScreenMode", 1);
        }
        if (value == 2)
        { 
            Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
            PlayerPrefs.SetInt("ScreenMode", 2);
        }
        if (value == 3)
        { 
            Screen.fullScreenMode = FullScreenMode.Windowed;
            PlayerPrefs.SetInt("ScreenMode", 3);
        }
    }
}
