using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 攻撃される側のカードのクラス
public class MagicDropManager : MonoBehaviour, IDropHandler
{

    public void OnDrop(PointerEventData eventData)
    {
        CardController magicCard = eventData.pointerDrag.GetComponent<CardController>(); // ドロップされたカードを取得
        CardController target = GetComponent<CardController>(); // nullの可能性もある
        
        if (magicCard == null)
        {
            return; // 何も処理しないで終わる
        }
        if (!magicCard.movement.isDraggable)
        {
            return; // ドラッグ不可なら処理を終了する
        }
        // if (GameManager.instance.step != GameManager.STEP.ATTACK) return;
        if (magicCard.CanUseMagic())
        {
            magicCard.UseMagicTo(target);
        }
    }

}