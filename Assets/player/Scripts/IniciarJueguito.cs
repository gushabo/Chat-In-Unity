using UnityEngine;

public class IniciarJueguito : MonoBehaviour
{
    private bool puedeJugar = false;
    GameContainerManager game;
    public GameObject CanvasBoton; 
    void Start()
    {
        game = FindAnyObjectByType<GameContainerManager>();
        CanvasBoton = GameObject.Find("P");
        CanvasBoton.SetActive(false);
    }

    private void Update()
    {
        if(puedeJugar && Input.GetKey(KeyCode.P)) game.ActivarMinijuego();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Jueguito"))
        {
            puedeJugar = true;
            CanvasBoton.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Jueguito"))
        {
            puedeJugar = false;
            CanvasBoton.SetActive(false);
        }
    }
}
