using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// カード自身を扱うためのコード
// CardPrefabにアタッチ
public class CardController : MonoBehaviour
{
    CardView view;  // カードの見た目（view）に関することを操作
    public CardModel model;    // カードのデータ（model）に関することを操作
    public CardMovement movement; // カードの移動（movement）に関することを操作

    void Awake()
    {
        view = GetComponent<CardView>();  // カードの見た目（view）のデータをCardViewコンポーネントから取得
        movement = GetComponent<CardMovement>();  // カードの移動（movement）のデータをCardMovementコンポーネントから取得
    }


    // カードを生成したときに呼ばれるメソッド
    public void Init(int cardID)
    {
        model = new CardModel(cardID); // カードのデータを生成
        view.Show(model); // CardViewクラス内の、データをカードの見た目に反映するShow()メソッドにカードのデータを渡す
    }

    
    // Aliveがfalseになっていたら破壊するメソッド
    public void CheckAlive()
    {
        if (model.isAlive)  // カードが生きている状態なら
        {
            view.Refresh(model);    // カードの見た目を更新する
        }
        else    // カードが死んでいる状態なら
        {
            Destroy(this.gameObject);    // カードを破壊する
        }
    }
}
