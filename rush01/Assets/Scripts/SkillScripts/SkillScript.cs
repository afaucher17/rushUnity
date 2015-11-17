using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public abstract class SkillScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public enum SkillType
	{
		TARGETED_AOE,
		SELF_AOE,
		PASSIVE_AOE,
		DIRECT_ATTACK,
		PASSIVE,
	}
	public SkillType		skillType;
	public bool				spellAttack;
	[Range(-1, 4)]
	public int				level;
	public string			Skillname;
	public int				range { get { return skillStats[level].range; }}
	public int				manaCost { get { return skillStats[level].manaCost; }}
	public float			coolDown { get { return skillStats[level].coolDown; }}
	public int				damage { get { return skillStats[level].damage; }}
	public int				AOE { get { return skillStats[level].AOE; }}
	public int				attackAnimationIndex { get { return skillStats[level].attackAnimationIndex; }}
	public float			damageMultiplier { get { return skillStats[level].damageMultiplier; }}
	public bool				onCoolDown;
	public string			toolTip;
	public bool				manaOverTime;
	public int				levelUnlocked;

	// UI
	public GameObject		button;
	public GameObject           dragging_icon;
	public static GameObject    itemBeingDragged;


	public SkillStat[]		skillStats = new SkillStat[5];
	public abstract bool	SelectSkill();
	public abstract	void	ApplyEffect(Vector3 target, GameObject origin);

	protected virtual void	Start()
	{
		button = GetComponentInChildren<Button>().gameObject;
	}

	public void OnBeginDrag (PointerEventData eventData)
	{
		if (this.level >= 0)
		{
			itemBeingDragged = GameObject.Instantiate(dragging_icon);
			itemBeingDragged.GetComponent<DraggingIconScript>().originScript = this;
			itemBeingDragged.transform.SetParent (this.transform);
			itemBeingDragged.GetComponent<Image>().sprite = GetComponent<Image>().sprite;
			itemBeingDragged.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		}
	}

	public void OnDrag (PointerEventData eventData)
	{
		if (itemBeingDragged != null)
			itemBeingDragged.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
	}

	public void OnEndDrag (PointerEventData eventData)
	{
		if (!itemBeingDragged.GetComponent<DraggingIconScript>().dragSuccessful)
		{
			Destroy (itemBeingDragged);
		}
	}
	
	public virtual void		UseSkill()
	{
		if (!manaOverTime)
			PlayerScript.instance.current_mana = Mathf.Clamp(PlayerScript.instance.current_mana - manaCost, 0, PlayerScript.instance.manaMax);
		StartCoroutine (doCoolDown ());
		Animator animator = PlayerScript.instance.animator;
		animator.SetInteger("AttackType", attackAnimationIndex);
		if (spellAttack)
		{
			if (animator.GetBool ("HasWeapon"))
				animator.SetTrigger ("Equip");
			animator.SetTrigger ("SpellAttack");
		}
		else
		{
			if (!animator.GetBool ("HasWeapon"))
				animator.SetTrigger ("Equip");
			animator.SetTrigger ("WeaponAttack");
		}
	}

	protected virtual IEnumerator doCoolDown ()
	{
		onCoolDown = true;
		yield return new WaitForSeconds(coolDown);
		onCoolDown = false;
	}

	protected virtual void Update()
	{
		Image image = GetComponent<Image>();
		button.SetActive (PlayerScript.instance.skillPoints > 0 && levelUnlocked <= PlayerScript.instance.level && level <= 3);
		image.color = (levelUnlocked > PlayerScript.instance.level || level < 0)
			? new Color(image.color.r, image.color.g, image.color.b, 0.5f) : new Color(image.color.r, image.color.g, image.color.b, 1f);
	}

	public void SpendSkillPoint()
	{
		if (level <= 3 && PlayerScript.instance.skillPoints > 0)
		{
			PlayerScript.instance.skillPoints--;
			level++;
		}
	}

	[System.Serializable]
	public class SkillStat
	{
		public int			range;
		public int			coolDown;
		public int			manaCost;
		public int			damage;
		public int			AOE;
		public int			attackAnimationIndex;
		public float		damageMultiplier = 1.0f;
		public float		duration;
	}
}

