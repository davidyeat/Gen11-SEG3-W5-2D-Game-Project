using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Temporary read-only checker. Writes a report of the open scene to the Console / Editor.log.
[InitializeOnLoad]
public static class RPSSetupCheck
{
    static RPSSetupCheck()
    {
        EditorApplication.delayCall += Report;
    }

    [MenuItem("Tools/RPS Setup Check")]
    public static void Report()
    {
        var sb = new StringBuilder();
        var scene = SceneManager.GetActiveScene();
        sb.AppendLine("RPSCHECK BEGIN scene=" + scene.name + " path=" + scene.path + " dirty=" + scene.isDirty + " playing=" + EditorApplication.isPlaying);
        foreach (var root in scene.GetRootGameObjects())
            Dump(root.transform, 0, sb);
        sb.AppendLine("RPSCHECK END");
        Debug.Log(sb.ToString());
    }

    static void Dump(Transform t, int depth, StringBuilder sb)
    {
        sb.Append("RPSCHECK ").Append(new string(' ', depth * 2)).Append(t.name).Append("  [");
        foreach (var c in t.GetComponents<Component>())
            sb.Append(c == null ? "MISSING-SCRIPT" : c.GetType().Name).Append(' ');
        sb.Append("]");

        var img = t.GetComponent<Image>();
        if (img != null) sb.Append(" sprite=").Append(img.sprite ? img.sprite.name : "none");
        var txt = t.GetComponent<Text>();
        if (txt != null) sb.Append(" text=\"").Append(txt.text).Append("\"");
        sb.AppendLine();

        var btn = t.GetComponent<Button>();
        if (btn != null)
        {
            int n = btn.onClick.GetPersistentEventCount();
            sb.AppendLine("RPSCHECK " + new string(' ', depth * 2 + 2) + "OnClick count=" + n);
            var so = new SerializedObject(btn);
            var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            for (int i = 0; i < calls.arraySize; i++)
            {
                var call = calls.GetArrayElementAtIndex(i);
                var target = call.FindPropertyRelative("m_Target").objectReferenceValue;
                var method = call.FindPropertyRelative("m_MethodName").stringValue;
                var mode = call.FindPropertyRelative("m_Mode").enumDisplayNames[call.FindPropertyRelative("m_Mode").enumValueIndex];
                var arg = call.FindPropertyRelative("m_Arguments.m_ObjectArgument").objectReferenceValue;
                sb.AppendLine("RPSCHECK " + new string(' ', depth * 2 + 2) + "  [" + i + "] target=" +
                    (target == null ? "NONE" : target.name + " (" + target.GetType().Name + ")") +
                    " method=" + (string.IsNullOrEmpty(method) ? "NONE" : method) +
                    " mode=" + mode +
                    " arg=" + (arg == null ? "NONE" : arg.name + " (" + arg.GetType().Name + ")"));
            }
        }

        foreach (Transform child in t)
            Dump(child, depth + 1, sb);
    }
}
