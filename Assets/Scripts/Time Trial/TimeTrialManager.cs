using System;
using UnityEngine;

public class TimeTrialManager : MonoBehaviour
{
    public static TimeTrialManager Instance { get; private set; }

    public bool IsRunning { get; private set; }
    public float ElapsedTime { get; private set; }

    [Tooltip("Identificador para guardar/recuperar mejor tiempo (PlayerPrefs)")]
    [SerializeField] string raceId = "DefaultRace";

    public event Action OnStarted;
    public event Action<float> OnFinished;

    float startTime;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (IsRunning)
            ElapsedTime = Time.time - startTime;
    }

    public void StartTimer()
    {
        if (IsRunning) return;
        startTime = Time.time;
        ElapsedTime = 0f;
        IsRunning = true;
        OnStarted?.Invoke();
    }

    public void StopTimer()
    {
        if (!IsRunning) return;
        ElapsedTime = Time.time - startTime;
        IsRunning = false;
        OnFinished?.Invoke(ElapsedTime);
        SaveBestTime(ElapsedTime);
    }

    string GetPrefsKey() => $"TimeTrial_{raceId}_Best";

    void SaveBestTime(float seconds)
    {
        string key = GetPrefsKey();
        float best = PlayerPrefs.GetFloat(key, float.MaxValue);
        if (seconds < best)
        {
            PlayerPrefs.SetFloat(key, seconds);
            PlayerPrefs.Save();
        }
    }

    public float GetBestTime()
    {
        string key = GetPrefsKey();
        float best = PlayerPrefs.GetFloat(key, float.MaxValue);
        return best == float.MaxValue ? -1f : best;
    }

    public void ResetBestTime()
    {
        PlayerPrefs.DeleteKey(GetPrefsKey());
    }
}
