using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    GameManager gameManager; // GameManagerのインスタンスを取得する変数
    void Start()
    {
        gameManager = GameManager.instance; // GameManagerのインスタンスを取得
    }
    
    // 敵のターンの処理を行うメソッド
    public IEnumerator EnemyTurn()
    {
        /*
        // フィールドのカードを攻撃可能にする
        CardController[] enemyFieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>();
        gameManager.SettingCanAttackView(enemyFieldCardList, true); // フィールドのカードに攻撃可能オーラを付ける

        //フィールドのカードを全て回復状態にする
        foreach (CardController enemyFieldCard in enemyFieldCardList)
        {
            enemyFieldCard.ChangeIsRefreshed(true);
        }
        */

        yield return new WaitForSeconds(1); // カードをフィールドに出す前に1秒置く

        gameManager.StepCalc(gameManager.isPlayerTurn, GameManager.STEP.MAIN);

        /* 場にカードを出す */
        // 手札のカードリストを取得
        CardController[] handCardList = gameManager.enemyHandTransform.GetComponentsInChildren<CardController>();

        // リザーブのコアのリストを取得
        CoreController[] reserveCoreList = gameManager.enemyReserveTransform.GetComponentsInChildren<CoreController>();
        
        // コスト以下のカードがあれば、カードをフィールドに出し続ける
        // while (Array.Exists(handCardList, card => gameManager.CalcNetCost(card) < reserveCoreList.Length))
        // 条件：モンスターカードならコストのみ
        // 条件：マジックカードならコストと、使用可能かどうか（CanUseMagic）
        while (Array.Exists(handCardList, card => 
               (card.model.cardType == CARDTYPE.SPIRIT && gameManager.CalcNetCost(card) < reserveCoreList.Length)
               || (card.model.cardType == CARDTYPE.MAGIC && gameManager.CalcNetCost(card) <= reserveCoreList.Length && card.CanUseMagic()) ))
        {
            // Net(正味)コストがリザーブのコアの総数未満のカードリストを取得
            CardController[] selectableHandCardList = Array.FindAll(handCardList, card => 
               (card.model.cardType == CARDTYPE.SPIRIT && gameManager.CalcNetCost(card) < reserveCoreList.Length)
               || (card.model.cardType == CARDTYPE.MAGIC && gameManager.CalcNetCost(card) <= reserveCoreList.Length && card.CanUseMagic()) );

            // 場に出すカードを選択
            CardController enemyCard = selectableHandCardList[0]; // とりあえずカードリストの一番最初のカードを選択
            // カードを表にする
            enemyCard.Show();
            if (enemyCard.model.cardType == CARDTYPE.MAGIC)
            {
                StartCoroutine(CastMagicOf(enemyCard));
                yield return new WaitForSeconds(0.51f); // カードが移動する時間待つ
            }
            
            if (enemyCard.model.cardType == CARDTYPE.SPIRIT)
            {
                // カードを移動
                StartCoroutine(enemyCard.movement.MoveToField(gameManager.enemyFieldTransform));
                yield return new WaitForSeconds(0.51f); // カードが移動する時間待つ
                // カードに乗せるコアを選択
                CoreController core = reserveCoreList[reserveCoreList.Length - 1]; // とりあえずリザーブのコアリストの一番最後のコアを選択
                // 維持コアの移動と召喚コストの支払いアニメーションを同時に開始する
                Coroutine coreMoveCoroutine = StartCoroutine(core.movement.MoveTo(enemyCard.iconTransform));
                enemyCard.OnField();
                yield return coreMoveCoroutine; // コアがenemyCardの子になる（MoveTo完了）まで待ってからLv/BPを確定させる    
            }
            gameManager.ArrangeCoresAndFixLv(gameManager.GetFriendFieldCards(enemyCard.model.isPlayerCard));

            Debug.Log($"{enemyCard.model.name}をLv{enemyCard.model.currentLv}で召喚！");

            // 手札のリストを更新
            handCardList = gameManager.enemyHandTransform.GetComponentsInChildren<CardController>();

            // リザーブのコアのリストを更新
            reserveCoreList = gameManager.enemyReserveTransform.GetComponentsInChildren<CoreController>();

            yield return new WaitForSeconds(1);
        }

        yield return new WaitForSeconds(1);

        /* 攻撃 */
        /*
        // フィールドのカードリストを取得
        CardController[] fieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>();
        // 攻撃可能カードがあれば攻撃を繰り返す
        while (Array.Exists(fieldCardList, card => card.model.canAttack))
        {
            // 攻撃可能カードを取得
            CardController[] enemyCanAttackCardList = Array.FindAll(fieldCardList, card => card.model.canAttack); // 検索：Array.FindAll
            CardController[] playerFieldCardList = gameManager.playerFieldTransform.GetComponentsInChildren<CardController>();


            // attackerカードを選択
            CardController attacker = enemyCanAttackCardList[0]; // defenderカードを選択（フィールドの攻撃可能カードから選択）  
            
            // プレイヤーのフィールドに回復状態のカードが存在する場合はカード同士で戦わせる
            if (playerFieldCardList.Length > 0 && Array.Exists(playerFieldCardList, card => card.model.isRefreshed))
            {
                // defenderカードを選択
                // シールドカードのみ攻撃対象にする
                if (Array.Exists(playerFieldCardList, card => card.model.ability == ABILITY.SHIELD))
                {
                    playerFieldCardList = Array.FindAll(playerFieldCardList, card => card.model.ability == ABILITY.SHIELD);
                }

                // 回復状態のカードのみ攻撃対象にする
                playerFieldCardList = Array.FindAll(playerFieldCardList, card => card.model.isRefreshed);

                CardController defender = playerFieldCardList[0]; // とりあえずPlayerフィールドの一番左のカードを選択
                // attackerとdefenderを戦わせる
                StartCoroutine(attacker.movement.MoveToTarget(defender.transform)); // カードの移動を行うCardMovementクラスのMoveToTarget()メソッドに、カードの移動先のTransformを渡す
                yield return new WaitForSeconds(0.51f);
                gameManager.CardsBattle(attacker, defender);
            }
            else // プレイヤーのフィールドにカードが存在しない場合は、敵はプレイヤーのHeroに攻撃する
            {
                StartCoroutine(attacker.movement.MoveToTarget(gameManager.playerHero)); // カードの移動を行うCardMovementクラスのMoveToTarget()メソッドに、カードの移動先のTransformを渡す
                yield return new WaitForSeconds(0.25f);
                gameManager.AttackToHero(attacker, false); // 敵がHeroに攻撃するのでisPlayerCardはfalseにする
                yield return new WaitForSeconds(0.25f); // カードが戻る時間待ってから、HeroのHPが0以下になったかどうかを判定する
                gameManager.CheckHeroHP(); // HeroのHPが0以下になったかどうかを判定→リザルト画面表示
            }
            fieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>(); // フィールドのカードリストを更新
            yield return new WaitForSeconds(1);
        }
        */

        /* 攻撃(バトスピ用) */
        gameManager.StepCalc(gameManager.isPlayerTurn, GameManager.STEP.ATTACK);
        // フィールドのカードリストを取得
        CardController[] fieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>();
        // 攻撃可能カードがあれば攻撃を繰り返す
        //while (Array.Exists(fieldCardList, card => card.model.canAttack))
        if (Array.Exists(fieldCardList, card => card.model.canAttack)) // いったんアタックしてくるスピリットは1体にする
        {
            // 攻撃可能カードを取得
            CardController[] enemyCanAttackCardList = Array.FindAll(fieldCardList, card => card.model.canAttack); // 検索：Array.FindAll

            // attackerカードを選択
            CardController attacker = enemyCanAttackCardList[0]; // defenderカードを選択（フィールドの攻撃可能カードから選択）

            // まずアタック(疲労)
            StartCoroutine(attacker.movement.TapCard(false));

            // タイマー起動(5秒) プレイヤーがフィールド上の回復状態のカードかHeroをクリックするのを待つ
            gameManager.selectedDefenderCard = null;
            gameManager.heroWasClicked = false;
            gameManager.isWaitingForDefenderSelection = true;

            // float attackTimer = 5f;
            // while (attackTimer > 0f && gameManager.selectedDefenderCard == null && !gameManager.heroWasClicked)
            while (gameManager.selectedDefenderCard == null && !gameManager.heroWasClicked && attacker.model.isAlive == true)
            {
                // attackTimer -= Time.deltaTime;
                yield return null;
            }
            gameManager.isWaitingForDefenderSelection = false;

            CardController defender = gameManager.selectedDefenderCard;
            gameManager.selectedDefenderCard = null;
            bool heroWasClicked = gameManager.heroWasClicked;
            gameManager.heroWasClicked = false;

            // 5秒以内にプレイヤーがフィールド上のいずれかの回復状態のカードをクリックしたらそのカードとバトル
            if (defender != null)
            {
                StartCoroutine(defender.movement.TapCard(false)); // defenderを疲労させる

                // attackerとdefenderを戦わせる
                StartCoroutine(attacker.movement.MoveToTarget(defender.transform)); // カードの移動を行うCardMovementクラスのMoveToTarget()メソッドに、カードの移動先のTransformを渡す
                yield return new WaitForSeconds(0.51f);
                gameManager.CardsBattle(attacker, defender);
            }
            else if (heroWasClicked) // 5秒以内にプレイヤーがヒーローをクリックしたらライフを減らす
            {
                StartCoroutine(attacker.movement.MoveToTarget(gameManager.playerHero)); // カードの移動を行うCardMovementクラスのMoveToTarget()メソッドに、カードの移動先のTransformを渡す
                yield return new WaitForSeconds(0.25f); // 敵がHeroに攻撃するのでisPlayerCardはfalseにする
                gameManager.AttackToHero(attacker);
                yield return new WaitForSeconds(0.25f); // カードが戻る時間待ってから、HeroのHPが0以下になったかどうかを判定する
                gameManager.CheckHeroHP(); // HeroのHPが0以下になったかどうかを判定→リザルト画面表示
            }
            else if (attacker.model.isAlive == false) // フラッシュ効果でフィールドを離れたら
            {
                yield return null;
            }
            else // プレイヤーが5秒何もせずに経過したらライフを減らす
            {
                StartCoroutine(attacker.movement.MoveToTarget(gameManager.playerHero)); // カードの移動を行うCardMovementクラスのMoveToTarget()メソッドに、カードの移動先のTransformを渡す
                yield return new WaitForSeconds(0.25f);
                gameManager.AttackToHero(attacker);
                yield return new WaitForSeconds(0.25f); // カードが戻る時間待ってから、HeroのHPが0以下になったかどうかを判定する
                gameManager.CheckHeroHP(); // HeroのHPが0以下になったかどうかを判定→リザルト画面表示
            }
            fieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>(); // フィールドのカードリストを更新
            yield return new WaitForSeconds(1);
        }

        // gameManager.step = GameManager.STEP.END;

        yield return new WaitForSeconds(1); // 1秒待機してからターン切替

        // gameManager.ChangeTurn(); // 敵のターンが終了したら、プレイヤーのターンに切り替える
        gameManager.StepCalc(gameManager.isPlayerTurn, GameManager.STEP.END);
    }

    public IEnumerator PlayerTurn(CardController attacker)
    {
        yield return new WaitForSeconds(1); // 考えてる風の1秒待機

        CardController[] handCardList = gameManager.enemyHandTransform.GetComponentsInChildren<CardController>();
        CardController[] fieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>();
        CoreController[] reserveCoreList = gameManager.enemyReserveTransform.GetComponentsInChildren<CoreController>();

        if (gameManager.isDuringAttack)
        {
            // フラッシュタイミング
            while (Array.Exists(handCardList, card =>
                   card.model.cardType == CARDTYPE.MAGIC && gameManager.CalcNetCost(card) <= reserveCoreList.Length && card.CanUseMagic() ))
            {
                CardController[] selectableHandCardList = Array.FindAll(handCardList, card =>
                    card.model.cardType == CARDTYPE.MAGIC && gameManager.CalcNetCost(card) <= reserveCoreList.Length && card.CanUseMagic() );

                // フラッシュタイミングで使用できるカードを選択
                CardController magicCard = selectableHandCardList[0]; // とりあえずカードリストの一番最初のカードを選択
                // カードを表にする
                magicCard.Show();
                
                if (magicCard.model.cardType == CARDTYPE.MAGIC)
                {
                    StartCoroutine(CastMagicOf(magicCard));
                    yield return new WaitForSeconds(0.51f); // カードが移動する時間待つ
                }
                
                gameManager.ArrangeCoresAndFixLv(gameManager.GetFriendFieldCards(magicCard.model.isPlayerCard));

                // 手札のリストを更新
                handCardList = gameManager.enemyHandTransform.GetComponentsInChildren<CardController>();

                // リザーブのコアのリストを更新
                reserveCoreList = gameManager.enemyReserveTransform.GetComponentsInChildren<CoreController>();

                yield return new WaitForSeconds(1);
            }

            // ブロックするかライフで受けるか選択
            
            // ブロック可能カードを取得
            CardController[] enemyCanBlockCardList = Array.FindAll(fieldCardList, card => card.model.isRefreshed);

            // フィールドに回復状態のスピリットがいればブロック
            if (enemyCanBlockCardList.Length > 0)
            {
                // blockerカードを選択
                CardController blocker = enemyCanBlockCardList[0]; // blockerカードを選択（フィールドのブロック可能カードから選択）

                // まずブロック(疲労)
                blocker.ChangeIsRefreshed(false);

                // フラッシュタイミング
                while (Array.Exists(handCardList, card =>
                    card.model.cardType == CARDTYPE.MAGIC && gameManager.CalcNetCost(card) <= reserveCoreList.Length && card.CanUseMagic() ))
                {
                    CardController[] selectableHandCardList = Array.FindAll(handCardList, card =>
                        card.model.cardType == CARDTYPE.MAGIC && gameManager.CalcNetCost(card) <= reserveCoreList.Length && card.CanUseMagic() );

                    // フラッシュタイミングで使用できるカードを選択
                    CardController magicCard = selectableHandCardList[0]; // とりあえずカードリストの一番最初のカードを選択

                    if (magicCard.model.cardType == CARDTYPE.MAGIC)
                    {
                        StartCoroutine(CastMagicOf(magicCard));
                        yield return new WaitForSeconds(0.51f); // カードが移動する時間待つ
                    }
                    
                    gameManager.ArrangeCoresAndFixLv(gameManager.GetFriendFieldCards(magicCard.model.isPlayerCard));

                    // 手札のリストを更新
                    handCardList = gameManager.enemyHandTransform.GetComponentsInChildren<CardController>();

                    // リザーブのコアのリストを更新
                    reserveCoreList = gameManager.enemyReserveTransform.GetComponentsInChildren<CoreController>();

                    yield return new WaitForSeconds(1);
                }

                // バトル
                StartCoroutine(attacker.movement.MoveToTarget(blocker.transform)); // カードの移動を行うCardMovementクラスのMoveToTarget()メソッドに、カードの移動先のTransformを渡す
                yield return new WaitForSeconds(0.51f);
                gameManager.CardsBattle(attacker, blocker); 
            }

            // フィールドに回復状態のスピリットがいなければライフで受ける
            if (enemyCanBlockCardList.Length == 0)
            {
                StartCoroutine(attacker.movement.MoveToTarget(gameManager.enemyHero)); // カードの移動を行うCardMovementクラスのMoveToTarget()メソッドに、カードの移動先のTransformを渡す
                yield return new WaitForSeconds(0.25f); // 敵がHeroに攻撃するのでisPlayerCardはfalseにする
                gameManager.AttackToHero(attacker);
                yield return new WaitForSeconds(0.25f); // カードが戻る時間待ってから、HeroのHPが0以下になったかどうかを判定する
                gameManager.CheckHeroHP();
            }

            // 破壊されたかもしれないのでフィールドのカードを更新
            fieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>();
            GameManager.instance.isDuringAttack = false;
        }
        
        yield return new WaitForEndOfFrame();
    }

    // マジックカードを発動させるメソッド
    IEnumerator CastMagicOf(CardController card)
    {
        CardController target = null;
        Transform movePosition = null;
        switch (card.model.magic)
        {
            case MAGIC.DESTROY_ENEMY_CARD:
                target = gameManager.GetOpponentFieldCards(card.model.isPlayerCard)[0];
                movePosition = target.transform;
                break;
            case MAGIC.REFRESH_FRIEND_CARDS:
                movePosition = gameManager.enemyFieldTransform;
                break;
            case MAGIC.DRAW:
                movePosition = gameManager.enemyFieldTransform;
                break;
            case MAGIC.DESTROY_ALL_CARDS:
                movePosition = gameManager.enemyFieldTransform;
                break;
        }
        StartCoroutine(card.movement.MoveToField(movePosition)); // カードの移動
        yield return new WaitForSeconds(0.25f);
        card.UseMagicTo(target);
    }
}
