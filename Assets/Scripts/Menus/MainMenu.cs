using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject joinMenu;

    [SerializeField] TMP_InputField inputField;

    [SerializeField] GameObject soundManager;

    [SerializeField] Sprite soundOnIcon;
    [SerializeField] Sprite soundOffIcon;

    private bool muted = false;

    public void OnClickCreateGame()
    {
        Connection.Instance.Host = true;
        Connection.Instance.GameId = Random.Range(1000, 8999).ToString();
        soundManager.GetComponent<BaseSounds>().PlaySelectOptionMenu();
        SceneManager.LoadScene("Base");
    }

    public void OnClickJoinGameMenu()
    {
        soundManager.GetComponent<BaseSounds>().PlaySelectOptionMenu();
        mainMenu.SetActive(false);
        joinMenu.SetActive(true);
    }

    public void OnClickJoinGameButton()
    {
        soundManager.GetComponent<BaseSounds>().PlaySelectOptionMenu();
        Connection.Instance.Host = false;
        Connection.Instance.GameId = inputField.text;

        SceneManager.LoadScene("Base");

    }

    public void OnClickMute()
    {
        if(!muted)
        {
            muted = true;
            AudioListener.pause = true;
            GameObject.Find("MuteButton").GetComponent<Image>().sprite = soundOffIcon;
        }
        else
        {
            muted = false;
            AudioListener.pause = false;
            GameObject.Find("MuteButton").GetComponent<Image>().sprite = soundOnIcon;
        }
    }

    public void ReturnJoinGame()
    {
        joinMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
}
