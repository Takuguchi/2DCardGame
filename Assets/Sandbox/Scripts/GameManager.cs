using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 手札にカードを生成

    [SerializeField] Transform playerHandTransform; // プレイヤーの手札のTransformを取得
    [SerializeField] GameObject cardPrefab; // カードのPrefabを取得
    void Start()
    {
        CreateCard(playerHandTransform); // プレイヤーの手札にカードを生成
        
    }

    // カードを生成するメソッド
    void CreateCard(Transform hand)
    {
        // カードのPrefabをインスタンス・親要素に任意のTransformを指定
        Instantiate(cardPrefab, hand, false);
    }

}
