using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// カードのPrefabにアタッチ
// カード側の動きを制御するクラス
public class CardMovement : MonoBehaviour, IDragHandler
{
    // カードをドラッグしたときに呼び出されるメソッド
    public void OnDrag(PointerEventData eventData)
    {
        // 左辺：カードの場所　右辺：マウスの場所
        transform.position = eventData.position;
    }
}
