using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 攻撃される側のカードのクラス
public class SpellDropManager : MonoBehaviour, IDropHandler
{

    public void OnDrop(PointerEventData eventData)
    {
        CardController spellCard = eventData.pointerDrag.GetComponent<CardController>(); // ドロップされたカードを取得
        CardController target = GetComponent<CardController>(); // nullの可能性もある
        
        if (spellCard == null)
        {
            return; // 何も処理しないで終わる
        }
        if (spellCard.CanUseSpell())
        {
            spellCard.UseSpellTo(target);
        }
    }

}
