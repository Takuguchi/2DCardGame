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

    GameManager gameManager;

    // スペルカードだったらtrueを返す
    public bool IsSpell
    {
        get { return model.spell != SPELL.NONE; }
    }

    void Awake()
    {
        view = GetComponent<CardView>();  // カードの見た目（view）のデータをCardViewコンポーネントから取得
        movement = GetComponent<CardMovement>();  // カードの移動（movement）のデータをCardMovementコンポーネントから取得
        gameManager = GameManager.instance;
    }


    // カードを生成したときに呼ばれるメソッド
    public void Init(int cardID, bool isPlayer)
    {
        model = new CardModel(cardID, isPlayer); // カードのデータを生成
        view.Show(model); // CardViewクラス内の、データをカードの見た目に反映するShow()メソッドにカードのデータを渡す
    }

    // カードの攻撃処理
    public void Attack(CardController enemyCard)
    {
        model.Attack(enemyCard); // 自分のカードの攻撃処理を呼び出す
        SetCanAttack(false);     // 攻撃したカードは攻撃できないようにする
    }

    // カードの回復処理
    public void Heal(CardController friendCard)
    {
        model.Heal(friendCard);
        friendCard.RefreshView();
    }

    // カードの表示を更新するメソッド
    public void RefreshView()
    {
        view.Refresh(model);
    }

    // カードを攻撃可能にするメソッド
    public void SetCanAttack(bool canAttack)
    {
        model.canAttack = canAttack; // カードのデータのcanAttackを引数のcanAttackにする
        view.SetActiveSelectablePanel(canAttack); // 攻撃可能なカードはオーラを表示する
    }

    // フィールドにカードを出したときに呼ばれるメソッド
    public void OnField()
    {
        gameManager.ReduceManaCost(model.cost, model.isPlayerCard); // カードをドロップしたらPlayerのManaコストを減らす
        model.isFieldCard = true; // カードをドロップしたらフィールドのカードにする
        if (model.ability == ABILITY.INIT_ATTACKABLE)
        {
            SetCanAttack(true); // カードのアビリティがINIT_ATTACKABLEなら、攻撃可能にする
        }
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

    // スペルカード
    public void UseSpellTo(CardController target)
    {
        switch (model.spell)
        {
            case SPELL.DAMAGE_ENEMY_CARD:
                // 特定の敵を攻撃する
                Attack(target);
                target.CheckAlive();
                break;
            case SPELL.DAMAGE_ENEMY_CARDS:
                // 相手フィールドの全てのカードに攻撃する
                CardController[] enemyCards = gameManager.GetEnemyFieldCards(this.model.isPlayerCard);
                foreach (CardController enemyCard in enemyCards)
                {
                    Attack(enemyCard);
                }
                foreach (CardController enemyCard in enemyCards)
                {
                    enemyCard.CheckAlive();
                }
                break;
            case SPELL.DAMAGE_ENEMY_HERO:
                // 相手のヒーローを攻撃する
                gameManager.AttackToHero(this);
                break;
            case SPELL.HEAL_FRIEND_CARD:
                // 特定のカードを回復させる
                Heal(target);
                break;
            case SPELL.HEAL_FRIEND_CARDS:
                // 自分のフィールドの全てのカードを回復させる
                CardController[] friendCards = gameManager.GetFriendFieldCards(this.model.isPlayerCard);
                foreach(CardController friendCard in friendCards)
                {
                    Heal(friendCard);
                }
                break;
            case SPELL.HEAL_FRIEND_HERO:
                // 自分のヒーローを回復させる
                gameManager.HealTohero(this);
                break;
            case SPELL.NONE:
                return; // ドロップされたカードがスペルカードでなかった場合は処理を終了
        }
        Destroy(this.gameObject); //スペルカード使用後は削除
    }

    // スペルカードが使用可能かどうか判定するメソッド
    public bool CanUseSpell()
    {
        switch (model.spell)
        {
            case SPELL.DAMAGE_ENEMY_CARD:
            case SPELL.DAMAGE_ENEMY_CARDS:
                // 相手のフィールドにカードがあれば使用可能
                CardController[] enemyCards = gameManager.GetEnemyFieldCards(this.model.isPlayerCard);
                return enemyCards.Length > 0;
            case SPELL.DAMAGE_ENEMY_HERO:
            case SPELL.HEAL_FRIEND_HERO:
                return true;
            case SPELL.HEAL_FRIEND_CARD:
            case SPELL.HEAL_FRIEND_CARDS:
                // 自分のフィールドにカードがあれば使用可能
                CardController[] friendCards = gameManager.GetFriendFieldCards(this.model.isPlayerCard);
                return friendCards.Length > 0;
            case SPELL.NONE:
                return false; // スペルカードでなかった場合は使用不可
        }
        return false;
    }
}
