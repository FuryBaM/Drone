using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class BaseRigidBody : MonoBehaviour
{
    protected Rigidbody rb;
    protected virtual void Awake() { rb = GetComponent<Rigidbody>(); }
    protected virtual void FixedUpdate() { HandlePhysics(); }
    protected abstract void HandlePhysics();
}
