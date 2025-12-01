using UnityEngine;

public class PausaJuego : MonoBehaviour
{
    [Header("UI de Pausa")]
    public GameObject panelPausa;

    private bool pausado = false;

    void Start()
    {
        if (panelPausa != null)
            panelPausa.SetActive(false);
        else
            Debug.LogError("❌ ERROR: No asignaste el Panel de Pausa en el Inspector.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausado)
                Reanudar();
            else
                Pausar();
        }
    }

    public void Pausar()
    {
        if (panelPausa == null) return;

        panelPausa.SetActive(true);
        Time.timeScale = 0f;
        pausado = true;
    }

    public void Reanudar()
    {
        if (panelPausa == null) return;

        panelPausa.SetActive(false);
        Time.timeScale = 1f;
        pausado = false;
    }

    public void SalirMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Escena");
    }

    public void SalirJuego()
    {
        Application.Quit();
        Debug.Log("Cerrar juego");
    }
}