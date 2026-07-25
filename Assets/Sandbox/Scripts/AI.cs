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

        /* 場にカードを出す */
        // 手札のカードリストを取得
        CardController[] handCardList = gameManager.enemyHandTransform.GetComponentsInChildren<CardController>();

        // リザーブのコアのリストを取得
        CoreController[] reserveCoreList = gameManager.enemyReserveTransform.GetComponentsInChildren<CoreController>();
        
        // コスト以下のカードがあれば、カードをフィールドに出し続ける
        while (Array.Exists(handCardList, card => gameManager.CalcNetCost(card) < gameManager.enemy.manaCost))
        {
            // Net(正味)コストがManaコスト未満のカードリストを取得
            CardController[] selectableHandCardList = Array.FindAll(handCardList, card => gameManager.CalcNetCost(card) < gameManager.enemy.manaCost);

            // 場に出すカードを選択
            CardController enemyCard = selectableHandCardList[0]; // とりあえずカードリストの一番最初のカードを選択

            // カードに乗せるコアを選択
            CoreController core = reserveCoreList[reserveCoreList.Length - 1]; // とりあえずリザーブのコアリストの一番最後のコアを選択

            // カードを移動
            StartCoroutine(enemyCard.movement.MoveToField(gameManager.enemyFieldTransform)); // カードの移動を行うCardMovementクラスのSetCardTransform()メソッドに、カードの移動先のTransformを渡す
            yield return new WaitForSeconds(0.51f); // カードが移動する時間待つ
            // 維持コアの移動と召喚コストの支払いアニメーションを同時に開始する
            Coroutine coreMoveCoroutine = StartCoroutine(core.movement.MoveTo(enemyCard.iconTransform)); // コアの移動を行うCoreMovementクラスのMoveTo()メソッドに、コアの移動先のTransformを渡す
            enemyCard.OnField();

            yield return coreMoveCoroutine; // コアがenemyCardの子になる（MoveTo完了）まで待ってからLv/BPを確定させる
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
        gameManager.step = GameManager.STEP.ATTACK;
        // フィールドのカードリストを取得
        CardController[] fieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>();
        // 攻撃可能カードがあれば攻撃を繰り返す
        while (Array.Exists(fieldCardList, card => card.model.canAttack))
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

}
