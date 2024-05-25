#if !MELONLOADER
using System;
using System.Collections.Generic;
using System.Linq;
using Fusion5vs5Gamemode.SDK.Internal;
using SLZ.MarrowEditor;
using UnityEditor;
using UnityEngine;

// This is copy pasted from FusionMarrowBehaviourEditor. The reason we need to create our own abstract MonoBehaviour parent class is that the Fusion5vs5Gamemode MonoBehaviours need values injected via FieldInjector.dll and FusionMarrowBehaviour is already injected into IL2CPP which leads to an exception. 
namespace Fusion5vs5Gamemode.SDK.Editor
{
    [CustomEditor(typeof(Fusion5vs5GamemodeBehaviour), editorForChildClasses: true)]
    public class Fusion5vs5GamemodeBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var behaviour = target as Fusion5vs5GamemodeBehaviour;
            if (behaviour.Comment != null)
            {
                EditorGUILayout.HelpBox(behaviour.Comment, MessageType.Info);
            }

            base.OnInspectorGUI();
        }
    }

    [CustomEditor(typeof(Fusion5vs5GamemodeDescriptor), editorForChildClasses: false)]
    public class Fusion5vs5GamemodeDescriptorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var behaviour = target as Fusion5vs5GamemodeDescriptor;
            if (behaviour == null)
                return;
            if (behaviour.Comment != null)
            {
                EditorGUILayout.HelpBox(behaviour.Comment, MessageType.Info);
            }

            EditorGUILayout.Space(20);

            base.OnInspectorGUI();

            EditorGUILayout.Space(20);

            Fusion5vs5GamemodeDescriptor[] descriptors;
            if ((descriptors = Resources.FindObjectsOfTypeAll<Fusion5vs5GamemodeDescriptor>().Where(e => e.gameObject.scene.name != null).ToArray()).Length > 1)
            {
                EditorGUILayout.HelpBox(
                    $"Warning, there is more than one Fusion5vs5GamemodeDescriptor component in this level ({descriptors.Length} in total)! This is not allowed and might break the Gamemode!",
                    MessageType.Warning);
                
                if (GUILayout.Button("Select"))
                {
                    Selection.objects = descriptors.Select(e => e.gameObject).ToArray();
                }
            }

            if (behaviour.CounterTerroristBuyZone == null)
            {
                EditorGUILayout.HelpBox("Counter Terrorist buy zone Collider must be set!", MessageType.Error);
            }
            else if (!behaviour.CounterTerroristBuyZone.isTrigger)
            {
                EditorGUILayout.HelpBox(
                    "Warning, the Counter Terrorist buy zone is not set to \"Is Trigger\". Players will collide with this buy zone!",
                    MessageType.Warning);
            }

            if (behaviour.CounterTerroristBuyZone != null && behaviour.CounterTerroristBuyZone.gameObject.layer != 27)
            {
                EditorGUILayout.HelpBox(
                    "The Counter Terrorist buy zone's GameObject must have its layer set to \"Trigger\", otherwise this buy zone will not work!",
                    MessageType.Error);
            }

            if (behaviour.TerroristBuyZone == null)
            {
                EditorGUILayout.HelpBox("Terrorist buy zone Collider must be set!", MessageType.Error);
            }
            else if (!behaviour.TerroristBuyZone.isTrigger)
            {
                EditorGUILayout.HelpBox(
                    "Warning, the Terrorist buy zone is not set to \"Is Trigger\". Players will collide with this buy zone!",
                    MessageType.Warning);
            }

            if (behaviour.TerroristBuyZone != null && behaviour.TerroristBuyZone.gameObject.layer != 27)
            {
                EditorGUILayout.HelpBox(
                    "The Terrorist buy zone's GameObject must have its layer set to \"Trigger\", otherwise this buy zone will not work!",
                    MessageType.Error);
            }

            if (behaviour.CounterTerroristBuyZone != null &&
                behaviour.TerroristBuyZone == behaviour.CounterTerroristBuyZone)
            {
                EditorGUILayout.HelpBox(
                    "Warning, the Terrorist buy zone and Counter Terrorist buy zone Colliders are the same!",
                    MessageType.Warning);
            }

            if (behaviour.CounterTerroristSpawnPoints.Count < 5)
            {
                EditorGUILayout.HelpBox(
                    "You must add at least 5 different Counter Terrorist spawn points to the list!",
                    MessageType.Error);
            }

            if (behaviour.TerroristSpawnPoints.Count < 5)
            {
                EditorGUILayout.HelpBox("You must add at least 5 different Terrorist spawn points to the list!",
                    MessageType.Error);
            }

            bool breakLoop = false;
            for (int i = 0; i < behaviour.CounterTerroristSpawnPoints.Count; ++i)
            {
                for (int j = i + 1; j < behaviour.CounterTerroristSpawnPoints.Count; ++j)
                {
                    if (behaviour.CounterTerroristSpawnPoints[i] == behaviour.CounterTerroristSpawnPoints[j])
                    {
                        EditorGUILayout.HelpBox("Warning, Counter Terrorist spawn point list contains duplicate Transforms!",
                            MessageType.Warning);
                        breakLoop = true;
                        break;
                    }
                }

                if (breakLoop)
                    break;
            }

            breakLoop = false;
            for (int i = 0; i < behaviour.TerroristSpawnPoints.Count; ++i)
            {
                for (int j = i + 1; j < behaviour.TerroristSpawnPoints.Count; ++j)
                {
                    if (behaviour.TerroristSpawnPoints[i] == behaviour.TerroristSpawnPoints[j])
                    {
                        EditorGUILayout.HelpBox("Warning, Counter Terrorist spawn point list contains duplicate Transforms!",
                            MessageType.Warning);
                        breakLoop = true;
                        break;
                    }
                }

                if (breakLoop)
                    break;
            }

            breakLoop = false;
            for (int i = 0; i < behaviour.TerroristSpawnPoints.Count; ++i)
            {
                for (int j = 0; j < behaviour.CounterTerroristSpawnPoints.Count; ++j)
                {
                    if (behaviour.TerroristSpawnPoints[i] == behaviour.CounterTerroristSpawnPoints[j])
                    {
                        EditorGUILayout.HelpBox("Warning, spawn lists for Counter Terrorists and Terrorists contain mutual Transforms!",
                            MessageType.Warning);
                        breakLoop = true;
                        break;
                    }
                }

                if (breakLoop)
                    break;
            }
        }
    }
}
#endif
