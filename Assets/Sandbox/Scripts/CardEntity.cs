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
}
