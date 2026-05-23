using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 手札にカードを生成

    [SerializeField] Transform playerHandTransform; // プレイヤーの手札のTransformを取得
    [SerializeField] CardController cardPrefab; // カードのPrefabをCardController型として取得
    void Start()
    {
        CreateCard(playerHandTransform); // プレイヤーの手札にカードを生成
        
    }

    // カードを生成するメソッド
    void CreateCard(Transform hand)
    {
        // カードのPrefabをCardController型としてインスタンス(生成)・親要素に任意のTransformを指定
        CardController card = Instantiate(cardPrefab, hand, false);
        card.Init();    // CardControllerクラスのInit()メソッドを呼び出す→任意のカードデータの各種変数を取得
    }

}
