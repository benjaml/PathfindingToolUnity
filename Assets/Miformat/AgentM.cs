﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgentM : MonoBehaviour {

	//public GameObject cancer;
	GameObject bullet;
    GameObject target;
	GameObject[] tabTarget;
	GameObject currentBullet = null;
	float distToTarget;
	NavMeshAgent agent;
	Vector3 startPos = new Vector3(-70,1,-20);
	float fireRate = 1;
	public int ID;
	int state = 1;
	bool isArrived = false;
	bool isAvoiding = false;
	GameObject toAvoid = null;



	bool isPatrol = false;
	bool isWalk = true;
	TransitionMif TtoPatrol;
	TransitionMif TtoWalk;
	TransitionMif TtoAvoid;
	PatrolMif patrol;
	WalkMif walk;
	AvoidMif avoid;
	List<TransitionMif> trans;
	StateMachineMif SMM;
	StateMachineMif SMM2;


	// Use this for initialization
	void Start () 
	{
		patrol = new PatrolMif ();
		walk = new WalkMif ();
		avoid = new AvoidMif ();
		TtoPatrol = new TransitionMif(patrol, CheckPatrol);
		TtoWalk = new TransitionMif(walk, CheckWalk);
		TtoAvoid = new TransitionMif(avoid, CheckAvoid);
		trans = new List<TransitionMif>();
		trans.Add (TtoWalk);
		trans.Add (TtoAvoid);
		patrol.Init (trans);
		trans = new List<TransitionMif>();
		trans.Add (TtoPatrol);
		trans.Add (TtoAvoid);
		walk.Init (trans);
		trans = new List<TransitionMif>();
		trans.Add (TtoPatrol);
		trans.Add (TtoWalk);
		avoid.Init (trans);
		SMM = new StateMachineMif (walk);
		SMM2 = new StateMachineMif (SMM);

		bullet = Resources.Load ("Bullet") as GameObject;
		//startPos = this.gameObject.transform.position;
		agent = GetComponent<NavMeshAgent>();
		tabTarget = GameObject.FindGameObjectsWithTag ("Target");
		RemoveTargetFromTab (this.gameObject);
		target = FindCloseTarget();
		while (target.GetComponent<AgentM> () != null) 
		{
			RemoveTargetFromTab (target);
			target = FindCloseTarget();
		}
		/*switch (ID) 
		{
			case 0:
				agent.destination = new Vector3 (-73, 1, -16);
				break;
			case 1:
				agent.destination = new Vector3 (-52,1,-14);
				break;
			case 2:
				agent.destination = new Vector3 (-62,1,-8);
				break;
		}*/
		/*if (target != null) 
		{
			agent.destination = target.transform.position; 
		}*/
		agent.destination = getVect (ID, state);
		getPoto ();
		timeToSwitch = maxTimeSwitch;
    }

	Vector3 getVect(int MID, int MState)
	{
		if (MID == 0) 
		{
			if (MState == 0) {return  new Vector3 (-73, 1, -16);} 
			else {return new Vector3 (-65, 1, -16);}
		}
		else if (MID == 1) 
		{
			if (MState == 0) {return  new Vector3 (-52,1,-14);} 
			else {return new Vector3 (-52,1,-22);}
		}
		else if (MID == 2) 
		{
			if (MState == 0) {return  new Vector3 (-62,1,-8);} 
			else {return new Vector3 (-43,1,-18);}
		}
		return Vector3.zero;
	}

	bool CheckPatrol()
	{
		return isPatrol;
	}

	bool CheckWalk()
	{
		return isWalk;
	}

	bool CheckAvoid()
	{
		return isAvoiding;
	}

	GameObject M1;
	GameObject M2;
	GameObject M3;

	void getPoto()
	{
		AgentM[] listM = GameObject.FindObjectsOfType<AgentM> ();
		foreach (AgentM AM in listM) 
		{
			if (AM.ID == 0) {M1 = AM.gameObject;}
			else if (AM.ID == 1) {M2 = AM.gameObject;}
			else if (AM.ID == 2) {M3 = AM.gameObject;}
		}
		isPatrol = false;
		isWalk = true;
	}
	
	float timeToSwitch;
	float maxTimeSwitch = 10;

	void switchPoto()
	{
		int rnd1 = Random.Range (0,3);
		int rnd2 = rnd1;
		while (rnd2 == rnd1) {rnd2 = Random.Range (0,3);}
		int tempID;
		GameObject Mate1 = null;
		switch(rnd1)
		{
			case 0:
				Mate1 = M1;
			break;
			case 1:
				Mate1 = M2;
			break;
			case 2:
				Mate1 = M3;
			break;
		}
		GameObject Mate2 = null;
		switch(rnd2)
		{
			case 0:
				Mate2 = M1;
			break;
			case 1:
				Mate2 = M2;
			break;
			case 2:
				Mate2 = M3;
			break;
		}
		tempID = Mate1.GetComponent<AgentM> ().ID;
		Mate1.GetComponent<AgentM> ().ID = Mate2.GetComponent<AgentM> ().ID;
		Mate2.GetComponent<AgentM> ().ID = tempID;
		getPoto ();
	}

	// Update is called once per frame
	void Update () 
	{
		SMM2.Execute ();
		timeToSwitch -= Time.deltaTime;
		if (timeToSwitch < 0) 
		{
			switchPoto (); 
			timeToSwitch = maxTimeSwitch;
		}
		if (target != null)
        {
			LookAtTarget ();
            Cheat ();
            //agent.destination = target.transform.position;
        }
		if (currentBullet != null) {Dunk ();}
		fireRate -= Time.deltaTime;
		distToTarget = Vector3.Distance (this.gameObject.transform.position, target.transform.position);
		if (distToTarget < 15) 
		{
			SphereCollider ball = this.gameObject.transform.GetChild (0).gameObject.GetComponent<SphereCollider> ();
			ball.enabled = false;
			if (fireRate < 0 && TargetOnSight ()) 
			{
				Coloring (this.gameObject);
				fireRate = 1;
				Shoot ();
			}
			ball.enabled = true;
		}
		else 
		{
			target = FindCloseTarget();
			while (target.GetComponent<AgentM> ()) 
			{
				RemoveTargetFromTab (target);
				target = FindCloseTarget();
			}
		}
		if (target == null) 
		{
			Debug.Log ("Miformat Have Finish");
		}
		if (agent.remainingDistance < 0.5f) 
		{
			isArrived = true;
			isAvoiding = false;
		}
		if (!isAvoiding && isArrived) {Patrol ();}
		if (!isArrived){Avoid ();}
	}

	void Patrol()
	{
		/*switch (ID) 
		{
		case 0:
			//agent.destination = new Vector3 (-73, 1, -16);
			//agent.destination = new Vector3 (-65, 1, -16);
			if (state == 0) 
			{
				agent.destination = new Vector3 (-73, 1, -16);
				state = 1;
			} 
			else 
			{
				agent.destination = new Vector3 (-65, 1, -16);
				state = 0;
			}
			//isArrived = false;
			break;
		case 1:
			//agent.destination = new Vector3 (-52,1,-14);
			if (state == 0) 
			{
				agent.destination = new Vector3 (-52,1,-14);
				state = 1;
			} 
			else 
			{
				agent.destination = new Vector3 (-52,1,-22);
				state = 0;
			}
			//isArrived = false;
			break;
		case 2:
			if (state == 0) 
			{
				agent.destination = new Vector3 (-62,1,-8);
				state = 1;
			} 
			else 
			{
				agent.destination = new Vector3 (-43,1,-18);
				state = 0;
			}
			//isArrived = false;
			break;
		}*/
		isPatrol = true;
		isWalk = false;
		if (state == 0){state = 1;} 
		else {state = 0;}
		agent.destination = getVect (ID, state);
		isArrived = false;
	}

	void Suicide()
	{
		GameObject[] tabBullet;
		tabBullet = GameObject.FindGameObjectsWithTag ("Bullet");
		foreach (GameObject go in tabBullet) 
		{
			float distBull = Vector3.Distance (this.gameObject.transform.position, go.transform.position);
			if (go.GetComponent<bulletScript> ().launcherName != "OSOK" && distBull < 2) {Death ();}
		}
	}

	bool TargetOnSight()
	{
		RaycastHit hit;
		Physics.Raycast (this.gameObject.transform.position, this.gameObject.transform.forward, out hit);
		if (hit.collider.gameObject.tag == "Target" && !hit.collider.gameObject.GetComponent<AgentM> ()) {return true;}
		else {return false;}
	}

	void Avoid()
	{
		Collider[] closeBullet = Physics.OverlapSphere (this.gameObject.transform.position, 5);
		foreach (Collider go in closeBullet) 
		{
			if (go.gameObject != toAvoid && go.gameObject.tag == "Bullet" && go.gameObject.GetComponent<bulletScript> ().launcherName != "OSOK") 
			{
				Vector3 newDest = this.gameObject.transform.position;
				if (go.gameObject.transform.position.x > this.gameObject.transform.position.x) {newDest += go.gameObject.transform.right*2;}
				else if(go.gameObject.transform.position.x < this.gameObject.transform.position.x) {newDest -= go.gameObject.transform.right*2;}

				if (go.gameObject.transform.position.z > this.gameObject.transform.position.z) {newDest += go.gameObject.transform.forward;}
				else if(go.gameObject.transform.position.z < this.gameObject.transform.position.z) {newDest -= go.gameObject.transform.forward;}

				//newDest = this.gameObject.transform.position + go.gameObject.transform.right * 2 + go.gameObject.transform.forward;
				agent.destination = newDest;
				isAvoiding = true;
				isArrived = false;
				toAvoid = go.gameObject;
				//Debug.Log (this.gameObject.name + " avoiding " + go.gameObject.name + " to : " + newDest);
				break;
			}
		}
	}



	GameObject FindCloseTarget()
	{
		GameObject T = null;
		float dist = 999999;
		foreach (GameObject go in tabTarget) 
		{
			if (go != null) 
			{
				float actualDist = Vector3.Distance (this.gameObject.transform.position, go.transform.position);
				if (actualDist < dist) 
				{
					dist = actualDist;
					T = go;
				}
			}
		}
		return T;
	}

	void Cheat()
	{
		if (distToTarget < 2) 
		{
			this.gameObject.GetComponent<BoxCollider> ().isTrigger = false;
			Danger ();
		} 
		else 
		{
			this.gameObject.GetComponent<BoxCollider> ().isTrigger = true;
		}
	}

	/*void Cancer(GameObject hit)
	{
		Vector3 pos = hit.transform.position;
		Destroy (hit);
		Instantiate (cancer, pos, Quaternion.identity);
	}*/

	void Danger()
	{
		Vector3 dir = target.GetComponent<NavMeshAgent> ().destination;
		agent.destination = this.transform.position + dir;
	}

	void RemoveTargetFromTab(GameObject toRemove)
	{
		int index = -1;
		foreach (GameObject go in tabTarget) 
		{
			index++;
			if (go == toRemove) 
			{
				tabTarget [index] = null;
			}
		}
	}

	void LookAtTarget()
	{
		this.gameObject.GetComponentInChildren<Transform> ().LookAt (target.transform.position);
	}

	void Coloring(GameObject toColor)
	{
		float r = Random.Range (0.0f,1.0f);
		float g = Random.Range (0.0f,1.0f);
		float b = Random.Range (0.0f,1.0f);
		Color col = new Color (r,g,b);
		if (toColor.GetComponent<MeshRenderer> ()) {toColor.GetComponent<MeshRenderer> ().material.color = col;}
	}

	void Dunk()
	{
		currentBullet.transform.localScale += currentBullet.transform.localScale * Time.deltaTime;
	}

	void OnCollisionEnter(Collision collision) 
	{
		if (collision.gameObject.tag == "Bullet" && collision.gameObject.GetComponent<bulletScript>().launcherName != "OSOK"){Death ();}
		if (collision.gameObject.tag == "Target") 
		{
			//Cancer (collision.gameObject);
			//this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
			Coloring (collision.gameObject);
			//RemoveTargetFromTab (collision.gameObject);
			target = FindCloseTarget();
			while (target.GetComponent<AgentM> ()) 
			{
				RemoveTargetFromTab (target);
				target = FindCloseTarget();
			}
			//if (target != null) 
			//{
			//	agent.destination = target.transform.position; 
			//}
		}
	}

	void Death()
	{
		agent.Warp (startPos);
		this.gameObject.transform.position = startPos;
		target = FindCloseTarget();
		agent.destination = target.transform.position;
		isPatrol = false;
		isWalk = true;
	}

	void Shoot()
	{
		Vector3 asmodunk = this.gameObject.transform.position;
		asmodunk.y += 1.8f;
		currentBullet = Instantiate (bullet, asmodunk, Quaternion.identity) as GameObject;
		target = FindCloseTarget();
		while (target.GetComponent<AgentM> ()) 
		{
			RemoveTargetFromTab (target);
			target = FindCloseTarget();
		}
		currentBullet.transform.LookAt (target.transform.position);
		currentBullet.GetComponent<bulletScript>().launcherName = "OSOK";
	}

	void OnCollisionExit(Collision collision) 
	{
		if (collision.gameObject.tag == "Target") 
		{
			//this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
			Coloring (collision.gameObject);
			//RemoveTargetFromTab (collision.gameObject);
			target = FindCloseTarget();
			while (target.GetComponent<AgentM> ()) 
			{
				RemoveTargetFromTab (target);
				target = FindCloseTarget();
			}
			//if (target != null) 
			//{
			//	agent.destination = target.transform.position; 
			//}
		}
	}
}
