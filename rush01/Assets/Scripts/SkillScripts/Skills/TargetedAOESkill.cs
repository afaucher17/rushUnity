﻿using UnityEngine;
using System.Collections;

public class TargetedAOESkill : SkillScript {
	public MouseAOEScript			spellAOE;
	public static MouseAOEScript	clone;
	public Vector3					AOEtarget;
	public GameObject				spell;


	void onMouseClick (Vector3 pos)
	{
		AOEtarget = pos;
		clone.onMouseClick -= onMouseClick;
		clone.onCancel -= onCancel;
		clone = null;
		PlayerScript.instance.AOETargeting = false;
		UseSkill ();
	}

	void onCancel(Vector3 pos)
	{
		clone.onMouseClick -= onMouseClick;
		clone.onCancel -= onCancel;
		clone = null;
		PlayerScript.instance.AOETargeting = false;
		PlayerScript.instance.currentSkill = null;
	}

	public override bool SelectSkill ()
	{
		if (onCoolDown || PlayerScript.instance.current_mana < manaCost)
			return false;
		if (clone != null)
		{
			clone.onMouseClick -= onMouseClick;
			clone.onCancel -= onCancel;
			Destroy (clone.gameObject);
			Destroy (clone);
		}
		clone = Instantiate(spellAOE);
		clone.onMouseClick += onMouseClick;
		clone.onCancel += onCancel;
		clone.range = AOE;
		PlayerScript.instance.AOETargeting = true;
		return true;
	}

	public override void ApplyEffect (Vector3 target, GameObject origin)
	{
		base.ApplyEffect (target, origin);
		GameObject clone = Instantiate (spell, AOEtarget + new Vector3(0, 0.5f, 0), Quaternion.LookRotation(Vector3.up)) as GameObject;
		clone.GetComponent<SimpleAOEScript>().damage = damage;
		clone.GetComponent<SimpleAOEScript>().radius = AOE / 2.0f;
	}

}
