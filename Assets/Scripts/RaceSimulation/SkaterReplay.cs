using System.Collections.Generic;
using UnityEngine;

public class SkaterReplay : MonoBehaviour
{
    public float heightOffset = 1.0f;


    public Vector2 trackCenter = new(42.92f, 27.24f);
    private int currentIndex;
    public List<CSVLoader.TimedPoint> points;

    private float raceTime;

    private void Update()
    {
        if (points == null || points.Count < 2)
            return;

        raceTime += Time.deltaTime;

        while (currentIndex < points.Count - 2 &&
               raceTime > points[currentIndex + 1].time)
            currentIndex++;

        var a = points[currentIndex];
        var b = points[currentIndex + 1];

        var t = Mathf.InverseLerp(a.time, b.time, raceTime);

        var newPos = SmoothInterp(currentIndex, t);

        // Fallback: if data is invalid (0,0), place skater at track center
        if (Mathf.Approximately(newPos.x, 0f) && Mathf.Approximately(newPos.z, 0f))
            newPos = new Vector3(trackCenter.x, heightOffset, trackCenter.y);

        newPos.y = heightOffset;

        transform.position = newPos;

        // Predict future position for smooth forward rotation
        var tAhead = Mathf.Clamp01(t + 0.05f);
        var futurePos = SmoothInterp(currentIndex, tAhead);

        // Apply same fallback for future direction vector
        if (Mathf.Approximately(futurePos.x, 0f) && Mathf.Approximately(futurePos.z, 0f))
            futurePos = new Vector3(trackCenter.x, heightOffset, trackCenter.y);

        futurePos.y = heightOffset;

        var dir = (futurePos - newPos).normalized;
        if (dir.sqrMagnitude > 0.0001f)
        {
            var targetRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
        }
    }


    public void Init(List<CSVLoader.TimedPoint> data)
    {
        points = new List<CSVLoader.TimedPoint>(data);

        // Mirror around track center on X axis
        for (var i = 0; i < points.Count; i++)
        {
            var p = points[i];
            var pos = p.pos;
            pos.x = 2f * trackCenter.x - pos.x;

            p.pos = pos;
            points[i] = p;
        }

        raceTime = 0f;
        currentIndex = 0;

        if (points != null && points.Count > 0)
            transform.position = points[0].pos;
    }

    private Vector3 SmoothInterp(int index, float t)
    {
        // Clamp indices
        var i0 = Mathf.Max(index - 1, 0);
        var i1 = index;
        var i2 = index + 1;
        var i3 = Mathf.Min(index + 2, points.Count - 1);

        var p0 = points[i0].pos;
        var p1 = points[i1].pos;
        var p2 = points[i2].pos;
        var p3 = points[i3].pos;

        // Catmull-Rom spline
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * (t * t) +
            (-p0 + 3f * p1 - 3f * p2 + p3) * (t * t * t)
        );
    }
}