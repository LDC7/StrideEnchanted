using System.Threading.Tasks;
using Stride.Engine;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Events;

namespace StrideEnchanted.Example;

public class UIScript : AsyncScript
{
  public Entity RootSphere { get; set; }

  public Prefab SpherePrefab { get; set; }

  public override async Task Execute()
  {
    var uiPage = this.Entity.Get<UIComponent>().Page;

    var addChildSphereButton = uiPage.RootElement.FindVisualChildOfType<Button>("AddChildSphereButton");
    addChildSphereButton.Click += this.HandleAddChildSphereButtonClick;

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
}
