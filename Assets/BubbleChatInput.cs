using UnityEngine;
using Unity.Netcode;
using TMPro;

public class BubbleChatInput : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField bubbleInput;

    private PlayerChatBubble localBubble;

    private void Start()
    {
        CacheLocalBubble();
    }

    private void Update()
    {
        // Enter para enviar SOLO si el input est� enfocado
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (bubbleInput != null && bubbleInput.isFocused)
            {
                print("bubbleChatInput");
                SendBubbleMessage();
            }
        }
    }

    private void CacheLocalBubble()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("[BubbleInput] No hay NetworkManager");
            return;
        }

        var localClient = NetworkManager.Singleton.LocalClient;
        if (localClient == null)
        {
            Debug.LogWarning("[BubbleInput] No hay LocalClient");
            return;
        }

        if (localClient.PlayerObject == null)
        {
            Debug.LogWarning("[BubbleInput] LocalClient no tiene PlayerObject a�n (quiz� a�n no spawnea)");
            return;
        }

        localBubble = localClient.PlayerObject.GetComponent<PlayerChatBubble>();

        if (localBubble == null)
        {
            Debug.LogWarning("[BubbleInput] El PlayerObject local no tiene PlayerChatBubble");
        }
        else
        {
            Debug.Log("[BubbleInput] LocalBubble cacheado correctamente");
        }
    }

    public void SendBubbleMessage()
    {
        print("BubbleChatInput_Funcion");
        if (bubbleInput == null) return;

        string msg = bubbleInput.text;
        if (string.IsNullOrWhiteSpace(msg)) return;

        if (localBubble == null)
            CacheLocalBubble();

        if (localBubble != null)
        {
            localBubble.SendChat(msg);
        }
        else
        {
            Debug.LogWarning("[BubbleInput] No se encontr� la burbuja del jugador local");
        }

        //bubbleInput.text = string.Empty;
        bubbleInput.ActivateInputField();
    }
}
