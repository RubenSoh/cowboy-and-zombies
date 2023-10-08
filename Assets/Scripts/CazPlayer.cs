using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.Purchasing;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;


public class CazPlayer : NetworkBehaviour
{
    private CazPlayerInput input;
    private CharacterController controller;
    private Vector3 playerVelocity;
    private float xRotation;

    private Vector3 defaultCameraPos;
    
    // Move settings
    public float speed = 5f;
    public float jumpHeight = 1f;
    public float gravity = -9.8f;

    // Camera settings
    public Camera cam;
    public float xSensitivity = 30f;
    public float ySensitivity = 30f;
    
    private Image damageEffect;
    private Image deathEffect;
    private HealthComponent health;
    private CazGameManager _gameManager;
    
    public NetworkVariable<int> money = new();
    
    private void Awake()
    {
        _gameManager = GameObject.Find("CazGameManager").GetComponent<CazGameManager>();
        defaultCameraPos = cam.transform.localPosition;
        health = GetComponent<HealthComponent>();
        input = new CazPlayerInput();
        input.OnFoot.Enable();
        damageEffect = GameObject.FindWithTag("DamageEffect").GetComponent<Image>();
        deathEffect = GameObject.Find("DEATH").GetComponent<Image>();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void ShootGun()
    {
        GetComponentInChildren<CazGun>().ShootGunServerRpc(cam.transform.forward);
    }
    
    public override void OnNetworkSpawn()
    {
        // Disable the camera for players that are not the client
        if (!IsOwner)
        {
            GetComponentInChildren<Camera>().enabled = false;
        }

        health.Health.OnValueChanged += OnHealthChange;
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, Screen.height - 110, 300, 100));
        var health = this.health.Health.Value;
        var maxHealth = this.health.maxHealth;
        var money = this.money.Value;
        
        GUILayout.Label("Health: " + health + " / " + maxHealth);
        GUILayout.Label("Money: " + money);
        GUILayout.EndArea();
    }

    private void OnHealthChange(float oldHealth, float newHealth)
    {
        if (newHealth <= 0)
        {
            deathEffect.enabled = true;
        }
        if (IsClient && IsOwner)
        {
            StartCoroutine(ShakeCamera());
        }
    }

    private IEnumerator ShakeCamera()
    {
        cam.transform.localPosition += UnityEngine.Random.insideUnitSphere * 0.2f;
        damageEffect.enabled = true;
        yield return new WaitForSeconds(0.1f);
        damageEffect.enabled = false;
        cam.transform.localPosition = defaultCameraPos;
    }
    
    public void FixedUpdate()
    {
        Vector2 movementInput = input.OnFoot.Movement.ReadValue<Vector2>();
        DoMove(movementInput);
    }

    public void LateUpdate()
    {
        Vector2 lookInput = input.OnFoot.Look.ReadValue<Vector2>();

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            DoLook(lookInput);
        }

        if (input.OnFoot.Shoot.WasPressedThisFrame())
        {
            ShootGun();
        }
    }
    
    private void DoLook(Vector2 lookIn)
    {
        float mouseX = lookIn.x;
        float mouseY = lookIn.y;
        //calculate camera rotation for looking up and down
        xRotation -= (mouseY * Time.deltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        //apply this to our camera transform.
        cam.transform.localRotation = Quaternion.Euler(xRotation,0,0);
        //rotate player to look left and right
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime * xSensitivity));
    }
    
    private void DoMove(Vector2 movementIn)
    {
        Vector3 moveDirection = Vector3.zero;
        
        moveDirection.x = movementIn.x;
        moveDirection.z = movementIn.y;
        
        controller.Move(transform.TransformDirection(moveDirection) * (speed * Time.deltaTime));
        playerVelocity.y += gravity * Time.deltaTime;
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        controller.Move(playerVelocity * Time.deltaTime);
        transform.position = controller.transform.position;
    }
    
    public void Move()
    {
    }


    [ServerRpc]
    void PurchaseItemServerRpc(NetworkBehaviourReference item)
    {
        if (!item.TryGet(out PurchasableItem p)) throw new Exception("wtf");

        if (p.price > money.Value) return;

        money.Value -= p.price;
        
        if (p.levelUp > 0)
        {
            _gameManager.LevelUp(p.levelUp);
        }
    }
    
    void Update()
    {
        if (!IsClient) return;
        if (input.OnFoot.Interact.WasPressedThisFrame())
        {
            var ray = new Ray(cam.transform.position, cam.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit)) return;
            
            var obj = hit.collider;
            PurchasableItem p;
            if (obj.TryGetComponent(out p))
            {
                PurchaseItemServerRpc(p);
            }
        }
    }
    
    
}