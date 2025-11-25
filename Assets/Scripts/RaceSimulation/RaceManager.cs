using UnityEngine;

public class RaceManager : MonoBehaviour
{
    [System.Serializable]
    public class SkaterEntry
    {
        public string skaterName;
        public TextAsset csvFile;
        public GameObject skaterPrefab;
    }

    public SkaterEntry[] skaters;
    public Transform xrRig;
    public int playerIndex = 0;

    void Start()
    {
        if (xrRig == null)
        {
            Debug.LogError("RaceManager: XR Rig not assigned!");
            return;
        }

        for (int i = 0; i < skaters.Length; i++)
        {
            SkaterEntry entry = skaters[i];

            if (entry.csvFile == null || entry.skaterPrefab == null)
            {
                Debug.LogError("RaceManager: Missing CSV or prefab for skater " + entry.skaterName);
                continue;
            }

            // Spawn skater
            GameObject obj = Instantiate(entry.skaterPrefab);
            obj.name = entry.skaterName;

            // Replay controller
            var replay = obj.AddComponent<SkaterReplay>();
            var data = CSVLoader.LoadTimedPositions(entry.csvFile);
            replay.Init(data);

            // Attach XR rig to chosen skater
            if (i == playerIndex)
                StartCoroutine(AttachRig(obj.transform));
        }
    }

    private System.Collections.IEnumerator AttachRig(Transform skater)
    {
        yield return null; // Wait one frame

        xrRig.SetParent(skater);
        xrRig.localPosition = new Vector3(0, 1.7f, 0); // "head height"
        xrRig.localRotation = Quaternion.identity;
    }
}