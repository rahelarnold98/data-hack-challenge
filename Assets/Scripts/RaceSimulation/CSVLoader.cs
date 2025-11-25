using System.Collections.Generic;
using UnityEngine;

public static class CSVLoader
{
    public struct TimedPoint
    {
        public float time;
        public Vector3 pos;
    }

    // Each row = 0.5 seconds → 2 FPS
    private const float SAMPLE_DT = 0.5f;

    public static List<TimedPoint> LoadTimedPositions(TextAsset csv)
    {
        var pts = new List<TimedPoint>();
        if (csv == null)
        {
            Debug.LogError("CSVLoader: CSV file is null.");
            return pts;
        }

        var lines = csv.text.Split('\n');
        float time = 0f;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var values = line.Trim().Split(',');

            if (values.Length < 2)
                continue;

            if (!float.TryParse(values[0], out float x)) continue;
            if (!float.TryParse(values[1], out float y)) continue;

            pts.Add(new TimedPoint
            {
                time = time,
                pos = new Vector3(x, 0f, y)
            });

            time += SAMPLE_DT;
        }

        return pts;
    }
}