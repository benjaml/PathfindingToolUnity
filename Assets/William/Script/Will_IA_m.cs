﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Will_IA_m : MonoBehaviour {
    
    float shootCooldown= 1f;
    float range = 20;
    Vector3 spawn;
    Rigidbody rigid;
    public List<GameObject> targets;
    public GameObject currentTarget;
    NavMeshAgent agent;
    GameObject bullet;
    bool canShoot = false;
    Vector3 strafeDest;
    float lastShoot=0;
    public bool isStrafing = false;
    void Start () {
        rigid = GetComponent<Rigidbody>();
        spawn = transform.position;
        agent = GetComponent<NavMeshAgent>();

        targets = transform.parent.GetComponent<TeamManagerWill>().ennemis;
        //targets = new List<GameObject>(GameObject.FindGameObjectsWithTag("Target"));
        //targets.Remove(this.gameObject);
        bullet = new GameObject();
        bullet = (GameObject) Resources.Load("Bullet");
        InvokeRepeating("targetUpdate", 0, 0.3f);
        //InvokeRepeating("shoot", 0.1f, shootCooldown);
    }
	
	void Update () {
        shoot();
        float d = Vector3.Distance(agent.velocity, Vector3.zero);
        if (d < 0.2f)
        {
            //Debug.Log("Reset Will :"+rigid.velocity+" distance:"+d);
            StopAllCoroutines();
            isStrafing = false;
        }
	}
    

    void targetUpdate()
    {
        GameObject tempTarget = targets[0];
        Vector3 myPos = transform.position;
        float distance = Vector3.Distance(myPos, targets[0].transform.position);

        for (int i = 1; i < targets.Count; i++)
        {
            float tempDist = Vector3.Distance(myPos, targets[i].transform.position);
            if (tempDist < distance)
            {
                distance = tempDist;
                tempTarget = targets[i];
            }
        }
        currentTarget = tempTarget;
        if (!isStrafing)
        {
            agent.SetDestination(currentTarget.transform.position);
        }
    }
    

    IEnumerator startStrafe()
    {
        isStrafing = true;
        Vector3 pos = transform.position;
        strafeDest = transform.position+(transform.right * 5);
        agent.SetDestination(strafeDest);
        yield return new WaitForSeconds(0.5f);
        //isStrafing = false;


        //float cmptTime = 0;
        //while (Vector3.Distance(strafeDest, transform.position) > 1 &&(cmptTime < 2))
        //{

        //    cmptTime +=Time.deltaTime;
        //   yield return 5;
        //}
        //agent.SetDestination(pos);
        //cmptTime = 0;
        //while (Vector3.Distance(pos, transform.position) > 1 && cmptTime < 2)
        //{
        //    cmptTime += Time.deltaTime;
        //    yield return 5;
        //}
    }


    void strafe()
    {
        if(!isStrafing)
        StartCoroutine(startStrafe());
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.tag == "Bullet")
        {
            isStrafing = false;
            StopAllCoroutines();
            agent.Warp(spawn);
            transform.position = spawn;
            targetUpdate();
        }
    }

    

    void shoot()
    {
        if (currentTarget == null)
            return;
        if (lastShoot + shootCooldown > Time.time)
            return;
        
        RaycastHit hit;
        Vector3 dir = currentTarget.transform.position - transform.position;
        if (Physics.Raycast(transform.position, dir, out hit))
        {
            //Debug.Log("ray " + hit.collider.name);
            if (hit.collider.gameObject == currentTarget)
            {
                strafe();
                shootBullet(currentTarget);     
            }
            else
            {
                isStrafing = false;
                StopAllCoroutines();

                //TEST
                foreach (GameObject obj in targets)
                {
                    dir = obj.transform.position - transform.position;
                    if (Physics.Raycast(transform.position, dir, out hit))
                    {
                        if (hit.collider.gameObject == obj)
                        {
                            shootBullet(obj);
                            return;
                        }
                    }
                }
                //
            }
            
        }

    }

    void shootBullet(GameObject targ)
    {
        lastShoot = Time.time;
        GameObject spawnedBullet = (GameObject)Instantiate(bullet, transform.position, transform.rotation);
        spawnedBullet.transform.LookAt(targ.transform.position + (targ.GetComponent<NavMeshAgent>().velocity.normalized));
        spawnedBullet.GetComponent<bulletScript>().launcherName = "TeamWill";
        Physics.IgnoreCollision(GetComponent<BoxCollider>(), spawnedBullet.GetComponent<CapsuleCollider>());
        
    }
}
