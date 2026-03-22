using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TimeTrialTrigger : MonoBehaviour
{
    [Tooltip("True = punto de salida (inicia temporizador). False = punto de llegada (detiene).")]
    public bool isStart = true;

    [Tooltip("Opcional: referencia directa al PlayerControler si quieres forzar quién activa.")]
    public PlayerControler playerReference;

    void Reset()
    {
        // Asegurar que hay un Collider marcado como Trigger
        var col = GetComponent<Collider>();
        if (col == null) col = gameObject.AddComponent<BoxCollider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerControler player = playerReference != null ? playerReference : other.GetComponentInParent<PlayerControler>();
        if (player == null) return;

        var mgr = TimeTrialManager.Instance;
        if (mgr == null) return;

        if (isStart)
        {
            mgr.StartTimer();
        }
        else
        {
            mgr.StopTimer();
        }
    }
}
