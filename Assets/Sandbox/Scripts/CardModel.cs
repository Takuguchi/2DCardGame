using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// カードデータそのものとその処理
public class CardModel
{
    // カードデータの各種変数を定義
    public string name; // カードの名前
    public int hp;      // カードのHP
    public int at;      // カードの攻撃力
    public int cost;    // カードのコスト
    public Sprite icon; // カードの絵柄

    // コンストラクタ
    public CardModel()
    {
        Debug.Log("CardModelのコンストラクタが呼ばれました");
        Debug.Log($"1. 名前：{name}, HP：{hp}, 攻撃力：{at}, コスト：{cost}");
        // Card1のデータを入れ物(CardEntityクラス)に渡す
        CardEntity cardEntity = Resources.Load<CardEntity>("CardEntityList/Card1");

        // CardEntityのデータをCardModel(このクラス)の変数に代入
        name = cardEntity.name;
        hp = cardEntity.hp;
        at = cardEntity.at; 
        cost = cardEntity.cost;
        icon = cardEntity.icon;
        Debug.Log($"2. 名前：{name}, HP：{hp}, 攻撃力：{at}, コスト：{cost}");
    }
}
