using Sirenix.OdinInspector.Editor;
using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Sirenix.OdinInspector;

namespace UnityRPGEditor.Editor
{
    public class UnityRPGEditorWindow : OdinMenuEditorWindow    // OdinMenuEditorWindow creates a list of editable assets in a new window
    {
        // MenuItem adds our static function to Unity editor windows
        [MenuItem("Tools/RPG Editor Window")]
        private static void OpenEditor()
        {
            // GetWindow will create a window using the selected type
            GetWindow<UnityRPGEditorWindow>();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            // create a new tree to display
            OdinMenuTree tree = new OdinMenuTree();

            List<Type> includedTypes = new List<Type>();
            includedTypes.Add(typeof(CharacterData));
            includedTypes.Add(typeof(ClassData));
            includedTypes.Add(typeof(WeaponData));
            includedTypes.Add(typeof(SkillData));

            // add items to tree
            foreach (Type type in includedTypes)
            {
                tree.AddAllAssetsAtPath(type.Name, "Assets/", type, true, false);
                tree.Add("New " + type.Name, new CreateNewAsset(type));     // add new asset custom button
            }

            // return completed tree
            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();

            MenuTree.DrawSearchToolbar();
        }
    }

    public class CreateNewAsset
    {
        private Type _type;
        [SerializeField, InlineEditor(Expanded = true)] private ScriptableObject _data;

        [field: SerializeField]
        public string Name { get; private set; } = "New Data";

        public CreateNewAsset(Type type)
        {
            _type = type;
            _data = ScriptableObject.CreateInstance(_type);
        }

        [Button("Create New")]
        private void CreateNew()
        {
            string path = GetProjectWindowPath();
            AssetDatabase.CreateAsset(_data, path + Name + ".asset");
            AssetDatabase.SaveAssets();
        }

        // get active folder path from Unity editor, this is normally a private function so we're using reflection to call it anyway
        private string GetProjectWindowPath()
        {
            // we're using reflection to analyze a class, find a specific private function, and then call it
            // we do this to gain editor functionality we don't normally have access to
            Type projectWindowUtilType = typeof(ProjectWindowUtil);
            MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic); // combining two masks
            object obj = getActiveFolderPath.Invoke(null, new object[0]); // generic cast, static function, no parameters
            string path = obj.ToString() + "/";
            return path;
        }
    }
}
