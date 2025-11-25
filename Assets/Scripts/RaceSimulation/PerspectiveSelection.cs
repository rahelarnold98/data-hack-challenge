using UnityEngine;

public class PerspectiveSelection : MonoBehaviour
{
    public CameraManager camManager;
    public RaceManager raceManager;

    public void SetFirstPerson(int index)
    {
        camManager.SetFirstPerson(raceManager.GetSkaterTransform(index));
    }

    public void SetThirdPerson(int index)
    {
        camManager.SetThirdPerson(raceManager.GetSkaterTransform(index));
    }
}