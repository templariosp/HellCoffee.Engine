using HellCoffee.Engine;
using GM = HellCoffee.Engine.GameManager.GameManager;

namespace HellCoffee.Demo;

/// <summary>
/// Ponto de entrada do jogo demo.
/// Resolução virtual: 320x180 (16:9 pixel art — escala inteira 3x = 960x540, 4x = 1280x720)
/// </summary>
public class DemoGame : Core
{
    public DemoGame() : base(virtualWidth: 320, virtualHeight: 180)
    {
        Window.Title = "HellCoffee Engine - Demo";
    }

    protected override void OnInitialize()
    {
        GM.Create();
        GM.Instance.Reset();

        Scenes.ChangeSceneImmediate(new DemoScene(), Content);
    }
}
