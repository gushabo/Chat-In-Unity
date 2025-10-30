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
    [Header("Panels")]
    public GameObject loginPanel;   // Panel: pide nombre
    public GameObject lobbyPanel;   // Panel: selección de sala (si usas dropdown, puedes mantenerlo)
    public GameObject chatPanel;    // Panel: chat

    [Header("Login")]
    public TMP_InputField nameInput;

    [Header("Lobby (opcional)")]
    public TMP_Dropdown roomDropdown;       // opcional si usas dropdown
    public TMP_InputField customRoomInput;  // opcional si quieres escribir una sala manual

    [Header("Header")]
    public TMP_Text roomHeaderText;         // Ej: "Room: Videojuegos 0/5"

    [Header("Chat UI")]
    public TMP_Text chatLog;
    public TMP_InputField messageInput;
    public Transform userListPanel;         // contenedor con Vertical Layout Group
    public GameObject userButtonPrefab;     // Button + TMP_Text

    [Header("Conn")]
    public string host = "127.0.0.1";
    public int port = 8080;

    // --- internos ---
    TcpClient client;
    StreamReader reader;
    StreamWriter writer;
    CancellationTokenSource cts;
    ConcurrentQueue<string> incoming = new ConcurrentQueue<string>();
    bool connected = false;

    string playerName = "Jugador";
    string currentRoom = null;

    const int ROOM_CAPACITY = 5; // debe coincidir con el servidor
    int lastUserCount = 0;

    void Start()
    {
        loginPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        chatPanel.SetActive(false);
        if (roomHeaderText) roomHeaderText.text = "";
    }

    // ---------------------------
    //  FLUJO DE ENTRADA
    // ---------------------------

    // Paso 1: escribir nombre y pasar al lobby/selección
    public void OnLoginNext()
    {
        playerName = nameInput ? nameInput.text.Trim() : "Jugador";
        if (string.IsNullOrEmpty(playerName)) playerName = "Jugador";

        loginPanel.SetActive(false);
        // Si usas botones directos de sala, puedes ir directo al chat:
        // pero por claridad dejamos el lobby, o puedes ocultarlo si no lo usas.
        lobbyPanel.SetActive(true);
    }

    // Si usas DROPDOWN/INPUT para elegir sala:
    public void OnJoinRoomPressed()
    {
        string chosen = (roomDropdown && roomDropdown.options.Count > 0)
            ? roomDropdown.options[roomDropdown.value].text.Trim()
            : "general";

        string custom = customRoomInput ? customRoomInput.text.Trim() : "";
        currentRoom = string.IsNullOrEmpty(custom) ? chosen : custom;
        if (string.IsNullOrEmpty(currentRoom)) currentRoom = "general";

        if (roomHeaderText) roomHeaderText.text = $"Room: {currentRoom} 0/{ROOM_CAPACITY}";
        _ = ConnectAndJoinAsync();
    }

    // Si usas BOTONES directos (Videojuegos/Anime/Películas/Libros):
    public void SelectRoom(string roomName)
    {
        currentRoom = string.IsNullOrEmpty(roomName) ? "general" : roomName.Trim();
        if (roomHeaderText) roomHeaderText.text = $"Room: {currentRoom} 0/{ROOM_CAPACITY}";
        _ = ConnectAndJoinAsync();
    }

    // Conecta al server, envía nombre, recibe confirmación y hace /join sala
    async Task ConnectAndJoinAsync()
    {
        try
        {
            chatLog.text = "";

            cts = new CancellationTokenSource();
            client = new TcpClient();
            await client.ConnectAsync(host, port);
            connected = true;

            var stream = client.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            // Enviar nombre
            await writer.WriteLineAsync(playerName);

            // Esperar confirmación + listado de rooms
            string line1 = await reader.ReadLineAsync(); // ">>> Conectado como X"
            string line2 = await reader.ReadLineAsync(); // ">>> Rooms: ..."
            EnqueueIf(line1);
            EnqueueIf(line2);

            // Enviar join a la sala elegida
            await writer.WriteLineAsync($"/join {currentRoom}");

            // Cambiar a panel de chat
            lobbyPanel.SetActive(false);
            chatPanel.SetActive(true);

            // Iniciar loop de lectura
            _ = Task.Run(() => ReadLoop(cts.Token));
        }
        catch (Exception ex)
        {
            AppendChat($">>> Error de conexión: {ex.Message}");
            connected = false;
        }
    }

    // ---------------------------
    //  LECTURA Y PROCESADO
    // ---------------------------

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

    void EnqueueIf(string s)
    {
        if (!string.IsNullOrEmpty(s)) incoming.Enqueue(s);
    }

    void Update()
    {
        while (incoming.TryDequeue(out var msg))
        {
            // Lista de usuarios -> actualiza panel y contador n/5
            if (msg.StartsWith(">>> Usuarios conectados:"))
            {
                UpdateUserList(msg);
                continue;
            }

            // Detecta confirmación de entrada al room: ">>> Entraste al room 'X' (n/5)"
            if (msg.StartsWith(">>> Entraste al room '"))
            {
                // extrae nombre de sala
                int start = msg.IndexOf('\'') + 1;
                int end = msg.IndexOf('\'', start);
                if (start > 0 && end > start)
                {
                    currentRoom = msg.Substring(start, end - start);
                }
                // extrae (n/5) si viene
                int paren = msg.LastIndexOf('(');
                if (paren >= 0)
                {
                    int slash = msg.IndexOf('/', paren);
                    int close = msg.IndexOf(')', paren);
                    if (slash > paren && close > slash)
                    {
                        var nStr = msg.Substring(paren + 1, slash - paren - 1).Trim();
                        if (int.TryParse(nStr, out var n)) lastUserCount = n;
                    }
                }
                UpdateRoomHeader();
                AppendChat(msg);
                continue;
            }

            // Sala llena -> volver al lobby (opcional)
            if (msg.StartsWith(">>> Room '") && msg.Contains("está lleno"))
            {
                AppendChat(msg);
                chatPanel.SetActive(false);
                lobbyPanel.SetActive(true);
                currentRoom = null;
                lastUserCount = 0;
                UpdateRoomHeader();
                continue;
            }

            // Mensaje normal
            AppendChat(msg);
        }
    }

    // ---------------------------
    //  UI Chat
    // ---------------------------

    void AppendChat(string text)
    {
        string color = "white";
        if (text.StartsWith(">>>")) color = "#A3B4FF";
        else if (text.Contains($"[{playerName}]")) color = "#7CFF7C";
        chatLog.text += $"<b><color={color}>{text}</color></b>\n";
    }

    public async void OnSendClicked()
    {
        if (!connected || writer == null) return;

        string msg = messageInput.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

        await writer.WriteLineAsync(msg);
        // Eco local
        AppendChat($"<color=#7CFF7C>[Tú]</color> {msg}");
        messageInput.text = "";
        messageInput.ActivateInputField();
    }

    // Actualiza lista de usuarios y header n/5
    void UpdateUserList(string msg)
    {
        foreach (Transform child in userListPanel)
            Destroy(child.gameObject);

        int idx = msg.IndexOf(":");
        string list = (idx >= 0 ? msg.Substring(idx + 1) : "").Trim();
        string[] names = list.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

        lastUserCount = (names.Length == 1 && names[0] == "") ? 0 : names.Length;
        UpdateRoomHeader();

        foreach (var n in names)
        {
            var go = Instantiate(userButtonPrefab, userListPanel);
            var txt = go.GetComponentInChildren<TMP_Text>();
            txt.text = n;

            var btn = go.GetComponent<Button>();
            if (n == playerName) btn.interactable = false;
            btn.onClick.AddListener(() =>
            {
                messageInput.text = $"/msg {n} ";
                messageInput.ActivateInputField();
            });
        }
    }

    void UpdateRoomHeader()
    {
        if (roomHeaderText != null && !string.IsNullOrEmpty(currentRoom))
            roomHeaderText.text = $"Room: {currentRoom} {lastUserCount}/{ROOM_CAPACITY}";
    }

    // ---------------------------
    //  Desconexión limpia
    // ---------------------------

    public async void Disconnect()
    {
        if (!connected) { // solo reset UI
            chatPanel.SetActive(false);
            lobbyPanel.SetActive(true);
            return;
        }

        try { if (writer != null) await writer.WriteLineAsync("/salir"); } catch {}
        try { cts?.Cancel(); } catch {}
        try { reader?.Close(); } catch {}
        try { writer?.Close(); } catch {}
        try { client?.Close(); } catch {}

        connected = false;
        currentRoom = null;
        lastUserCount = 0;
        UpdateRoomHeader();

        chatPanel.SetActive(false);
        lobbyPanel.SetActive(true);

        // Limpia UI
        foreach (Transform t in userListPanel) Destroy(t.gameObject);
        chatLog.text = "";
        messageInput.text = "";
        AppendChat(">>> Desconectado.");
    }

    void OnApplicationQuit()
    {
        try { cts?.Cancel(); } catch {}
        try { client?.Close(); } catch {}
    }
}
