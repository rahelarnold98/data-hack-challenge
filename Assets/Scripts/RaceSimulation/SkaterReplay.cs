using System.Collections.Generic;
using UnityEngine;

public class SkaterReplay : MonoBehaviour
{
    public List<CSVLoader.TimedPoint> points;

    private float raceTime = 0f;
    private int currentIndex = 0;

    public void Init(List<CSVLoader.TimedPoint> data)
    {
        points = data;
        raceTime = 0f;
        currentIndex = 0;

        if (points != null && points.Count > 0)
            transform.position = points[0].pos;
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

        // Move skater
        transform.position = Vector3.Lerp(a.pos, b.pos, t);

        // Rotate towards next point
        Vector3 dir = b.pos - transform.position;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }
}