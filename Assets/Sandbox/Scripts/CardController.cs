using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// カード自身を扱うためのコード
// CardPrefabにアタッチ
public class CardController : MonoBehaviour
{
    // カードの見かけ（view）に関することを操作
    // カードのデータ（model）に関することを操作
    
    CardModel model;    // カードデータを格納する変数

    // カードを生成したときに呼ばれるメソッド
    public void Init(CardModel model)
    {
        model = new CardModel();    // カードデータインスタンスを生成
    }
    
}
