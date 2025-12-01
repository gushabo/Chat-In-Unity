/*using UnityEngine;
using UnityEngine.UI;

public class PrefabVidas : MonoBehaviour
{
    public GameManager JuegoGame;
    public Image[] CorazonesVida;

    // Update is called once per frame
    void Update()
    {
        if (JuegoGame == null) return;

        //Actualizacion de vida
        for (int i = 0; i < CorazonesVida.Length; i++)
        {
            CorazonesVida[i].enabled = i < JuegoGame.vidas;
        }
    }
}*/
