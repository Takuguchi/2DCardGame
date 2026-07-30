using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// データをもらってカードの見た目を表示するクラス
public class CardView : MonoBehaviour
{
    [SerializeField] Text nameText; // カードの名前を表示するText
    [SerializeField] Text hpText;   // カードのHPを表示するText
    [SerializeField] Text atText;   // カードの攻撃力を表示するText
    [SerializeField] Text costText; // カードのコストを表示するText
    [SerializeField] Image iconImage; // カードの絵柄を表示するImage
    [SerializeField] GameObject selectablePanel; // カードが選択可能かどうかを表示するパネル
    [SerializeField] GameObject shieldPanel; // カードがシールドを持っているかどうかを表示するパネル
    [SerializeField] GameObject maskPanel; // カードの裏面を表示するパネル

    // CardModel型のデータを取得してカードの見た目に反映するメソッド
    public void SetCard(CardModel cardModel)
    {
        nameText.text = cardModel.name; // カードの名前を表示するTextにカードの名前を代入
        hpText.text = cardModel.hp.ToString(); // カードのHPを表示するTextにカードのHPを代入
        atText.text = cardModel.at.ToString(); // カードの攻撃力を表示するTextにカードの攻撃力を代入
        costText.text = cardModel.cost.ToString(); // カードのコストを表示するTextにカードのコストを代入
        iconImage.sprite = cardModel.icon; // カードの絵柄を表示するImageにカードの絵柄を代入
        maskPanel.SetActive(!cardModel.isPlayerCard);

        // カードのアビリティがシールドなら、シールドパネルを表示する
        if (cardModel.ability == ABILITY.SHIELD)
        {
            shieldPanel.SetActive(true); // シールドパネルを表示する
        }
        else
        {
            shieldPanel.SetActive(false); // シールドパネルを非表示にする
        }
        if (cardModel.spell != SPELL.NONE)
        {
            hpText.gameObject.SetActive(false); // スペルカードはHPを非表示にする
        }
    }

    public void Show()
    {
        maskPanel.SetActive(false);
    }

    // カードのデータが変化したときに呼ばれるメソッド
    public void Refresh(CardModel cardModel)
    {
        hpText.text = cardModel.hp.ToString(); // カードのHPを表示するTextにカードのHPを代入
        atText.text = cardModel.at.ToString(); // カードの攻撃力を表示するTextにカードの攻撃力を代入
    }

    // カードが選択可能かどうかを表示するメソッド
    public void SetActiveSelectablePanel(bool flag)
    {
        selectablePanel.SetActive(flag);
    }

}
