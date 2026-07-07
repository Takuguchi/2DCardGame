using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 扱いやすいオブジェクトとして生成
[CreateAssetMenu(fileName = "CardEntity", menuName = "Create CardEntity")]

// ID（入れ物）、カードデータそのもの
// ScriptableObject：カードのデータベース　ゲームオブジェクトにアタッチしなくていい
public class CardEntity : ScriptableObject
{
    public new string name; // カードの名前
    public int hp;      // カードのHP
    public int at;      // カードの攻撃力
    public int cost;    // カードのコスト
    public Sprite icon; // カードの絵柄
    public ABILITY ability; // カードのアビリティ

    public int coreLv1; // レベル1の所要コア
    public int bpLv1;   // レベル1のBP
    public int coreLv2; // レベル2の所要コア
    public int bpLv2;   // レベル2のBP
    public int coreLv3; // レベル3の所要コア
    public int bpLv3;   // レベル3のBP
}

public enum ABILITY
{
    NONE,   // アビリティなし
    INIT_ATTACKABLE, // 1ターン目で攻撃可能
    SHIELD  // シールド
}