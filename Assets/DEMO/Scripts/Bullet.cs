using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform particleBoom;
    EnemyAI enemyAI;
    private void Update()
    {
        particleBoom = GameObject.Find("ParticleExplosion").transform;
    }
    public void OnCollisionEnter(Collision other)
    { 
        if(other.transform.tag == "Enemy")
        {
            other.gameObject.GetComponent<EnemyAI>().HealthSystem(15);
        }
        Destroy(this.gameObject);
        var explosion = Instantiate(particleBoom, this.transform.position, Quaternion.identity);
        Destroy(explosion.gameObject, 1f);
       
    }
}
