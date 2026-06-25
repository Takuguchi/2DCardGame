using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject resultPanel; // ゲーム終了時に表示するパネルを取得
    [SerializeField] Text resultText;  // ゲーム終了時に表示するテキストを取得
    [SerializeField] Transform playerHandTransform, // プレイヤーの手札のTransformを取得
                               playerFieldTransform, // プレイヤーのフィールドのTransformを取得   
                               enemyHandTransform,  // 敵の手札のTransformを取得 
                               enemyFieldTransform; // 敵のフィールドのTransformを取得
    [SerializeField] CardController cardPrefab; // カードのPrefabをCardController型として取得

    bool isPlayerTurn; // プレイヤーのターンかどうかを判定する変数

    List<int> playerDeck = new List<int>() {3, 1, 2, 2, 3}, // プレイヤーのデッキのカードIDを格納するリスト
              enemyDeck  = new List<int>() {2, 1, 3, 1, 3};  // 敵のデッキのカードIDを格納するリスト
    
    [SerializeField] Text playerHeroHpText; // プレイヤーのHeroのHPを表示するTextを取得
    [SerializeField] Text enemyHeroHpText; // 敵のHeroのHPを表示するTextを取得   

    int playerHeroHp = 30; // プレイヤーのHeroのHP
    int enemyHeroHp  = 30; // 敵のHeroのHP

    [SerializeField] Text playerManaCostText; // プレイヤーのマナコストを表示するTextを取得
    [SerializeField] Text enemyManaCostText; // 敵のマナコストを表示するTextを取得   

    public int playerManaCost = 30; // プレイヤーのマナコスト
    int enemyManaCost  = 30; // 敵のマナコスト


    // シングルトン化（GameManagerにどこからでもアクセスできるようにする）
    public static GameManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


    void Start()
    {
        StartGame(); // ゲーム開始時にStartGame()メソッドを呼び出す
    }

    // ゲーム開始時に呼ばれるメソッド
    void StartGame()
    {
        resultPanel.SetActive(false); // ゲーム開始時はリザルト画面を非表示にしておく
        playerHeroHp = 1; // プレイヤーのHeroのHPを1にする
        enemyHeroHp = 1; // 敵のHeroのHPをリザルト画面の確認のために1にする
        playerManaCost = 1; // プレイヤーのマナコストを1にする
        enemyManaCost = 1; // 敵のマナコストを1にする
        ShowHeroHP(); // HeroのHP表示を変更するメソッドを呼び出す
        ShowManaCost(); // マナコストの表示を変更するメソッドを呼び出す
        SettingInitHand();
        isPlayerTurn = true; // プレイヤーのターンから開始する
        TurnCalc(); // ターン処理を行うメソッドを呼び出す
    }

    // マナコストの表示を変更するメソッド
    void ShowManaCost()
    {
        playerManaCostText.text = playerManaCost.ToString();
        enemyManaCostText.text = enemyManaCost.ToString();
    }

    // マナコストを消費するメソッド
    public void ReduceManaCost(int cost, bool isPlayerCard)
    {
        if (isPlayerCard)
        {
            playerManaCost -= cost; // プレイヤーのマナコストを消費する
        }
        else
        {
            enemyManaCost -= cost; // 敵のマナコストを消費する
        }
        ShowManaCost(); // マナコストの表示を更新する
    }

    // ゲームをリスタートするメソッド
    public void Restart()
    {
        // HandとFieldのカードを削除
        foreach (Transform card in playerHandTransform)
        {
            Destroy(card.gameObject);
        }
        foreach (Transform card in playerFieldTransform)
        {
            Destroy(card.gameObject);
        }
        foreach (Transform card in enemyHandTransform)
        {
            Destroy(card.gameObject);
        }
        foreach (Transform card in enemyFieldTransform)
        {
            Destroy(card.gameObject);
        }

        // デッキを生成
        playerDeck = new List<int>() {3, 1, 2, 2, 3}; // プレイヤーのデッキのカードIDを格納するリスト
        enemyDeck  = new List<int>() {2, 1, 3, 1, 3};  // 敵のデッキのカードIDを格納するリスト

        StartGame(); // ゲーム開始時に呼ばれるメソッドを呼び出す
    }

    // ゲーム開始時に手札を初期化するメソッド
    void SettingInitHand()
    {
        // カードをそれぞれに3枚配る
        for (int i = 0; i < 3; i++)
        {
            GiveCardToHand(playerDeck, playerHandTransform); // プレイヤーの手札にカードを生成
            GiveCardToHand(enemyDeck, enemyHandTransform);  // 敵の手札にカードを生成
        }
    }

    // デッキからカードを手札に配るメソッド
    void GiveCardToHand(List<int> deck, Transform hand)
    {
        if (deck.Count == 0) // デッキにカードがない場合
        {
            return; // 何も処理しないで終わる
        }
        int cardID = deck[0]; // デッキの一番上のカードIDを取得
        deck.RemoveAt(0); // デッキの一番上のカードIDをデッキから削除
        CreateCard(cardID, hand); // カードを生成するメソッドにカードIDと手札のTransformを渡す
    }


    // カードを生成するメソッド
    void CreateCard(int cardID, Transform hand)
    {
        // カードのPrefabをCardController型としてインスタンス(生成)・親要素に任意のTransformを指定
        CardController card = Instantiate(cardPrefab, hand, false);
        card.Init(cardID);    // CardControllerクラスのInit()メソッドを呼び出す→任意のカードデータの各種変数を取得
    }

    // ターン処理を行うメソッド
    void TurnCalc()
    {
        if (isPlayerTurn)
        {
            // プレイヤーのターンの処理
            PlayerTurn();
        }
        else
        {
            // 敵のターンの処理
            EnemyTurn();
        }
    }

    // ターンを切り替えるメソッド
    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn; // ターンを切り替える
        if (isPlayerTurn)
        {
            GiveCardToHand(playerDeck, playerHandTransform); // プレイヤーの手札にカードを1枚生成（ドロー）
        }
        else
        {
            GiveCardToHand(enemyDeck, enemyHandTransform);  // 敵の手札にカードを1枚生成（ドロー）
        }
        TurnCalc(); // ターン処理を行うメソッドを呼び出
    }

    // プレイヤーのターンの処理を行うメソッド
    void PlayerTurn()
    {
        Debug.Log("Playerのターン");
        // フィールドのカードを攻撃可能にする
        CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();
        foreach (CardController card in playerFieldCardList)
        {
            card.SetCanAttack(true);    // cardを攻撃可能にする
        }
    }

    // 敵のターンの処理を行うメソッド
    void EnemyTurn()
    {
        Debug.Log("Enemyのターン");
        // フィールドのカードを攻撃可能にする
        CardController[] enemyFieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();
        foreach (CardController card in enemyFieldCardList)
        {
            card.SetCanAttack(true);    // cardを攻撃可能にする
        }

        

        /* 場にカードを出す */
        // 手札のカードリストを取得
        CardController[] handCardList = enemyHandTransform.GetComponentsInChildren<CardController>();
        
        // Manaコスト以下のカードリストを取得
        CardController[] selectableHandCardList = Array.FindAll(handCardList, card => card.model.cost <= enemyManaCost);

        // Manaコスト以下のカードリストが1枚以上存在する場合
        if (selectableHandCardList.Length > 0)
        {
            // 場に出すカードを選択
            CardController enemyCard = selectableHandCardList[0]; // とりあえずカードリストの一番最初のカードを選択
            // カードを移動
            enemyCard.movement.SetCardTransform(enemyFieldTransform); // カードの移動を行うCardMovementクラスのSetCardTransform()メソッドに、カードの移動先のTransformを渡す
            ReduceManaCost(enemyCard.model.cost, false); // カードを出したら敵のManaコストを減らす　引数isPlayerCardはfalseで渡す
            enemyCard.model.isFieldCard = true; // カードを出したらフィールドのカードにする

        }

        /* 攻撃 */
        // フィールドのカードリストを取得
        CardController[] fieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();
        // 攻撃可能カードを取得
        CardController[] enemyCanAttackCardList = Array.FindAll(fieldCardList, card => card.model.canAttack); // 検索：Array.FindAll
        CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();


        // 攻撃可能カードが存在する場合
        if (enemyCanAttackCardList.Length > 0)
        {
            // attackerカードを選択
            CardController attacker = enemyCanAttackCardList[0]; // defenderカードを選択（フィールドの攻撃可能カードから選択）  
            
            // プレイヤーのフィールドにカードが存在する場合はカード同士で戦わせる
            if (playerFieldCardList.Length > 0)
            {
                // defenderカードを選択
                CardController defender = playerFieldCardList[0]; // とりあえずPlayerフィールドの一番左のカードを選択
                // attackerとdefenderを戦わせる
                CardsBattle(attacker, defender);
            }
            else // プレイヤーのフィールドにカードが存在しない場合は、敵はプレイヤーのHeroに攻撃する
            {
                AttackToHero(attacker, false); // 敵がHeroに攻撃するのでisPlayerCardはfalseにする    
            }
        }


        ChangeTurn(); // 敵のターンが終了したら、プレイヤーのターンに切り替える
    }

    public void CardsBattle(CardController attacker, CardController defender)
    {
        Debug.Log("CardsBattle");
        Debug.Log("attacker HP:" + attacker.model.hp);
        Debug.Log("defender HP:" + defender.model.hp);

        attacker.Attack(defender); // attackerの攻撃力分のダメージをdefenderに与える
        defender.Attack(attacker); // defenderの攻撃力分のダメージをattackerに与える
        
        Debug.Log("attacker HP:" + attacker.model.hp);
        Debug.Log("defender HP:" + defender.model.hp);
        attacker.CheckAlive(); // attackerのカードの見た目を更新する
        defender.CheckAlive(); // defenderのカードの見た目を更新する
    }

    // HeroのHP表示を変更するメソッド
    void ShowHeroHP()
    {
        playerHeroHpText.text = playerHeroHp.ToString(); // プレイヤーのHeroのHPを表示するTextを変更
        enemyHeroHpText.text = enemyHeroHp.ToString(); // 敵のHeroのHPを表示するTextを変更
    }

    // Heroに攻撃するメソッド
    public void AttackToHero(CardController attacker, bool isPlayerCard)
    {
        // attackerがプレイヤーのカードだった場合
        if (isPlayerCard)
        {
            enemyHeroHp -= attacker.model.at; // 敵のHeroのHPを攻撃力分下げる
        }
        // attackerが敵のカードだった場合
        else
        {
            playerHeroHp -= attacker.model.at; // プレイヤーのHeroのHPを攻撃力分下げる
        }
        attacker.SetCanAttack(false); // 一度攻撃したらattackerを攻撃不可にする
        ShowHeroHP(); // HeroのHP表示を変更
        CheckHeroHP(); // HeroのHPが0以下になったかどうかを判定
    }

    // HeroのHPが0以下になったかどうかを判定→リザルト画面を表示
    void CheckHeroHP()
    {
        if (playerHeroHp <= 0 || enemyHeroHp <= 0) // HeroのHPが0以下になったら
        {
            resultPanel.SetActive(true); // リザルト画面を表示する
            if (playerHeroHp <= 0) // プレイヤーのHeroが倒されていたのなら
            {
                resultText.text = "LOSE"; // LOSEと表示する
            }
            else // 敵のHeroを倒したのなら
            {
                resultText.text = "WIN"; // WINと表示する
            }
        }
    }
}
