using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] public Transform toFollow;

    // Update is called once per frame
    void Update()
    {

        if (toFollow) transform.position = toFollow.position;
    }
}