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
        Debug.Log("Enemyのターン");
        // フィールドのカードを攻撃可能にする
        CardController[] enemyFieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>();
        gameManager.SettingCanAttackView(enemyFieldCardList, true); // フィールドのカードに攻撃可能オーラを付ける

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
            StartCoroutine(core.movement.MoveToCard(enemyCard.transform)); // コアの移動を行うCoreMovementクラスのMoveToCard()メソッドに、コアの移動先のTransformを渡す
            enemyCard.OnField(false, enemyCard.transform); // CardControllerクラスのOnField()メソッドを呼び出す(敵側なのでisPlayer引数はfalseで渡す)
            Debug.Log(enemyCard.model.name + "を召喚！");            

            // 手札のリストを更新
            handCardList = gameManager.enemyHandTransform.GetComponentsInChildren<CardController>();

            // リザーブのコアのリストを更新
            reserveCoreList = gameManager.enemyReserveTransform.GetComponentsInChildren<CoreController>();

            yield return new WaitForSeconds(1);
        }

        yield return new WaitForSeconds(1);

        /* 攻撃 */
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
            
            // プレイヤーのフィールドにカードが存在する場合はカード同士で戦わせる
            if (playerFieldCardList.Length > 0)
            {
                // defenderカードを選択
                // シールドカードのみ攻撃対象にする
                if (Array.Exists(playerFieldCardList, card => card.model.ability == ABILITY.SHIELD))
                {
                    playerFieldCardList = Array.FindAll(playerFieldCardList, card => card.model.ability == ABILITY.SHIELD);
                }

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

        

        yield return new WaitForSeconds(1); // 1秒待機してからターン切替

        gameManager.ChangeTurn(); // 敵のターンが終了したら、プレイヤーのターンに切り替える
    }

}
