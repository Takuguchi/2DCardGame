using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 攻撃される側のカードのクラス
public class AttackedCard : MonoBehaviour, IDropHandler
{

    public void OnDrop(PointerEventData eventData)
    {
        /* 攻撃 */
        // attackerカードを選択
        CardController attacker = eventData.pointerDrag.GetComponent<CardController>(); // ドロップされたカードを取得
        // defenderカードを選択
        CardController defender = GetComponent<CardController>(); // 自分自身のCardControllerを取得
        // attackerとdefenderを戦わせる
        GameManager.instance.CardsBattle(attacker, defender);

    }

}
