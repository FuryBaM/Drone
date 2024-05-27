using UnityEngine;

public class DirectionIndicator : MonoBehaviour
{
    [SerializeField] private Rigidbody _targetRigidbody;
    [SerializeField] private GameObject _indicatorPrefab;
    [SerializeField] private float _indicatorDistance = 2f;
    [SerializeField] private float _indicatorRotationSpeed = 180f;

    private GameObject _indicator;

    private void Start()
    {
        if (_indicatorPrefab != null)
        {
            _indicator = Instantiate(_indicatorPrefab, transform.position, Quaternion.identity);
            _indicator.transform.parent = transform;
        }
    }

    private void Update()
    {
        if (_targetRigidbody != null && _indicator != null)
        {
            Vector3 velocityDirection = _targetRigidbody.velocity.normalized;
            _indicator.transform.position = transform.position + velocityDirection * _indicatorDistance;
            Quaternion targetRotation = Quaternion.LookRotation(velocityDirection, transform.up);
            _indicator.transform.rotation = Quaternion.RotateTowards(_indicator.transform.rotation, targetRotation, _indicatorRotationSpeed * Time.deltaTime);
        }
    }
}
