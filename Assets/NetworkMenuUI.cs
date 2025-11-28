using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startPanel;        // Panel con los botones Host/Client
    public GameObject customizationPanel; // Panel con botones de apariencia

    [Header("Buttons")]
    public Button hostButton;
    public Button clientButton;

    public a ScriptChido;
    private void Start()
    {
        // Aseguramos estado inicial
        if (startPanel != null) startPanel.SetActive(true);
        if (customizationPanel != null) customizationPanel.SetActive(false);

        // Conectar listeners
        hostButton.onClick.AddListener(StartAsHost);
        clientButton.onClick.AddListener(StartAsClient);
    }

    private void StartAsHost()
    {
        Debug.Log("Iniciando como HOST...");
        //NetworkManager.Singleton.StartHost();
        ScriptChido.StartRelay();
        SwitchToCustomizationPanel();
    }

    private void StartAsClient()
    {
        Debug.Log("Iniciando como CLIENT...");
        //NetworkManager.Singleton.StartClient();
        ScriptChido.JoinRelay();
        SwitchToCustomizationPanel();

    }

    private void SwitchToCustomizationPanel()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (customizationPanel != null) customizationPanel.SetActive(true);
    }
}
