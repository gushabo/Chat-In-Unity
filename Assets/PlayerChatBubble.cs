using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerChatBubble : NetworkBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject bubbleRoot;    // El canvas/fondo de la burbuja
    [SerializeField] private TextMeshProUGUI chatText; // El texto dentro de la burbuja

    [Header("Par�metros")]
    [SerializeField] private float showTime = 4f;      // Segundos visibles antes de ocultarse

    // Mensaje sincronizado en red
    private NetworkVariable<FixedString128Bytes> message =
        new NetworkVariable<FixedString128Bytes>(
            writePerm: NetworkVariableWritePermission.Owner);

    private Coroutine hideCoroutine;

    private void Awake()
    {
        // SOLO como respaldo, pero lo ideal es asignar todo en el inspector
        if (bubbleRoot == null && transform.childCount > 0)
        {
            bubbleRoot = transform.GetChild(0).gameObject;
        }
    }

    public override void OnNetworkSpawn()
    {
        //Debug.Log($"[Bubble] OnNetworkSpawn en {name}, IsOwner = {IsOwner}, IsLocalPlayer = {IsLocalPlayer}");

        message.OnValueChanged += OnMessageChanged;

        // Al principio, burbuja oculta
        SetBubbleVisible(false);
    }

    public override void OnNetworkDespawn()
    {
        message.OnValueChanged -= OnMessageChanged;
    }

    private void OnMessageChanged(FixedString128Bytes oldValue, FixedString128Bytes newValue)
    {
        string text = newValue.ToString();
        //Debug.Log($"[Bubble] {name} OnMessageChanged: '{oldValue}' -> '{newValue}'");

        if (chatText != null)
            chatText.text = text;
        else
            Debug.LogWarning($"[Bubble] {name} no tiene chatText asignado");

        bool hasText = !string.IsNullOrWhiteSpace(text);
        SetBubbleVisible(hasText);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        if (hasText)
            hideCoroutine = StartCoroutine(HideAfterSeconds());
    }

    private IEnumerator HideAfterSeconds()
    {
        yield return new WaitForSeconds(showTime);

        // Borramos el mensaje (esto se replica)
        if (IsOwner)
        {
            //Debug.Log($"[Bubble] {name} limpiando mensaje despu�s de {showTime}s");
            message.Value = string.Empty;
        }
    }

    private void SetBubbleVisible(bool visible)
    {
        if (bubbleRoot != null)
        {
            bubbleRoot.SetActive(visible);
        }
        else
        {
            Debug.LogWarning($"[Bubble] {name} no tiene bubbleRoot asignado");
        }
    }

    // === API p�blica para el jugador local ===
    public void SendChat(string msg)
    {
        //Debug.Log($"[Bubble] {name} SendChat('{msg}') IsOwner={IsOwner}");

        if (!IsOwner) return;
        if (string.IsNullOrWhiteSpace(msg)) return;

        // El owner escribe la NetworkVariable
        message.Value = msg;
    }
}
