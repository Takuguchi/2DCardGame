using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// カードがドロップされたときにフィールドに重なっていればフィールドを親に変えるクラス
public class DropPlace : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        //　ドラッグしてきたデータを代入
        CardController card = eventData.pointerDrag.GetComponent<CardController>();
        if (!card.movement.isDraggable)
        {
            return; // ドラッグ不可なら処理を終了する
        }
        if (card != null) // カードがきちんと存在していれば
        {
            card.movement.defaultParent = this.transform; // ドロップされたカードの親をフィールドにする
            GameManager.instance.ReduceManaCost(card.model.cost, true); // カードをドロップしたらPlayerのManaコストを減らす
        }
    }
}

