﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour {

	public delegate void  EnemyEvent();
	public event EnemyEvent OnDeath;
	public string enemyName;
	public int[] spawnStats;
	public int str;
	public int agi;
	public int con;
	public int armor;
	public int level;
	public int expGiven;
	public int moneyGiven;
	public int hpMax { get { return con * 5; } }
	public int current_hp;
	public int minDamage { get { return Mathf.RoundToInt (str / 2); }}
	public int maxDamage { get { return minDamage + str; }}
	public bool dead;
	public GameObject intruder;
	public enum EnemyType {
		SKELETON,
		NONE
	}
	public Dictionary<EnemyType, int[]> baseStats = new Dictionary<EnemyType, int[]>
	{
		{EnemyType.SKELETON, new int[5] {1, 1, 1, 5, 5}}
	};
	public EnemyType type;
	public Animator animator;
	public NavMeshAgent agent;
	public AudioSource aS;
	public AudioSource swordS;
	public AudioClip death;
	public AudioClip move;
	public AudioClip attack;
	public Curves.StatCurve[] statCurves;

	public GameObject		damageText;

	private uint _framesToWait = 600;

	protected void OnTriggerEnter (Collider col) {
		if (col.gameObject.tag == "Player")
			intruder = col.gameObject;
	}

	protected void OnTriggerExit (Collider col) {
		if (col.gameObject.tag == "Player")
			intruder = null;
	}

	protected void InstantiateStats () {
		foreach (Curves.StatCurve statCurve in statCurves) {
			switch (statCurve.stat) {
				case Curves.Stat.STRENGTH:
					str = Curves.ApplyCurve(statCurve.curve, level, str);
					break;
				case Curves.Stat.AGILITY:
					agi = Curves.ApplyCurve(statCurve.curve, level, agi);
					break;
				case Curves.Stat.CONSTITUTION:
					con = Curves.ApplyCurve(statCurve.curve, level, con);
					break;
				case Curves.Stat.ARMOR:
					armor = Curves.ApplyCurve(statCurve.curve, level, armor);
					break;
				case Curves.Stat.EXPERIENCE:
					expGiven = Curves.ApplyCurve(statCurve.curve, level, expGiven);
					break;
				case Curves.Stat.MONEY:
					moneyGiven = Curves.ApplyCurve(statCurve.curve, level, moneyGiven);
					break;
				default:
					break;
			}
		}
	}

	protected void CheckHealth () {
		if (current_hp <= 0 && !dead) {
			dead = true;
			OnDeath();
			animator.SetTrigger ("Death");
			aS.clip = death;
			aS.Play ();
			PlayerScript.instance.xp += expGiven;
			LootManager.instance.DropLoot(this);
			StartCoroutine (BodyDissolve());
		}
	}

	protected IEnumerator BodyDissolve ()
	{
		agent.enabled = false;
		GetComponent<Rigidbody>().isKinematic = true;
		GetComponent<BoxCollider> ().enabled = false;
		Destroy (GetComponentInChildren<Canvas> ().gameObject);
		float length = 0.0f;
		foreach (AnimatorClipInfo animinfo in animator.GetCurrentAnimatorClipInfo(0))
		{
			if (animinfo.clip.name.StartsWith ("Death"))
				length = animinfo.clip.length;
		}
		yield return new WaitForSeconds (length);
		for (int i = 0; i < _framesToWait; i++) {
			yield return new WaitForEndOfFrame ();
			this.transform.position = new Vector3 (this.transform.position.x, this.transform.position.y - 0.00125f, this.transform.position.z);
		}
		Destroy (gameObject);
	}

	protected void CheckAggroRange () {
		if (intruder != null) {
			agent.SetDestination (intruder.transform.position);
			if (!aS.isPlaying) {
				aS.clip = move;
				aS.Play ();
			}
			animator.SetBool ("Move", true);
		} else {
			agent.SetDestination (this.transform.position);
			aS.Stop ();
			animator.SetBool ("Move", false);
		}

	}

	protected void Attack () {
		if (intruder) {
			float distanceToTarget;
			bool closeEnough = false;

			distanceToTarget = Vector3.Distance (this.transform.position, intruder.transform.position);
			closeEnough = distanceToTarget > agent.stoppingDistance ? false : true;
			if (closeEnough) {
				Vector3 targetPosition = new Vector3 (intruder.transform.position.x, this.transform.position.y, intruder.transform.position.z);
				
				this.transform.LookAt (targetPosition);
				animator.SetBool ("Move", false);
				animator.SetTrigger ("Attack");
			}
		}
	}

	public void AttackSound () {
		aS.clip = attack;
		aS.Play ();
	}

	public virtual void Damage () {
		float val = 75 + agi - PlayerScript.instance.agi - Random.Range (1, 101);
		bool hit = val > 0 ? true : false;

		if (intruder && Vector3.Distance (this.transform.position, intruder.transform.position) <= 2.0 && hit) {
			PlayerScript.instance.DamagePlayer (GetDamage ());
			swordS.Play ();
		} else {
			PlayerScript.instance.DamagePlayer (0, true);
		}
	}

	public int GetDamage()
	{
		return Random.Range (minDamage, maxDamage + 1);
	}

	public void ReceiveDamage (int damage, bool miss = false, bool heal = false) {
		GameObject clone = Instantiate (Resources.Load ("Prefabs/GUI/DamageText", typeof(GameObject)) as GameObject, this.transform.position + new Vector3(0, this.agent.height, 0), Quaternion.identity) as GameObject;
		clone.GetComponent<DamageTextScript>().SetText ((!miss) ? damage.ToString () : "Miss", false);
		this.current_hp = Mathf.Clamp (this.current_hp - damage, 0, this.hpMax);
	}

	protected virtual void Start () {
		spawnStats = baseStats [type];
		str = spawnStats [0];
		agi = spawnStats [1];
		con = spawnStats [2];
		expGiven = spawnStats [3];
		moneyGiven = spawnStats [4];
		InstantiateStats ();
		current_hp = hpMax;
		dead = false;
		agent.stoppingDistance = 2.0f;
	}

	protected virtual void Update () {
		CheckHealth ();
		if (current_hp > 0) {
			CheckAggroRange ();
			Attack ();
		}
	}
}
