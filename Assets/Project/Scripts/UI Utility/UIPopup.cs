using UnityEngine;

public class UIPopup : MonoBehaviour
{
    public UIFade fade;
    public UIScale scale;

    public float Show()
    {
        fade.FadeIn();
        return scale.Play();
        
    }

    public void Hide()
    {
        fade.FadeOut();
    }
}