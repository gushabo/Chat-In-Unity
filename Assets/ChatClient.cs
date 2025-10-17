using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatClient : MonoBehaviour
{
    [Header("Login UI")]
    public GameObject loginPanel;
    public TMP_InputField nameInput;

    [Header("Conexión")]
    public string host = "127.0.0.1";
    public int port = 8080;
    public string playerName = "Jugador";

    [Header("UI")]
    public GameObject chatPanel;
    public TMP_Text chatLog;
    public TMP_InputField messageInput;
    public Transform userListPanel;
    public GameObject userButtonPrefab;

    TcpClient client;
    StreamReader reader;
    StreamWriter writer;
    CancellationTokenSource cts;
    ConcurrentQueue<string> incoming = new ConcurrentQueue<string>();
    bool connected = false;

    void Start()
    {
        chatPanel.SetActive(false);
        loginPanel.SetActive(true);
    }

    public void OnConnectPressed()
    {
        playerName = nameInput.text.Trim();
        if (string.IsNullOrEmpty(playerName))
            playerName = "Jugador";

        loginPanel.SetActive(false);
        _ = ConnectAsync();
    }

    async Task ConnectAsync()
    {
        try
        {
            chatPanel.SetActive(true);
            chatLog.text = "";

            cts = new CancellationTokenSource();
            client = new TcpClient();
            await client.ConnectAsync(host, port);
            connected = true;

            var stream = client.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            // Enviar nombre al servidor
            await writer.WriteLineAsync(playerName);

            // Esperar confirmación antes de continuar
            string confirm = await reader.ReadLineAsync();
            AppendChat(confirm ?? ">>> Conectado sin respuesta del servidor.");

            // Iniciar bucle de lectura
            _ = Task.Run(() => ReadLoop(cts.Token));

        }
        catch (Exception ex)
        {
            AppendChat($">>> Error de conexión: {ex.Message}");
        }
    }

    async Task ReadLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                string line = await reader.ReadLineAsync();
                if (line == null)
                {
                    incoming.Enqueue(">>> Servidor desconectado.");
                    connected = false;
                    break;
                }
                incoming.Enqueue(line);
            }
        }
        catch (Exception ex)
        {
            incoming.Enqueue($">>> Conexión interrumpida: {ex.Message}");
            connected = false;
        }
    }

    void Update()
    {
        while (incoming.TryDequeue(out var msg))
        {
            if (msg.StartsWith(">>> Usuarios conectados:"))
                UpdateUserList(msg);
            else
                AppendChat(msg);
        }
    }

    void AppendChat(string text)
    {
        if (chatLog == null) return;

        string color = "white";

        if (text.StartsWith(">>>") || text.StartsWith("Usuarios conectados:"))
            color = "#A3B4FF";
        else if (text.Contains($"[{playerName}]"))
            color = "#7CFF7C";

        chatLog.text += $"<b><color={color}>{text}</color></b>\n";
    }

    public async void OnSendClicked()
    {
        if (!connected || writer == null) return;

        var msg = messageInput.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

        await writer.WriteLineAsync(msg);
        AppendChat($"<color=#7CFF7C>[Tú]</color> {msg}");
        messageInput.text = "";
        messageInput.ActivateInputField();
    }

    void UpdateUserList(string msg)
    {
        foreach (Transform child in userListPanel)
            Destroy(child.gameObject);

        string lista = msg.Substring(msg.IndexOf(":") + 1).Trim();
        string[] nombres = lista.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string nombre in nombres)
        {
            GameObject boton = Instantiate(userButtonPrefab, userListPanel);
            TMP_Text texto = boton.GetComponentInChildren<TMP_Text>();
            texto.text = nombre;

            if (nombre == playerName)
                boton.GetComponent<Button>().interactable = false;

            boton.GetComponent<Button>().onClick.AddListener(() =>
            {
                messageInput.text = $"/msg {nombre} ";
                messageInput.ActivateInputField();
            });
        }
    }

    public async void Disconnect()
    {
        if (!connected) return;

        try
        {
            // Enviar aviso al servidor (opcional)
            if (writer != null)
                await writer.WriteLineAsync("/salir");

            // Cancelar la lectura y cerrar conexión
            cts?.Cancel();
            reader?.Close();
            writer?.Close();
            client?.Close();

            connected = false;

            // Limpiar UI
            AppendChat(">>> Desconectado del servidor.");
            chatPanel.SetActive(false);
            loginPanel.SetActive(true);
            chatLog.text = "";
            messageInput.text = "";

            Debug.Log("Cliente desconectado correctamente.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error al desconectarse: {ex.Message}");
        }
    }
    
    
    void OnApplicationQuit()
    {
        try { cts?.Cancel(); } catch { }
        try { client?.Close(); } catch { }
        connected = false;
    }
}
