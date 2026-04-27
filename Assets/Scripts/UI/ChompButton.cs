using UnityEngine;
using UnityEngine.EventSystems;

// Attach to the round Chomp button (mobile only).
public class ChompButton : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData e)
        => PlayerController.Instance?.TriggerChomp();
}
