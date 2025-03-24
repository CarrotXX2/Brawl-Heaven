using UnityEngine;
using UnityEngine.UI;

public class ChangeImageOnClick : MonoBehaviour
{
    public Image targetImage; // De UI Image die veranderd moet worden
    public Sprite map1Image;  // De afbeelding voor Map 1
    public Sprite map2Image;  // De afbeelding voor Map 2
    public Sprite map3Image;  // De afbeelding voor Map 3
    public Sprite map4Image;  // De afbeelding voor Map 4

    public string prefix = "Stage "; // Prefix voor de naam van de targetImage

    public void OnMap1ButtonClicked()
    {
        if (targetImage != null && map1Image != null)
        {
            targetImage.sprite = map1Image;
            targetImage.gameObject.name = $"{prefix} 1";
        }
    }

    public void OnMap2ButtonClicked()
    {
        if (targetImage != null && map2Image != null)
        {
            targetImage.sprite = map2Image;
            targetImage.gameObject.name = $"{prefix} 2";
        }
    }

    public void OnMap3ButtonClicked()
    {
        if (targetImage != null && map3Image != null)
        {
            targetImage.sprite = map3Image;
            targetImage.gameObject.name = $"{prefix }3";
        }
    }
    public void OnMap4ButtonClicked()
    {
        if (targetImage != null && map4Image != null)
        {
            targetImage.sprite = map4Image;
            targetImage.gameObject.name = $"{prefix} 4";
        }
    }
}