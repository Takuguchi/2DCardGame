using UnityEditor; // CustomEditorやSerializedPropertyなど、エディタ拡張用のAPIを使うため
using UnityEngine; // GUIContentなど、UI表示用のAPIを使うため

// magicの選択値に応じて、必要なフィールドだけをInspectorに表示する
[CustomEditor(typeof(CardEntity))] // このクラスがCardEntityのInspector表示をカスタマイズすることを宣言
public class CardEntityEditor : Editor // Editorを継承してInspectorの見た目を上書きする
{
    SerializedProperty magicDrawCountProp; // magicDrawCountフィールドへの参照を保持しておく変数

    void OnEnable() // このEditorがInspectorに表示されるたびに呼ばれる初期化処理
    {
        magicDrawCountProp = serializedObject.FindProperty("magicDrawCount"); // 対象アセットからmagicDrawCountプロパティを取得
    }

    public override void OnInspectorGUI() // Inspectorを描画する本体（デフォルトのDrawDefaultInspectorの代わり）
    {
        serializedObject.Update(); // アセットの最新の値をSerializedObjectに反映

        SerializedProperty prop = serializedObject.GetIterator(); // 全フィールドを順番に走査するイテレータを取得
        bool enterChildren = true; // 最初の一回だけ子プロパティにも入るようにするフラグ
        while (prop.NextVisible(enterChildren)) // 次に表示すべきプロパティがある間ループ
        {
            enterChildren = false; // 2回目以降は同じ階層の兄弟プロパティだけを辿る

            // magicDrawCountはmagicの直後に条件付きで表示するので、通常の並びではスキップ
            if (prop.name == "m_Script" || prop.name == "magicDrawCount") continue; // スクリプト参照欄と手動描画するフィールドは通常描画から除外

            EditorGUILayout.PropertyField(prop, true); // 現在のプロパティを通常通りInspectorに描画

            if (prop.name == "magic" && (MAGIC)prop.enumValueIndex == MAGIC.DRAW) // 今描画したのがmagicで、値がDRAWの場合のみ
            {
                EditorGUI.indentLevel++; // 関連フィールドだとわかるように一段インデントする
                EditorGUILayout.PropertyField(magicDrawCountProp, new GUIContent("ドロー枚数")); // ドロー枚数の入力欄を表示
                EditorGUI.indentLevel--; // インデントを元に戻す
            }
        }

        serializedObject.ApplyModifiedProperties(); // Inspectorでの変更をアセットに保存
    }
}
