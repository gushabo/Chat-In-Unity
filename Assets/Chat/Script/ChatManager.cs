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

    public string playerName;

    private void Awake()
    {
        ChatManager.instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendChatMessage(chatInput.text, playerName);
            chatInput.text = "";


        }
    }

    public void SendChatMessage(string _message, string _fromWho = null)
    {
        if (string.IsNullOrWhiteSpace(_message)) return;
        
        string s = _fromWho + " > " + _message;
        SendChatMessageServerRpc(s);
    }

    void AddMessage(string msg)
    {
        ChatMsg CM = Instantiate(chatMsg, chatContent.transform);
        CM.SetText(msg);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string msg)
    {
        ReceiveChatMessageClientRpc(msg);
    }

    [ClientRpc]
    void ReceiveChatMessageClientRpc(string msg)
    {
        ChatManager.instance.AddMessage(msg);
    }
    
    
    
}
