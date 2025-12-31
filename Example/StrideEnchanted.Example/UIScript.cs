using System.Threading.Tasks;
using Stride.Core.Serialization;
using Stride.Engine;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Events;

namespace StrideEnchanted.Example;

public class UIScript : AsyncScript
{
  public Entity RootSphere { get; set; }

  public Prefab SpherePrefab { get; set; }

  public UrlReference<Scene> ChildScene { get; set; }

  private bool isChildSceneShowed = false;
  private Scene loadedScene;

  public override async Task Execute()
  {
    var uiPage = this.Entity.Get<UIComponent>().Page;

    var addChildSphereButton = uiPage.RootElement.FindVisualChildOfType<Button>("AddChildSphereButton");
    addChildSphereButton.Click += this.HandleAddChildSphereButtonClick;

    var showChildSceneButton = uiPage.RootElement.FindVisualChildOfType<Button>("ShowChildSceneButton");
    showChildSceneButton.Click += this.HandleShowChildSceneButtonClick;

    while (this.Game.IsRunning && !this.CancellationToken.IsCancellationRequested)
    {
      await this.Script.NextFrame();
    }
  }

  private void HandleAddChildSphereButtonClick(object sender, RoutedEventArgs args)
  {
    var childs = this.SpherePrefab.Instantiate();
    foreach (var child in childs)
      this.RootSphere.AddChild(child);
  }

  private void HandleShowChildSceneButtonClick(object sender, RoutedEventArgs args)
  {
    if (this.isChildSceneShowed)
    {
      if (this.loadedScene == null)
        return;

      this.loadedScene.Parent = null;
      this.Content.Unload(this.loadedScene);

      this.isChildSceneShowed = false;
      this.loadedScene = null;
    }
    else
    {
      if (this.loadedScene != null)
        return;

      var scene = this.Content.Load(this.ChildScene);
      scene.Parent = this.Entity.Scene;

      this.isChildSceneShowed = true;
      this.loadedScene = scene;
    }
  }
}
