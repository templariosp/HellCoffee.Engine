using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HellCoffee.Engine.Input;

public class KeyboardInput
{
    private KeyboardState _previous;
    private KeyboardState _current;

    public void Update()
    {
        _previous = _current;
        _current = Keyboard.GetState();
    }

    public bool IsDown(Keys key) => _current.IsKeyDown(key);
    public bool IsUp(Keys key) => _current.IsKeyUp(key);
    public bool JustPressed(Keys key) => _current.IsKeyDown(key) && _previous.IsKeyUp(key);
    public bool JustReleased(Keys key) => _current.IsKeyUp(key) && _previous.IsKeyDown(key);
    public bool AnyKeyPressed() => _current.GetPressedKeyCount() > 0 && _previous.GetPressedKeyCount() == 0;

    public IEnumerable<Keys> GetPressedKeys() => _current.GetPressedKeys();
}
