﻿using UnityEngine;
using System.Collections;

public class BattleCharacter : BattleActor {

	[SerializeField] BattleCharacterAnimator m_charAnimator;


	override protected void Start () {
		base.Start ();
		m_type = ActorType.CHARACTER;

        m_lifeGauge.ChangeOrientation(UIGauge.ORIENTATION.HORIZONTAL, UIGauge.ALIGN.LEFT);
        m_manaGauge.ChangeOrientation(UIGauge.ORIENTATION.HORIZONTAL, UIGauge.ALIGN.LEFT);
    }

	#region LOADING 
	override public void Load(string _name){
        base.Load(_name);

		m_charAnimator.LoadSprites(_name);
        var charData = ProfileManager.instance.GetCharacter(_name);
        var levelupData = DataManager.instance.CharacterManager.GetLevelByXp(charData.Category, charData.Xp);
        if(charData != null && levelupData.Stats != null)
        {
            m_maxStats = new Stats(levelupData.Stats);
            m_currentStats = new Stats(levelupData.Stats);
        }
        CurrentStats.MP = 0;
        RefreshManaGauge();
    }
	#endregion

	override protected void UpdateAttacking(){

	}

	#region ACTION

	override public int Attack( NoteData _noteData){
		m_state = State.ATTACKING;

		m_charAnimator.Attack ();

		this.AddMP (5);

		return CurrentStats.Attack;
	}

	override public int TakeDamage(int _damage, NoteData _note){
		int damage = _damage;
		damage -= CurrentStats.Defense ;
		//Reduce damage by blocking
		switch(_note.HitAccuracy){
			case BattleScoreManager.Accuracy.PERFECT :
				damage = damage - (int) (damage * CurrentStats.blockPerfectModifier);
				break;
			case BattleScoreManager.Accuracy.GREAT :
				damage = damage - (int) (damage * CurrentStats.blockGreatModifier);
				break;
			case BattleScoreManager.Accuracy.GOOD :
				damage = damage - (int) (damage * CurrentStats.blockGoodModifier);
				break;
			case BattleScoreManager.Accuracy.MISS :
				damage = damage - (int) (damage * CurrentStats.blockBadModifier);
				break;
		}

		if (damage < 0)
			damage = 0;
		CurrentStats.HP -=  damage;

		m_charAnimator.TakeHit ();

		RefreshLifeGauge ();

		CheckDeath ();

		return damage;
	}

	#endregion

	override protected bool Die(){
		base.Die ();
		Utils.SetAlpha(m_sprite,0.0f);
		return true;
	}
}
