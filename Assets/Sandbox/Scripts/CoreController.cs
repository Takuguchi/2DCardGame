using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// コア自身を扱うためのコード
// CorePrefabにアタッチ
public class CoreController : MonoBehaviour
{
    public CoreMovement movement; // コアの移動（movement）に関することを操作

    // public Transform coreTransform; // コアのTransformを取得する変数

    void Awake()
    {
        movement = GetComponent<CoreMovement>();  // コアの移動（movement）のデータをCoreMovementコンポーネントから取得
    }
}
