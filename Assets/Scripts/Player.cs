using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [Header("Component References")]
    [SerializeField] CharacterController characterController;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] private GameObject shootPrefab;

    [Header("Player Movement Parameters")]
    [SerializeField] private Vector2 camSens;
    [SerializeField] private float gravity = -1.0f;
    [SerializeField] private float jumpForce = 1.0f;
    [SerializeField] private float jumpDamping = 0.9f;
    [SerializeField] private float maxMoveVel = 0.8f;
    [SerializeField] private float maxSlopeStep = 0.3f;
    [SerializeField] private float startMoveVel = 0.2f;
    [SerializeField] private float timeToMaxVel = 0.1f;
    
    // globals
    GameObject lookDirection;
    private Vector3 moveInputDir;
    private Vector3 camRotateDir;
    private Vector3 addVel = Vector3.zero;
    private bool jumpPressed = false;
    private PlayerInput playerInput;
    private float timeMoving;
    private float curMoveVel;

    void Awake()
    {
        enabled = false;
    }

    public float GetDotVel()
    {
        return curMoveVel;
    }

    public override void OnNetworkSpawn()
    {
        Cursor.lockState = CursorLockMode.Locked;

        lookDirection = new GameObject("lookDirection");
        lookDirection.transform.SetParent(transform);
        enabled = IsOwner;
        if (IsOwner)
        {
            // cameraTransform = Camera.main.transform;
            Debug.Log("Add Component");
            cameraTransform = GameObject.Find("Main Camera").transform;
            Camera.main.GetComponent<FollowTransform>().toFollow = transform;
            Camera.main.GetComponent<FollowTransform>().enabled = true;
            playerInput = gameObject.AddComponent<PlayerInput>();
            playerInput.actions = inputActionAsset;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // calculate current movement speed
        if (moveInputDir != Vector3.zero) timeMoving += Time.deltaTime;
        else timeMoving = 0;

        // standard analouge lateral movement
        curMoveVel = startMoveVel + Mathf.SmoothStep(0, maxMoveVel - startMoveVel, timeMoving / timeToMaxVel);
        lookDirection.transform.eulerAngles = new Vector3(0, cameraTransform.eulerAngles.y, 0f);
        Vector3 forward = lookDirection.transform.TransformDirection(moveInputDir);

        // snap to slope/ground
        RaycastHit rh;
        Physics.Raycast(transform.position, Vector3.down, out rh, characterController.height / 2f + maxSlopeStep, LayerMask.GetMask("Default"));
        Vector3 tangent = Vector3.Cross(rh.normal, forward);
        // adjust movement to ground normal only if we are on the ground
        if (rh.collider != null) forward = -Vector3.Cross(rh.normal, tangent);

        // move controller
        characterController.Move(forward * curMoveVel);

        // slight "gravity" to update character controller
        characterController.Move(new Vector3(0, -0.01f, 0));

        // actual gravity
        if (!characterController.isGrounded)
        {
            addVel.y += gravity;
        }
        else 
        {
            addVel.y = 0;
        }

        // jump and jump damping
        if (jumpPressed && characterController.isGrounded)
        {
            addVel.y = jumpForce;
        }
        if (!jumpPressed && addVel.y > 0)
        {
            addVel.y *= jumpDamping;
        }

        characterController.Move(addVel);
    }

    public void OnMove(InputValue value)
    {
        Vector2 v = value.Get<Vector2>();
        moveInputDir.x = v.x;
        moveInputDir.z = v.y;
    }

    public void OnLook(InputValue value)
    {
        Vector2 v = value.Get<Vector2>();
        camRotateDir = new Vector3(v.y * camSens.y, v.x * camSens.x, 0);
        cameraTransform.eulerAngles += camRotateDir * 0.2f;
    }

    public void OnJump(InputValue value)
    {
        jumpPressed = value.isPressed;
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            // spawn pill object
            var shoot = Instantiate(shootPrefab);
            var shootNetworkObject = shoot.GetComponent<NetworkObject>();
            shootNetworkObject.Spawn();
            shootNetworkObject.GetComponent<Pill>().SetupServerRpc(OwnerClientId, transform.position, cameraTransform.forward, curMoveVel + 5f);
        }
    }

}
