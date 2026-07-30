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
    SerializedProperty bpBravedProp;   // bpBravedフィールドへの参照を保持しておく変数
    SerializedProperty braveConditionCostProp;
    SerializedProperty gainedBpProp;
    SerializedProperty symbolsProp;          // symbolsフィールド(リスト)への参照を保持しておく変数
    SerializedProperty reductionSymbolsProp; // reductionSymbolsフィールド(リスト)への参照を保持しておく変数

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
        bpBravedProp = serializedObject.FindProperty("bpBraved");     // 対象アセットからbpBravedプロパティを取得
        braveConditionCostProp = serializedObject.FindProperty("braveConditionCost");
        gainedBpProp = serializedObject.FindProperty("gainedBp");
        symbolsProp = serializedObject.FindProperty("symbols");                   // 対象アセットからsymbolsプロパティを取得
        reductionSymbolsProp = serializedObject.FindProperty("reductionSymbols"); // 対象アセットからreductionSymbolsプロパティを取得
    }

    // ラベルの下に「色(enum)→数(int)」のペアを複数行、1行ずつ横並びで表示する（追加・削除も可能）
    void DrawSymbolList(string label, SerializedProperty listProp)
    {
        float labelWidth = EditorGUIUtility.labelWidth; // 現在のラベル幅を取得
        float removeButtonWidth = 18f; // 削除ボタンの幅
        float spacing = 4f; // 欄同士の間隔

        if (listProp.arraySize == 0) // 要素が1つもない場合はラベルだけの行を表示
        {
            Rect emptyRowRect = EditorGUILayout.GetControlRect();
            Rect emptyLabelRect = new Rect(emptyRowRect.x, emptyRowRect.y, labelWidth, emptyRowRect.height);
            EditorGUI.LabelField(emptyLabelRect, label);
        }

        int removeIndex = -1; // このフレームで削除する要素のインデックス（削除はループの後でまとめて行う）

        for (int i = 0; i < listProp.arraySize; i++)
        {
            SerializedProperty elementProp = listProp.GetArrayElementAtIndex(i);
            SerializedProperty colorProp = elementProp.FindPropertyRelative("color");
            SerializedProperty countProp = elementProp.FindPropertyRelative("count");

            Rect rowRect = EditorGUILayout.GetControlRect();

            Rect labelRect = new Rect(rowRect.x, rowRect.y, labelWidth, rowRect.height);
            EditorGUI.LabelField(labelRect, i == 0 ? label : " "); // 1行目だけラベルを表示、2行目以降は空欄

            float fieldsWidth = rowRect.width - labelWidth - removeButtonWidth - spacing; // ラベルと削除ボタンを除いた幅
            float colorWidth = (fieldsWidth - spacing) * 0.5f; // 色欄の幅
            Rect colorRect = new Rect(rowRect.x + labelWidth, rowRect.y, colorWidth, rowRect.height); // 色欄の領域（左側）
            Rect countRect = new Rect(colorRect.xMax + spacing, rowRect.y, fieldsWidth - colorWidth - spacing, rowRect.height); // 数欄の領域（右側）
            Rect removeRect = new Rect(countRect.xMax + spacing, rowRect.y, removeButtonWidth, rowRect.height); // 削除ボタンの領域

            EditorGUI.PropertyField(colorRect, colorProp, GUIContent.none); // 色の選択欄を表示
            EditorGUI.PropertyField(countRect, countProp, GUIContent.none); // 数の入力欄を表示

            if (GUI.Button(removeRect, "-")) // 削除ボタンが押されたらインデックスを記録
            {
                removeIndex = i;
            }
        }

        if (removeIndex >= 0) // 削除ボタンが押されていた場合、該当要素を削除
        {
            listProp.DeleteArrayElementAtIndex(removeIndex);
        }

        Rect addRowRect = EditorGUILayout.GetControlRect();
        Rect addButtonRect = new Rect(addRowRect.x + labelWidth, addRowRect.y, 60f, addRowRect.height);
        if (GUI.Button(addButtonRect, "+")) // 追加ボタンが押されたら末尾に新しい要素を追加
        {
            listProp.InsertArrayElementAtIndex(listProp.arraySize);
        }
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
                || prop.name == "bpBraved"
                || prop.name == "braveConditionCost"
                || prop.name == "gainedBp"
                || prop.name == "reductionSymbols" || prop.name == "symbols") continue; // スクリプト参照欄と手動描画するフィールドは通常描画から除外

            EditorGUILayout.PropertyField(prop, true); // 現在のプロパティを通常通りInspectorに描画

            if (prop.name == "cost") // 今描画したのがcostの場合、直後にReductionSymbolsとSymbolsのリストを表示
            {
                DrawSymbolList("Reduction Symbols", reductionSymbolsProp);
                DrawSymbolList("Symbols", symbolsProp);
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
            
            if (prop.name == "cardType" && (CARDTYPE)prop.enumValueIndex == CARDTYPE.BRAVE) // 今描画したのがcardTypeで、値がBRAVEの場合のみ
            {
                EditorGUI.indentLevel++; // 関連フィールドだとわかるように一段インデントする
                EditorGUILayout.PropertyField(bpLv1Prop, new GUIContent("Lv1"));   // レベル1のBPの入力欄を表示
                EditorGUILayout.PropertyField(bpBravedProp, new GUIContent("合体+"));   // 合体中のBPの入力欄を表示
                EditorGUILayout.PropertyField(braveConditionCostProp, new GUIContent("合体条件：コスト"));   // 合体条件のコスト入力欄を表示
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

    // trueを返さないと、Projectウィンドウ等のサムネイル生成時にRenderStaticPreviewが呼ばれない
    public override bool HasPreviewGUI()
    {
        CardEntity entity = target as CardEntity;
        return entity != null && entity.icon != null;
    }

    // Inspector下部のプレビュー欄に、iconに設定した画像を表示する
    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        CardEntity entity = target as CardEntity;
        if (entity == null || entity.icon == null)
        {
            base.OnPreviewGUI(r, background);
            return;
        }
        GUI.DrawTexture(r, entity.icon.texture, ScaleMode.ScaleToFit);
    }

    // Projectウィンドウ・Hierarchy等で表示されるサムネイルアイコンを、iconに設定した画像に差し替える
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        CardEntity entity = target as CardEntity;
        Texture sourceTexture = entity != null && entity.icon != null ? entity.icon.texture : null;
        if (sourceTexture == null) return null; // iconが未設定ならデフォルトのアイコン表示のままにする

        // Graphics.Blit + RenderTexture.ReadPixelsで焼き込むため、テクスチャのRead/Write設定を変更しなくてもよい
        RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
        RenderTexture previousActive = RenderTexture.active;

        Graphics.Blit(sourceTexture, renderTexture);
        RenderTexture.active = renderTexture;

        Texture2D preview = new Texture2D(width, height, TextureFormat.RGBA32, false);
        preview.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        preview.Apply();

        RenderTexture.active = previousActive;
        RenderTexture.ReleaseTemporary(renderTexture);

        return preview;
    }
}
