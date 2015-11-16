﻿using UnityEngine;
using System.Collections;

public class SelfTargetedAOEScript : SkillScript {
	public GameObject spell;

	public override bool SelectSkill ()
	{
		if (onCoolDown || PlayerScript.instance.current_mana < manaCost)
			return false;
		UseSkill ();
		return true;
	}

	public override void ApplyEffect (Vector3 target, Vector3 origin)
	{
		Instantiate (spell, target + new Vector3(0, 1.5f, 0), Quaternion.identity);
	}
}
