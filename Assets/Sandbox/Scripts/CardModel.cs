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
    public bool isAlive;       // カードが生きているかどうか
    public bool canAttack;     // カードが攻撃可能かどうか
    public bool isFieldCard;   // フィールドのカードかどうか
    public bool isPlayerCard;  // プレイヤーのカードかどうか

    // コンストラクタ
    public CardModel(int cardID, bool isPlayer)
    {
        // Card1のデータを入れ物(CardEntityクラス)に渡す
        CardEntity cardEntity = Resources.Load<CardEntity>("CardEntityList/Card" + cardID);

        // CardEntityのデータをCardModel(このクラス)の変数に代入
        name = cardEntity.name;
        hp = cardEntity.hp;
        at = cardEntity.at; 
        cost = cardEntity.cost;
        icon = cardEntity.icon;
        isAlive = true; // カードは最初は生きている状態
        isPlayerCard = isPlayer; // 引数のisPlayerを代入
    }

    // カードのダメージ処理
    void Damage(int dmg)
    {
        hp -= dmg;

        if (hp <= 0) // HPが0以下になると不都合があるため、0にする
        {
            hp = 0;
            isAlive = false; // カードは死んでいる状態
        }
    }

    // カードの攻撃処理
    public void Attack(CardController card)
    {
        card.model.Damage(at); // 攻撃力分のダメージを相手のカードに与える
    }
}
