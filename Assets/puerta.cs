using UnityEngine;

public class DoorSimpleSinPadre : MonoBehaviour
{
    public float anguloAbierto = 90f;
    public float velocidad = 2f;

    private bool abierta = false;
    private bool jugadorCerca = false;

    private Quaternion rotacionCerrada;
    private Quaternion rotacionAbierta;

    private Collider colisionFisica; // referencia al collider físico (no trigger)

    void Start()
    {
        rotacionCerrada = transform.rotation;
        rotacionAbierta = Quaternion.Euler(transform.eulerAngles + new Vector3(0, anguloAbierto, 0));

        // Detecta el primer collider que NO sea trigger para desactivarlo cuando la puerta esté abierta
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider c in colliders)
        {
            if (!c.isTrigger)
            {
                colisionFisica = c;
                break;
            }
        }
    }

    void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            abierta = !abierta;

            // Desactiva la colisión física si se abre
            if (colisionFisica != null)
                colisionFisica.enabled = !abierta;
        }

        Quaternion objetivo = abierta ? rotacionAbierta : rotacionCerrada;
        transform.rotation = Quaternion.Lerp(transform.rotation, objetivo, Time.deltaTime * velocidad);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            jugadorCerca = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            jugadorCerca = false;
    }
}
