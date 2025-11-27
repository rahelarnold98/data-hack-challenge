using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public SkaterEntry[] skaters;
    public Transform xrRig;
    public int playerIndex;
    public CameraManager cameraManager;

    private readonly List<Transform> skaterTransforms = new();

    public int SkaterCount => skaterTransforms.Count;

    private void Start()
    {
        if (xrRig == null)
        {
            Debug.LogError("RaceManager: XR Rig not assigned!");
            return;
        }

        for (var i = 0; i < skaters.Length; i++)
        {
            var entry = skaters[i];

            if (entry.csvFile == null || entry.skaterPrefab == null)
            {
                Debug.LogError("RaceManager: Missing CSV or prefab for skater " + entry.skaterName);
                continue;
            }

            // Spawn skater
            var obj = Instantiate(entry.skaterPrefab);
            obj.transform.position = new Vector3(0, 1, 0);

            var renderer = obj.GetComponentInChildren<MeshRenderer>();
            if (renderer != null && entry.skaterMaterial != null)
                renderer.material = entry.skaterMaterial;

            obj.name = entry.skaterName;

            // Replay controller
            var replay = obj.AddComponent<SkaterReplay>();
            var data = CSVLoader.LoadTimedPositions(entry.csvFile);
            replay.Init(data);

            // Store skater transform for the camera system
            skaterTransforms.Add(obj.transform);

            // Attach XR rig + set first-person view
            if (i == playerIndex)
            {
                StartCoroutine(AttachRig(obj.transform));
                cameraManager.SetFirstPerson(obj.transform);
            }
        }
    }

    public Transform GetSkaterTransform(int index)
    {
        return skaterTransforms[index];
    }


    private IEnumerator AttachRig(Transform skater)
    {
        yield return null; // Wait one frame

        xrRig.SetParent(skater);
        xrRig.localPosition = new Vector3(0, 0f, 0); // "head height"
        xrRig.localRotation = Quaternion.identity;
    }

    [Serializable]
    public class SkaterEntry
    {
        public string skaterName;
        public TextAsset csvFile;
        public GameObject skaterPrefab;
        public Material skaterMaterial;
    }
}