using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LayerEnumGenerator : EnumGeneratorBase
{
    private const int MAX_LAYERS = 32;

    [MenuItem(MENU_PREFIX + "Generate Layer Enums(ELayers)", priority = 3)]
    public static void GenerateLayerEnum()
    {
        Generate("ELayers.cs", "ELayers", (writer) =>
        {
            HashSet<string> usedNames = new HashSet<string>();
            for (int i = 0; i < MAX_LAYERS; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    string safeName = IdentifierUtils.SanitizeIdentifier(layerName);
                    if (usedNames.Add(safeName))
                    {
                        writer.WriteLine($"    {safeName} = {i},");
                    }
                    else
                    {
                        Debug.LogError($"[LayerEnumGenerator] Duplicate layer enum name detected: {safeName} (Layer index: {i})");
                    }
                }
            }
        });
    }
}