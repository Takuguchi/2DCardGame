using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// カードのPrefabにアタッチ
// カード側の動きを制御するクラス
public class CardMovement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Transform defaultParent; // カードの親の位置を保存する変数

    public bool isDraggable; // カードがドラッグ可能かどうかを判定する変数
    
    // ドラッグ開始時に呼び出されるメソッド
    public void OnBeginDrag(PointerEventData eventData)
    {
        // カードのコストとPlayerのManaコストを比較
        CardController card = GetComponent<CardController>();

        // フィールドのカードじゃない場合＝手札のカードの場合
        if (!card.model.isFieldCard && card.model.cost <= GameManager.instance.playerManaCost)
        {
            isDraggable = true; // カードのコストがPlayerのManaコスト以下ならドラッグ可能
        }
        else if (card.model.isFieldCard && card.model.canAttack) // フィールドのカードで、かつ攻撃可能なら
        {
            isDraggable = true; // ドラッグ可能
        }
        else
        {
            isDraggable = false; // カードのコストがPlayerのManaコストより大きいならドラッグ不可
        }

        if (!isDraggable) 
        {
            return; // ドラッグ不可なら処理を終了する
        }

        // 自分自身の親を取得（初期位置が手札なら親は手札）
        defaultParent = transform.parent;

        // 親の親を、親に設定→カードの移動がスムーズになる
        transform.SetParent(defaultParent.parent, false);

        // ドロップ先の判定をするため、カードのRaycastを無効にする
        GetComponent<CanvasGroup>().blocksRaycasts = false; 
    }
    
    // カードをドラッグしている最中に呼び出されるメソッド
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) 
        {
            return; // ドラッグ不可なら処理を終了する
        }
        
        // 左辺：カードの位置　右辺：マウスの位置
        transform.position = eventData.position;
    }

    // ドラッグ終了時(離したとき)に呼び出されるメソッド
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) 
        {
            return; // ドラッグ不可なら処理を終了する
        }
        
        // カードの親をドラッグ開始時の親に設定→手札に戻る
        transform.SetParent(defaultParent, false);

        // ドロップ先の判定をするため、カードのRaycastを有効にする
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    public void SetCardTransform(Transform parentTransform)
    {
        defaultParent = parentTransform;
        transform.SetParent(defaultParent);
    }
}
