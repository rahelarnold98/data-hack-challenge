using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Settings")]
    public string raceSceneName = "RaceVideoScene";

    public void StartRace()
    {
        SceneManager.LoadScene(raceSceneName);
    }

    public void QuitApp()
    {
        Application.Quit();
    }
}
