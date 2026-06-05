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
    public CardModel(int cardID)
    {
        // Debug.Log("CardModelのコンストラクタが呼ばれました");
        // Debug.Log($"1. 名前：{name}, HP：{hp}, 攻撃力：{at}, コスト：{cost}");
        // Card1のデータを入れ物(CardEntityクラス)に渡す
        CardEntity cardEntity = Resources.Load<CardEntity>("CardEntityList/Card" + cardID);

        // CardEntityのデータをCardModel(このクラス)の変数に代入
        name = cardEntity.name;
        hp = cardEntity.hp;
        at = cardEntity.at; 
        cost = cardEntity.cost;
        icon = cardEntity.icon;
        // Debug.Log($"2. 名前：{name}, HP：{hp}, 攻撃力：{at}, コスト：{cost}");
    }

    // カードのダメージ処理
    void Damage(int dmg)
    {
        hp -= dmg;

        if (hp <= 0) // HPが0以下になると不都合があるため、0にする
        {
            hp = 0;
        }
    }

    // カードの攻撃処理
    public void Attack(CardController card)
    {
        card.model.Damage(at); // 攻撃力分のダメージを相手のカードに与える
    }
}
