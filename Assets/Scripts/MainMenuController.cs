using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Settings")]
    public string raceSceneName = "RaceVideoScene";
    public string athletesSceneName = "AthletesScene";

    public void StartRace()
    {
        SceneManager.LoadScene(raceSceneName);
    }
    
    public void StartAthletes()
    {
        SceneManager.LoadScene(athletesSceneName);
    }


    public void QuitApp()
    {
        Application.Quit();
    }
}
