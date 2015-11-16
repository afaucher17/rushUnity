﻿using UnityEngine;
using System.Collections;

public class PassiveAOESpellScript : SkillScript {
	public FollowTargetAOEScript	spell;
	public static FollowTargetAOEScript	clone;

	public override bool SelectSkill ()
	{
		StopAllCoroutines ();
		if (clone != null)
		{
			clone.Destroy ();
			return false;
		}
		if (onCoolDown || PlayerScript.instance.current_mana < manaCost)
			return false;
		UseSkill ();
		return true;
	}

	IEnumerator UseMana ()
	{
		while (true)
		{
			PlayerScript.instance.current_mana = Mathf.Clamp(PlayerScript.instance.current_mana - manaCost, 0, PlayerScript.instance.manaMax);
			if (PlayerScript.instance.current_mana == 0)
			{
				clone.Destroy ();
				break ;
			}
			yield return new WaitForSeconds(1.0f);
		}
	}

	public override void ApplyEffect (Vector3 target, GameObject origin)
	{
		StartCoroutine (UseMana());
		clone = Instantiate (spell);
		clone.transform.position = PlayerScript.instance.transform.position;
		clone.target = PlayerScript.instance.gameObject;
		clone.damage = damage;
		clone.radius = AOE;
	}
}