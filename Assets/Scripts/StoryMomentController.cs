using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;

[Serializable]
public class StoryMoment
{
    public string title;
    [TextArea] public string description;
    public double triggerTime;
    public float duration = 4f;
}

public class StoryMomentController : MonoBehaviour
{
    [Header("References")]
    public VideoPlayer videoPlayer;

    // Background card (always visible)
    public GameObject storyPanel;

    // Content holder that fades
    public RectTransform storyContent;       
    public TMP_Text storyTitleText;
    public TMP_Text storyDescriptionText;

    [Header("Timing")]
    public float fadeDuration = 0.3f;          

    [Header("Moments")]
    public List<StoryMoment> moments = new List<StoryMoment>();

    private int currentIndex = 0;
    private CanvasGroup contentCg;
    private Coroutine running;

    void Awake()
    {
        if (storyPanel != null)
            storyPanel.SetActive(true);        

        if (storyContent != null)
        {
            contentCg = storyContent.GetComponent<CanvasGroup>();
            if (!contentCg) contentCg = storyContent.gameObject.AddComponent<CanvasGroup>();
            contentCg.alpha = 0f; 
            contentCg.interactable = false;
            contentCg.blocksRaycasts = false;
        }
    }

    void Update()
    {
        if (videoPlayer == null || !videoPlayer.isPlaying) return;

        double t = videoPlayer.time;

        if (currentIndex < moments.Count)
        {
            var next = moments[currentIndex];
            if (t >= next.triggerTime)
            {
                ShowStory(next);
                currentIndex++;
            }
        }
    }

    void ShowStory(StoryMoment m)
    {
        if (storyTitleText)       storyTitleText.text = m.title;
        if (storyDescriptionText) storyDescriptionText.text = m.description;

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FadeContentMoment(m.duration));
    }

    IEnumerator FadeContentMoment(float visibleSeconds)
    {
        yield return FadeTo(1f, fadeDuration);

        yield return new WaitForSeconds(visibleSeconds);

        yield return FadeTo(0f, fadeDuration);
    }

    IEnumerator FadeTo(float target, float time)
    {
        if (contentCg == null) yield break;
        float start = contentCg.alpha;
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / time);
            contentCg.alpha = Mathf.Lerp(start, target, k);
            yield return null;
        }
        contentCg.alpha = target;
    }
}
