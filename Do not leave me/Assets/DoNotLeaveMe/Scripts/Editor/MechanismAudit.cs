using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 机关接线体检：把当前打开的场景里所有踏板（ActuatorTrigger）和门（Door）
/// 的实际接线打到 Console 上，并把明显有问题的地方标成警告。
///
/// 为什么需要它：踏板和门大多是嵌套预制体实例，Inspector 里要一个个点开才能看，
/// 而「数组长度被改成 0 但旧引用还留着」这种问题在 Inspector 里看起来是空的、
/// 很容易被当成「本来就该是空的」放过去。
///
/// 用法：打开要查的关卡，点 Tools / DoNotLeaveMe / 诊断 / 体检当前场景的机关接线。
/// 只读，不改任何东西。
/// </summary>
public static class MechanismAudit
{
    [MenuItem("Tools/DoNotLeaveMe/诊断/体检当前场景的机关接线")]
    public static void AuditLoadedScenes()
    {
        var triggers = new List<ActuatorTrigger>();
        var doors = new List<Door>();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
                continue;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                triggers.AddRange(root.GetComponentsInChildren<ActuatorTrigger>(true));
                doors.AddRange(root.GetComponentsInChildren<Door>(true));
            }
        }

        if (triggers.Count == 0 && doors.Count == 0)
        {
            Debug.LogWarning("[机关体检] 当前打开的场景里没有踏板也没有门。是不是关卡场景没打开？");
            return;
        }

        // 门 -> 有哪些踏板指向它
        var doorUsers = new Dictionary<Door, List<string>>();
        foreach (Door door in doors)
            doorUsers[door] = new List<string>();

        StringBuilder report = new StringBuilder();
        report.AppendLine($"[机关体检] 踏板 {triggers.Count} 个，门 {doors.Count} 扇。");
        report.AppendLine();

        int problems = 0;

        foreach (ActuatorTrigger trigger in triggers)
        {
            SerializedObject so = new SerializedObject(trigger);
            SerializedProperty actuators = so.FindProperty("actuators");
            string requirement = ((TriggerRequirement)so.FindProperty("requirement").enumValueIndex).ToString();
            bool permanent = so.FindProperty("permanent").boolValue;
            bool opensTransitionDoor = so.FindProperty("opensTransitionDoor").boolValue;
            string successorScene = so.FindProperty("successorScene").stringValue;
            bool preload = so.FindProperty("preloadRouteSuccessor").boolValue;

            report.AppendLine($"● 踏板  {Path(trigger.transform)}");
            report.AppendLine($"    谁能踩 = {requirement}   permanent = {permanent}");

            Collider collider = trigger.GetComponent<Collider>();
            if (collider == null)
            {
                report.AppendLine("    !! 没有 Collider，永远踩不到");
                problems++;
            }
            else if (!collider.isTrigger)
            {
                report.AppendLine("    !! Collider 没勾 Is Trigger，OnTriggerEnter 不会触发");
                problems++;
            }

            if (trigger.GetComponent<PedalPress>() == null)
                report.AppendLine("    -- 没有 PedalPress，踩下去不会有下沉动画");

            int size = actuators.arraySize;
            if (size == 0 && !opensTransitionDoor && !preload && string.IsNullOrEmpty(successorScene))
            {
                report.AppendLine("    !! actuators 是空的，而且也没配过关输出 —— 这块踏板踩了什么都不会发生");
                problems++;
            }
            else if (size == 0)
            {
                report.AppendLine("    (actuators 空，但配了过关输出，可能是故意的)");
            }

            for (int i = 0; i < size; i++)
            {
                Object target = actuators.GetArrayElementAtIndex(i).objectReferenceValue;
                if (target == null)
                {
                    report.AppendLine($"    !! actuators[{i}] 是空引用");
                    problems++;
                    continue;
                }

                Door door = target as Door;
                string kind = door != null ? "门" : target.GetType().Name;
                string where = target is Component component ? Path(component.transform) : target.name;
                report.AppendLine($"    -> 开 {kind}: {where}");

                if (door != null && doorUsers.ContainsKey(door))
                    doorUsers[door].Add(Path(trigger.transform));

                if (!(target is ILevelActuator))
                {
                    report.AppendLine($"    !! actuators[{i}] 挂的东西没实现 ILevelActuator，运行时会被跳过");
                    problems++;
                }
            }

            if (opensTransitionDoor || preload || !string.IsNullOrEmpty(successorScene))
                report.AppendLine($"    过关输出: opensTransitionDoor={opensTransitionDoor} preload={preload} successorScene='{successorScene}'");

            report.AppendLine();
        }

        report.AppendLine("—— 门这边 ——");
        foreach (KeyValuePair<Door, List<string>> pair in doorUsers)
        {
            if (pair.Value.Count == 0)
            {
                report.AppendLine($"!! 门 {Path(pair.Key.transform)} 没有任何踏板指向它（要么靠 DoorInteraction 按 E 开，要么就是漏接线了）");
                problems++;
            }
            else
            {
                report.AppendLine($"○ 门 {Path(pair.Key.transform)}  <- {string.Join(", ", pair.Value)}");
            }
        }

        report.AppendLine();
        report.AppendLine(problems == 0
            ? "没发现明显问题。"
            : $"发现 {problems} 处可疑的地方（上面带 !! 的行）。");

        if (problems > 0)
            Debug.LogWarning(report.ToString());
        else
            Debug.Log(report.ToString());
    }

    static string Path(Transform node)
    {
        string path = node.name;
        Transform cursor = node.parent;
        int guard = 0;
        while (cursor != null && guard++ < 12)
        {
            path = cursor.name + "/" + path;
            cursor = cursor.parent;
        }

        return node.gameObject.scene.name + " : " + path;
    }
}
