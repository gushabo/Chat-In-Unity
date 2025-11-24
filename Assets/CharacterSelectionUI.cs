using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectionUI : MonoBehaviour
{
    public Button nextButton;
    public Button prevButton;

    public Image previewImage;
    public TMP_Text previewLabel;

    private static CharacterSelectionUI instance;

    public static CharacterSelectionUI Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    public void UpdatePreview(Sprite sprite, string label)
    {
        if (previewImage != null && sprite != null)
            previewImage.sprite = sprite;

        if (previewLabel != null)
            previewLabel.text = label;
    }
}
