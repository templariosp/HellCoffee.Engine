using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HellCoffee.Engine.Input;

public enum MouseButton { Left, Middle, Right, X1, X2 }

public class MouseInput
{
    private MouseState _previous;
    private MouseState _current;

    public Point Position => _current.Position;
    public int X => _current.X;
    public int Y => _current.Y;
    public Point Delta => _current.Position - _previous.Position;
    public int ScrollWheelDelta => _current.ScrollWheelValue - _previous.ScrollWheelValue;

    public void Update()
    {
        _previous = _current;
        _current = Mouse.GetState();
    }

    public bool IsDown(MouseButton button) => GetButtonState(_current, button) == ButtonState.Pressed;
    public bool IsUp(MouseButton button) => GetButtonState(_current, button) == ButtonState.Released;

    public bool JustPressed(MouseButton button)
        => GetButtonState(_current, button) == ButtonState.Pressed
        && GetButtonState(_previous, button) == ButtonState.Released;

    public bool JustReleased(MouseButton button)
        => GetButtonState(_current, button) == ButtonState.Released
        && GetButtonState(_previous, button) == ButtonState.Pressed;

    private static ButtonState GetButtonState(MouseState state, MouseButton button) => button switch
    {
        MouseButton.Left => state.LeftButton,
        MouseButton.Middle => state.MiddleButton,
        MouseButton.Right => state.RightButton,
        MouseButton.X1 => state.XButton1,
        MouseButton.X2 => state.XButton2,
        _ => ButtonState.Released
    };
}
