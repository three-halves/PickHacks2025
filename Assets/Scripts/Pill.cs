using Unity.Netcode;
using UnityEngine;

public class Pill : NetworkBehaviour
{
    private ulong ownerId;
    [SerializeField] private Rigidbody rb;

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
            rb.linearVelocity = direction * velocity;
            rb.angularVelocity = rb.transform.TransformDirection(new Vector3(Random.Range(5f, 15f), 0f, 0f));
        }
        
    }

    public void Break()
    {
        Player owner = NetworkManager.ConnectedClients[ownerId].PlayerObject.GetComponent<Player>();
        Vector3 dist = owner.transform.position- transform.position;
        owner.ApplyInstantVelocity(1 / (dist.magnitude * dist.magnitude) * 2f * dist.normalized);

        GetComponent<NetworkObject>().Despawn();
    }

    void OnCollisionEnter(Collision collision) 
    {
        Break();
    }


}