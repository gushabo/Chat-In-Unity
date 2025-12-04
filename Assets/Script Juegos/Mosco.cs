using UnityEngine;

public class Mosco : MonoBehaviour
{
    public float velocidad = 2f;
    public float amplitudLateral = 0.5f;
    public float frecuenciaLateral = 2f;
    public int puntosPorMatar = 1;

    [Header("Cooldown para evitar múltiples clics")]
    public float cooldownClick = 0.3f;
    private bool puedeClickear = true;
    private float tiempoCooldown = 0f;

    private Vector3 posInicial;
    private float tiempoOffset;
    private GameManager gm;

    void Start()
    {
        posInicial = transform.position;
        tiempoOffset = Random.Range(0f, 2f);

        gm = FindObjectOfType<GameManager>();

        if (gm == null)
            Debug.LogError("❌ Mosco NO encontró GameManager.");
    }

    void Update()
    {
        if (GameManager.juegoTerminado) return;

        // Movimiento hacia arriba
        transform.position += Vector3.up * velocidad * Time.deltaTime;

        // Zig-zag lateral
        float desplazamientoX = Mathf.Sin(Time.time * frecuenciaLateral + tiempoOffset) * amplitudLateral;
        transform.position = new Vector3(
            posInicial.x + desplazamientoX,
            transform.position.y,
            transform.position.z
        );

        // Cooldown debug
        if (!puedeClickear)
        {
            tiempoCooldown -= Time.deltaTime;

            if (tiempoCooldown <= 0f)
            {
                puedeClickear = true;
                Debug.Log($"🟢 Cooldown terminado → Mosco {name} ya puede clickearse otra vez.");
            }
        }

        // Si sale de pantalla
        float limiteY = Camera.main.orthographicSize + 1f;

        if (transform.position.y > limiteY)
        {
            if (!GameManager.juegoTerminado)
                gm.PerderVida();

            Respawn();
        }
    }

    void Respawn()
    {
        float limiteX = Camera.main.orthographicSize * Camera.main.aspect;

        float nuevoX = Random.Range(-limiteX, limiteX);
        float nuevoY = -Camera.main.orthographicSize - 1f;

        transform.position = new Vector3(nuevoX, nuevoY, 0f);
        posInicial = transform.position;

        puedeClickear = true; // Se reinicia el cooldown al respawnear
        Debug.Log($"🔄 Respawn del Mosco {name} → cooldown reiniciado.");
    }

    void OnMouseDown()
    {
        if (!puedeClickear)
        {
            Debug.Log($"❌ Intento de click en Mosco {name} DURANTE COOLDOWN.");
            return;
        }

        if (!GameManager.juegoTerminado)
        {
            Debug.Log($"🟡 Mosco {name} clickeado → inicia cooldown de {cooldownClick} segundos.");
            puedeClickear = false;
            tiempoCooldown = cooldownClick;

            gm.SumarPuntos(puntosPorMatar);
            Destroy(gameObject);
        }
    }
}
