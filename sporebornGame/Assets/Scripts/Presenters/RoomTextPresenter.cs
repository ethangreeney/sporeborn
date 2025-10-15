// RoomTextPresenter.cs

using UnityEngine;
using TMPro;
using System.Collections;

public class RoomTextPresenter : MonoBehaviour
{
    public TMP_Text roomTextUI;
    public float fadeInTime = 0.5f;
    public float displayTime = 2f;
    public float fadeOutTime = 0.5f;

    private Coroutine currentDisplayCoroutine;

    void Start()
    {
        // Ensure the text is invisible on start
        if (roomTextUI != null)
        {
            roomTextUI.alpha = 0f;
        }
    }

    public void ShowRoomText(string text)
    {
        if (roomTextUI == null || string.IsNullOrEmpty(text))
        {
            return;
        }

        // If text is already being shown, stop it to show the new one
        if (currentDisplayCoroutine != null)
        {
            StopCoroutine(currentDisplayCoroutine);
        }

        // Start the new text display
        currentDisplayCoroutine = StartCoroutine(DisplayTextCoroutine(text));
    }

    private IEnumerator DisplayTextCoroutine(string text)
    {
        roomTextUI.text = text;

        // Fade In
        float elapsedTime = 0f;
        while (elapsedTime < fadeInTime)
        {
            roomTextUI.alpha = Mathf.Clamp01(elapsedTime / fadeInTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        roomTextUI.alpha = 1f;

        // Hold
        yield return new WaitForSeconds(displayTime);

        // Fade Out
        elapsedTime = 0f;
        while (elapsedTime < fadeOutTime)
        {
            roomTextUI.alpha = 1f - Mathf.Clamp01(elapsedTime / fadeOutTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        roomTextUI.alpha = 0f;

        currentDisplayCoroutine = null;
    }
}