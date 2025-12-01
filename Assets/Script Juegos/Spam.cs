using UnityEngine;

public class Spam : MonoBehaviour
{
    public GameObject MoscoPrefab;
    public float Inter = 1f;
    public float RangoX = 10f;

    private GameManager gamemanager;

    void Start()
    {
        gamemanager = FindFirstObjectByType<GameManager>(); // ← actualizado
        InvokeRepeating(nameof(SpamMosco), 1f, Inter);
    }

    void SpamMosco()
    {
        if (gamemanager == null || !gamemanager.enabled) return;

        Vector3 Pocision = new Vector3(Random.Range(-RangoX, RangoX), -5.5f, 0f);
        Instantiate(MoscoPrefab, Pocision, Quaternion.identity);
    }
}
