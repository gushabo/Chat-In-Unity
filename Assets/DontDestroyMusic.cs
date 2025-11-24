using UnityEngine;

public class DontDestroyMusic : MonoBehaviour
{
    private static DontDestroyMusic instancia;

    void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject); // Este objeto sobrevive entre escenas
        }
        else
        {
            Destroy(gameObject); // Si ya existe, destruimos duplicados
        }
    }
    void Start()
    {
        // Aplicar el volumen guardado al arrancar
        float volumenGuardado = PlayerPrefs.GetFloat("volume", 1f);
        GetComponent<AudioSource>().volume = volumenGuardado;
    }

}
