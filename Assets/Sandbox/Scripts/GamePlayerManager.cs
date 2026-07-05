using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayerManager : MonoBehaviour
{
    public List<int> deck = new List<int>(); // プレイヤーのデッキのカードIDを格納するリスト

    public int heroHp; // HeroのHP
    public int manaCost; // マナコスト
    public int defaultManaCost; // マナコストの初期値(ターンごとに増加)

    public void Init(List<int> cardDeck)
    {
        this.deck = cardDeck; // デッキを初期化する
        heroHp = 5;
        manaCost = 4;
        defaultManaCost = 4;
    }

    public void IncreaseManaCost()
    {
        defaultManaCost++; // プレイヤーのターンになったらマナコストを1増やす
        manaCost = defaultManaCost; // プレイヤーのマナコストに初期値を代入

    }
}
