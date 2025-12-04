using UnityEngine;

public class GameContainerManager : MonoBehaviour
{
    [Header("Tus sistemas a apagar")]
    public GameObject[] cosasQueDeboApagar;

    [Header("Prefab del minijuego")]
    public GameObject miniJuegoPrefab;

    private GameObject instanciaMiniJuego;
    private Camera cameraMinijuego;
    private GameManager gm;

    [Header("Tu cámara principal")]
    public Camera cameraPrincipal;

    public void ActivarMinijuego()
    {
        // 1. Apagar tus sistemas normales
        foreach (var obj in cosasQueDeboApagar)
            obj.SetActive(false);

        // 2. Instanciar el minijuego si no existe
        if (instanciaMiniJuego == null)
        {
            instanciaMiniJuego = Instantiate(miniJuegoPrefab);

            // Tomar la cámara del prefab
            cameraMinijuego = instanciaMiniJuego.GetComponentInChildren<Camera>(true);

            if (cameraMinijuego == null)
                Debug.LogError("El prefab del minijuego NO tiene cámara dentro.");

            // Tomar GameManager
            gm = instanciaMiniJuego.GetComponentInChildren<GameManager>();
        }

        // 3. Encender el prefab
        instanciaMiniJuego.SetActive(true);

        // 4. Cambiar cámaras
        cameraPrincipal.gameObject.SetActive(false);

        cameraMinijuego.gameObject.SetActive(true);
        cameraMinijuego.tag = "MainCamera";   // ahora esta es la oficial

        // 5. Reiniciar la partida del minijuego
        gm.ReiniciarPartida();

        Debug.Log("Minijuego ACTIVADO.");
    }

    public void DesactivarMinijuego()
    {
        if (instanciaMiniJuego != null)
        {
            instanciaMiniJuego.SetActive(false);
        }

        // Restaurar cámara principal
        cameraMinijuego.tag = "Untagged";
        cameraMinijuego.gameObject.SetActive(false);

        cameraPrincipal.tag = "MainCamera";
        cameraPrincipal.gameObject.SetActive(true);

        // Encender tus sistemas
        foreach (var obj in cosasQueDeboApagar)
            obj.SetActive(true);

        Debug.Log("Minijuego DESACTIVADO.");
    }
}
