using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Settings")]
    public string raceSceneName = "RaceVideoScene";
    public string athletesSceneName = "AthletesScene";
    public string immersiveSceneName = "ImmersiveRaceScene";
    public string mainSceneName = "MainMenu";

    public void StartRace()
    {
        SceneManager.LoadScene(raceSceneName);
    }
    
    public void StartAthletes()
    {
        SceneManager.LoadScene(athletesSceneName);
    }
    
    public void StartImmersive()
    {
        SceneManager.LoadScene(immersiveSceneName);
    }
    
    public void BackToMain()
    {
        SceneManager.LoadScene(mainSceneName);
    }


    public void QuitApp()
    {
        Application.Quit();
    }
}
