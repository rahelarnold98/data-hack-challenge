using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AthleteCardView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image photo;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text countryText;
    [SerializeField] private Image flagImage;
    [SerializeField] private TMP_Text dobText;

    [Header("Buttons")]
    [SerializeField] private Button btnInstagram;
    [SerializeField] private Button btnISU;
    [SerializeField] private Button btnOlympics;

    private AthleteData data;

    public void Setup(AthleteData athlete)
    {
        data = athlete;

        if (photo) photo.sprite = athlete.photo;
        if (nameText) nameText.text = athlete.displayName;

        if (flagImage)
        {
            flagImage.enabled = athlete.flagSprite != null;
            flagImage.sprite  = athlete.flagSprite;
        }

        if (countryText) countryText.text = athlete.country;

        if (dobText)
        {
            var d = athlete.DateOfBirth();
            dobText.text = $"Birthday: {d:dd.MM.yyyy}";
        }

        WireButton(btnInstagram, athlete.instagramUrl);
        WireButton(btnISU, athlete.isuUrl);
        WireButton(btnOlympics, athlete.olympicsUrl);
    }

    private void WireButton(Button button, string url)
    {
        if (!button) return;

        button.onClick.RemoveAllListeners();
        bool hasUrl = !string.IsNullOrWhiteSpace(url);
        button.interactable = hasUrl;

        if (hasUrl)
            button.onClick.AddListener(() => Application.OpenURL(url));
    }
}
