using UnityEngine;
using UnityEngine.SceneManagement;

public class BotonesManager : MonoBehaviour
{
    //Funcion para iniciar juego
    public void JugarGame()
    {
        SceneManager.LoadScene("Juego");
    }

    //Menu principal
    public void MenuPrincipal()
    {
        SceneManager.LoadScene("Menu");
    }

    //Record De jugadores
    public void TopPuntaje()
    {
        SceneManager.LoadScene("Puntaje");
    }

    //Salida del juego completamente
    public void Salirjuego()
    {
        Debug.Log("Salida del juego");
        Application.Quit();
    }
}
