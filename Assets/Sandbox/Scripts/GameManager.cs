using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] Transform playerHandTransform, // プレイヤーの手札のTransformを取得
                               enemyHandTransform;  // 敵の手札のTransformを取得 
    [SerializeField] CardController cardPrefab; // カードのPrefabをCardController型として取得

    bool isPlayerTurn; // プレイヤーのターンかどうかを判定する変数

    void Start()
    {
        StartGame(); // ゲーム開始時にStartGame()メソッドを呼び出す
    }

    // ゲーム開始時に呼ばれるメソッド
    void StartGame()
    {
        SettingInitHand();
        isPlayerTurn = true; // プレイヤーのターンから開始する
        TurnCalc(); // ターン処理を行うメソッドを呼び出す
    }

    // ゲーム開始時に手札を初期化するメソッド
    void SettingInitHand()
    {
        // カードをそれぞれに3枚配る
        for (int i = 0; i < 3; i++)
        {
            CreateCard(playerHandTransform); // プレイヤーの手札にカードを生成
            CreateCard(enemyHandTransform);  // 敵の手札にカードを生成
        }
    }

    // ターン処理を行うメソッド
    void TurnCalc()
    {
        if (isPlayerTurn)
        {
            // プレイヤーのターンの処理
            PlayerTurn();
        }
        else
        {
            // 敵のターンの処理
            EnemyTurn();
        }
    }

    // ターンを切り替えるメソッド
    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn; // ターンを切り替える
        if (isPlayerTurn)
        {
            CreateCard(playerHandTransform); // プレイヤーの手札にカードを1枚生成（ドロー）
        }
        else
        {
            CreateCard(enemyHandTransform);  // 敵の手札にカードを1枚生成（ドロー）
        }
        TurnCalc(); // ターン処理を行うメソッドを呼び出
    }

    // プレイヤーのターンの処理を行うメソッド
    void PlayerTurn()
    {
        Debug.Log("Playerのターン");
        
    }

    // 敵のターンの処理を行うメソッド
    void EnemyTurn()
    {
        Debug.Log("Enemyのターン");
        
        ChangeTurn(); // 敵のターンが終了したら、プレイヤーのターンに切り替える
    }

    // カードを生成するメソッド
    void CreateCard(Transform hand)
    {
        // カードのPrefabをCardController型としてインスタンス(生成)・親要素に任意のTransformを指定
        CardController card = Instantiate(cardPrefab, hand, false);
        card.Init(2);    // CardControllerクラスのInit()メソッドを呼び出す→任意のカードデータの各種変数を取得
    }

}
