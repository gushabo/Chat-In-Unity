using TMPro;
using UnityEngine;


public class ChatMsg : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMesh;
    
    public void SetText(string text) => textMesh.text = text;
}
