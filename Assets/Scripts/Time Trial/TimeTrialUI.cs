using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeTrialUI : MonoBehaviour
{
    [Header("Text targets (usa Text o TextMeshProUGUI)")]
    public Text uiText;
    public TextMeshProUGUI tmpText;

    [Header("Opciones")]
    [Tooltip("Mostrar el mejor tiempo guardado al finalizar")]
    public bool showBest = true;

    void OnEnable()
    {
        if (TimeTrialManager.Instance != null)
        {
            TimeTrialManager.Instance.OnStarted += OnStarted;
            TimeTrialManager.Instance.OnFinished += OnFinished;
        }
    }

    void OnDisable()
    {
        if (TimeTrialManager.Instance != null)
        {
            TimeTrialManager.Instance.OnStarted -= OnStarted;
            TimeTrialManager.Instance.OnFinished -= OnFinished;
        }
    }

    void Update()
    {
        var mgr = TimeTrialManager.Instance;
        if (mgr == null) return;
        if (mgr.IsRunning)
            SetText(FormatTime(mgr.ElapsedTime));
    }

    void OnStarted()
    {
        SetText(FormatTime(0f));
    }

    void OnFinished(float seconds)
    {
        string result = FormatTime(seconds);
        if (showBest)
        {
            float best = TimeTrialManager.Instance.GetBestTime();
            if (best > 0f)
                result += "\nBest: " + FormatTime(best);
        }
        SetText(result);
    }

    string FormatTime(float t)
    {
        int minutes = (int)(t / 60f);
        float seconds = t - minutes * 60;
        return string.Format("{0:00}:{1:00.00}", minutes, seconds);
    }

    void SetText(string value)
    {
        if (tmpText != null) tmpText.text = value;
        else if (uiText != null) uiText.text = value;
    }
}
