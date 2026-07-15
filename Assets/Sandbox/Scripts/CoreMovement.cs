using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

// コアのPrefabにアタッチ
// コア側の動きを制御するクラス
public class CoreMovement : MonoBehaviour
{
    public Transform defaultParent; // コアの親の位置を保存する変数

    // offset：同じplaceに複数のコアを重ねて置くときに、少しずらして配置したい場合に指定する
    public IEnumerator MoveTo(Transform place, Vector2 offset = default)
    {
        // 一度親をCanvasに変更する
        // ※ defaultParent.parent だと、フィールド上のカードが持つコア（Card→Coreの2階層）の場合に
        //   PlayerFieldのHorizontalLayoutGroup直下へ一瞬入ってしまい、Cardの再配置が起きて見た目が乱れるため、
        //   常にヒエラルキーのルート（Canvas）へ退避させる
        transform.SetParent(transform.root, true);

        yield return new WaitForEndOfFrame(); // カード側のレイアウト確定を待つ(要検索)

        // DOTweenでコアをフィールドに移動
        transform.DOMove((Vector2)place.position + offset, 0.25f);
        yield return new WaitForSeconds(0.25f);

        defaultParent = place;
        transform.SetParent(defaultParent);
    }

    // 同じカードに乗る複数のコアが重ならないよう、中心(0, centerY)を基準に放射状に配置した位置を返す
    // 3個なら三角形、4個なら四角形、5個なら五角形…のように正多角形の頂点に、6個だけは2行×3列のグリッドに配置する
    // index：このコアが何番目か　total：乗せるコアの総数
    public static Vector2 GetRadialOffset(int index, int total, float radiusX = 20f, float radiusY = 15f, float centerY = -40f)
    {
        if (total <= 1)
        {
            return new Vector2(0f, centerY); // 1個だけなら中央に置く
        }

        if (total == 6)
        {
            // 2行×3列のグリッド配置
            int col = index % 3;
            int row = index / 3;
            float gridX = Mathf.Lerp(-radiusX, radiusX, col / 2f);
            float gridY = Mathf.Lerp(radiusY, -radiusY, row) + centerY;
            return new Vector2(gridX, gridY);
        }

        // それ以外は正多角形の頂点に配置(12時方向から時計回り)
        float angle = index * (360f / total) * Mathf.Deg2Rad - Mathf.PI / 2f;
        float x = Mathf.Cos(angle) * radiusX;
        float y = Mathf.Sin(angle) * radiusY + centerY;
        return new Vector2(x, y);
    }

    // スピリット破壊時に転用予定
    /*
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
        transform.SetParent(defaultParent);
        transform.SetSiblingIndex(siblingIndex);
    }
    */


    void Start()
    {
        defaultParent = transform.parent;
    }
}
