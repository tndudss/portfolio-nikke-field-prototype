using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
public static class FieldSetup {
 [MenuItem("Prototype/Rebuild Field Scene")]
 public static void Build() {
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  new GameObject("Event Field Runtime").AddComponent<EventField>();
  Directory.CreateDirectory("Assets/Scenes");
  EditorSceneManager.SaveScene(scene,"Assets/Scenes/EventField.unity");
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/EventField.unity",true)};
  PlayerSettings.productName="Neon Event Field";
  PlayerSettings.companyName="Field Prototype";
  PlayerSettings.defaultScreenWidth=1440; PlayerSettings.defaultScreenHeight=900;
  AssetDatabase.SaveAssets();
 }
}
