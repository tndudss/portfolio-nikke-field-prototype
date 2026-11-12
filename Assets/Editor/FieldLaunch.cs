using UnityEditor;
using UnityEditor.SceneManagement;
public static class FieldLaunch {
 public static void Run() {
  EditorSceneManager.OpenScene("Assets/Scenes/EventField.unity");
  EditorApplication.delayCall += () => EditorApplication.isPlaying = true;
 }
}
