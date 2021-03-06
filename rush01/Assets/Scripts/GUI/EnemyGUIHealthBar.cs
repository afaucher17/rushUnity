﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyGUIHealthBar : MonoBehaviour {

	void Update () {
		if (PlayerScript.instance.enemyTarget) {
			GetComponent<Image>().enabled = true;
			GetComponent<Image> ().fillAmount = (PlayerScript.instance.enemyTarget.GetComponent<Enemy>().current_hp * 100f /
				PlayerScript.instance.enemyTarget.GetComponent<Enemy> ().hpMax) / 100f;
		} else {
			GetComponent<Image>().enabled = false;
		}
	}
}
