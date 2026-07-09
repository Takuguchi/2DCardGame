using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

// コアのPrefabにアタッチ
// コア側の動きを制御するクラス
public class CoreMovement : MonoBehaviour
{
    public Transform defaultParent; // コアの親の位置を保存する変数

    public IEnumerator MoveToCard(Transform card)
    {
        // 一度親をCanvasに変更する
        transform.SetParent(defaultParent.parent, true);
        // DOTweenでコアをフィールドに移動
        transform.DOMove(card.position, 0.25f);
        yield return new WaitForSeconds(0.25f); 

        defaultParent = card;
        transform.SetParent(defaultParent);
    }

    // スピリット破壊時に転用予定
    /*
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
    */


    void Start()
    {
        defaultParent = transform.parent;
    }
}
