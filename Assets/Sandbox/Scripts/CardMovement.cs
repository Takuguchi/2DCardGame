using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

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

        // フィールドのコアの総数を取得
        int filedCoreNum = 0;
        CardController[] playerFieldCards = GameManager.instance.GetPlayerFieldCards();
        for (int i = 0; i < playerFieldCards.Length; i++)
        {
            CoreController[] cores = playerFieldCards[i].GetComponentsInChildren<CoreController>(); // カードに乗っているコアを取得
            filedCoreNum += cores.Length;
        }
        Debug.Log("フィールドのコアの総数：" + filedCoreNum);

        // リザーブのコアを取得
        CoreController[] reserveCoreList = GameManager.instance.playerReserveTransform.GetComponentsInChildren<CoreController>();


        // フィールドのカードじゃない場合＝手札のカードの場合
        if (card.model.isPlayerCard 
            && GameManager.instance.isPlayerTurn
            && !card.model.isFieldCard
            && GameManager.instance.CalcNetCost(card) < reserveCoreList.Length + filedCoreNum)
        {
            isDraggable = true; // カードのNet(正味)コストがPlayerのManaコスト+フィールドのコアの総数"未満"ならドラッグ可能(維持コアが1個以上必要なため)
        }
        else if (card.model.isPlayerCard && GameManager.instance.isPlayerTurn && card.model.isFieldCard && card.model.canAttack) // フィールドのカードで、かつ攻撃可能なら
        {
            isDraggable = true; // ドラッグ可能
        }
        else
        {
            isDraggable = false; // カードのコストがPlayerのManaコスト"以上"ならドラッグ不可(維持コアが1個以上必要なため)
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

    public IEnumerator MoveToField(Transform field)
    {
        // 一度親をCanvasに変更する
        transform.SetParent(defaultParent.parent, false);
        // DOTweenでカードをフィールドに移動
        transform.DOMove(field.position, 0.25f);
        yield return new WaitForSeconds(0.25f); 

        defaultParent = field;
        transform.SetParent(defaultParent);
    }

    public IEnumerator MoveToTarget(Transform target)
    {
        // 攻撃後にカードを元の位置に戻すために、自分の位置と並びを保存しておく
        Vector3 currentPosition = transform.position;
        int siblingIndex = transform.GetSiblingIndex();
        
        // 一度親をCanvasに変更する
        transform.SetParent(defaultParent.parent);
        // DOTweenでカードをTargetに移動
        transform.DOMove(target.position, 0.25f);        
        yield return new WaitForSeconds(0.25f);

        // 元の位置と並びに戻す
        transform.DOMove(currentPosition, 0.25f);
        yield return new WaitForSeconds(0.25f);
        transform.SetParent(defaultParent);
        transform.SetSiblingIndex(siblingIndex);
    }


    void Start()
    {
        defaultParent = transform.parent;
    }
}
