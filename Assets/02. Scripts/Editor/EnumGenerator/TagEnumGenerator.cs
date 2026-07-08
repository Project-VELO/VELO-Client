using System.Collections.Generic;
using UnityEditor;

public class TagEnumGenerator : EnumGeneratorBase
{
    [MenuItem(MENU_PREFIX + "Generate Tag Enums(ETags)", priority = 3)]
    public static void GenerateTagEnum()
    {
        Generate("ETags.cs", "ETags", (writer) =>
        {
            string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
            HashSet<string> usedNames = new HashSet<string>();
            foreach (string tag in tags)
            {
                string safeName = IdentifierUtils.SanitizeIdentifier(tag);
                if (usedNames.Add(safeName))
                {
                    writer.WriteLine($"    {safeName},");
                }
                else
                {
                    UnityEngine.Debug.LogError($"[TagEnumGenerator] Duplicate tag enum name detected: {safeName}");
                }
            }
        });
    }
}