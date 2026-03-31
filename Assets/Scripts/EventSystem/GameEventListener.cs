using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private CustomGameEvent response = null;

    private void OnEnable()
    {
        gameEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        gameEvent.UnregisterListener(this);
    }

    public void OnEventRaised(Component sender, object parameter)
    {
        response?.Invoke(sender, parameter);
    }

    public void OnEventRaised(Component sender, GameObject target, object parameter)
    {
        if (target != gameObject)
            return;

        response?.Invoke(sender, parameter);
    }
}
