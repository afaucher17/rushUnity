﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class PlayerHealthBar : MonoBehaviour {

	void Update () {
		GetComponent<Image>().fillAmount = (PlayerScript.instance.current_hp * 100f / PlayerScript.instance.hpMax) / 100f;
	}
}
