using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 体型审计：报告每个 PlayerActor 的模型实测高度、脚底偏差(sink)、
/// 胶囊与模型的贴合差、Body 缩放值。选中若干 actor 则只审它们，否则审全部。
/// </summary>
public static class ActorBodyAudit
{
    [MenuItem("Tools/DoNotLeaveMe/Actor/Audit Body Fit")]
    static void Audit()
    {
        PlayerActor[] actors = Selection.gameObjects.Length > 0
            ? Selection.gameObjects.Select(go => go.GetComponent<PlayerActor>()).Where(a => a != null).ToArray()
            : Object.FindObjectsOfType<PlayerActor>(true);
        if (actors.Length == 0)
        {
            Debug.LogWarning("[ActorBodyAudit] 没有找到 PlayerActor（或选中物体不带该组件）。");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"[ActorBodyAudit] actors={actors.Length}");
        foreach (PlayerActor actor in actors)
        {
            float footY = actor.transform.position.y;
            CapsuleCollider capsule = actor.GetComponentInChildren<CapsuleCollider>();
            if (capsule == null)
            {
                sb.AppendLine($"{actor.name}: 无 CapsuleCollider!");
                continue;
            }

            Renderer[] renderers = actor.GetComponentsInChildren<Renderer>(false);
            if (renderers.Length == 0)
            {
                sb.AppendLine($"{actor.name}: 无渲染器（视觉未加载？）");
                continue;
            }

            Bounds model = renderers[0].bounds;
            foreach (Renderer r in renderers)
                model.Encapsulate(r.bounds);

            Transform body = actor.transform.Find("Body");

            float sink = footY - model.min.y;
            Bounds capsuleBounds = capsule.bounds;
            float bottomError = capsuleBounds.min.y - footY;
            Vector3 lowerGap = capsuleBounds.min - model.min;
            Vector3 upperGap = capsuleBounds.max - model.max;
            string direction = capsule.direction == 0 ? "X" : capsule.direction == 1 ? "Y" : "Z";
            sb.AppendLine($"{actor.name}: bodyScale={(body != null ? body.localScale.x.ToString("F3") : "无Body!")}"
                + $" | modelHeight={model.size.y:F3} sink={sink:F3}"
                + $" | capsuleDir={direction} worldCenter={capsuleBounds.center:F3} worldSize={capsuleBounds.size:F3}"
                + $" | capsuleWorldY[{capsuleBounds.min.y:F2}..{capsuleBounds.max.y:F2}] bottomError={bottomError:F3}"
                + $" vs model[{model.min.y:F2}..{model.max.y:F2}]"
                + $" | lowerGap={lowerGap:F3} upperGap={upperGap:F3}");

            if (System.Math.Abs(sink) > 0.02f)
                sb.AppendLine($"  ^ 警告: 脚底偏差 {sink:F3} 超过 2cm，请微调 Loader 偏移 Y。");
            if (System.Math.Abs(bottomError) > 0.02f)
                sb.AppendLine($"  ^ 警告: 胶囊底部偏离脚底根节点 {bottomError:F3}，请校准 Capsule Center。");
        }
        Debug.Log(sb.ToString());
    }
}
