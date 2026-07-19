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
    public SPELL spell;
}

public enum ABILITY
{
    NONE,   // アビリティなし
    INIT_ATTACKABLE, // 1ターン目で攻撃可能
    SHIELD  // シールド
}

public enum SPELL
{
    NONE,
    DAMAGE_ENEMY_CARD,
    DAMAGE_ENEMY_CARDS,
    DAMAGE_ENEMY_HERO,
    HEAL_FRIEND_CARD,
    HEAL_FRIEND_CARDS,
    HEAL_FRIEND_HERO
}