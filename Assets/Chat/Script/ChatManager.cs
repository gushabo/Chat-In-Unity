using System;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager instance;

    [SerializeField] ChatMsg chatMsg;
    [SerializeField] CanvasGroup chatContent;
    [SerializeField] TMP_InputField chatInput;

    private void Awake()
    {
        ChatManager.instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (chatInput.isFocused)
            {
                print("ChatInput_Update");
                SendChatMessage(chatInput.text);
                chatInput.text = "";
            }
            else
            {
                chatInput.ActivateInputField();
            }
        }
    }

    public void SendChatMessage(string _message)
    {
        if (string.IsNullOrWhiteSpace(_message)) return;

        // Obtener el nombre del jugador local
        string playerName = GetLocalPlayerName();

        // Enviar a las burbujas (PlayerChatBubble)
        SendToBubble(_message, playerName);

        // Enviar al chat view
        string formattedMessage = playerName + " > " + _message;
        SendChatMessageServerRpc(formattedMessage);
    }

    private void SendToBubble(string message, string playerName)
    {
        // Buscar el PlayerChatBubble del jugador local
        var localPlayer = NetworkManager.Singleton?.LocalClient?.PlayerObject;
        if (localPlayer != null)
        {
            var bubble = localPlayer.GetComponent<PlayerChatBubble>();
            if (bubble == null)
                bubble = localPlayer.GetComponentInChildren<PlayerChatBubble>();

            if (bubble != null)
            {
                bubble.SendChat(message);
            }
        }
    }

    private string GetLocalPlayerName()
    {
        var localPlayer = NetworkManager.Singleton?.LocalClient?.PlayerObject;
        if (localPlayer != null)
        {
            var nameDisplay = localPlayer.GetComponent<PlayerNameDisplay>();
            if (nameDisplay != null)
            {
                return nameDisplay.GetPlayerName();
            }
        }
        return "Player";
    }

    void AddMessage(string msg)
    {
        ChatMsg CM = Instantiate(chatMsg, chatContent.transform);
        CM.SetText(msg);
    }

    [Rpc(SendTo.Server)]
    public void SendChatMessageServerRpc(string msg)
    {
        ReceiveChatMessageClientRpc(msg);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ReceiveChatMessageClientRpc(string msg)
    {
        ChatManager.instance.AddMessage(msg);
    }
}