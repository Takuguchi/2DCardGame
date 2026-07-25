using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 攻撃される側のカードのクラス
public class AttackedCard : MonoBehaviour, IDropHandler, IPointerClickHandler
{

    // 敵の攻撃時、プレイヤーが防御カードとしてこのカードをクリックしたときに呼ばれる
    public void OnPointerClick(PointerEventData eventData)
    {
        CardController defender = GetComponent<CardController>(); // 自分自身のCardControllerを取得
        if (defender == null)
        {
            return;
        }
        GameManager.instance.SelectDefenderCard(defender);
    }

    public void OnDrop(PointerEventData eventData)
    {
        /* 攻撃 */
        // attackerカードを選択
        CardController attacker = eventData.pointerDrag.GetComponent<CardController>(); // ドロップされたカードを取得
        // defenderカードを選択
        CardController defender = GetComponent<CardController>(); // 自分自身のCardControllerを取得
        
        // attackerかdefenderが取得できなかった場合
        if (attacker == null || defender == null)
        {
            return; // 何も処理しないで終わる
        }
        if (!attacker.movement.isDraggable)
        {
            return; // ドラッグ不可なら処理を終了する
        }
        if (attacker.model.isPlayerCard == defender.model.isPlayerCard)
        {
            return; // attackerとdefenderがプレイヤー同士, または敵同士のカードだった場合は何も処理しないで終わる
        }
        if (defender.model.isRefreshed == false)
        {
            return; // defenderが疲労状態の場合はブロックできない
        }

        // シールドカードがあれば、シールドカード以外は攻撃できない
        CardController[] enemyFieldCards = GameManager.instance.GetOpponentFieldCards(attacker.model.isPlayerCard);
        if (Array.Exists(enemyFieldCards, card => card.model.ability == ABILITY.SHIELD) && defender.model.ability != ABILITY.SHIELD)
        {
            return; // 何も処理しないで終わる
        }

        // attackerが攻撃可能だった場合
        if (attacker.model.canAttack)
        {
            // attackerとdefenderを戦わせる
            GameManager.instance.CardsBattle(attacker, defender);
        }
    }

}
