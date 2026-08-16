using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

// カードのPrefabにアタッチ
// カード側の動きを制御するクラス
public class CardMovement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Transform defaultParent; // カードの親の位置を保存する変数

    public bool isDraggable; // カードがドラッグ可能かどうかを判定する変数
    
    // ドラッグ開始時に呼び出されるメソッド
    public void OnBeginDrag(PointerEventData eventData)
    {
        // カードのコストとPlayerのManaコストを比較
        CardController card = GetComponent<CardController>();

        // フィールドのコアの総数を取得
        int filedCoreNum = 0;
        CardController[] playerFieldCards = GameManager.instance.playerFieldTransform.GetComponentsInChildren<CardController>();
        for (int i = 0; i < playerFieldCards.Length; i++)
        {
            CoreController[] cores = playerFieldCards[i].GetComponentsInChildren<CoreController>(); // カードに乗っているコアを取得
            filedCoreNum += cores.Length;
        }
        Debug.Log("フィールドのコアの総数：" + filedCoreNum);

        // リザーブのコアを取得
        CoreController[] reserveCoreList = GameManager.instance.playerReserveTransform.GetComponentsInChildren<CoreController>();


        // フィールドのカードじゃない場合＝手札のカードの場合
        if (card.model.isPlayerCard
            && GameManager.instance.isPlayerTurn
            && GameManager.instance.step == GameManager.STEP.MAIN
            && !card.model.isFieldCard
            && (card.model.cardType == CARDTYPE.SPIRIT || card.model.cardType == CARDTYPE.BRAVE)
            && GameManager.instance.CalcNetCost(card) < reserveCoreList.Length + filedCoreNum)
        {
            isDraggable = true; // カードのNet(正味)コストがPlayerのManaコスト+フィールドのコアの総数"未満"ならドラッグ可能(維持コアが1個以上必要なため)
        }
        else if (card.model.isPlayerCard
                 && GameManager.instance.isPlayerTurn
                 && GameManager.instance.step == GameManager.STEP.MAIN
                 && card.model.isFieldCard
                 && card.model.cardType == CARDTYPE.BRAVE)
        {
            isDraggable = true; // 合体(ブレイヴ)させるためならドラッグ可能
        }
        else if (card.model.isPlayerCard
                 && GameManager.instance.isPlayerTurn
                 && GameManager.instance.step == GameManager.STEP.ATTACK
                 && card.model.isFieldCard
                 && card.model.isRefreshed
                 && card.model.canAttack) // フィールドのカードで、アタックステップで、かつ回復状態で、かつ攻撃可能なら
        {
            isDraggable = true; // ドラッグ可能
        }
        else if (card.model.isPlayerCard
                 // && GameManager.instance.step == GameManager.STEP.ATTACK
                 && card.model.cardType == CARDTYPE.MAGIC
                 && GameManager.instance.CalcNetCost(card) <= reserveCoreList.Length + filedCoreNum)
        {
            isDraggable = true; // ドラッグ可能
        }
        else
        {
            isDraggable = false; // カードのコストがPlayerのManaコスト"以上"ならドラッグ不可(維持コアが1個以上必要なため)
        }

        if (!isDraggable) 
        {
            return; // ドラッグ不可なら処理を終了する
        }

        // 自分自身の親を取得（初期位置が手札なら親は手札）
        defaultParent = transform.parent;

        // 親の親を、親に設定→カードの移動がスムーズになる
        transform.SetParent(defaultParent.parent, false);

        // ドロップ先の判定をするため、カードのRaycastを無効にする
        GetComponent<CanvasGroup>().blocksRaycasts = false; 
    }

    // プレイヤーがフィールド上のスピリットをクリックしたときに呼ばれる
    public void OnPointerClick(PointerEventData eventData)
    {
        CardController card = GetComponent<CardController>();
        if (card == null) return;
        Debug.Log("クリックされました");

        // メインステップ
        if (card.model.isPlayerCard
                 && GameManager.instance.isPlayerTurn
                 && GameManager.instance.step == GameManager.STEP.MAIN
                 && card.model.isFieldCard)
        {
            StartCoroutine(ArrangeCores(card));
        }

        if (card.model.isPlayerCard
                 && GameManager.instance.isPlayerTurn
                 && GameManager.instance.step == GameManager.STEP.ATTACK
                 && card.model.isFieldCard
                 && card.model.isRefreshed
                 && card.model.canAttack) // フィールドのカードで、アタックステップで、かつ回復状態で、かつ攻撃可能なら
        {
            card.ChangeIsRefreshed(false);
            GameManager.instance.isDuringAttack = true;
            card.WhenAttack();
            StartCoroutine(GameManager.instance.enemyAI.PlayerTurn(card));
        }
    }
    
    // カードをドラッグしている最中に呼び出されるメソッド
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) 
        {
            return; // ドラッグ不可なら処理を終了する
        }
        
        // 左辺：カードの位置　右辺：マウスの位置
        transform.position = eventData.position;
    }

    // ドラッグ終了時(離したとき)に呼び出されるメソッド
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) 
        {
            return; // ドラッグ不可なら処理を終了する
        }
        
        // カードの親をドラッグ開始時の親に設定→手札に戻る
        transform.SetParent(defaultParent, false);

        // ドロップ先の判定をするため、カードのRaycastを有効にする
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    // デッキから手札に配られたカードを、デッキの位置から手札の位置へ移動させるメソッド
    public IEnumerator MoveFromDeck(Vector3 deckPosition)
    {
        defaultParent = transform.parent; // 生成時の親（手札）を保存する

        // レイアウト確定待ちの間、手札の位置に一瞬映ってしまわないように非表示にする
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.alpha = 0f;

        yield return new WaitForEndOfFrame(); // HorizontalLayoutGroupによる手札内の配置確定を待つ

        Vector3 handPosition = transform.position; // 配置確定後の手札内の位置を保存する

        // 一度親をCanvasに変更する（レイアウトの影響を受けないようにするため）
        transform.SetParent(defaultParent.parent, true);
        transform.position = deckPosition; // デッキの位置から開始する

        if (canvasGroup != null) canvasGroup.alpha = 1f; // デッキの位置から再表示する

        // DOTweenでカードをデッキの位置から手札の位置へ移動
        transform.DOMove(handPosition, 0.25f);
        yield return new WaitForSeconds(0.25f);

        transform.SetParent(defaultParent);
    }

    public IEnumerator MoveToField(Transform field)
    {
        // 一度親をCanvasに変更する
        transform.SetParent(defaultParent.parent, false);
        // DOTweenでカードをフィールドに移動
        transform.DOMove(field.position, 0.25f);
        yield return new WaitForSeconds(0.25f); 

        defaultParent = field;
        transform.SetParent(defaultParent);
    }

    // カード上のコアの数を増やしたり減らしたりするメソッド
    public IEnumerator ArrangeCores(CardController card)
    {
        CoreController[] onCores = card.iconTransform.GetComponentsInChildren<CoreController>();
        CoreController[] reserveCoreList = GameManager.instance.playerReserveTransform.GetComponentsInChildren<CoreController>();
        Debug.Log($"onCores:{onCores.Length}個");
        Coroutine coreMoveCoroutine = null;
        if (reserveCoreList.Length == 0)
        {
            int moveCoreNum = onCores.Length - card.model.coreLv1;
            for (int i = 0; i < moveCoreNum; i++)
            {
                coreMoveCoroutine = StartCoroutine(onCores[i].movement.MoveTo(GameManager.instance.playerReserveTransform));
            }
        }
        if(reserveCoreList.Length > 0)
        {
            coreMoveCoroutine = StartCoroutine(reserveCoreList[reserveCoreList.Length - 1].movement.MoveTo(card.iconTransform));
        }
        if (coreMoveCoroutine != null)
        {
            yield return coreMoveCoroutine;
        }
        reserveCoreList = GameManager.instance.playerReserveTransform.GetComponentsInChildren<CoreController>();
        Debug.Log($".reserveCoreList:{reserveCoreList.Length}個");
        GameManager.instance.ArrangeCoresAndFixLv(GameManager.instance.GetFriendFieldCards(card.model.isPlayerCard));        
    }

    // 合体(ブレイヴ)するメソッド
    public void BraveTo(Transform target)
    {
        // 一度親をCanvasに変更する
        transform.SetParent(defaultParent.parent, false);
        transform.position = target.position;
        defaultParent = target;
        transform.SetParent(defaultParent);
    }

    public IEnumerator MoveToTarget(Transform target)
    {
        // 攻撃後にカードを元の位置に戻すために、自分の位置と並びを保存しておく
        Vector3 currentPosition = transform.position;
        int siblingIndex = transform.GetSiblingIndex();
        
        // 一度親をCanvasに変更する
        transform.SetParent(defaultParent.parent);
        // DOTweenでカードをTargetに移動
        transform.DOMove(target.position, 0.25f);        
        yield return new WaitForSeconds(0.25f);

        // 元の位置と並びに戻す
        transform.DOMove(currentPosition, 0.25f);
        yield return new WaitForSeconds(0.25f);
        if (this != null)
        {
            transform.SetParent(defaultParent);
            transform.SetSiblingIndex(siblingIndex);
        }
    }

    // カードの疲労/回復状態にするメソッド
    public IEnumerator TapCard(bool isRefreshed)
    {
        if (isRefreshed == true)
        {
            transform.DORotate(new Vector3(0, 0, 0), 0.1f);
        }
        else
        {
            transform.DORotate(new Vector3(0, 0, 90), 0.1f);
        }
        yield return new WaitForSeconds(0.1f);
    }

    void Start()
    {
        defaultParent = transform.parent;
    }

    void OnDestroy()
    {
        // 破棄されるカードに対して実行中のTweenが残っているとDOTweenがエラーを出すため、破棄時にkillする
        transform.DOKill();
    }
}
