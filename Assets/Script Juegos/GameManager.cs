using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int puntos = 0;
    public int vidas = 3;
    public float tiempo = 60f;
    public Text textoPuntos;
    public Text textoVidas;
    public Text textoTiempo;
    public bool juegoTerminado = false;

    private Record puntajes;
    private UIManager ui;

    void Start()
    {
        puntajes = FindFirstObjectByType<Record>();
        ui = FindFirstObjectByType<UIManager>();

        ActualizarUI();
    }

    void Update()
    {
        if (!juegoTerminado)
        {
            tiempo -= Time.deltaTime;

            // 🟢 Cuando el tiempo llega a cero → GANAS
            if (tiempo <= 0)
            {
                tiempo = 0;
                GanarPartida();
            }

            ActualizarUI();
        }
    }

    public void SumarPuntos()
    {
        puntos++;
        ActualizarUI();
        // ❌ Ya NO gana por puntos
    }

    public void PerderVida()
    {
        vidas--;
        ActualizarUI();

        // 🟥 Si pierde todas las vidas → Game Over
        if (vidas <= 0)
        {
            TerminarJuego();
        }
    }

    void ActualizarUI()
    {
        textoPuntos.text = "Puntos: " + puntos;
        textoVidas.text = "Vidas: " + vidas;
        textoTiempo.text = "Tiempo: " + tiempo.ToString("F0");
    }

    void TerminarJuego()
    {
        if (juegoTerminado) return;

        juegoTerminado = true;

        if (puntajes != null)
            puntajes.GuardarPuntajeJuego("Jugador", puntos);

        SceneManager.LoadScene("GameOver");
    }

    // 🟢 NUEVO — GANAR SOLO POR TIEMPO
    void GanarPartida()
    {
        if (juegoTerminado) return;

        juegoTerminado = true;

        if (puntajes != null)
            puntajes.GuardarPuntajeJuego("Jugador", puntos);

        SceneManager.LoadScene("Ganaste");
    }
}
