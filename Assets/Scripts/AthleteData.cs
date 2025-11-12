using UnityEngine;
using System;

[CreateAssetMenu(menuName = "ShortTrackXR/Athlete Data", fileName = "Athlete_")]
public class AthleteData : ScriptableObject
{
    [Header("Basics")]
    public string id;
    public string displayName;
    public string country;   
    public Sprite flagSprite; 

    [Header("Bio")]
    public Sprite photo;
    public int birthYear;
    public int birthMonth;
    public int birthDay;

    [Header("Links")]
    [TextArea(1, 3)] public string instagramUrl;
    [TextArea(1, 3)] public string isuUrl;
    [TextArea(1, 3)] public string olympicsUrl;

    public DateTime DateOfBirth()
    {
        return new DateTime(
            Mathf.Max(1, birthYear),
            Mathf.Clamp(birthMonth, 1, 12),
            Mathf.Clamp(birthDay, 1, 28)
        );
    }
}