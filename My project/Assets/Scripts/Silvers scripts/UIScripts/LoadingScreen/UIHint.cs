using System.Collections;
using UnityEngine;
using TMPro;

public class UIHint : MonoBehaviour
{
    public TextMeshProUGUI tipText;
    public string[] tips;
    public float tipChangeInterval; 

    private int _previousTipIndex;

    private void Start()
    {
        StartCoroutine(ShowRandomTips());
    }

    private IEnumerator ShowRandomTips()
    {
        while (true)
        {
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, tips.Length);
            } while (randomIndex == _previousTipIndex);

            _previousTipIndex = randomIndex;
            tipText.text = tips[randomIndex];

            yield return new WaitForSeconds(tipChangeInterval);
        }
    }
}