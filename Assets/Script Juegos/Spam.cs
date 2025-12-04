using UnityEngine;

public class Spam : MonoBehaviour
{
    public GameObject moscoPrefab;
    public float intervalo = 1f;
    public float rangoX = 8f;

    private GameObject moscoActivo;

    void Start()
    {
        InvokeRepeating(nameof(SpawnearMosco), 0f, intervalo);
    }

    void SpawnearMosco()
    {
        // Si ya hay una mosca viva → NO crear otra
        if (moscoActivo != null) return;

        float x = Random.Range(-rangoX, rangoX);
        float y = -Camera.main.orthographicSize - 1f; // Debajo de la cámara
        Vector3 posicion = new Vector3(x, y, 0f);

        moscoActivo = Instantiate(moscoPrefab, posicion, Quaternion.identity);
    }
}