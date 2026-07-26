using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GamePlayerManager player; 
    public GamePlayerManager enemy;
    public AI enemyAI; // 敵のAIを取得;
    [SerializeField] UIManager uiManager; // UIManagerを取得
    public Transform playerHandTransform, // プレイヤーの手札のTransformを取得
                               playerFieldTransform, // プレイヤーのフィールドのTransformを取得
                               enemyHandTransform,  // 敵の手札のTransformを取得
                               enemyFieldTransform, // 敵のフィールドのTransformを取得
                               playerReserveTransform,  // プレイヤーのリザーブのTransformを取得
                               enemyReserveTransform,   // 敵のリザーブのTransformを取得
                               playerTrashTransform,    // プレイヤーのトラッシュのTransformを取得
                               enemyTrashTransform,     // 敵のトラッシュのTransformを取得
                               playerLifeTransform,     // プレイヤーのライフのTransformを取得
                               enemyLifeTransform;      // 敵のライフのTransformを取得

    [SerializeField] CardController cardPrefab; // カードのPrefabをCardController型として取得
    [SerializeField] CoreController corePrefab; // コアのPrefabをCoreController型として取得

    public bool isPlayerTurn; // プレイヤーのターンかどうかを判定する変数
    public Transform playerHero; // プレイヤーのHeroのTransform
    public Transform enemyHero; // 敵のHeroのTransform

    public bool isWaitingForDefenderSelection; // 敵の攻撃時、プレイヤーの防御カード選択待ちかどうかを判定する変数
    public CardController selectedDefenderCard; // プレイヤーが防御カードとして選択したカード
    public bool heroWasClicked; // 敵の攻撃時、プレイヤーがHeroをクリックしたかどうか

    public bool isDuringAttack; // スピリットがアタック中かどうか

    public int turnCount = 0;

    public STEP step;

    public enum STEP
    {
        NONE,
        START,
        CORE,
        DRAW,
        REFRESH,
        MAIN,
        ATTACK,
        END
    }

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
        uiManager.HideResultPanel(); // ゲーム開始時はリザルト画面を非表示にする
        player.Init(new List<int>() { 11, 3, 8, 9, 2, 3, 7, 3, 1 }); // プレイヤーのデッキを初期化する
        enemy.Init(new List<int>() { 10, 4, 5, 9, 5, 8, 6, 4, 5, 4 }); // 敵のデッキを初期化する
        uiManager.ShowHeroHP(player.heroHp, enemy.heroHp); // HeroのHP表示を変更するメソッドを呼び出す
        uiManager.ShowManaCost(player.manaCost, enemy.manaCost); // マナコストの表示を変更するメソッドを呼び出す
        turnCount = 1;
        SettingInitHand();
        SettingInitCore();
        isPlayerTurn = true; // プレイヤーのターンから開始する
        TurnCalc(); // ターン処理を行うメソッドを呼び出す
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
        foreach (Transform core in playerReserveTransform)
        {
            Destroy(core.gameObject);
        }
        foreach (Transform core in enemyReserveTransform)
        {
            Destroy(core.gameObject);
        }
        foreach (Transform core in playerTrashTransform)
        {
            Destroy(core.gameObject);
        }
        foreach (Transform core in enemyTrashTransform)
        {
            Destroy(core.gameObject);
        }
        foreach (Transform core in playerLifeTransform)
        {
            Destroy(core.gameObject);
        }
        foreach (Transform core in enemyLifeTransform)
        {
            Destroy(core.gameObject);
        }

        // デッキを生成
        player.deck = new List<int>() { 0, 1, 2, 3, 3, 1 }; // プレイヤーのデッキのカードIDを格納するリスト
        enemy.deck  = new List<int>() { 4, 5, 6, 6, 4 };  // 敵のデッキのカードIDを格納するリスト

        StartGame(); // ゲーム開始時に呼ばれるメソッドを呼び出す
    }

    // ゲーム開始時に手札を初期化するメソッド
    void SettingInitHand()
    {
        // カードをそれぞれに3枚配る
        for (int i = 0; i < 4; i++)
        {
            GiveCardToHand(player.deck, playerHandTransform); // プレイヤーの手札にカードを生成
            GiveCardToHand(enemy.deck, enemyHandTransform);  // 敵の手札にカードを生成
        }
    }

    // デッキからカードを手札に配るメソッド
    public void GiveCardToHand(List<int> deck, Transform hand)
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

    // ゲーム開始時にリザーブとライフのコアを初期化するメソッド
    void SettingInitCore()
    {
        CreateCore(playerReserveTransform, 4);
        CreateCore(enemyReserveTransform, 4);
        CreateCore(playerLifeTransform, 5);
        CreateCore(enemyLifeTransform, 5);        
    }
    
    // 任意の位置に任意の数のコアを生成するメソッド
    public void CreateCore(Transform place, int coreNum)
    {
        for (int i = 0; i < coreNum; i++)
        {
            CoreController core = Instantiate(corePrefab, place, false);
        }
    }

    // ターン処理を行うメソッド
    void TurnCalc()
    {
        StopAllCoroutines(); // 安全のためにコルーチン開始前に他を止めておく
        // StartCoroutine(CountDown()); // カウントダウンを開始
        if (turnCount == 1)
        {
            StepCalc(isPlayerTurn, STEP.START);
            StepCalc(isPlayerTurn, STEP.DRAW);
            StepCalc(isPlayerTurn, STEP.MAIN);
        }
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

    // ステップ処理を行うメソッド
    public void StepCalc(bool isPlayerTurn, STEP step)
    {
        switch (step)
        {
            case STEP.START:
                Debug.Log(isPlayerTurn ? "Playerのターン" : "Enemyのターン");
                Debug.Log("ターン" + turnCount);
                Debug.Log("スタートステップ！");
                this.step = STEP.START;
                break;
            case STEP.CORE:
                Debug.Log("コアステップ！");
                this.step = STEP.CORE;
                if (isPlayerTurn) CreateCore(playerReserveTransform, 1);
                else CreateCore(enemyReserveTransform, 1);
                break;
            case STEP.DRAW:
                Debug.Log("ドローステップ！");
                this.step = STEP.DRAW;
                if (isPlayerTurn) GiveCardToHand(player.deck, playerHandTransform);
                else GiveCardToHand(enemy.deck, enemyHandTransform);
                break;
            case STEP.REFRESH:
                Debug.Log("リフレッシュステップ！");
                this.step = STEP.REFRESH;
                // フィールドのカードを全て攻撃可能にする
                CardController[] fieldCards = GetFriendFieldCards(isPlayerTurn);
                SettingCanAttackView(fieldCards, true); // フィールドのカードに攻撃可能オーラを付ける
                //フィールドのカードを全て回復状態にする
                foreach (CardController fieldCard in fieldCards)
                {
                    fieldCard.ChangeIsRefreshed(true);
                }
                if (isPlayerTurn)
                {
                    CoreController[] playerTrashCoreList = playerTrashTransform.GetComponentsInChildren<CoreController>();
                    // トラッシュのコアを全てリザーブに移動
                    foreach (CoreController core in playerTrashCoreList)
                    {
                        // GameManagerではなくcore自身にコルーチンを紐付ける（直後のTurnCalc()内のStopAllCoroutines()で移動が中断されないようにするため）
                        core.StartCoroutine(core.movement.MoveTo(playerReserveTransform)); // コアをリザーブへ移動
                    }
                }
                else
                {
                    CoreController[] enemyTrashCoreList = enemyTrashTransform.GetComponentsInChildren<CoreController>();
                    // トラッシュのコアを全てリザーブに移動
                    foreach (CoreController core in enemyTrashCoreList)
                    {
                        core.StartCoroutine(core.movement.MoveTo(enemyReserveTransform)); // コアをリザーブへ移動
                    }
                }
                break;
            case STEP.MAIN:
                Debug.Log("メインステップ！");
                this.step = STEP.MAIN;
                uiManager.ChangeButtonText();
                break;
            case STEP.ATTACK:
                Debug.Log("アタックステップ！");
                this.step = STEP.ATTACK;
                uiManager.ChangeButtonText();
                break;
            case STEP.END:
                Debug.Log("ターンエンド");
                this.step = STEP.END;
                turnCount++;
                ChangeTurn();
                break;
        }
    }

    // カウントダウンを表示するメソッド(コルーチンを使用)
    IEnumerator CountDown()
    {
        timeCount = 20; // カウントダウンの初期値を8にする
        uiManager.UpdateTime(timeCount); // カウントダウンのTextを更新する

        // カウントが0秒になるまではコルーチンを回す
        while (timeCount > 0)
        {
            yield return new WaitForSeconds(1); // 1秒待機
            timeCount--; // カウント(秒数)を1減らす
            uiManager.UpdateTime(timeCount); // カウントダウンのTextを更新する
        }
        ChangeTurn(); // カウントが0になったらターンを切り替える
    }

    // 自分のフィールドのカード(プレイヤー→プレイヤー, 敵AI→敵AI)を取得するメソッド
    public CardController[] GetFriendFieldCards(bool isPlayer)
    {
        if (isPlayer)
        {
            return playerFieldTransform.GetComponentsInChildren<CardController>();
        }
        else
        {
            return enemyFieldTransform.GetComponentsInChildren<CardController>();
        }
    }
    
    // 相手のフィールドのカード(プレイヤー→敵AI, 敵AI→プレイヤー)を取得するメソッド
    public CardController[] GetOpponentFieldCards(bool isPlayer)
    {
        if (isPlayer)
        {
            return enemyFieldTransform.GetComponentsInChildren<CardController>();
        }
        else
        {
            return playerFieldTransform.GetComponentsInChildren<CardController>();
        }
    }

    // ターンエンドボタンを押したときに呼ばれるメソッド
    public void OnClickTurnEndButton()
    {
        if (isPlayerTurn) // プレイヤーのターンのときだけターンを切り替える
        {
            if (uiManager.buttonText.text == "TurnEnd")
            {
                StepCalc(isPlayerTurn, STEP.END);
            }
            if (uiManager.buttonText.text == "AttackStep")
            {
                StepCalc(isPlayerTurn, STEP.ATTACK);
            }
            // ChangeTurn();
        }
    }

    /*
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
            // player.IncreaseManaCost(); // プレイヤーのターンになったらマナコストを1増やす
            player.IncreaseManaCost(playerFieldCardList.Length); // 今は各カードに乗せるコアは1つずつなのでリストの長さを引数に渡す
            CreateCore(playerReserveTransform, 1); // プレイヤーのコアを1つ生成する
            GiveCardToHand(player.deck, playerHandTransform); // プレイヤーの手札にカードを1枚生成（ドロー）
        }
        else
        {
            // enemy.IncreaseManaCost(); // 敵のターンになったらマナコストを1増やす
            enemy.IncreaseManaCost(enemyFieldCardList.Length);
            CreateCore(enemyReserveTransform, 1); // 敵のコアを1つ生成する
            GiveCardToHand(enemy.deck, enemyHandTransform);  // 敵の手札にカードを1枚生成（ドロー）
        }
        uiManager.ShowManaCost(player.manaCost, enemy.manaCost); // マナコストの表示を更新する
        TurnCalc(); // ターン処理を行うメソッドを呼び出す
    }
    */

    // ターンを切り替えるメソッド
    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn; // ターンを切り替える

        /*
        CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(playerFieldCardList, false); // フィールドのカードの攻撃可能オーラを消す
        CardController[] enemyFieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(enemyFieldCardList, false); // フィールドのカードの攻撃可能オーラを消す
        */

        StepCalc(isPlayerTurn, STEP.START);
        StepCalc(isPlayerTurn, STEP.CORE);
        StepCalc(isPlayerTurn, STEP.DRAW);
        StepCalc(isPlayerTurn, STEP.REFRESH);
        StepCalc(isPlayerTurn, STEP.MAIN);

        // CoreController[] playerFieldCoreList = playerFieldTransform.GetComponentsInChildren<CoreController>();
        // CoreController[] playerReserveCoreList = playerReserveTransform.GetComponentsInChildren<CoreController>();
        // CoreController[] playerTrashCoreList = playerTrashTransform.GetComponentsInChildren<CoreController>();
        // CoreController[] enemyFieldCoreList = enemyFieldTransform.GetComponentsInChildren<CoreController>();
        // CoreController[] enemyReserveCoreList = enemyReserveTransform.GetComponentsInChildren<CoreController>();
        // CoreController[] enemyTrashCoreList = enemyTrashTransform.GetComponentsInChildren<CoreController>();

        // Debug.Log("playerFieldCoreList.Length:" + playerFieldCoreList.Length);
        if (isPlayerTurn)
        {
            // player.IncreaseManaCost(); // プレイヤーのターンになったらマナコストを1増やす
            // player.IncreaseManaCost(playerFieldCoreList.Length);
            // CreateCore(playerReserveTransform, 1); // プレイヤーのコアを1つ生成する
            // GiveCardToHand(player.deck, playerHandTransform); // プレイヤーの手札にカードを1枚生成（ドロー）
            
            /*
            // トラッシュのコアを全てリザーブに移動
            foreach (CoreController core in playerTrashCoreList)
            {
                // GameManagerではなくcore自身にコルーチンを紐付ける（直後のTurnCalc()内のStopAllCoroutines()で移動が中断されないようにするため）
                core.StartCoroutine(core.movement.MoveTo(playerReserveTransform)); // コアをリザーブへ移動
            }
            */
        }
        else
        {
            // enemy.IncreaseManaCost(); // 敵のターンになったらマナコストを1増やす
            // enemy.IncreaseManaCost(enemyFieldCoreList.Length);
            // CreateCore(enemyReserveTransform, 1); // 敵のコアを1つ生成する
            // GiveCardToHand(enemy.deck, enemyHandTransform);  // 敵の手札にカードを1枚生成（ドロー）

            /*
            // トラッシュのコアを全てリザーブに移動
            foreach (CoreController core in enemyTrashCoreList)
            {
                core.StartCoroutine(core.movement.MoveTo(enemyReserveTransform)); // コアをリザーブへ移動
            }
            */
        }
        // uiManager.ShowManaCost(player.manaCost, enemy.manaCost); // マナコストの表示を更新する
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
        /*
        // フィールドのカードを攻撃可能にする
        CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(playerFieldCardList, true); // フィールドのカードに攻撃可能オーラを付ける

        //フィールドのカードを全て回復状態にする
        foreach (CardController playerFieldCard in playerFieldCardList)
        {
            playerFieldCard.ChangeIsRefreshed(true);
        }
        */
    }

    // 軽減シンボルを加味した正味コストを計算するメソッド
    public int CalcNetCost(CardController card)
    {
        int netCost = card.model.cost;
        int fieldSymbols = 0;

        if (card.model.isPlayerCard)
        {
            // フィールドの総シンボルを計算(パターン1：symbolsを合算)
            CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();

            foreach (CardController cards in playerFieldCardList)
            {
                if (cards == card) continue; // 召喚中のカード自身のシンボルはカウントしない
                fieldSymbols += cards.model.symbols;
            }
        }
        else
        {
            CardController[] enemyFieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();

            foreach (CardController cards in enemyFieldCardList)
            {
                if (cards == card) continue; // 召喚中のカード自身のシンボルはカウントしない
                fieldSymbols += cards.model.symbols;
            }
        }
        Debug.Log("フィールドのシンボルの数:" + fieldSymbols);
        
        if (fieldSymbols > card.model.reductionSymbols)
        {
            fieldSymbols = card.model.reductionSymbols;
        }

        netCost -= fieldSymbols;

        Debug.Log(card.model.cost + "コスト" + fieldSymbols + "軽減" + netCost + "コスト");

        return netCost;
    }

    // マナコストを消費するメソッド
    public void ReduceManaCost(int cost, bool isPlayerCard)
    {
        if (isPlayerCard)
        {
            player.manaCost -= cost; // プレイヤーのマナコストを消費する
        }
        else
        {
            enemy.manaCost -= cost; // 敵のマナコストを消費する
        }
        uiManager.ShowManaCost(player.manaCost, enemy.manaCost); // マナコストの表示を変更するメソッドを呼び出す
    }

    // バトスピ用（オーバーロード）
    public void ReduceManaCost(CardController card)
    {
        int netCost = CalcNetCost(card);
        if (card.model.isPlayerCard)
        {
            player.manaCost -= netCost;
            player.manaCost--; // 維持コア1個分のマナコストを消費する
            // card.model.coreNum++; // カード上のコアの数を1増やす

            // コアの移動コルーチンが全て終わってからコア0チェックを行う
            StartCoroutine(MoveCoresAndCheckCoreZero(card));
        }
        else
        {
            enemy.manaCost -= netCost;
            enemy.manaCost--; // 維持コア1個分のマナコストを消費する
            // card.model.coreNum++; // カード上のコアの数を1増やす

            // 支払った分（維持コアを除く）のコアをリザーブからトラッシュへ移動する
            CoreController[] reserveCoreList = enemyReserveTransform.GetComponentsInChildren<CoreController>();
            for (int i = 0; i < netCost && i < reserveCoreList.Length; i++)
            {
                CoreController core = reserveCoreList[reserveCoreList.Length - 1 - i];
                core.StartCoroutine(core.movement.MoveTo(enemyTrashTransform));
            }
            // Lv・BPの再計算は、維持コアがカードの子になった後にAI側で呼び出す
        }
        uiManager.ShowManaCost(player.manaCost, enemy.manaCost); // マナコストの表示を変更するメソッドを呼び出す
    }
    
    // MoveCores()が開始したコア移動コルーチンが全て終わってから、コアが0個のカードをチェックするコルーチン
    private IEnumerator MoveCoresAndCheckCoreZero(CardController card)
    {
        List<Coroutine> coreMoveCoroutines = MoveCores(card);
        foreach (Coroutine coreMoveCoroutine in coreMoveCoroutines)
        {
            yield return coreMoveCoroutine; // MoveCores()が開始したコア移動コルーチンが全て終わるのを待つ
        }
        CheckIfCoreZero(card); // コアが0個のカードをチェック
        
        yield return new WaitForEndOfFrame();

        ArrangeCoresAndFixLv(GetFriendFieldCards(card.model.isPlayerCard));
    }

    // コスト支払い時のコアの移動処理
    // 呼び出し元でコルーチンの完了を待てるように、開始したコルーチンを全て返す
    public List<Coroutine> MoveCores(CardController card)
    {
        List<Coroutine> coreMoveCoroutines = new List<Coroutine>();

        // リザーブのコアを取得
        CoreController[] reserveCoreList = playerReserveTransform.GetComponentsInChildren<CoreController>();

        int netCost = CalcNetCost(card); // 正味コスト
        Debug.Log("netCost:" +  netCost);
        Debug.Log("reserveCoreList.Length:" +  reserveCoreList.Length);

        // 召喚したカードにリザーブからコアを移動
        if (netCost < reserveCoreList.Length)
        {
            Debug.Log("召喚コストと維持コアをリザーブから支払える場合");

            // 支払った分（維持コアを除く）のコアをリザーブからトラッシュへ移動する
            for (int i = 0; i < netCost && i < reserveCoreList.Length; i++)
            {
                CoreController costCore = reserveCoreList[reserveCoreList.Length - 1 - i];
                // GameManagerではなくcore自身にコルーチンを紐付ける（GameManager.StopAllCoroutines()の影響を受けないようにするため）
                coreMoveCoroutines.Add(costCore.StartCoroutine(costCore.movement.MoveTo(playerTrashTransform)));
            }

            // リザーブのコアのリストを更新
            reserveCoreList = playerReserveTransform.GetComponentsInChildren<CoreController>();

            Debug.Log("reserveCoreList.Length:" +  reserveCoreList.Length);
            
            if (card.model.cardType == CARDTYPE.SPIRIT)
            {
                // 維持コア1個乗せる
                CoreController core = reserveCoreList[reserveCoreList.Length - 1];
                // coreMoveCoroutines.Add(StartCoroutine(core.movement.MoveTo(card.transform)));
                // Vector2 offset = CoreMovement.GetRadialOffset(0, 1);
                // coreMoveCoroutines.Add(StartCoroutine(core.movement.MoveTo(card.transform, offset))); // ちょい下にずらす
                
                // coreMoveCoroutines.Add(StartCoroutine(core.movement.MoveTo(card.transform, new Vector2(0f, -40f)))); // ちょい下にずらす

                // 維持コアリザーブのコア全部乗せてみる
                for (int i = 0; i < reserveCoreList.Length; i++)
                {
                    // とりあえずリザーブにある分は全部カードに乗せる
                    // Vector2 offset = CoreMovement.GetRadialOffset(i, reserveCoreList.Length);
                    coreMoveCoroutines.Add(StartCoroutine(reserveCoreList[i].movement.MoveTo(card.iconTransform)));
                }

                CardController[] fieldCards = GetFriendFieldCards(card.model.isPlayerCard);

                // (7コスト以上のカードを召喚する場合は、なるべくLv2に上げる)
                if (card.model.cost > 6)
                {
                    // 周りを全て消滅させる
                    List<CoreController> coresToMove = new List<CoreController>();
                    for (int i = 0; i < fieldCards.Length; i++)
                    {
                        CoreController[] onCores = fieldCards[i].GetComponentsInChildren<CoreController>();
                        for (int j = 0; j < onCores.Length; j++)
                        {
                            coresToMove.Add(onCores[j]);
                        }
                    }
                    for (int i = 0; i < coresToMove.Count; i++)
                    {
                        // Vector2 offset = CoreMovement.GetRadialOffset(i, coresToMove.Count);
                        coreMoveCoroutines.Add(StartCoroutine(coresToMove[i].movement.MoveTo(card.iconTransform)));
                    }
                }                
            } 
        }
        else if (netCost == reserveCoreList.Length)
        {
            // 召喚コストはリザーブから支払えるが、維持コアはフィールドから支払う必要がある場合
            Debug.Log("召喚コストはリザーブから支払えるが、維持コアはフィールドから支払う必要がある場合");

            // 支払った分（維持コアを除く）のコアをリザーブからトラッシュへ移動する
            for (int i = 0; i < netCost && i < reserveCoreList.Length; i++)
            {
                CoreController costCore = reserveCoreList[reserveCoreList.Length - 1 - i];
                // GameManagerではなくcore自身にコルーチンを紐付ける（GameManager.StopAllCoroutines()の影響を受けないようにするため）
                coreMoveCoroutines.Add(costCore.StartCoroutine(costCore.movement.MoveTo(playerTrashTransform)));
            }

            CardController[] fieldCards = GetFriendFieldCards(card.model.isPlayerCard);;

            if (card.model.cardType == CARDTYPE.SPIRIT)
            {
                /*
                // フィールドの一番左のカードのコアを維持コアとして使用
                CoreController core = fieldCards[0].GetComponentInChildren<CoreController>();
                coreMoveCoroutines.Add(StartCoroutine(core.movement.MoveTo(card.transform, new Vector2(0f, -40f))));
                */

                // フィールドのカードのコアが各1個の場合は、やむなくフィールドの一番左のカードのコアに乗っているコア全て(仮)を維持コアとして使用
                // CoreController[] cores = fieldCards[0].GetComponentsInChildren<CoreController>();
                // coreMoveCoroutines.Add(StartCoroutine(cores[0].movement.MoveTo(card.transform, new Vector2(0f, -40f))));
                // Debug.Log("1個移動, cores.Length:" + cores.Length);

                /*
                for (int i = 0; i < cores.Length - 1; i++)
                {
                    Vector2 offset = CoreMovement.GetRadialOffset(i, cores.Length - 1);
                    coreMoveCoroutines.Add(StartCoroutine(cores[i].movement.MoveTo(card.transform, offset)));
                    Debug.Log("cores[" + i + "]移動");
                }
                */

                Debug.Log("フィールドのカード:" + fieldCards.Length + "枚");
                // (7コスト以上のカードを召喚する場合は、フィールドのコアを全て乗せる)
                if (card.model.cost > 6)
                {
                    // 周りを全て消滅させる
                    List<CoreController> coresToMove = new List<CoreController>();
                    for (int i = 0; i < fieldCards.Length; i++)
                    {
                        CoreController[] onCores = fieldCards[i].GetComponentsInChildren<CoreController>();
                        for (int j = 0; j < onCores.Length; j++)
                        {
                            coresToMove.Add(onCores[j]);
                        }
                    }
                    for (int i = 0; i < coresToMove.Count; i++)
                    {
                        Vector2 offset = CoreMovement.GetRadialOffset(i, coresToMove.Count);
                        coreMoveCoroutines.Add(StartCoroutine(coresToMove[i].movement.MoveTo(card.iconTransform, offset)));
                    }
                }
                else // それ以外のザコカードの場合
                {
                    int onMaxCoreNum = 1;
                    int onMaxCoreIndex = 0;
                    // 一番乗っているコアの数が多いカードを特定
                    for (int i = 0; i < fieldCards.Length; i++)
                    {
                        CoreController[] onCores = fieldCards[i].GetComponentsInChildren<CoreController>();
                        Debug.Log("fieldCards[" + i + "]上のコアの数:" + onCores.Length + "個");
                        if (onCores.Length > onMaxCoreNum)
                        {
                            onMaxCoreNum = onCores.Length;
                            onMaxCoreIndex = i;
                        }
                    }
                    CoreController[] moveCores = fieldCards[onMaxCoreIndex].GetComponentsInChildren<CoreController>();
                    coreMoveCoroutines.Add(StartCoroutine(moveCores[0].movement.MoveTo(card.iconTransform)));
                }                
            }
        }
        else
        {
            // 召喚コストをリザーブとフィールドから支払い、維持コアもフィールドから支払う場合
            Debug.Log("召喚コストをリザーブとフィールドから支払い、維持コアもフィールドから支払う場合");

            foreach (CoreController reserveCores in reserveCoreList)
            {
                // とりあえずリザーブにある分は全部トラッシュに支払う
                coreMoveCoroutines.Add(StartCoroutine(reserveCores.movement.MoveTo(playerTrashTransform)));
            }
            // 不足コスト
            int lackCoreNum = netCost - reserveCoreList.Length;
            Debug.Log("不足コスト：" + lackCoreNum);
            CardController[] fieldCards = GetFriendFieldCards(card.model.isPlayerCard);;
            
            /*
            // コアをいくつフィールドから支払うか
            for (int i = 0; i < lackCoreNum; i++)
            {
                CoreController lackCore = fieldCards[i].GetComponentInChildren<CoreController>();
                coreMoveCoroutines.Add(StartCoroutine(lackCore.movement.MoveTo(playerTrashTransform)));
                // fieldCards = GetPlayerFieldCards(); // カードリストを更新
            }
            Debug.Log("不足コストは" + fieldCards[lackCoreNum].model.name + "から確保");
            CoreController core = fieldCards[lackCoreNum].GetComponentInChildren<CoreController>();
            coreMoveCoroutines.Add(StartCoroutine(core.movement.MoveTo(card.transform)));
            */
            
            // 不足分を、フィールドの各カードから順番に（コアが足りなければ次のカードから）支払う
            int lackRemaining = lackCoreNum;
            foreach (CardController fieldCard in fieldCards)
            {
                if (lackRemaining <= 0) break;

                CoreController[] onCores = fieldCard.GetComponentsInChildren<CoreController>();
                int payNum = Mathf.Min(lackRemaining, onCores.Length);
                for (int i = 0; i < payNum; i++)
                {
                    coreMoveCoroutines.Add(StartCoroutine(onCores[i].movement.MoveTo(playerTrashTransform)));
                }
                lackRemaining -= payNum;
            }

            Debug.Log("フィールドのカード:" + fieldCards.Length + "枚");

            if (card.model.cardType == CARDTYPE.SPIRIT)
            {
                // (7コスト以上のカードを召喚する場合は、フィールドのコアを全て乗せる)
                if (card.model.cost > 6)
                {
                    // 周りを全て消滅させる
                    List<CoreController> coresToMove = new List<CoreController>();
                    for (int i = 0; i < fieldCards.Length; i++)
                    {
                        CoreController[] onCores = fieldCards[i].GetComponentsInChildren<CoreController>();
                        for (int j = 0; j < onCores.Length; j++)
                        {
                            coresToMove.Add(onCores[j]);
                        }
                    }
                    for (int i = 0; i < coresToMove.Count; i++)
                    {
                        Vector2 offset = CoreMovement.GetRadialOffset(i, coresToMove.Count);
                        coreMoveCoroutines.Add(StartCoroutine(coresToMove[i].movement.MoveTo(card.iconTransform, offset)));
                    }
                }
                else
                {
                    // フィールドのカードのコアが各何個か調べ、一番多く乗っているところから1個移動
                    int onMaxCoreNum = 0;
                    int onMaxCoreIndex = 0;
                    // 一番乗っているコアの数が多いカードを特定
                    for (int i = 0; i < fieldCards.Length; i++)
                    {
                        CoreController[] onCores = fieldCards[i].GetComponentsInChildren<CoreController>();
                        Debug.Log("fieldCards[" + i + "]上のコアの数:" + onCores.Length + "個");
                        if (onCores.Length > onMaxCoreNum)
                        {
                            onMaxCoreNum = onCores.Length;
                            onMaxCoreIndex = i;
                        }
                    }
                    CoreController[] moveCores = fieldCards[onMaxCoreIndex].GetComponentsInChildren<CoreController>();
                    coreMoveCoroutines.Add(StartCoroutine(moveCores[0].movement.MoveTo(card.iconTransform)));
                }                
            }
        }

        return coreMoveCoroutines;
    }

    public void CheckIfCoreZero(CardController card)
    {
        CardController[] friendFieldCards = GetFriendFieldCards(card.model.isPlayerCard);
        for (int i = 0; i < friendFieldCards.Length; i++)
        {
            CoreController[] cores = friendFieldCards[i].GetComponentsInChildren<CoreController>(); // カードに乗っているコアを取得
            if (cores.Length == 0)
            {
                Destroy(friendFieldCards[i].gameObject);
            }
        }
    }

    // コアの配置とLv・BPを更新するメソッド
    public void ArrangeCoresAndFixLv(CardController[] fieldCards)
    {
        for (int i = 0; i < fieldCards.Length; i++)
        {
            CoreController[] onCores = fieldCards[i].GetComponentsInChildren<CoreController>();
            // 維持コア0で消滅、filedCards再整列後に呼びたい
            // GridLayoutGroup追加により、コアの整列は必要なくなったためコメントアウト
            // for (int j = 0; j < onCores.Length; j++)
            // {
                // Vector2 offset = CoreMovement.GetRadialOffset(j, onCores.Length);
                // coreMoveCoroutines.Add(StartCoroutine(onCores[j].movement.MoveTo(card.transform, offset)));
                // StartCoroutine(onCores[j].movement.MoveTo(fieldCards[i].iconTransform));
            // }

            Debug.Log(onCores.Length + "個");
            if (onCores.Length >= fieldCards[i].model.coreLv1)
            {
                fieldCards[i].model.currentLv = 1;
                fieldCards[i].model.currentBp = fieldCards[i].model.bpLv1; // コアが減ってたらLv1だもんね。
                if (onCores.Length >= fieldCards[i].model.coreLv2)
                {
                    fieldCards[i].model.currentLv = 2;
                    fieldCards[i].model.currentBp = fieldCards[i].model.bpLv2;
                    if (fieldCards[i].model.coreLv3 != 0 && onCores.Length >= fieldCards[i].model.coreLv3)
                    {
                        fieldCards[i].model.currentLv = 3;
                        fieldCards[i].model.currentBp = fieldCards[i].model.bpLv3;
                    }
                }
            }
            fieldCards[i].model.FixBp();
            Debug.Log(fieldCards[i].model.name + " Lv:" + fieldCards[i].model.currentLv + " BP:" + fieldCards[i].model.currentBp);
        }
        
    }

    // 敵の攻撃時、プレイヤーが防御カードとしてクリックしたカードを選択するメソッド
    public void SelectDefenderCard(CardController card)
    {
        if (!isWaitingForDefenderSelection) return; // 防御カード選択待ちでなければ何もしない
        if (!card.model.isPlayerCard || !card.model.isRefreshed) return; // プレイヤーの回復状態のカード以外は選択できない

        // シールドカードがあれば、シールドカード以外は選択できない
        CardController[] playerFieldCards = GetFriendFieldCards(card.model.isPlayerCard);
        if (Array.Exists(playerFieldCards, c => c.model.ability == ABILITY.SHIELD) && card.model.ability != ABILITY.SHIELD)
        {
            return;
        }

        selectedDefenderCard = card; // 選択されたカードを防御カードとして記録
    }

    // 敵の攻撃時、プレイヤーがHeroをクリックしたときに呼ばれるメソッド
    public void SelectHeroAsTarget()
    {
        if (!isWaitingForDefenderSelection) return; // 防御カード選択待ちでなければ何もしない

        heroWasClicked = true; // Heroがクリックされたことを記録
    }

    public void CardsBattle(CardController attacker, CardController defender)
    {
        Debug.Log("CardsBattle");
        // Debug.Log("attacker HP:" + attacker.model.hp);
        // Debug.Log("defender HP:" + defender.model.hp);
        attacker.model.FixBp();
        defender.model.FixBp();
        Debug.Log("attacker:" + attacker.model.name + " Lv" + attacker.model.currentLv + " BP:" + attacker.model.currentBp);
        Debug.Log("defender:" + defender.model.name + " Lv" + defender.model.currentLv + " BP:" + defender.model.currentBp);

        attacker.Attack(defender); // attackerの攻撃力分のダメージをdefenderに与える
        defender.Attack(attacker); // defenderの攻撃力分のダメージをattackerに与える

        attacker.ChangeIsRefreshed(false);
        defender.ChangeIsRefreshed(false);
        
        // Debug.Log("attacker HP:" + attacker.model.hp);
        // Debug.Log("defender HP:" + defender.model.hp);
        attacker.CheckAlive(); // attackerのカードの見た目を更新する
        defender.CheckAlive(); // defenderのカードの見た目を更新する
    }

    public void OnDestroyed(bool isPlayerCard, CoreController[] cores)
    {
        Transform reserve;
        if (isPlayerCard)
        {
            player.manaCost++;
            reserve = playerReserveTransform;
        }
        else
        {
            enemy.manaCost++;
            reserve = enemyReserveTransform;
        }

        foreach (CoreController core in cores)
        {
            StartCoroutine(core.movement.MoveTo(reserve)); // コアをリザーブへ移動
        }

        uiManager.ShowManaCost(player.manaCost, enemy.manaCost); // マナコストの表示を更新する
    }

    // Heroに攻撃するメソッド
    public void AttackToHero(CardController attacker)
    {
        CoreController[] playerLifeCoreList = playerLifeTransform.GetComponentsInChildren<CoreController>();
        CoreController[] enemyLifeCoreList = enemyLifeTransform.GetComponentsInChildren<CoreController>();
        // attackerがプレイヤーのカードだった場合
        if (attacker.model.isPlayerCard)
        {
            enemy.heroHp -= attacker.model.symbols; // 敵のHeroのライフをシンボル分下げる
            // enemy.IncreaseManaCost(); // ライフで受けたコアをリザーブに移動
            enemy.manaCost++; // リザーブを1増やす
            enemy.defaultManaCost++; // コアの総数も1増やす
            // enemyLifeCoreList[enemyLifeCoreList.Length - 1].StartCoroutine(enemyLifeCoreList[enemyLifeCoreList.Length - 1].movement.MoveTo(enemyReserveTransform)); // コアをリザーブへ移動            
            for (int i = 0; i < attacker.model.symbols; i++)
            {
                Destroy(enemyLifeCoreList[enemyLifeCoreList.Length - 1 - i].gameObject); // 破壊    
            }
            CreateCore(enemyReserveTransform, attacker.model.symbols); // 生成
        }
        // attackerが敵のカードだった場合
        else
        {
            player.heroHp -= attacker.model.symbols; // プレイヤーのライフをシンボル分下げる
            // player.IncreaseManaCost(); // ライフで受けたコアをリザーブに移動
            player.manaCost++; // リザーブを1増やす
            player.defaultManaCost++; // コアの総数も1増やす
            // playerLifeCoreList[playerLifeCoreList.Length - 1].StartCoroutine(playerLifeCoreList[playerLifeCoreList.Length - 1].movement.MoveTo(playerReserveTransform)); // コアをリザーブへ移動
            for (int i = 0; i < attacker.model.symbols; i++)
            {
                Destroy(playerLifeCoreList[playerLifeCoreList.Length - 1 - i].gameObject); // 破壊
            }
            CreateCore(playerReserveTransform, attacker.model.symbols); // 生成
        }
        // attacker.SetCanAttack(false); // 一度攻撃したらattackerを攻撃不可にする
        attacker.ChangeIsRefreshed(false);
        uiManager.ShowHeroHP(player.heroHp, enemy.heroHp); // HeroのHP表示を変更
        uiManager.ShowManaCost(player.manaCost, enemy.manaCost); // マナコストの表示を更新する
    }

    // HeroのHPが0以下になったかどうかを判定→リザルト画面を表示
    public void CheckHeroHP()
    {
        if (player.heroHp <= 0 || enemy.heroHp <= 0) // HeroのHPが0以下になったら
        {
            ShowResultPanel(player.heroHp);
        }
    }

    // リザルト画面を表示するメソッド
    void ShowResultPanel(int heroHp)
    {
        StopAllCoroutines(); // コルーチンを止める
        uiManager.ShowResultPanel(heroHp); // リザルト画面を表示するメソッドを呼び出す
    }
}