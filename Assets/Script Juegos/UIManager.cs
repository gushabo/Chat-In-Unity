using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject GameOver;

    public void MostrarGameOver()
    {
        Debug.Log("UIManager → Activando pantalla Game Over");
        GameOver.SetActive(true);
    }
}