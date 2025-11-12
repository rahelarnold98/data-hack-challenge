using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StoryMomentsFile
{
    public string raceId;
    public double videoOffset;
    public List<StoryMoment> moments;
}

public class StoryMomentJsonLoader : MonoBehaviour
{
    [Header("JSON source")]
    public TextAsset jsonFile;
    public string expectedRaceId = "";

    [Header("Target Controller")]
    public StoryMomentController target; 

    void Awake()
    {
        if (jsonFile == null || target == null)
        {
            Debug.LogError("StoryMomentJsonLoader: assign jsonFile and target!");
            return;
        }

        var raw = jsonFile.text.Trim();
        if (!raw.StartsWith("{")) raw = "{ " + raw + " }";

        StoryMomentsFile data;
        try
        {
            data = JsonUtility.FromJson<StoryMomentsFile>(raw);
        }
        catch (Exception e)
        {
            Debug.LogError($"StoryMomentJsonLoader: JSON parse failed: {e.Message}");
            return;
        }

        if (!string.IsNullOrEmpty(expectedRaceId) && !string.Equals(data.raceId, expectedRaceId, StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning($"StoryMomentJsonLoader: raceId mismatch. JSON={data.raceId}, expected={expectedRaceId}");
        }

        if (data.moments != null)
        {
            foreach (var m in data.moments)
            {
                m.triggerTime += data.videoOffset;
            }
        }

        target.moments = data.moments ?? new List<StoryMoment>();
        Debug.Log($"StoryMomentJsonLoader: loaded {target.moments.Count} story moments for raceId '{data.raceId}'.");
    }
}
