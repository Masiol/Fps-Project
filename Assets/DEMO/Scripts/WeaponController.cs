using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private float launchVelocity;
    [SerializeField] private float fireRate = 10f;
    public bool automaticShoot; // if no = semiAuto | if true = fullAuto
    private Vector3 destination;
    public GameObject bulletProjectile;
    public Transform startShootingPoint;
    public Camera cam;

    [Header("References")]
    public CharacterController characterController;
    public InputPlayer inputPlayer;
    private GameObject bulletHole;
    public GunGlowing gunGlowing;

    [Header("Gun")]
    public GameObject muzzleFlash;
    [HideInInspector]
    public int maxAmmo = 40;
    [HideInInspector]
    public int currentAmmo;
    [HideInInspector]
    public bool isEmpty = false;
    [HideInInspector]
    public AudioSource _audioSource;
    public AudioClip[] audioClips;
    [SerializeField] private float scopingTime = 2f;
    public bool isScoping = false;
    public Vector3 startScopePoint;
    public Vector3 endScopePoint;
    public Vector3 currentPos;

    public Text currentAmmoText;
    public Text maxAmmoText;

    [Header("Recoil")]
    public Slider recoilProgressBar;
    public float recoilTime = 4;
    private float currentProgressBarValue;
    private float maxProgressBarValue;
    [HideInInspector]
    public bool isReloading;

    private void Start()
    {
        recoilProgressBar.gameObject.SetActive(false);
        _audioSource = GetComponent<AudioSource>();
        currentAmmo = maxAmmo;
        maxAmmoText.text = "/" + " " + maxAmmo.ToString();
        muzzleFlash.SetActive(false);
        bulletHole = GameObject.Find("BulletHole");
        startScopePoint = this.transform.localPosition;
        endScopePoint = new Vector3(0.0f, -0.2f, 0.15f);
    }
    private void Update()
    {
        currentPos = this.transform.localPosition;
        currentAmmoText.text = currentAmmo.ToString();

        //Reloading
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo != maxAmmo && !isScoping)
        {
            if (isScoping)
            {
                StopScoping();
                isScoping = false;
                
            }
            recoilTime = 0;
            isReloading = true;
            recoilProgressBar.gameObject.SetActive(true);
            inputPlayer.Reloading();


        }
        else if (Input.GetKeyUp(KeyCode.R) || inputPlayer.playerJump)
        {
            isReloading = false;
            recoilTime = 4f;
            recoilProgressBar.gameObject.SetActive(false);
            inputPlayer.CancelReloading();
        }
        if (isReloading)
        {
            recoilTime += Time.deltaTime / 1;
            recoilProgressBar.value = recoilTime;

            if (recoilTime >= 4)
            {
                isEmpty = false;
                isReloading = false;
                currentAmmo = maxAmmo;
                recoilProgressBar.gameObject.SetActive(false);
            }
        }
        if(isScoping)
        {
            inputPlayer.currentSpeed = inputPlayer.crouchMovementSpeed;
        }
    }
    public void FullSemi()
    {
        automaticShoot = !automaticShoot;

    }
    public IEnumerator RapidFire()
    {
        if (automaticShoot)
        {
            while (true)
            {
                Shoot();
                gunGlowing.startGlowing = true;
                yield return new WaitForSeconds(1 / fireRate);
            }
        }
        else if (!automaticShoot)
        {
            Shoot();
            yield return null;
        }
    }
    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        muzzleFlash.SetActive(true);
        StartCoroutine(MuzzleFlash());

        if (Physics.Raycast(ray, out hit))
        {
            destination = hit.point;
        }
        else
        {
            destination = ray.GetPoint(1000);
        }

        InstantiateProjectile();
        //Instantiate(bulletHole, hit.point + hit.normal * 0.01f, Quaternion.FromToRotation(-Vector3.forward, hit.normal * 0.025f));
        currentAmmo -= 1;
        _audioSource.clip = audioClips[Random.Range(0, audioClips.Length - 1)];
        _audioSource.Play();

    }
    private void InstantiateProjectile()
    {
        var projectileObj = Instantiate(bulletProjectile, startShootingPoint.position, Quaternion.identity) as GameObject;
        projectileObj.GetComponent<Rigidbody>().velocity = (destination - startShootingPoint.position).normalized * launchVelocity;
        Destroy(projectileObj, 2);
    }
    IEnumerator MuzzleFlash()
    {
        yield return new WaitForSeconds(0.01f);
        muzzleFlash.SetActive(false);
    }
    #region - SCOPING - 
    public void Scoping()
    {
        isScoping = true;
        
        StartCoroutine(Move(startScopePoint, endScopePoint, scopingTime));

        if(isScoping)
        { 
            if (isReloading)
            {
            isReloading = false;
            inputPlayer.CancelReloading();
            }
        }
       
    }
    public void StopScoping()
    {
        isScoping = false;
        inputPlayer.currentSpeed = inputPlayer.walkingMovementSpeed;
        StartCoroutine(MoveBack(currentPos, startScopePoint, scopingTime));
        this.transform.localPosition = startScopePoint;
        
        
    }
    IEnumerator Move(Vector3 beginPos, Vector3 endPos, float time)
    {
        for (float t = 0; t < 1; t += Time.deltaTime / time *2)
        {
            transform.localPosition = Vector3.Lerp(beginPos, endPos, t);
            yield return null;
        }
    }
    IEnumerator MoveBack(Vector3 beginPos, Vector3 endPos, float time)
    {
        for (float t = 0; t < 1; t += Time.deltaTime / time * 2)
        {
            transform.localPosition = Vector3.Lerp(beginPos, endPos, t);
            yield return null;
        }
    }

}
    #endregion
  


