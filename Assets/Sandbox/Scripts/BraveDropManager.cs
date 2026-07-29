using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 合体(ブレイヴ)される側のカードのクラス
public class BraveDropManager : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        CardController braveCard = eventData.pointerDrag.GetComponent<CardController>(); // ドロップされたカードを取得
        CardController target = GetComponent<CardController>(); // 合体(ブレイヴ)先のスピリット、nullの可能性もある
        
        if (braveCard == null)
        {
            return; // 何も処理しないで終わる
        }
        if (!braveCard.movement.isDraggable)
        {
            return; // ドラッグ不可なら処理を終了する
        }
        if (braveCard.CanBrave(target))
        {
            braveCard.BraveTo(target);
        }
    }

}