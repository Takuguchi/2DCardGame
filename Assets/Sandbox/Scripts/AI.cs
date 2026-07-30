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
        
        // コスト以下のカードがあれば、カードをフィールドに出し続ける
        // 条件：モンスターカードならコストのみ
        // 条件：スペルカードならコストと、使用可能かどうか（CanUseSpell）
        while (Array.Exists(handCardList, card => 
            (card.model.cost <= gameManager.enemy.manaCost)
            && (!card.IsSpell || (card.IsSpell && card.CanUseSpell())) ))
        {
            // 召喚/使用可能なカードリストを取得
            CardController[] selectableHandCardList = Array.FindAll(handCardList, card => 
                (card.model.cost <= gameManager.enemy.manaCost)
                && (!card.IsSpell || (card.IsSpell && card.CanUseSpell())) );
            // 召喚/使用するカードを選択
            CardController selectCard = selectableHandCardList[0]; // とりあえずカードリストの一番最初のカードを選択
            // カードを表にする
            selectCard.Show();
            // スペルカードなら使用する
            if (selectCard.IsSpell)
            {
                StartCoroutine(CastSpellOf(selectCard));
            }
            // モンスターカードなら
            else
            {
                // カードを移動
                StartCoroutine(selectCard.movement.MoveToField(gameManager.enemyFieldTransform)); // カードの移動を行うCardMovementクラスのSetCardTransform()メソッドに、カードの移動先のTransformを渡す
                selectCard.OnField(); // CardControllerクラスのOnField()メソッドを呼び出す                
            }
            yield return new WaitForSeconds(1);
            // 1秒待ってから手札のリストを更新
            handCardList = gameManager.enemyHandTransform.GetComponentsInChildren<CardController>();
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
                gameManager.AttackToHero(attacker);
                yield return new WaitForSeconds(0.25f); // カードが戻る時間待ってから、HeroのHPが0以下になったかどうかを判定する
                gameManager.CheckHeroHP(); // HeroのHPが0以下になったかどうかを判定→リザルト画面表示
            }
            fieldCardList = gameManager.enemyFieldTransform.GetComponentsInChildren<CardController>(); // フィールドのカードリストを更新
            yield return new WaitForSeconds(1);
        }

        

        yield return new WaitForSeconds(1); // 1秒待機してからターン切替

        gameManager.ChangeTurn(); // 敵のターンが終了したら、プレイヤーのターンに切り替える
    }

    // スペルカードを発動させるメソッド
    IEnumerator CastSpellOf(CardController card)
    {
        CardController target = null;
        Transform movePosition = null;
        switch (card.model.spell)
        {
            case SPELL.DAMAGE_ENEMY_CARD:
                target = gameManager.GetEnemyFieldCards(card.model.isPlayerCard)[0];
                movePosition = target.transform;
                break;
            case SPELL.HEAL_FRIEND_CARD:
                target = gameManager.GetFriendFieldCards(card.model.isPlayerCard)[0];
                movePosition = target.transform;
                break;
            case SPELL.DAMAGE_ENEMY_CARDS:
                movePosition = gameManager.playerFieldTransform;
                break;
            case SPELL.HEAL_FRIEND_CARDS:
                movePosition = gameManager.enemyFieldTransform;
                break;
            case SPELL.DAMAGE_ENEMY_HERO:
                movePosition = gameManager.playerHero;
                break;
            case SPELL.HEAL_FRIEND_HERO:
                movePosition = gameManager.enemyHero;
                break;
        }
        // 移動先としてターゲット/それぞれのフィールド/それぞれのHeroのTransformが必要
        StartCoroutine(card.movement.MoveToField(movePosition)); // カードの移動
        yield return new WaitForSeconds(0.25f);
        card.UseSpellTo(target);
    }
}
