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

    // CardModel型のデータを取得してカードの見た目に反映するメソッド
    public void Show(CardModel cardModel)
    {
        nameText.text = cardModel.name; // カードの名前を表示するTextにカードの名前を代入
        hpText.text = cardModel.hp.ToString(); // カードのHPを表示するTextにカードのHPを代入
        atText.text = cardModel.at.ToString(); // カードの攻撃力を表示するTextにカードの攻撃力を代入
        costText.text = cardModel.cost.ToString(); // カードのコストを表示するTextにカードのコストを代入
        iconImage.sprite = cardModel.icon; // カードの絵柄を表示するImageにカードの絵柄を代入

    }
}
