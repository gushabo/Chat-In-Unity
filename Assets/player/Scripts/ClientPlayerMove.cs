using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class ClientPlayerMove : NetworkBehaviour
{
    [Header("Referencias")]
    [SerializeField] private playerMovement m_PlayerMovement;
    [SerializeField] private CinemachineCamera vcamPrefab;

    private CinemachineCamera myCam;

    public NetworkVariable<bool> IsWalking = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public NetworkVariable<int> LookDirection = new NetworkVariable<int>(
        1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    // NUEVO: Estado Sitting
    public NetworkVariable<bool> IsSitting = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private void Awake()
    {
        m_PlayerMovement.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            m_PlayerMovement.enabled = true;

            myCam = Instantiate(vcamPrefab);
            myCam.Follow = transform;
            myCam.LookAt = transform;
        }
    }

    private void Update()
    {
        m_PlayerMovement.UpdateAnimator(IsWalking.Value, IsSitting.Value);
        m_PlayerMovement.UpdateFlip(LookDirection.Value);
    }
}
