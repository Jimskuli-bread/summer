using UnityEngine;
using TMPro;
using System.Collections;

public class PlayMessageUI : MonoBehaviour
{
    public TextMeshProUGUI messageText; // Assign in Inspector
    public float displayDuration = 2.5f;
    [TextArea]
    public string autoMessage = "Welcome to Heinola!";

    void Awake()
    {
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    void Start()
    {
        if (!string.IsNullOrEmpty(autoMessage))
            ShowMessage(autoMessage);
    }

    public void ShowMessage(string text)
    {
        if (messageText != null)
        {
            messageText.text = text;
            messageText.gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(HideAfterDelay());
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }
}
