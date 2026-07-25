using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject resultPanel; // ゲーム終了時に表示するパネルを取得
    [SerializeField] Text resultText;  // ゲーム終了時に表示するテキストを取得

    [SerializeField] Text playerHeroHpText; // プレイヤーのHeroのHPを表示するTextを取得
    [SerializeField] Text enemyHeroHpText; // 敵のHeroのHPを表示するTextを取得   
    
    [SerializeField] Text playerManaCostText; // プレイヤーのマナコストを表示するTextを取得
    [SerializeField] Text enemyManaCostText; // 敵のマナコストを表示するTextを取得   

    [SerializeField] Text timeCountText; // カウントダウンのTextを取得
    public Text buttonText; // ターンエンドボタンのTextを取得
    
    // リザルト画面を非表示にするメソッド
    public void HideResultPanel()
    {
        resultPanel.SetActive(false); // ゲーム開始時はリザルト画面を非表示にしておく

    }

    // マナコストの表示を変更するメソッド
    public void ShowManaCost(int playerManaCost, int enemyManaCost)
    {
        playerManaCostText.text = playerManaCost.ToString();
        enemyManaCostText.text = enemyManaCost.ToString();
    }

    // カウントダウンのTextを更新するメソッド
    public void UpdateTime(int timeCount)
    {
        timeCountText.text = timeCount.ToString(); // カウントダウンのTextを初期値にする
 
    }

    // HeroのHP表示を変更するメソッド
    public void ShowHeroHP(int playerHeroHp, int enemyHeroHp)
    {
        playerHeroHpText.text = playerHeroHp.ToString(); // プレイヤーのHeroのHPを表示するTextを変更
        enemyHeroHpText.text = enemyHeroHp.ToString(); // 敵のHeroのHPを表示するTextを変更
    }

    // リザルト画面を表示するメソッド
    public void ShowResultPanel(int heroHp)
    {
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

    public void ChangeButtonText()
    {
        if (GameManager.instance.step == GameManager.STEP.MAIN)
        {
            buttonText.text = "AttackStep";
        }
        if (GameManager.instance.step == GameManager.STEP.ATTACK)
        {
            buttonText.text = "TurnEnd";
        }
    }
}
