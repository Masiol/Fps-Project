using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEnemy : MonoBehaviour
{
    private Transform particleBoom;
    EnemyAI enemyAI;
    private void Update()
    {
        particleBoom = GameObject.Find("ParticleExplosion").transform;
    }
    public void OnTriggerEnter(Collider other)
    { 
        if(other.transform.tag == "Player")
        {
            other.gameObject.GetComponent<HealthPlayer>().DoDamage(5);
        }
        Destroy(this.gameObject);
        var explosion = Instantiate(particleBoom, this.transform.position, Quaternion.identity);
        Destroy(explosion.gameObject, 1f);
       
    }
}
