using UnityEditor; // CustomEditorやSerializedPropertyなど、エディタ拡張用のAPIを使うため
using UnityEngine; // GUIContentなど、UI表示用のAPIを使うため

// magicの選択値に応じて、必要なフィールドだけをInspectorに表示する
[CustomEditor(typeof(CardEntity))] // このクラスがCardEntityのInspector表示をカスタマイズすることを宣言
public class CardEntityEditor : Editor // Editorを継承してInspectorの見た目を上書きする
{
    SerializedProperty magicDrawCountProp; // magicDrawCountフィールドへの参照を保持しておく変数
    SerializedProperty magicBpProp; // magicBpフィールドへの参照を保持しておく変数
    SerializedProperty coreLv1Prop; // coreLv1フィールドへの参照を保持しておく変数
    SerializedProperty bpLv1Prop;   // bpLv1フィールドへの参照を保持しておく変数
    SerializedProperty coreLv2Prop; // coreLv2フィールドへの参照を保持しておく変数
    SerializedProperty bpLv2Prop;   // bpLv2フィールドへの参照を保持しておく変数
    SerializedProperty coreLv3Prop; // coreLv3フィールドへの参照を保持しておく変数
    SerializedProperty bpLv3Prop;   // bpLv3フィールドへの参照を保持しておく変数

    void OnEnable() // このEditorがInspectorに表示されるたびに呼ばれる初期化処理
    {
        magicDrawCountProp = serializedObject.FindProperty("magicDrawCount"); // 対象アセットからmagicDrawCountプロパティを取得
        magicBpProp = serializedObject.FindProperty("magicBp"); // 対象アセットからmagicBpプロパティを取得
        coreLv1Prop = serializedObject.FindProperty("coreLv1"); // 対象アセットからcoreLv1プロパティを取得
        bpLv1Prop = serializedObject.FindProperty("bpLv1");     // 対象アセットからbpLv1プロパティを取得
        coreLv2Prop = serializedObject.FindProperty("coreLv2"); // 対象アセットからcoreLv2プロパティを取得
        bpLv2Prop = serializedObject.FindProperty("bpLv2");     // 対象アセットからbpLv2プロパティを取得
        coreLv3Prop = serializedObject.FindProperty("coreLv3"); // 対象アセットからcoreLv3プロパティを取得
        bpLv3Prop = serializedObject.FindProperty("bpLv3");     // 対象アセットからbpLv3プロパティを取得
    }

    public override void OnInspectorGUI() // Inspectorを描画する本体（デフォルトのDrawDefaultInspectorの代わり）
    {
        serializedObject.Update(); // アセットの最新の値をSerializedObjectに反映

        SerializedProperty prop = serializedObject.GetIterator(); // 全フィールドを順番に走査するイテレータを取得
        bool enterChildren = true; // 最初の一回だけ子プロパティにも入るようにするフラグ
        while (prop.NextVisible(enterChildren)) // 次に表示すべきプロパティがある間ループ
        {
            enterChildren = false; // 2回目以降は同じ階層の兄弟プロパティだけを辿る

            // magicDrawCount/magicBp/レベル別ステータスは他のプロパティの直後に条件付きで表示するので、通常の並びではスキップ
            if (prop.name == "m_Script" || prop.name == "magicDrawCount" || prop.name == "magicBp"
                || prop.name == "coreLv1" || prop.name == "bpLv1"
                || prop.name == "coreLv2" || prop.name == "bpLv2"
                || prop.name == "coreLv3" || prop.name == "bpLv3") continue; // スクリプト参照欄と手動描画するフィールドは通常描画から除外

            EditorGUILayout.PropertyField(prop, true); // 現在のプロパティを通常通りInspectorに描画

            if (prop.name == "magic") // 今描画したのがmagicの場合のみ、値に応じた関連フィールドを続けて表示
            {
                MAGIC magicValue = (MAGIC)prop.enumValueIndex; // 選択中のmagicの値を取得

                if (magicValue == MAGIC.DRAW)
                {
                    EditorGUI.indentLevel++; // 関連フィールドだとわかるように一段インデントする
                    EditorGUILayout.PropertyField(magicDrawCountProp, new GUIContent("ドロー枚数")); // ドロー枚数の入力欄を表示
                    EditorGUI.indentLevel--; // インデントを元に戻す
                }
                else if (magicValue == MAGIC.DESTROY_ENEMY_CARD || magicValue == MAGIC.DESTROY_ALL_CARDS)
                {
                    EditorGUI.indentLevel++; // 関連フィールドだとわかるように一段インデントする
                    EditorGUILayout.PropertyField(magicBpProp, new GUIContent("破壊対象BP")); // 破壊対象BPの入力欄を表示
                    EditorGUI.indentLevel--; // インデントを元に戻す
                }
            }

            if (prop.name == "cardType" && (CARDTYPE)prop.enumValueIndex == CARDTYPE.SPIRIT) // 今描画したのがcardTypeで、値がSPIRITの場合のみ
            {
                EditorGUI.indentLevel++; // 関連フィールドだとわかるように一段インデントする
                EditorGUILayout.PropertyField(coreLv1Prop, new GUIContent("Lv1")); // レベル1の所要コアの入力欄を表示
                EditorGUILayout.PropertyField(bpLv1Prop, new GUIContent(" "));   // レベル1のBPの入力欄を表示
                EditorGUILayout.PropertyField(coreLv2Prop, new GUIContent("Lv2")); // レベル2の所要コアの入力欄を表示
                EditorGUILayout.PropertyField(bpLv2Prop, new GUIContent(" "));   // レベル2のBPの入力欄を表示
                EditorGUILayout.PropertyField(coreLv3Prop, new GUIContent("Lv3")); // レベル3の所要コアの入力欄を表示
                EditorGUILayout.PropertyField(bpLv3Prop, new GUIContent(" "));   // レベル3のBPの入力欄を表示
                EditorGUI.indentLevel--; // インデントを元に戻す
            }
        }

        serializedObject.ApplyModifiedProperties(); // Inspectorでの変更をアセットに保存
    }
}
