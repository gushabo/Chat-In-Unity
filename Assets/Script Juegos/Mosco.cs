using UnityEngine;

public class Mosco : MonoBehaviour
{
    public float Velocidad = 5f;
    private GameManager JuegoManager;
    private bool Muerto = false;

    // Delay entre clics
    public float delayClick = 1f;
    private bool puedeClic = true;

    // Movimiento aleatorio
    private Vector3 direccionActual;
    private float tiempoCambioDireccion;

    void Start()
    {
        JuegoManager = FindFirstObjectByType<GameManager>();

        CambiarDireccion();
        tiempoCambioDireccion = Random.Range(0.3f, 1.2f);
    }

    void Update()
    {
        if (Muerto) return;

        transform.Translate(direccionActual * Velocidad * Time.deltaTime);

        tiempoCambioDireccion -= Time.deltaTime;
        if (tiempoCambioDireccion <= 0)
        {
            CambiarDireccion();
            tiempoCambioDireccion = Random.Range(0.3f, 1.2f);
        }

        if (SalioDePantalla())
        {
            JuegoManager.PerderVida();
            Destroy(gameObject);
        }
    }

    private void OnMouseDown()
    {
        if (!puedeClic) return; // ⛔ NO puedes clic aún
        if (Muerto) return;

        // Bloquea clics por 1 segundo
        puedeClic = false;
        Invoke(nameof(ReactivarClick), delayClick);

        // Lógica de muerte
        Muerto = true;
        JuegoManager.SumarPuntos();
        Destroy(gameObject);
    }

    void ReactivarClick()
    {
        puedeClic = true;
    }

    void CambiarDireccion()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(0.3f, 1f);
        direccionActual = new Vector3(x, y, 0).normalized;
    }

    bool SalioDePantalla()
    {
        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);

        if (pos.y > 1f && pos.y > 0.5f)
            return true;

        if (pos.x < 0f || pos.x > 1f)
            return true;

        return false;
    }
}