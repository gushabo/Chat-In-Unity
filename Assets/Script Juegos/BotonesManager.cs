using UnityEngine;

public class BotonesManager : MonoBehaviour
{
    public GameManager gameManager; // <-- Esto debe asignarse

    public void ReiniciarPartida()
    {
        if (gameManager != null)
        {
            gameManager.ReiniciarPartida();
        }
        else
        {
            Debug.LogError("❌ BotonesManager: gameManager no está asignado en el Inspector.");
        }
    }
}