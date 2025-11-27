using UnityEngine;
using FMODUnity;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] EventReference eventReference;
    [SerializeField] float rate = 0.5f;
    [SerializeField] GameObject player;
    [SerializeField] playerMovement playerMovimiento;

    private float time;

    public void PlayFootStep()
    {
        RuntimeManager.PlayOneShotAttached(eventReference, player);
    }
    private void Update()
    {
        time += Time.deltaTime;
        if(playerMovimiento.move.magnitude > 0.1 && time >= rate)
        {
            PlayFootStep();
            Debug.Log("Pisa");
            time = 0;
        }
    }
}
