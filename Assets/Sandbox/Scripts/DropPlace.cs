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
        
        if (card != null) // カードがきちんと存在していれば
        {
            if (!card.movement.isDraggable)
            {
               return; // ドラッグ不可なら処理を終了する
            }
            card.movement.defaultParent = this.transform; // ドロップされたカードの親をフィールドにする
            
            // ドロップしたカードがフィールドのカードだった場合
            if (card.model.isFieldCard)
            {
                return; // Manaコストを減らす必要はないため、処理を終了する
            }
            
            GameManager.instance.ReduceManaCost(card.model.cost, true); // カードをドロップしたらPlayerのManaコストを減らす
            card.model.isFieldCard = true; // カードをドロップしたらフィールドのカードにする
        }
    }
}

