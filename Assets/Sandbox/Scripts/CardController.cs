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
    // public Transform iconTransform => view.iconTransform; // コアの移動先として使うIconのTransform
    public Transform iconTransform;
    GameManager gameManager;

    void Awake()
    {
        view = GetComponent<CardView>();  // カードの見た目（view）のデータをCardViewコンポーネントから取得
        movement = GetComponent<CardMovement>();  // カードの移動（movement）のデータをCardMovementコンポーネントから取得
        iconTransform = view.iconImage.transform;
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

    // カードを攻撃可能にするメソッド
    public void SetCanAttack(bool canAttack)
    {
        model.canAttack = canAttack; // カードのデータのcanAttackを引数のcanAttackにする
        view.SetActiveSelectablePanel(canAttack); // 攻撃可能なカードはオーラを表示する
    }

    // カードの疲労/回復状態を切り替えるメソッド
    public void ChangeIsRefreshed(bool isRefreshed)
    {
        model.isRefreshed = isRefreshed;
        StartCoroutine(movement.TapCard(isRefreshed));
    }

    // フィールドにカードを出したときに呼ばれるメソッド
    public void OnField()
    {
        // gameManager.ReduceManaCost(model.cost, isPlayer); // カードをドロップしたらPlayerのManaコストを減らす
        gameManager.ReduceManaCost(this); //バトスピ用
        view.Refresh(model); // coreNumが増加したはずなのでカードの見た目を更新する
        model.isFieldCard = true; // カードをドロップしたらフィールドのカードにする
        model.isRefreshed = true;
        model.FixBp();
        
        /*
        if (model.ability == ABILITY.INIT_ATTACKABLE)
        {
            SetCanAttack(true); // カードのアビリティがINIT_ATTACKABLEなら、攻撃可能にする
        }
        */
        if (gameManager.turnCount != 1) SetCanAttack(true); // 1ターン目はアタックできない
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
            CoreController[] cores = GetComponentsInChildren<CoreController>(); // 破壊されるカードに乗っているコアを取得
            Destroy(this.gameObject);    // カードを破壊する
            gameManager.OnDestroyed(model.isPlayerCard, cores); // 破壊されたカード上のコアをリザーブに移動
        }
    }

    // マジックカードが使用可能かどうか判定するメソッド
    public bool CanUseMagic(CardController target)
    {
        CardController[] friendFieldCards = gameManager.GetFriendFieldCards(this.model.isPlayerCard);
        CardController[] opponentFieldCards = gameManager.GetOpponentFieldCards(this.model.isPlayerCard);

        switch (model.magic)
        {
            case MAGIC.DESTROY_ENEMY_CARD:
                // 相手のフィールドにカードがあれば使用可能
                
                if (target == null) return false;
                if (target.model.isPlayerCard == model.isPlayerCard) return false;
                return opponentFieldCards.Length > 0;
            case MAGIC.REFRESH_FRIEND_CARDS:
                // 自分のフィールドにカードがあれば使用可能
                CardController[] friendCards = gameManager.GetFriendFieldCards(this.model.isPlayerCard);
                return friendCards.Length > 0;
            case MAGIC.DRAW:
                return gameManager.step == GameManager.STEP.MAIN;
            case MAGIC.DESTROY_ALL_CARDS:
                return friendFieldCards.Length > 0 || opponentFieldCards.Length > 0;
            case MAGIC.NONE:
                return false; // マジックカードでなかった場合は使用不可
        }
        return false;
    }

    // マジックカード
    public void UseMagicTo(CardController target)
    {
        gameManager.ReduceManaCost(this); // コストの支払い

        Debug.Log(this.model.name + "を使用！");

        CardController[] friendFieldCards = gameManager.GetFriendFieldCards(this.model.isPlayerCard);
        CardController[] opponentFieldCards = gameManager.GetOpponentFieldCards(this.model.isPlayerCard);

        switch (model.magic)
        {
            case MAGIC.DESTROY_ENEMY_CARD:
                // 特定の敵を攻撃する
                Attack(target);
                target.CheckAlive();
                break;
            case MAGIC.REFRESH_FRIEND_CARDS:
                // 自分のスピリットすべてを回復させる
                foreach (CardController friendFieldCard in friendFieldCards)
                {
                    friendFieldCard.ChangeIsRefreshed(true);
                }
                break;
            case MAGIC.DRAW:
                for (int i = 0; i < 2; i++)
                {
                    if (this.model.isPlayerCard) gameManager.GiveCardToHand(gameManager.player.deck, gameManager.playerHandTransform);
                    else gameManager.GiveCardToHand(gameManager.enemy.deck, gameManager.enemyHandTransform);
                }
                break;
            case MAGIC.DESTROY_ALL_CARDS:
                // at以下のスピリットすべてを破壊する
                foreach (CardController opponentFieldCard in opponentFieldCards)
                {
                    Attack(opponentFieldCard);
                    opponentFieldCard.CheckAlive();
                }
                foreach (CardController friendFieldCard in friendFieldCards)
                {
                    Attack(friendFieldCard);
                    friendFieldCard.CheckAlive();
                }
                break;
            case MAGIC.NONE:
                return;
        }
        Destroy(this.gameObject); //スペルカード使用後は削除(本当はトラッシュに移動してから非表示にしたい)
    }
}
