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
        CardMovement card = eventData.pointerDrag.GetComponent<CardMovement>();
        if (card != null) // カードがきちんと存在していれば
        {
            // ドロップされたカードの親をフィールドにする
            card.defaultParent = this.transform;
        }
    }
}

