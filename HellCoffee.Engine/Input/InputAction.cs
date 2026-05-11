using Microsoft.Xna.Framework.Input;

namespace HellCoffee.Engine.Input;

/// <summary>
/// Mapeia uma ação de jogo (ex: "jump") a um conjunto de teclas e botões de gamepad.
/// Permite customização fácil de controles sem alterar código de gameplay.
/// </summary>
public class InputAction
{
    private readonly List<Keys> _keys = new();
    private readonly List<Buttons> _buttons = new();

    public string Name { get; }

    public InputAction(string name)
    {
        Name = name;
    }

    public InputAction AddKey(Keys key) { _keys.Add(key); return this; }
    public InputAction AddButton(Buttons button) { _buttons.Add(button); return this; }

    public bool IsDown(InputManager input)
        => _keys.Any(k => input.Keyboard.IsDown(k))
        || (input.Gamepad.IsConnected && _buttons.Any(b => input.Gamepad.IsDown(b)));

    public bool JustPressed(InputManager input)
        => _keys.Any(k => input.Keyboard.JustPressed(k))
        || (input.Gamepad.IsConnected && _buttons.Any(b => input.Gamepad.JustPressed(b)));

    public bool JustReleased(InputManager input)
        => _keys.Any(k => input.Keyboard.JustReleased(k))
        || (input.Gamepad.IsConnected && _buttons.Any(b => input.Gamepad.JustReleased(b)));
}
