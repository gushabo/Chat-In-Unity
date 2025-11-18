using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAppearanceNet : NetworkBehaviour
{
    [Header("Animators")]
    public RuntimeAnimatorController[] animatorOptions;

    [Header("Preview data (opcional)")]
    public Sprite[] previewSprites;      // mismo tamaño que animatorOptions
    public string[] previewNames;        // mismo tamaño que animatorOptions

    private NetworkVariable<int> selectedAnimator = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private Animator animator;

    public override void OnNetworkSpawn()
    {
        animator = GetComponentInChildren<Animator>();

        // Suscribirse a cambios de red
        selectedAnimator.OnValueChanged += OnAnimatorChanged;

        // Aplicar inmediatamente
        ApplyAnimator(selectedAnimator.Value);

        // Solo el Owner conecta los botones
        if (IsOwner)
            ConnectUIButtons();
    }

    // ====================================
    // Conectar botones dinámicamente
    // ====================================
    private void ConnectUIButtons()
    {
        var ui = CharacterSelectionUI.Instance;
        if (ui == null)
        {
            Debug.LogWarning("UI de selección no está en la escena.");
            return;
        }

        ui.nextButton.onClick.RemoveAllListeners();
        ui.prevButton.onClick.RemoveAllListeners();

        ui.nextButton.onClick.AddListener(NextAppearance);
        ui.prevButton.onClick.AddListener(PreviousAppearance);

        UpdatePreviewUI(selectedAnimator.Value);
    }

    // ====================================
    // Métodos para botones UI
    // ====================================
    public void NextAppearance()
    {
        if (!IsOwner) return;

        int next = selectedAnimator.Value + 1;
        if (next >= animatorOptions.Length)
            next = 0;

        selectedAnimator.Value = next;
    }

    public void PreviousAppearance()
    {
        if (!IsOwner) return;

        int prev = selectedAnimator.Value - 1;
        if (prev < 0)
            prev = animatorOptions.Length - 1;

        selectedAnimator.Value = prev;
    }

    // ====================================
    // Aplicar cambios sincronizados
    // ====================================
    private void OnAnimatorChanged(int oldVal, int newVal)
    {
        ApplyAnimator(newVal);

        if (IsOwner)
            UpdatePreviewUI(newVal);
    }

    private void ApplyAnimator(int index)
    {
        if (animator == null) return;

        animator.runtimeAnimatorController = animatorOptions[index];
    }

    private void UpdatePreviewUI(int index)
    {
        var ui = CharacterSelectionUI.Instance;
        if (ui == null) return;

        Sprite sprite = null;
        string label = "";

        if (previewSprites != null && index < previewSprites.Length)
            sprite = previewSprites[index];

        if (previewNames != null && index < previewNames.Length)
            label = previewNames[index];

        ui.UpdatePreview(sprite, label);
    }
}
