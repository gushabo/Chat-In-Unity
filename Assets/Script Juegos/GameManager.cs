using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Valores del juego")]
    public int vidas = 3;
    public int puntos = 0;
    public float tiempoJuego = 60f;

    [Header("UI")]
    public Text textoVidas;
    public Text textoPuntos;
    public Text textoTiempo;

    [Header("Paneles")]
    public GameObject panelGameOver;
    public GameObject panelGanar;

    public static bool juegoTerminado = false;

    void Start()
    {
        ActualizarVidas();
        ActualizarPuntos();
        ActualizarTiempo();

        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelGanar != null) panelGanar.SetActive(false);

        juegoTerminado= false;
    }

    void Update()
    {
        if (juegoTerminado) return;

        tiempoJuego -= Time.deltaTime;
        ActualizarTiempo();

        if (tiempoJuego <= 0)
        {
            tiempoJuego = 0;
            GanarJuego();
        }
    }

    // -----------------------------
    // RESTAR VIDA
    // -----------------------------
    public void PerderVida()
    {
        if (juegoTerminado) return;

        vidas--;
        Debug.Log("🟥 VIDA PERDIDA. Vidas actuales: " + vidas);

        ActualizarVidas();

        if (vidas <= 0)
        {
            GameOver();
        }
    }

    // -----------------------------
    // SUMAR PUNTOS
    // -----------------------------
    public void SumarPuntos(int cantidad)
    {
        if (juegoTerminado) return;

        puntos += cantidad;
        ActualizarPuntos();
    }

    // -----------------------------
    // GAME OVER
    // -----------------------------
    void GameOver()
    {
        Debug.Log("🔥 SE ACTIVÓ GAME OVER");

        juegoTerminado = true;

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);

            Canvas canvas = panelGameOver.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = true;
                canvas.sortingOrder = 999;
            }
        }
    }

    // -----------------------------
    // GANAR
    // -----------------------------
    void GanarJuego()
    {
        Debug.Log("🏆 SE ACTIVÓ GANAR");

        juegoTerminado = true;

        if (panelGanar != null)
        {
            panelGanar.SetActive(true);

            Canvas canvas = panelGanar.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = true;
                canvas.sortingOrder = 999;
            }
        }
    }

    // -----------------------------
    // UI
    // -----------------------------
    void ActualizarVidas()
    {
        if (textoVidas != null)
            textoVidas.text = "Vidas: " + vidas;
    }

    void ActualizarPuntos()
    {
        if (textoPuntos != null)
            textoPuntos.text = "Puntos: " + puntos;
    }

    void ActualizarTiempo()
    {
        if (textoTiempo != null)
            textoTiempo.text = "Tiempo: " + Mathf.CeilToInt(tiempoJuego);
    }


    // =====================================================
    // 🔵 FUNCIÓN NUEVA — REINICIAR PARTIDA
    // =====================================================
    public void ReiniciarPartida()
    {
        juegoTerminado = false;

        vidas = 3;
        puntos = 0;
        tiempoJuego = 60f;

        ActualizarVidas();
        ActualizarPuntos();
        ActualizarTiempo();

        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelGanar != null) panelGanar.SetActive(false);

        Debug.Log("🔄 Partida reiniciada.");
    }
}