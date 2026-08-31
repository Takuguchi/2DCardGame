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
    public COLOR color; // カードの色
    public int cost;    // カードのコスト
    // public int coreNum; // カード上に乗っているコアの数
    public int currentLv; // 現在のレベル
    public int currentBp; // 現在のBP
    public List<Symbol> reductionSymbols; // 軽減シンボル(色と数のリスト)
    public List<Symbol> symbols; // シンボル(色と数のリスト)
    public Sprite icon; // カードの絵柄
    public CARDTYPE cardType;
    public ABILITY ability; // カードのアビリティ
    public int gainedBp; // 加算BP
    public MAGIC magic;
    public int magicDrawCount; // MAGIC.DRAW時のドロー枚数
    public int magicOpenCount; // MAGIC.OPEN時のオープン枚数
    public int magicBp; // 破壊対象BP

    public int coreLv1; // レベル1の所要コア
    public int bpLv1;   // レベル1のBP
    public int coreLv2; // レベル2の所要コア
    public int bpLv2;   // レベル2のBP
    public int coreLv3; // レベル3の所要コア
    public int bpLv3;   // レベル3のBP
    public int bpBraved; // 合体中にプラスされるBP
    public int braveConditionCost; // 合体条件のコスト

    public bool isAlive;       // カードが生きているかどうか
    public bool canAttack;     // カードが攻撃可能かどうか
    public bool isRefreshed;     // カードが回復状態かどうか
    public bool isFieldCard;   // フィールドのカードかどうか
    public bool isPlayerCard;  // プレイヤーのカードかどうか

    // コンストラクタ
    public CardModel(CardEntity cardEntity, bool isPlayer)
    {
        // CardEntityのデータをCardModel(このクラス)の変数に代入
        name = cardEntity.name;
        // hp = cardEntity.hp;
        // at = cardEntity.at; 
        color = cardEntity.color;
        cost = cardEntity.cost;
        // coreNum = cardEntity.coreNum;
        reductionSymbols = new List<Symbol>(cardEntity.reductionSymbols); // アセットのリストを複製して代入
        symbols = new List<Symbol>(cardEntity.symbols); // アセットのリストを複製して代入
        icon = cardEntity.icon;
        cardType = cardEntity.cardType;
        ability = cardEntity.ability;
        gainedBp = cardEntity.gainedBp;
        magic = cardEntity.magic;
        magicDrawCount = cardEntity.magicDrawCount;
        magicOpenCount = cardEntity.magicOpenCount;
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
        bpBraved = cardEntity.bpBraved;
        braveConditionCost = cardEntity.braveConditionCost;
    }

    public void DestroyWithOrFewer(int bp)
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
        if ((card.model.cardType == CARDTYPE.SPIRIT || card.model.cardType == CARDTYPE.BRAVE)
            && (cardType == CARDTYPE.SPIRIT || cardType == CARDTYPE.BRAVE))
        {
            if (ability == ABILITY.GAIN_BP_ATTACK && GameManager.instance.isPlayerTurn == isPlayerCard)
            {
                currentBp += gainedBp;
            }
            if (card.model.ability == ABILITY.GAIN_BP_BLOCK && GameManager.instance.isPlayerTurn != card.model.isPlayerCard)
            {
                card.model.currentBp += card.model.gainedBp;
            }
            card.model.CompareBp(currentBp, name); // 攻撃側のBPと名前を渡す 引数nameはただのデバッグ用            
        }
        if ((card.model.cardType == CARDTYPE.SPIRIT || card.model.cardType == CARDTYPE.BRAVE)
            && cardType == CARDTYPE.MAGIC
            && (magic == MAGIC.DESTROY_ENEMY_CARD || magic == MAGIC.DESTROY_ALL_CARDS))
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

    // 指定した色のシンボルの合計数を取得
    public int GetSymbolCount(COLOR color)
    {
        int count = 0;
        foreach (Symbol symbol in symbols)
        {
            if (symbol.color == color) count += symbol.count;
        }
        return count;
    }

    // シンボルの合計数を取得(色を問わない)
    public int GetTotalSymbols()
    {
        int total = 0;
        foreach (Symbol symbol in symbols)
        {
            total += symbol.count;
        }
        return total;
    }
}
