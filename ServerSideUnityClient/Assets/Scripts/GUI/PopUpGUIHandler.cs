using Data;
using TMPro;
using UnityEngine;
using System.Collections;

public class PopUpGUIHandler : MonoBehaviour
{
    public static PopUpGUIHandler Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI text;

    [Header("Timing")]
    [SerializeField] private float timeToShow = 0.5f;

    [Header("Animation (Optional)")]
    [SerializeField] private RectTransform popupRoot; // assign the panel / text root
    [SerializeField] private float popDuration = 0.12f;
    [SerializeField] private float popScale = 1.08f;

    private Coroutine _popupRoutine;

    private void Awake()
    {
        // 👇 ADD THIS! Detach from any parent so we become a "Root" object
        transform.SetParent(null);
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Optional: keep across scenes (only if you want global popups)
        DontDestroyOnLoad(gameObject);
    }

    public void Show(InfoPopupArgs infoPopupArgs)
    {
        if (text == null) return;

        if (_popupRoutine != null)
            StopCoroutine(_popupRoutine);

        _popupRoutine = StartCoroutine(PopUpCoroutine(infoPopupArgs));
    }

    // Backwards compatible with your previous method name
    public void HandlePopupRequest(InfoPopupArgs infoPopupArgs) => Show(infoPopupArgs);

    public void HandlePopupRequest(string textToPop, InfoPopupType type)
    {
        InfoPopupArgs infoPopupArgs = new InfoPopupArgs()
        {
            Text = textToPop,
            Type = type
        };

        Show(infoPopupArgs);
    }

    private void ApplyStyleAndText(InfoPopupArgs infoPopupArgs)
    {
        switch (infoPopupArgs.Type)
        {
            case InfoPopupType.Log:
                text.color = Color.white;
                break;
            case InfoPopupType.Warning:
                text.color = Color.yellow;
                break;
            case InfoPopupType.Error:
                text.color = Color.red;
                break;
        }

        text.SetText(infoPopupArgs.Text);
    }

    private IEnumerator PopUpCoroutine(InfoPopupArgs infoPopupArgs)
    {
        ApplyStyleAndText(infoPopupArgs);

        if (popupRoot != null)
        {
            popupRoot.gameObject.SetActive(true);
            popupRoot.localScale = Vector3.one;

            yield return PopScale(popupRoot, Vector3.one, Vector3.one * popScale, popDuration * 0.5f);
            yield return PopScale(popupRoot, Vector3.one * popScale, Vector3.one, popDuration * 0.5f);
        }

        yield return new WaitForSeconds(timeToShow);

        text.SetText("");
        _popupRoutine = null;
    }

    private IEnumerator PopScale(RectTransform target, Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = duration <= 0f ? 1f : Mathf.Clamp01(t / duration);
            target.localScale = Vector3.Lerp(from, to, p);
            yield return null;
        }
        target.localScale = to;
    }
}
