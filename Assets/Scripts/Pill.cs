using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Pill : NetworkBehaviour
{
    private ulong ownerId;
    [SerializeField] private Rigidbody rb;

    public override void OnNetworkSpawn()
    {

    }

    [ServerRpc(RequireOwnership = false)]
    public void SetupServerRpc(ulong _ownerId, Vector3 position, Vector3 direction, float velocity, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            ownerId = _ownerId;
            transform.position = position + direction; 
            rb.transform.rotation = Quaternion.LookRotation(direction);
            rb.transform.Rotate(90f, 0f, 0f);
            rb.linearVelocity = direction * velocity + new Vector3(0, 3f, 0);
            rb.angularVelocity = rb.transform.TransformDirection(new Vector3(Random.Range(5f, 15f), 0f, 0f));
        }
        
    }

    void OnCollisionEnter(Collision collision) 
    {
        GetComponent<NetworkObject>().Despawn();
    }
}