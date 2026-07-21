using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 攻撃される側のカードのクラス
public class AttackedHero : MonoBehaviour, IDropHandler
{

    public void OnDrop(PointerEventData eventData)
    {
        /* 攻撃 */
        // attackerカードを選択
        CardController attacker = eventData.pointerDrag.GetComponent<CardController>(); // ドロップされたカードを取得
        
        // attackerが取得できなかった場合
        if (attacker == null)
        {
            return; // 何も処理しないで終わる
        }
        // 敵フィールドにシールドカードがあれば攻撃できない
        CardController[] enemyFieldCards = GameManager.instance.GetEnemyFieldCards(attacker.model.isPlayerCard);
        if (Array.Exists(enemyFieldCards, card => card.model.ability == ABILITY.SHIELD))
        {
            return; // 何も処理しないで終わる
        }

        // attackerが攻撃可能だった場合
        if (attacker.model.canAttack)
        {
            // attackerがHeroに攻撃する
            GameManager.instance.AttackToHero(attacker);
            GameManager.instance.CheckHeroHP(); // HeroのHPが0になったかどうかを確認する
        }


    }

}
