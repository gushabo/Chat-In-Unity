using UnityEngine;

public class BackgroundBreathing : MonoBehaviour
{
    [Header("Configuración del Zoom")]
    [Tooltip("La velocidad a la que respira el fondo")]
    public float speed = 1.0f;

    [Tooltip("Qué tanto se va a agrandar (0.05 es un 5% más grande)")]
    public float zoomStrength = 0.05f;

    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        float scaleOffset = Mathf.Sin(Time.time * speed) * zoomStrength;

        // Aplicamos la nueva escala (x, y, z)
        transform.localScale = initialScale + (Vector3.one * scaleOffset);
    }
}