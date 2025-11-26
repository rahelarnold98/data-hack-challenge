using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform xrRig;                 // XR Origin (your main player headset)
    public Camera thirdPersonCamera;        // A separate Unity camera
    public float followDistance = 6f;
    public float followHeight = 2f;

    private Transform currentTarget;

    public void SetFirstPerson(Transform target)
    {
        // XR Rig gets parented to racer
        xrRig.SetParent(target);
        xrRig.localPosition = new Vector3(0, 1.7f, 0);
        xrRig.localRotation = Quaternion.identity;

        // Disable 3rd person camera
        thirdPersonCamera.enabled = false;
    }

    public void SetThirdPerson(Transform target)
    {
        currentTarget = target;

        // Enable 3rd person camera, disable XR rig tracking
        thirdPersonCamera.enabled = true;
        xrRig.SetParent(null);
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