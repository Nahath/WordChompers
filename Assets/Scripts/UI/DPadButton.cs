using UnityEngine;
using UnityEngine.EventSystems;

// Attach to each of the four D-pad directional buttons.
// Set the Direction field in the inspector.
public class DPadButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum Direction { Up, Down, Left, Right }

    [SerializeField] private Direction direction;

    public void OnPointerDown(PointerEventData e)
        => PlayerController.Instance?.SetDPadDirection(direction, true);

    public void OnPointerUp(PointerEventData e)
        => PlayerController.Instance?.SetDPadDirection(direction, false);
}
