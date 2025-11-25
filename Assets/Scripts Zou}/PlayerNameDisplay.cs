using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Collections;

public class PlayerNameDisplay : NetworkBehaviour
{
    [Header("Configuración")]
    public float verticalOffset = 2.0f; // Altura sobre el player
    public Color textColor = Color.white;
    public int fontSize = 24;

    private GameObject nameDisplay;
    private TextMeshPro nameText;
    private NetworkVariable<FixedString64Bytes> networkPlayerName = new NetworkVariable<FixedString64Bytes>("Player");

    public override void OnNetworkSpawn()
    {
        // Crear el display del nombre si no existe
        CreateNameDisplay();

        if (IsOwner)
        {
            SetPlayerName(GameDataManager.Instance.playerName);
        }

        networkPlayerName.OnValueChanged += OnNameChanged;
        UpdateNameDisplay(networkPlayerName.Value.ToString());
    }

    private void CreateNameDisplay()
    {
        // Buscar si ya existe un nameDisplay
        nameDisplay = new GameObject("PlayerNameDisplay");
        nameDisplay.transform.SetParent(transform);
        nameDisplay.transform.localPosition = new Vector3(0, verticalOffset, 0);

        // Crear el componente TextMeshPro
        nameText = nameDisplay.AddComponent<TextMeshPro>();

        // Configurar el texto
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.fontSize = fontSize;
        nameText.color = textColor;
        nameText.sortingOrder = 1000; // Para que esté por encima de otros objetos

        // Configurar el RectTransform si existe
        var rectTransform = nameDisplay.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(3, 1);
        }
    }

    private void SetPlayerName(string name)
    {
        if (IsServer)
        {
            networkPlayerName.Value = name;
        }
        else
        {
            SetPlayerNameServerRpc(name);
        }
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerNameServerRpc(string name, RpcParams rpcParams = default)
    {
        networkPlayerName.Value = name;
    }

    private void OnNameChanged(FixedString64Bytes oldName, FixedString64Bytes newName)
    {
        UpdateNameDisplay(newName.ToString());
    }

    private void UpdateNameDisplay(string name)
    {
        if (nameText != null)
        {
            nameText.text = name;
        }
    }

    public string GetPlayerName()
    {
        return networkPlayerName.Value.ToString();
    }

    public override void OnNetworkDespawn()
    {
        networkPlayerName.OnValueChanged -= OnNameChanged;

        // Destruir el display cuando el player se destruya
        if (nameDisplay != null)
        {
            Destroy(nameDisplay);
        }
    }

    private void Update()
    {
        // Hacer que el texto siempre mire hacia la cámara
        if (nameDisplay != null && Camera.main != null)
        {
            nameDisplay.transform.LookAt(nameDisplay.transform.position + Camera.main.transform.rotation * Vector3.forward,
                                        Camera.main.transform.rotation * Vector3.up);
        }
    }
}