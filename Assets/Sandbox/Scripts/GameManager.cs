using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] AI enemyAI; // 敵のAIを取得;
    [SerializeField] GameObject resultPanel; // ゲーム終了時に表示するパネルを取得
    [SerializeField] Text resultText;  // ゲーム終了時に表示するテキストを取得
    public Transform playerHandTransform, // プレイヤーの手札のTransformを取得
                               playerFieldTransform, // プレイヤーのフィールドのTransformを取得   
                               enemyHandTransform,  // 敵の手札のTransformを取得 
                               enemyFieldTransform; // 敵のフィールドのTransformを取得
    [SerializeField] CardController cardPrefab; // カードのPrefabをCardController型として取得

    public bool isPlayerTurn; // プレイヤーのターンかどうかを判定する変数

    List<int> playerDeck = new List<int>() {3, 1, 2, 2, 3}, // プレイヤーのデッキのカードIDを格納するリスト
              enemyDeck  = new List<int>() {2, 1, 3, 1, 3};  // 敵のデッキのカードIDを格納するリスト
    
    [SerializeField] Text playerHeroHpText; // プレイヤーのHeroのHPを表示するTextを取得
    [SerializeField] Text enemyHeroHpText; // 敵のHeroのHPを表示するTextを取得   

    int playerHeroHp = 30; // プレイヤーのHeroのHP
    int enemyHeroHp  = 30; // 敵のHeroのHP

    public Transform playerHero; // プレイヤーのHeroのTransform

    [SerializeField] Text playerManaCostText; // プレイヤーのマナコストを表示するTextを取得
    [SerializeField] Text enemyManaCostText; // 敵のマナコストを表示するTextを取得   

    public int playerManaCost = 30; // プレイヤーのマナコスト
    public int enemyManaCost  = 30; // 敵のマナコスト
    int playerDefaultManaCost; // プレイヤーのマナコストの初期値(ターンごとに増加)
    int enemyDefaultManaCost; // 敵のマナコストの初期値(ターンごとに増加)

    // 時間管理
    [SerializeField] Text timeCountText; // カウントダウンのTextを取得
    int timeCount; // 時間をカウントする変数

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
        playerHeroHp = 10; // プレイヤーのHeroのHPを10にする
        enemyHeroHp = 10; // 敵のHeroのHPを10にする
        playerManaCost = playerDefaultManaCost = 10; // プレイヤーのマナコストを1にする
        enemyManaCost = enemyDefaultManaCost = 10; // 敵のマナコストを1にする
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
        if (hand.name == "PlayerHand")
        {
            card.Init(cardID, true);    // CardControllerクラスのInit()メソッドを呼び出す(isPlayerはtrueで渡す)        
        }
        else
        {
            card.Init(cardID, false);    // CardControllerクラスのInit()メソッドを呼び出す(isPlayerはfalseで渡す)
        }
    }

    // ターン処理を行うメソッド
    void TurnCalc()
    {
        StopAllCoroutines(); // 安全のためにコルーチン開始前に他を止めておく
        StartCoroutine(CountDown()); // カウントダウンを開始
        if (isPlayerTurn)
        {
            // プレイヤーのターンの処理
            PlayerTurn();
        }
        else
        {
            // 敵のターンの処理
            StartCoroutine(enemyAI.EnemyTurn());
        }
    }

    // カウントダウンを表示するメソッド(コルーチンを使用)
    IEnumerator CountDown()
    {
        timeCount = 20; // カウントダウンの初期値を8にする
        timeCountText.text = timeCount.ToString(); // カウントダウンのTextを初期値にする

        // カウントが0秒になるまではコルーチンを回す
        while (timeCount > 0)
        {
            yield return new WaitForSeconds(1); // 1秒待機
            timeCount--; // カウント(秒数)を1減らす
            timeCountText.text = timeCount.ToString(); // カウントダウンのTextを更新する
        }
        ChangeTurn(); // カウントが0になったらターンを切り替える
    }

    // 敵のフィールドのカードを取得するメソッド
    public CardController[] GetEnemyFieldCards()
    {
        return enemyFieldTransform.GetComponentsInChildren<CardController>();
    }

    // ターンエンドボタンを押したときに呼ばれるメソッド
    public void OnClickTurnEndButton()
    {
        if (isPlayerTurn) // プレイヤーのターンのときだけターンを切り替える
        {
            ChangeTurn(); 
        }
    }

    // ターンを切り替えるメソッド
    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn; // ターンを切り替える

        CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(playerFieldCardList, false); // フィールドのカードの攻撃可能オーラを消す
        CardController[] enemyFieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(enemyFieldCardList, false); // フィールドのカードの攻撃可能オーラを消す

        if (isPlayerTurn)
        {
            playerDefaultManaCost++; // プレイヤーのターンになったらマナコストを1増やす
            playerManaCost = playerDefaultManaCost; // プレイヤーのマナコストに初期値を代入
            GiveCardToHand(playerDeck, playerHandTransform); // プレイヤーの手札にカードを1枚生成（ドロー）
        }
        else
        {
            enemyDefaultManaCost++; // 敵のターンになったらマナコストを1増やす
            enemyManaCost = enemyDefaultManaCost; // 敵のマナコストに初期値を代入
            GiveCardToHand(enemyDeck, enemyHandTransform);  // 敵の手札にカードを1枚生成（ドロー）
        }
        ShowManaCost(); // マナコストの表示を更新する
        TurnCalc(); // ターン処理を行うメソッドを呼び出す
    }

    // 攻撃可能オーラを付けたり消したりするメソッド
    public void SettingCanAttackView(CardController[] fieldCardList, bool canAttack)
    {
        foreach (CardController card in fieldCardList)
        {
            card.SetCanAttack(canAttack);    // cardを攻撃可能にするかどうか
        }
    }

    // プレイヤーのターンの処理を行うメソッド
    void PlayerTurn()
    {
        Debug.Log("Playerのターン");
        // フィールドのカードを攻撃可能にする
        CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(playerFieldCardList, true); // フィールドのカードに攻撃可能オーラを付ける
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
    }

    // HeroのHPが0以下になったかどうかを判定→リザルト画面を表示
    public void CheckHeroHP()
    {
        if (playerHeroHp <= 0 || enemyHeroHp <= 0) // HeroのHPが0以下になったら
        {
            ShowResultPanel(playerHeroHp);
            
        }
    }

    // リザルト画面を表示するメソッド
    void ShowResultPanel(int heroHp)
    {
        StopAllCoroutines(); // コルーチンを止める
        resultPanel.SetActive(true); // リザルト画面を表示する
        if (heroHp <= 0) // Heroが倒されていたのなら
        {
            resultText.text = "LOSE"; // LOSEと表示する
        }
        else // 敵のHeroを倒したのなら
        {
            resultText.text = "WIN"; // WINと表示する
        }        
    }
}
