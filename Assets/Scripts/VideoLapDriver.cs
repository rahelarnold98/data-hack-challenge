using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


public class VideoLapDriver : MonoBehaviour
{
    [Header("References")]
    public VideoPlayer videoPlayer;
    public RaceDataLoader loader;

    [Header("Timing")]
    public float startOffsetSeconds = 0f;

    public float lapLatchTolerance = 0.10f;

    private List<float> lapEnds = new();
    private int totalLaps = 0;
    private int lastRenderedLiveLap = int.MinValue;

    private void Start()
    {
        if (videoPlayer == null || loader == null)
        {
            Debug.LogError("VideoLapDriver: Missing references (VideoPlayer / RaceDataLoader).");
            enabled = false;
            return;
        }

        lapEnds = loader.LapEndTimes ?? new List<float>();
        totalLaps = loader.TotalLaps;
        if (lapEnds.Count != totalLaps)
        {
            Debug.LogWarning($"VideoLapDriver: Lap timeline length ({lapEnds.Count}) != TotalLaps ({totalLaps}). Using min().");
            totalLaps = Mathf.Min(totalLaps, lapEnds.Count);
        }

        ForceRenderForTime(0f);
    }

    
    private void Update()
    {
        if (!videoPlayer.isPrepared && !videoPlayer.isPlaying) return;
        if (loader == null || loader.overlayUI == null) return;

        double raw = videoPlayer.time;
        float t = Mathf.Max(0f, (float)raw - startOffsetSeconds);

        int completed = 0;
        for (int i = 0; i < totalLaps; i++)
        {
            if (t + lapLatchTolerance >= lapEnds[i]) completed++;
            else break;
        }

        int liveLap = Mathf.Clamp(completed + 1, 1, totalLaps + 1);

        if (liveLap != lastRenderedLiveLap)
        {
            loader.RenderForLiveLap(liveLap);
            lastRenderedLiveLap = liveLap;
        }


        bool shouldShowFastest =
            loader.FastestLapNumber > 0 &&
            completed >= Mathf.Max(1, loader.FastestLapNumber);


        loader.overlayUI.ShowFastestLap(shouldShowFastest ? loader.FastestLapLabel : "");

        if (completed > totalLaps && lastRenderedLiveLap != totalLaps + 1)
        {
            loader.RenderForLiveLap(totalLaps + 1);
            lastRenderedLiveLap = totalLaps + 1;

        }
    }



    private void ForceRenderForTime(float timeSinceRaceStart)
    {
        int completed = 0;
        for (int i = 0; i < totalLaps; i++)
        {
            if (timeSinceRaceStart + lapLatchTolerance >= lapEnds[i]) completed++;
            else break;
        }
        int liveLap = Mathf.Clamp(completed + 1, 1, totalLaps + 1);
        loader.RenderForLiveLap(liveLap);
        lastRenderedLiveLap = liveLap;
    }
}
