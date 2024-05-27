using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class BaseRigidBody : MonoBehaviour 
{
    [Header("Rigid Body Properties")]

    protected float _startDrag;

    protected float _startAngularDrag;

    protected Rigidbody _rigidBody;

    [SerializeField] private float _weight = 0.454f;

    private void Awake() 
    {
        _rigidBody = GetComponent<Rigidbody>();

        if (_rigidBody) {
            _rigidBody.mass = _weight;
            _startDrag = _rigidBody.drag;
            _startAngularDrag = _rigidBody.angularDrag;
        }
    }

    private void FixedUpdate() 
    {
        if (!_rigidBody) {
            return;
        }

        HandlePhysics();
    }

    protected virtual void HandlePhysics() {}
}