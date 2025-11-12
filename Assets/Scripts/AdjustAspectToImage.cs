using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AdjustAspectToImage : MonoBehaviour
{
    void Start()
    {
        var img = GetComponent<Image>();
        if (img.sprite != null)
        {
            RectTransform rt = GetComponent<RectTransform>();
            float aspect = (float)img.sprite.rect.width / img.sprite.rect.height;

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rt.rect.height * aspect);
        }
    }
}