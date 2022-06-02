using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform viewPoint;
    public float mouseSensitivity = 1.0f;
    private Vector2 mouseInput;
    private float verticalMouseInput;
    private Camera playerCam;
    public float fov = 60f;

    private Vector3 moveInput;
    private Vector3 moveDirection;
    public float moveSpeed = 4.0f;

    public Vector3 jumpForce = new Vector3(0, 5, 0);
    public Transform groundCheckPoint;
    public LayerMask groundLayers;
    Rigidbody rb;

    private bool isCursorAppear = false;

    public List<GunScript> guns = new List<GunScript>();
    private int gunIndex = 0;

    private float shotTimer;

    public int[] ammunition;
    public int[] maxAmmunition;
    public int[] ammoClip;
    public int[] maxAmmoClip;

    public GameObject bulletImpact;

    UIManager uiManager;

    SpawnManager spawnManager;

    private void Awake()
    {
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();

        spawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();
    }


    private void Start()
    {
        Initailize();

        CursorLock();
    }

    public void Initailize()
    {
        playerCam = Camera.main;

        rb = GetComponent<Rigidbody>();

        uiManager.SetBulletText(ammoClip[gunIndex], ammunition[gunIndex]);

        transform.position = spawnManager.GetSpawnPoint().position;
    }

    private void Update()
    {
        PlayerRotate();

        PlayerMove();

        if (IsOnGround())
        {
            Run();

            Jump();
        }

        CursorLock();

        SwitchingGuns();

        Aim();

        Fire();

        Reload();
    }

    public void FixedUpdate()
    {
        uiManager.SetBulletText(ammoClip[gunIndex], ammunition[gunIndex]);
    }


    public void LateUpdate()
    {
        CameraControll();
    }

    public void PlayerRotate()
    {
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X") * mouseSensitivity, 
            Input.GetAxisRaw("Mouse Y") * mouseSensitivity);

        transform.rotation = Quaternion.Euler(transform.eulerAngles.x,
            transform.eulerAngles.y + mouseInput.x,
            transform.eulerAngles.z);

        verticalMouseInput += mouseInput.y;
        verticalMouseInput = Mathf.Clamp(verticalMouseInput, -50f, 50f);

        viewPoint.rotation = Quaternion.Euler(-verticalMouseInput,
            viewPoint.transform.rotation.eulerAngles.y,
            viewPoint.transform.rotation.eulerAngles.z);


    }


    public void PlayerMove()
    {
        moveInput = new Vector3(Input.GetAxisRaw("Horizontal"),
            0,
            Input.GetAxisRaw("Vertical"));

        moveDirection = ((transform.forward * moveInput.z) + (transform.right * moveInput.x)).normalized;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    public bool IsOnGround()
    {
        return Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.20f, groundLayers);
    }

    public void Jump()
    {
        if(IsOnGround() && Input.GetKeyDown(KeyCode.Space)) rb.AddForce(jumpForce, ForceMode.Impulse);
    }

    public void Run()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) moveSpeed = 8.0f;

        else moveSpeed = 4.0f;
    }

    public void CursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) isCursorAppear = true; 
        else if (Input.GetMouseButton(0)) isCursorAppear = false;

        if (!isCursorAppear) Cursor.lockState = CursorLockMode.Locked;
        else Cursor.lockState = CursorLockMode.None;
    }

    public void CameraControll()
    {
        playerCam.transform.position = viewPoint.position;
        playerCam.transform.rotation = viewPoint.rotation;
    }

    public void SwitchingGuns()
    {
        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0.1f)
        {
            Debug.Log("スクロールup");

            gunIndex++;

            if (gunIndex >= guns.Count) gunIndex = 0;

            SwitchGun();
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel") < -0.1f)
        {
            Debug.Log("スクロールdown");

            gunIndex--;

            if (gunIndex < 0) gunIndex = guns.Count - 1;

            SwitchGun();
        }
        for (int i = 0; i < guns.Count; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))//ループの数値＋１をして文字列に変換。その後、押されたか判定
            {
                gunIndex = i;//銃を扱う数値を設定

                //実際に武器を切り替える関数
                SwitchGun();

            }
        }
    }

    public void SwitchGun()
    {
        foreach(GunScript gun in guns)
        {
            gun.gameObject.SetActive(false);
        }

        guns[gunIndex].gameObject.SetActive(true);
    }

    public void Aim()
    {
        if (Input.GetMouseButton(1))
        {
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, guns[gunIndex].adsZoom, guns[gunIndex].adsSpeed * Time.deltaTime);
        }
        else
        {
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, fov, guns[gunIndex].adsSpeed * Time.deltaTime);
        }
    }

    public void Fire()
    {
        if (Input.GetMouseButton(0) && ammoClip[gunIndex] > 0 && Time.time > shotTimer)
        {
            FiringBullet();
        }
    }

    private void FiringBullet()
    {
        ammoClip[gunIndex]--;

        Ray ray = playerCam.ViewportPointToRay(new Vector2(.5f, .5f));

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject bulletImpactObject = Instantiate(guns[gunIndex].bulletImpact,
                hit.point + (hit.normal * .002f), 
                Quaternion.LookRotation(hit.normal, Vector3.up));

            Destroy(bulletImpactObject, 10f);
        }

        shotTimer = Time.time + guns[gunIndex].shootInterval;

    }

    public void Reload()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            int amountNeed = maxAmmoClip[gunIndex] - ammoClip[gunIndex];
            int amountAvailable = amountNeed <= ammunition[gunIndex] ? amountNeed : ammunition[gunIndex];

            if(amountAvailable != 0 && ammunition[gunIndex] != 0)
            {
                ammunition[gunIndex] -= amountAvailable;
                ammoClip[gunIndex] += amountAvailable;
            }
        }
    }
}
