using System.Collections.Generic;
using UnityEngine;

public class SkaterReplay : MonoBehaviour
{
    public List<CSVLoader.TimedPoint> points;

    private float raceTime = 0f;
    private int currentIndex = 0;

    public Vector2 trackCenter = new Vector2(42.92f, 27.24f);

    public void Init(List<CSVLoader.TimedPoint> data)
    {
        points = new List<CSVLoader.TimedPoint>(data);

        // Mirror around track center on X axis
        for (int i = 0; i < points.Count; i++)
        {
            var p = points[i];        
            Vector3 pos = p.pos;
            pos.x = 2f * trackCenter.x - pos.x;

            p.pos = pos;
            points[i] = p;
        }

        raceTime = 0f;
        currentIndex = 0;

        if (points != null && points.Count > 0)
            transform.position = points[0].pos;
    }

    Vector3 SmoothInterp(int index, float t)
    {
        // Clamp indices
        int i0 = Mathf.Max(index - 1, 0);
        int i1 = index;
        int i2 = index + 1;
        int i3 = Mathf.Min(index + 2, points.Count - 1);

        Vector3 p0 = points[i0].pos;
        Vector3 p1 = points[i1].pos;
        Vector3 p2 = points[i2].pos;
        Vector3 p3 = points[i3].pos;

        // Catmull-Rom spline
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * (t * t) +
            (-p0 + 3f * p1 - 3f * p2 + p3) * (t * t * t)
        );
    }

    void Update()
    {
        if (points == null || points.Count < 2)
            return;

        raceTime += Time.deltaTime;

        // Advance the current index based on elapsed time
        while (currentIndex < points.Count - 2 &&
               raceTime > points[currentIndex + 1].time)
        {
            currentIndex++;
        }

        var a = points[currentIndex];
        var b = points[currentIndex + 1];

        // Get interpolation fraction between timestamps
        float t = Mathf.InverseLerp(a.time, b.time, raceTime);

        // Position using Catmull-Rom
        Vector3 newPos = SmoothInterp(currentIndex, t);
        transform.position = newPos;

        // Predict next position slightly ahead on spline
        float tAhead = Mathf.Clamp01(t + 0.05f);
        Vector3 futurePos = SmoothInterp(currentIndex, tAhead);

        Vector3 dir = (futurePos - newPos).normalized;
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
        }
    }
}
