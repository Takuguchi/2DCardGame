using System;
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
    public Transform braveTransform;
    GameManager gameManager;

    void Awake()
    {
        view = GetComponent<CardView>();  // カードの見た目（view）のデータをCardViewコンポーネントから取得
        movement = GetComponent<CardMovement>();  // カードの移動（movement）のデータをCardMovementコンポーネントから取得
        iconTransform = view.iconImage.transform;
        gameManager = GameManager.instance;
    }


    // カードを生成したときに呼ばれるメソッド
    public void Init(CardEntity cardEntity, bool isPlayer)
    {
        model = new CardModel(cardEntity, isPlayer); // カードのデータを生成
        view.SetCard(model); // CardViewクラス内の、データをカードの見た目に反映するSetCard()メソッドにカードのデータを渡す
    }

    // カードの攻撃処理
    public void Attack(CardController enemyCard)
    {
        model.Attack(enemyCard); // 自分のカードの攻撃処理を呼び出す
        SetCanAttack(false);     // 攻撃したカードは攻撃できないようにする
    }

    public void Show()
    {
        view.Show(); // カードを表にする
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
        if (model.isRefreshed) SetCanAttack(true);
        else SetCanAttack(false);
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

    public void FixBp()
    {
        CardController braveCard = braveTransform.GetComponentInChildren<CardController>();
        model.FixBp();
        if (braveCard != null)
        {
            model.currentBp += braveCard.model.bpBraved;
            Debug.Log("model.currentBp:" + model.currentBp);
        }
    }

    // 合体(ブレイヴ)中のブレイヴのシンボルの数を返すメソッド
    public int BraveSymbols()
    {
        int count = 0;
        CardController braveCard = braveTransform.GetComponentInChildren<CardController>();
        if (braveCard != null)
        {
            count += braveCard.model.GetTotalSymbols();
        }
        return count;
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

    // 合体(ブレイヴ)可能かどうか判定するメソッド
    public bool CanBrave(CardController target)
    {
        if (target.model.cardType != CARDTYPE.SPIRIT) return false; // スピリットでなければ処理終了
        if (target.model.cost < model.braveConditionCost)
        {
            Debug.Log("合体(ブレイヴ)できません");
            return false; // 合体(ブレイヴ)条件：コスト〇以上 を満たしていなければ処理終了
        }
        return true;
    }

    public void BraveTo(CardController target)
    {
        Debug.Log($"{model.name}を{target.model.name}に合体(ブレイヴ)!");
        movement.BraveTo(target.braveTransform);

        target.FixBp(); // ブレイヴのBPを加算
    }

    // マジックカードが使用可能かどうか判定するメソッド
    public bool CanUseMagic()
    {
        CardController[] friendFieldCards = gameManager.GetFriendFieldCards(this.model.isPlayerCard);
        CardController[] opponentFieldCards = gameManager.GetOpponentFieldCards(this.model.isPlayerCard);

        switch (model.magic)
        {
            case MAGIC.DESTROY_ENEMY_CARD:
                // 相手のフィールドにカードがあれば使用可能
                return opponentFieldCards.Length > 0;
            case MAGIC.REFRESH_FRIEND_CARDS:
                if (model.isPlayerCard == gameManager.isPlayerTurn || gameManager.step != GameManager.STEP.ATTACK) return false;
                // 自分のフィールドに疲労状態のカードがあれば使用可能
                CardController[] exhaustedCards = Array.FindAll(friendFieldCards, card => !card.model.isRefreshed);
                return exhaustedCards.Length > 0;
            case MAGIC.DRAW:
                return gameManager.step == GameManager.STEP.MAIN;
            case MAGIC.DESTROY_ALL_CARDS:
                return friendFieldCards.Length > 0 || opponentFieldCards.Length > 0;
            case MAGIC.OPEN:
                return gameManager.step == GameManager.STEP.MAIN;
            case MAGIC.NONE:
                return false; // マジックカードでなかった場合は使用不可
        }
        return false;
    }

    // マジックカード
    public void UseMagicTo(CardController target)
    {
        Debug.Log(this.model.name + "を使用！");

        CardController[] friendFieldCards = gameManager.GetFriendFieldCards(this.model.isPlayerCard);
        CardController[] opponentFieldCards = gameManager.GetOpponentFieldCards(this.model.isPlayerCard);

        switch (model.magic)
        {
            case MAGIC.DESTROY_ENEMY_CARD:
                // 特定の敵を攻撃する
                if (target == null) return;
                if (target.model.isPlayerCard == model.isPlayerCard) return;
                target.FixBp();
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
                for (int i = 0; i < model.magicDrawCount; i++)
                {
                    if (model.isPlayerCard) gameManager.GiveCardToHand(gameManager.player.deck, gameManager.playerHandTransform);
                    else gameManager.GiveCardToHand(gameManager.enemy.deck, gameManager.enemyHandTransform);
                }
                break;
            case MAGIC.DESTROY_ALL_CARDS:
                // 破壊対象BP以下のスピリットすべてを破壊する
                if (target != null && target.model.isPlayerCard && !target.model.isFieldCard) return; // 自分の手札のカードに誤ってドロップして使用されないように
                if (target != null && !target.model.isPlayerCard && !target.model.isFieldCard) return; // 相手の手札のカードにも同様
                foreach (CardController opponentFieldCard in opponentFieldCards)
                {
                    opponentFieldCard.FixBp();
                    Attack(opponentFieldCard);
                    opponentFieldCard.CheckAlive();
                }
                foreach (CardController friendFieldCard in friendFieldCards)
                {
                    Attack(friendFieldCard);
                    friendFieldCard.CheckAlive();
                }
                break;
            case MAGIC.OPEN:
                // オープン演出の完了待ちと、その後の判定・移動が必要なためコルーチンで処理する
                // コスト支払いと自身の破棄もコルーチンの最後で行う
                StartCoroutine(OpenMagicRoutine());
                return;
            case MAGIC.NONE:
                return;
        }
        gameManager.ReduceManaCost(this); // コストの支払い
        Destroy(this.gameObject); //スペルカード使用後は削除(本当はトラッシュに移動してから非表示にしたい)
    }

    // MAGIC.OPEN専用の処理：デッキの上からカードをオープンし、演出が終わってから条件判定を行うコルーチン
    private IEnumerator OpenMagicRoutine()
    {
        gameManager.ReduceManaCost(this); // コストの支払い（使用した瞬間に支払う。他のマジックと同じタイミング）

        CardController[] opendCards = new CardController[model.magicOpenCount];
        List<Coroutine> openCoroutines = new List<Coroutine>();

        for (int i = 0; i < model.magicOpenCount; i++)
        {
            Coroutine openCoroutine;
            CardController opendCard = model.isPlayerCard
                ? gameManager.OpenCard(gameManager.player.deck, gameManager.playerFieldTransform, out openCoroutine)
                : gameManager.OpenCard(gameManager.enemy.deck, gameManager.enemyFieldTransform, out openCoroutine);
            opendCards[i] = opendCard;
            if (openCoroutine != null) openCoroutines.Add(openCoroutine);
        }

        // オープン演出（デッキ→フィールドへの移動）が全て終わるまで待つ
        foreach (Coroutine openCoroutine in openCoroutines)
        {
            yield return openCoroutine;
        }

        yield return new WaitForSeconds(1.5f); // オープンしたカードを少し見せる

        // 条件を満たしたカードは手札へ、それ以外は破壊する
        List<Coroutine> resultCoroutines = new List<Coroutine>();
        foreach (CardController opendCard in opendCards)
        {
            if (opendCard == null) continue; // デッキにカードがない場合は処理をスキップする
            if (opendCard.model.name == "光龍騎神サジット・アポロドラゴン" || opendCard.model.name == "輝竜シャイン・ブレイザー")
            {
                Transform handTransform = opendCard.model.isPlayerCard ? gameManager.playerHandTransform : gameManager.enemyHandTransform;
                resultCoroutines.Add(opendCard.StartCoroutine(opendCard.movement.MoveToField(handTransform)));
            }
            else
            {
                Destroy(opendCard.gameObject); // 条件を満たさなかったカードは破壊する
            }
        }

        // 手札への移動が全て終わるまで待つ
        foreach (Coroutine resultCoroutine in resultCoroutines)
        {
            yield return resultCoroutine;
        }

        Destroy(this.gameObject); //スペルカード使用後は削除(本当はトラッシュに移動してから非表示にしたい)
    }

    public void WhenAttack()
    {
        
        CardController braveCard = braveTransform.GetComponentInChildren<CardController>();
        CardController[] opponentFieldCards = gameManager.GetOpponentFieldCards(model.isPlayerCard);
        if (braveCard != null)
        {
            if (braveCard.model.name == "砲竜バル・ガンナー")
            {
                CardController[] canDestroyCards = Array.FindAll(opponentFieldCards, card => card.model.currentBp <= 4000);
                if (model.isPlayerCard)
                {
                    gameManager.GiveCardToHand(gameManager.player.deck, gameManager.playerHandTransform);
                }
                else
                {
                    gameManager.GiveCardToHand(gameManager.enemy.deck, gameManager.enemyHandTransform);                  
                }
                // BP4000以下の相手のスピリットを1体破壊
                if (canDestroyCards.Length > 0)
                {
                    canDestroyCards[0].model.DestroyWithOrFewer(4000);
                    canDestroyCards[0].CheckAlive();
                }
            }
        }
    }
    
    public void WhenReduceOpponentLife()
    {
        if (model.name == "ヴェロキ・ハルパー" && model.currentLv >= 2)
        {
            if (model.isPlayerCard) gameManager.GiveCardToHand(gameManager.player.deck, gameManager.playerHandTransform);
            else gameManager.GiveCardToHand(gameManager.enemy.deck, gameManager.enemyHandTransform);
        }
    }
}
