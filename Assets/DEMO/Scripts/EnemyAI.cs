using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    [Header("NavMesh")]
    private NavMeshAgent agent; 

    [Header("References")] 
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Transform startShootPoint;
    private Transform Player;
    private Animator anim;
    public Material material; 
    private AudioSource audioSource;
    public AudioClip[] enemySound; //0 - Shooting 1-Dying 2-Walking 

    [Header("Patrolling")]
    private float distanceToPlayer;
    [SerializeField] private float safeDistance = 28f;
    private int destPoint = 0;
    public Transform[] points;
    public bool isPatrolling;
    public bool isIdle;

    [Header("Chasing")]
    [SerializeField] private float sightRange = 22f;
    public bool isChasing;

    [Header("Attacking")]
    [SerializeField] private float attackRange = 14f;
    public bool isAttacking;
    [SerializeField] private float damage;
    [SerializeField] private float fireRateEnemy = 0.5f;
    private bool alreadyAttacked = false;
    
    [Header("Health")]
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private Slider healthbar;
    private bool isDied;

    [Header("Others")]
    private float dissolve = 0.5f;
    private bool stopCount = true;
    private bool checkDissapear;


    void Start()
    {
        Player = GameObject.Find("Player").transform;
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();

        material = this.GetComponentInChildren<SkinnedMeshRenderer>().material;
        GoNextPoint();

        //Health
        healthbar = this.GetComponentInChildren<Slider>();
        currentHealth = maxHealth;
        healthbar.value = currentHealth;
        healthbar.maxValue = maxHealth;
    }
    private void GoNextPoint()
    {
       // Debug.Log("Patroluje");
        if (points.Length == 0)
            return;

        anim.SetBool("Walking", isPatrolling);
        anim.SetBool("Chasing", false);
        agent.speed = 3f;
        agent.destination = points[destPoint].position;
        destPoint = (destPoint + 1) % points.Length;
    }
    private void Update()
    {
        distanceToPlayer = Vector3.Distance(this.transform.position, Player.transform.position);
        Vector3 targetPos = new Vector3(Player.transform.position.x, transform.position.y, Player.transform.position.z);
        if (!isDied)
        {
            if (distanceToPlayer <= attackRange)
            {
                isAttacking = true;
                isChasing = false;
                isPatrolling = false;
                isIdle = false;
                transform.LookAt(targetPos);
            }
            else if (distanceToPlayer < sightRange && distanceToPlayer < safeDistance)
            {
                transform.LookAt(targetPos);
                isChasing = true;
                isPatrolling = false;
                isIdle = false;
                isAttacking = false;
            }
            else if (distanceToPlayer > safeDistance)
            {
                isAttacking = false;
                isChasing = false;

                if (points.Length > 0)
                {
                    isPatrolling = true;
                }
                else
                {
                    isIdle = true;
                }
            }

            if (!isAttacking && !isChasing && points.Length == 0)
            {
                Idle();
            }
            if (isAttacking && !isChasing && !isIdle)
            {
                Attacking();
            }
            if (!isAttacking && isChasing && !isIdle)
            {
                Chasing();
            }

            if (!agent.pathPending && agent.remainingDistance < 0.5f && points.Length != 0 && isPatrolling && !isAttacking && !isChasing)
                GoNextPoint();

           
        }
        #region - DISSOLVE SHADER -
        if (isDied && stopCount)
        {
            dissolve -= 0.8f * Time.deltaTime;
            //material.SetFloat("Dissolve", dissolve);
        }

        if(dissolve <= -1)
        {
            checkDissapear = true;
            stopCount = false;
        }
        if (checkDissapear)
        {
            material.SetFloat("StartDissapear", 1);
            dissolve += 0.8f * Time.deltaTime;
            material.SetFloat("Dissolve", dissolve);
            if(dissolve > 0.8f)
            {
                healthbar.gameObject.SetActive(false);
                audioSource.Stop();
            }
            
        }
        #endregion

    }
    private void Idle()
    {
        //Debug.Log("Stay");
        agent.speed = 3f;
    }
 
    private void Attacking()
    {
        attackRange = 18.5f;
        //Debug.Log("Atakuje");
        agent.SetDestination(transform.position);
        anim.SetBool("Shooting", isAttacking);



        if (!alreadyAttacked && !isDied)
        {
            Rigidbody rb = Instantiate(bullet, startShootPoint.transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            muzzleFlash.SetActive(true);
            audioSource.clip = enemySound[0];
            audioSource.Play();

            rb.AddForce(transform.forward * 20f, ForceMode.Impulse);
            rb.AddForce(transform.up * -1, ForceMode.Impulse);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), fireRateEnemy);
        }
    }   
    private void ResetAttack()
    {
        StartCoroutine(MuzzleFlash());
        alreadyAttacked = false;
    }
    public void Chasing()
    {
        agent.SetDestination(Player.transform.position);
        agent.speed = 4.25f;
        anim.SetBool("Walking", false);
        anim.SetBool("Chasing", isChasing);
        anim.SetBool("Shooting", false);
        //Debug.Log("Lece do ataku");
    }
    public void HealthSystem(float damage)
    {
        currentHealth -= damage;
        healthbar.value = currentHealth;
        sightRange = 50f;
        safeDistance = 80f;
        Chasing();
        if (currentHealth <= 0)
        {
            isDied = true;
            DiedEnemy();
        }
    }
    private void DiedEnemy()
    {
        audioSource.clip = enemySound[1];
        audioSource.Play();
        this.GetComponent<CapsuleCollider>().enabled = false;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        anim.SetBool("Walking", false);
        anim.SetBool("Chasing", false);
        anim.SetBool("Shooting", false);
        anim.SetTrigger("Death");
        material.SetFloat("_StartDissolve", 1);
        
    }
    IEnumerator MuzzleFlash()
    {
        yield return new WaitForSeconds(0.1f);
        muzzleFlash.SetActive(false);
    }
}
