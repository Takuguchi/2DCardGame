using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// カードデータそのものとその処理
public class CardModel
{
    // カードデータの各種変数を定義
    public string name; // カードの名前
    // public int hp;      // カードのHP
    // public int at;      // カードの攻撃力
    public int cost;    // カードのコスト
    // public int coreNum; // カード上に乗っているコアの数
    public int currentLv; // 現在のレベル
    public int currentBp; // 現在のBP
    public int reductionSymbols; // 軽減シンボルの数
    public int symbols; // シンボルの数
    public Sprite icon; // カードの絵柄
    public CARDTYPE cardType;
    public ABILITY ability; // カードのアビリティ
    public int gainedBp; // 加算BP
    public MAGIC magic;
    public int magicDrawCount; // MAGIC.DRAW時のドロー枚数
    public int magicBp; // 破壊対象BP

    public int coreLv1; // レベル1の所要コア
    public int bpLv1;   // レベル1のBP
    public int coreLv2; // レベル2の所要コア
    public int bpLv2;   // レベル2のBP
    public int coreLv3; // レベル3の所要コア
    public int bpLv3;   // レベル3のBP

    public bool isAlive;       // カードが生きているかどうか
    public bool canAttack;     // カードが攻撃可能かどうか
    public bool isRefreshed;     // カードが回復状態かどうか
    public bool isFieldCard;   // フィールドのカードかどうか
    public bool isPlayerCard;  // プレイヤーのカードかどうか

    // コンストラクタ
    public CardModel(int cardID, bool isPlayer)
    {
        // Card1のデータを入れ物(CardEntityクラス)に渡す
        CardEntity cardEntity = Resources.Load<CardEntity>("CardEntityList/Card" + cardID);

        // CardEntityのデータをCardModel(このクラス)の変数に代入
        name = cardEntity.name;
        // hp = cardEntity.hp;
        // at = cardEntity.at; 
        cost = cardEntity.cost;
        // coreNum = cardEntity.coreNum;
        reductionSymbols = cardEntity.reductionSymbols;
        symbols = cardEntity.symbols;
        icon = cardEntity.icon;
        cardType = cardEntity.cardType;
        ability = cardEntity.ability;
        gainedBp = cardEntity.gainedBp;
        magic = cardEntity.magic;
        magicDrawCount = cardEntity.magicDrawCount;
        magicBp = cardEntity.magicBp;
        isAlive = true; // カードは最初は生きている状態
        isPlayerCard = isPlayer; // 引数のisPlayerを代入
        
        // 所要コアとLvごとのBPを代入
        coreLv1 = cardEntity.coreLv1;
        bpLv1 = cardEntity.bpLv1;
        coreLv2 = cardEntity.coreLv2;
        bpLv2 = cardEntity.bpLv2;
        coreLv3 = cardEntity.coreLv3;
        bpLv3 = cardEntity.bpLv3;
    }

    void DestroyWithOrFewer(int bp)
    {
        currentBp = GetBp();
        Debug.Log(name + ": " + currentBp);

        if (currentBp - bp <= 0) // bp以下のスピリットを破壊
        {
            isAlive = false; // カードは死んでいる状態
        }
        else
        {
            isAlive = true; // カードは生きている状態
        }
    }

    // BPを比較するメソッド
    void CompareBp(int attackerBp, string attackerName) // 引数string attackerNameはただのデバッグ用
    {
        // FixBp();
        if (currentBp <= attackerBp)
        {
            Debug.Log($"{name}(BP:{currentBp}) <= {attackerName}(BP:{attackerBp}) → {name}破壊");
            isAlive = false;
        }
        else
        {
            Debug.Log($"{name}(BP:{currentBp}) > {attackerName}(BP:{attackerBp}) → {name}生存");
            isAlive = true;
        }
    }

    // カードの攻撃処理
    public void Attack(CardController card)
    {
        if (card.model.cardType == CARDTYPE.SPIRIT && this.cardType == CARDTYPE.SPIRIT)
        {
            if (this.ability == ABILITY.GAIN_BP_ATTACK && GameManager.instance.isPlayerTurn == this.isPlayerCard)
            {
                currentBp += gainedBp;
            }
            if (card.model.ability == ABILITY.GAIN_BP_BLOCK && GameManager.instance.isPlayerTurn != card.model.isPlayerCard)
            {
                card.model.currentBp += card.model.gainedBp;
            }
            card.model.CompareBp(currentBp, name); // 攻撃側のBPと名前を渡す 引数nameはただのデバッグ用            
        }
        if (card.model.cardType == CARDTYPE.SPIRIT
            && this.cardType == CARDTYPE.MAGIC
            && (this.magic == MAGIC.DESTROY_ENEMY_CARD || this.magic == MAGIC.DESTROY_ALL_CARDS))
        {
            card.model.DestroyWithOrFewer(this.magicBp);
        }
    }

    // BPを更新するメソッド
    public void FixBp()
    {
        if (currentLv == 3) currentBp = bpLv3;
        else if (currentLv == 2) currentBp = bpLv2;
        else currentBp = bpLv1;
    }
    
    // BP取得
    public int GetBp()
    {
        if (currentLv == 3) return bpLv3;
        else if (currentLv == 2) return bpLv2;
        else return bpLv1;
    }
}
