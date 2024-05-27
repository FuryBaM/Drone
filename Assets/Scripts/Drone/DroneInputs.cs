using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]

public class DroneInputs : MonoBehaviour 
{
  private Vector2 _cyclic;
  
  private float _throttle;
  
  private float _pedals;
  
  public Vector2 Cyclic 
  {
    get => _cyclic;
  }

  public float Throttle 
  {
    get => _throttle;
  }

  public float Pedals 
  {
    get => _pedals;
  }
  
  private void OnCyclic(InputValue value) 
  {
    _cyclic = value.Get<Vector2>();
  }
  
  private void OnThrottle(InputValue value) 
  {
    _throttle = value.Get<float>();
  }
  
  private void OnPedals(InputValue value) 
  {
    _pedals = value.Get<float>();
  }
}

