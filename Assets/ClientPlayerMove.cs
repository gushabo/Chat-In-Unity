using Unity.Netcode;
using UnityEngine;

public class ClientPlayerMove : NetworkBehaviour
{
    [SerializeField] private playerMovement m_PlayerMovement;

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
        }


        
    }

    [Rpc(SendTo.Server)]
    private void UpdateInputServerRpc(Vector2 move, Vector2 look, bool jump, bool sprint)
    {

    }

}
