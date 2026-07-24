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
        SetCanAttack(true); // バトスピは召喚したターンでも攻撃できるので。
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

    // マジックカード
    public void UseMagicTo(CardController target)
    {
        switch (model.magic)
        {
            case MAGIC.DESTROY_ENEMY_CARD:
                // 特定の敵を攻撃する
                if (target == null) return;
                if (target.model.isPlayerCard == model.isPlayerCard) return;
                Attack(target);
                target.CheckAlive();
                break;
            case MAGIC.REFRESH_FRIEND_CARDS:
                // 自分のスピリットすべてを回復させる
                CardController[] playerCards = gameManager.GetPlayerFieldCards();
                foreach (CardController playerCard in playerCards)
                {
                    playerCard.ChangeIsRefreshed(true);
                }
                break;
            case MAGIC.NONE:
                return;
        }
        Destroy(this.gameObject); //スペルカード使用後は削除(本当はトラッシュに移動してから非表示にしたい)
    }
}
