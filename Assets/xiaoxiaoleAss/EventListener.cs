using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,IPointerEnterHandler
{
    public Action onclick;
    public Action onPointDown;
    public Action onPointUp;
    public Action onPointEnter;

    public Action onLongPress;
    public float durationThreshold = 0.4f;
    private bool isPointerDown = false;
    private bool longPressTriggered = false;
    private float timePressStarted;

    void Update(){
        if(onLongPress != null && isPointerDown && !longPressTriggered){
            if(Time.time - timePressStarted > durationThreshold)
            {
                longPressTriggered = true;
                onLongPress.Invoke();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(!longPressTriggered){
            if(onclick != null) onclick();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        timePressStarted = Time.time;
        isPointerDown = true;
        longPressTriggered = false;

        if(null != onPointDown) onPointDown();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(null != onPointUp) onPointUp();
        isPointerDown = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointEnter?.Invoke();
    }
    
    public static void RemoveEventListener(GameObject obj){
        EventListener listener = obj.GetComponent<EventListener>();
        if(listener == null) return;
        GameObject.Destroy(listener);
    }
}