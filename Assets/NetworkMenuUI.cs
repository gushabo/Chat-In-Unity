using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startPanel; // Panel principal con todo
    public GameObject customizationPanel; // Panel con botones de apariencia

    [Header("UI Elements in Start Panel")]
    public TMP_InputField nameInputField;
    public Button hostButton;
    public Button clientButton;
    public Button toggleCustomizationButton;
    public Button toggleChatButton; // Nuevo botón para chat

    [Header("Customization Panel")]
    public GameObject characterCustomizationPanel;

    [Header("Chat Panel")]
    public GameObject chatView; // Referencia al panel del chat

    public a ScriptChido;

    private void Start()
    {
        // Aseguramos estado inicial
        if (startPanel != null) startPanel.SetActive(true);
        if (customizationPanel != null) customizationPanel.SetActive(false);
        if (characterCustomizationPanel != null) characterCustomizationPanel.SetActive(false);
        if (toggleCustomizationButton != null) toggleCustomizationButton.gameObject.SetActive(false);
        if (toggleChatButton != null) toggleChatButton.gameObject.SetActive(false);

        // Validar estado inicial de los botones
        ValidateNameInput();

        // Conectar listeners
        hostButton.onClick.AddListener(() => StartConnection(true));
        clientButton.onClick.AddListener(() => StartConnection(false));

        // Botón para mostrar/ocultar personalización
        if (toggleCustomizationButton != null)
            toggleCustomizationButton.onClick.AddListener(ToggleCustomizationPanel);

        // Botón para mostrar/ocultar chat
        if (toggleChatButton != null)
            toggleChatButton.onClick.AddListener(ToggleChatView);

        // Enter para conectar (actúa como host por defecto)
        nameInputField.onSubmit.AddListener((text) => StartConnection(true));

        // Escuchar cambios en el input field
        nameInputField.onValueChanged.AddListener((text) => ValidateNameInput());
    }

    private void ValidateNameInput()
    {
        bool hasValidName = !string.IsNullOrWhiteSpace(nameInputField.text);

        // Habilitar/deshabilitar botones según si hay nombre
        hostButton.interactable = hasValidName;
        clientButton.interactable = hasValidName;
    }

    private void StartConnection(bool isHost)
    {
        // Validar y guardar nombre
        if (string.IsNullOrWhiteSpace(nameInputField.text))
        {
            nameInputField.text = "Player";
        }

        // Guardar el nombre en GameDataManager
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SetPlayerName(nameInputField.text);
        }

        // Cambiar a panel de personalización
        if (startPanel != null) startPanel.SetActive(false);
        if (customizationPanel != null) customizationPanel.SetActive(true);

        // Mostrar botones de personalización y chat
        if (toggleCustomizationButton != null)
            toggleCustomizationButton.gameObject.SetActive(true);
        if (toggleChatButton != null)
            toggleChatButton.gameObject.SetActive(true);

        // Iniciar conexión
        if (isHost)
        {
            Debug.Log("Iniciando como HOST...");
            ScriptChido.StartRelay();
        }
        else
        {
            Debug.Log("Iniciando como CLIENT...");
            ScriptChido.JoinRelay();
        }
    }

    private void ToggleCustomizationPanel()
    {
        if (characterCustomizationPanel != null)
        {
            bool isActive = !characterCustomizationPanel.activeSelf;
            characterCustomizationPanel.SetActive(isActive);
        }
    }

    private void ToggleChatView()
    {
        if (chatView != null)
        {
            chatView.SetActive(!chatView.activeSelf);
        }
    }
}