using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform xrRig;                 // XR Origin (your main player headset)
    public Camera thirdPersonCamera;        // A separate Unity camera
    public float followDistance = 6f;
    public float followHeight = 2f;

    public RaceManager raceManager;   // drag in Inspector

    private Transform currentTarget;
    private int currentSkaterIndex = 0;   // who we’re currently following

    void Start()
    {
        // default to the RaceManager's player index, if you want
        currentSkaterIndex = raceManager.playerIndex;
    }

    // Called by the TMP Dropdown (On Value Changed (int))
    public void OnDropdownChanged(int index)
    {
        Debug.Log("Dropdown changed: " + index);

        int lastSkaterIndex = raceManager.SkaterCount - 1;
        int topViewIndex   = raceManager.SkaterCount;   // with 5 skaters, this is 5

        if (index >= 0 && index <= lastSkaterIndex)
        {
            // First-person on the chosen skater
            currentSkaterIndex = index;
            Transform skater = raceManager.GetSkaterTransform(currentSkaterIndex);
            SetFirstPerson(skater);
        }
        else if (index == topViewIndex)
        {
            // Top view = third person following the currently selected skater
            Transform skater = raceManager.GetSkaterTransform(currentSkaterIndex);
            SetThirdPerson(skater);
        }
    }

    public void SetFirstPerson(Transform target)
    {
        // XR Rig gets parented to racer
        xrRig.SetParent(target);
        xrRig.localPosition = new Vector3(0, 0, 0);
        xrRig.localRotation = Quaternion.identity;

        // Disable 3rd person camera
        thirdPersonCamera.enabled = false;
    }

    public void SetThirdPerson(Transform target)
    {
        xrRig.SetParent(null);
        currentTarget = target;

        // Enable 3rd person camera, disable XR rig tracking
        thirdPersonCamera.enabled = true;
    }

    void LateUpdate()
    {
        if (!thirdPersonCamera.enabled || currentTarget == null)
            return;

        // Use the target forward (already updated from the replay script)
        Vector3 forward = currentTarget.forward;
        
        Vector3 desiredPos =
            currentTarget.position
            - forward * followDistance
            + Vector3.up * followHeight;

        thirdPersonCamera.transform.position = desiredPos;
        thirdPersonCamera.transform.rotation =
            Quaternion.LookRotation(currentTarget.position - desiredPos, Vector3.up);
    }

}