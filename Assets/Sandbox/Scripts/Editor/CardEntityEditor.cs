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
    SerializedProperty gainedBpProp;
    SerializedProperty symbolsProp;             // symbolsフィールドへの参照を保持しておく変数
    SerializedProperty symbolColorProp;         // symbolColorフィールドへの参照を保持しておく変数
    SerializedProperty reductionSymbolsProp;      // reductionSymbolsフィールドへの参照を保持しておく変数
    SerializedProperty reductionSymbolColorProp;  // reductionSymbolColorフィールドへの参照を保持しておく変数

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
        gainedBpProp = serializedObject.FindProperty("gainedBp");
        symbolsProp = serializedObject.FindProperty("symbols");         // 対象アセットからsymbolsプロパティを取得
        symbolColorProp = serializedObject.FindProperty("symbolColor"); // 対象アセットからsymbolColorプロパティを取得
        reductionSymbolsProp = serializedObject.FindProperty("reductionSymbols");         // 対象アセットからreductionSymbolsプロパティを取得
        reductionSymbolColorProp = serializedObject.FindProperty("reductionSymbolColor"); // 対象アセットからreductionSymbolColorプロパティを取得
    }

    // ラベル1つの下に「色(enum)→数(int)」の順で2つのプロパティを1行に横並びで描画する
    void DrawColorAndIntRow(string label, SerializedProperty colorProp, SerializedProperty intProp)
    {
        Rect rowRect = EditorGUILayout.GetControlRect(); // 1行分の描画領域を確保
        float labelWidth = EditorGUIUtility.labelWidth; // 現在のラベル幅を取得

        Rect labelRect = new Rect(rowRect.x, rowRect.y, labelWidth, rowRect.height); // 行ラベル用の領域
        EditorGUI.LabelField(labelRect, label); // 行ラベルを表示

        float fieldsWidth = rowRect.width - labelWidth; // ラベルを除いた残り幅
        float spacing = 4f; // 2つの欄の間隔
        float colorWidth = (fieldsWidth - spacing) * 0.5f; // 色欄の幅
        Rect colorRect = new Rect(rowRect.x + labelWidth, rowRect.y, colorWidth, rowRect.height); // 色欄の領域（左側）
        Rect intRect = new Rect(colorRect.xMax + spacing, rowRect.y, fieldsWidth - colorWidth - spacing, rowRect.height); // 数欄の領域（右側）

        EditorGUI.PropertyField(colorRect, colorProp, GUIContent.none); // 色の選択欄を表示
        EditorGUI.PropertyField(intRect, intProp, GUIContent.none); // 数の入力欄を表示
    }

    public override void OnInspectorGUI() // Inspectorを描画する本体（デフォルトのDrawDefaultInspectorの代わり）
    {
        serializedObject.Update(); // アセットの最新の値をSerializedObjectに反映

        SerializedProperty prop = serializedObject.GetIterator(); // 全フィールドを順番に走査するイテレータを取得
        bool enterChildren = true; // 最初の一回だけ子プロパティにも入るようにするフラグ
        while (prop.NextVisible(enterChildren)) // 次に表示すべきプロパティがある間ループ
        {
            enterChildren = false; // 2回目以降は同じ階層の兄弟プロパティだけを辿る

            // magicDrawCount/magicBp/レベル別ステータス/シンボル関連フィールドは他のプロパティの直後に条件付きで表示するので、通常の並びではスキップ
            if (prop.name == "m_Script" || prop.name == "magicDrawCount" || prop.name == "magicBp"
                || prop.name == "coreLv1" || prop.name == "bpLv1"
                || prop.name == "coreLv2" || prop.name == "bpLv2"
                || prop.name == "coreLv3" || prop.name == "bpLv3"
                || prop.name == "gainedBp"
                || prop.name == "reductionSymbols" || prop.name == "reductionSymbolColor"
                || prop.name == "symbols" || prop.name == "symbolColor") continue; // スクリプト参照欄と手動描画するフィールドは通常描画から除外

            EditorGUILayout.PropertyField(prop, true); // 現在のプロパティを通常通りInspectorに描画

            if (prop.name == "cost") // 今描画したのがcostの場合、直後にReductionSymbols行とSymbols行を1行ずつ横並びで表示
            {
                DrawColorAndIntRow("Reduction Symbols", reductionSymbolColorProp, reductionSymbolsProp);
                DrawColorAndIntRow("Symbols", symbolColorProp, symbolsProp);
            }

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

            if (prop.name == "ability") // 今描画したのがabilityの場合のみ、値に応じた関連フィールドを続けて表示
            {
                ABILITY abilityValue = (ABILITY)prop.enumValueIndex; // 選択中のabilityの値を取得

                if (abilityValue == ABILITY.GAIN_BP_ATTACK || abilityValue == ABILITY.GAIN_BP_BLOCK)
                {
                    EditorGUI.indentLevel++; // 関連フィールドだとわかるように一段インデントする
                    EditorGUILayout.PropertyField(gainedBpProp, new GUIContent("BP+")); // 加算BPの入力欄を表示
                    EditorGUI.indentLevel--; // インデントを元に戻す
                }
            }
        }

        serializedObject.ApplyModifiedProperties(); // Inspectorでの変更をアセットに保存
    }
}
