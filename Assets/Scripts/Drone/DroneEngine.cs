using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DroneEngine : MonoBehaviour, IEngine 
{
    [Header("Engine Properties")]
    [SerializeField] private float _maxPower = 400f; // Максимальная мощность в ваттах
    [SerializeField] private float _propellerRadius = 0.1f; // Радиус пропеллера в метрах
    [SerializeField] private bool _isClockwised = true;
    [SerializeField] private Transform _propeller;

    [Header("Aerodynamics")]
    [SerializeField] private float _airDensity = 1.225f; // Плотность воздуха в кг/м^3

    [SerializeField] private float _thrust; // Тяга двигателя
    [SerializeField] private float _angularVelocity = 0f;
    [SerializeField] private float _currentRPM; // Текущие обороты двигателя
    [SerializeField] private float _currentPower = 0;

    public void InitEngine() {

    }

    public void UpdateEngine(Rigidbody rigidbody, float throttle) 
    {
        // Рассчитываем тягу (силу, поднимающую вверх сам винт)
        CalculateThrust(throttle);

        // Применяем тягу к дрону
        ApplyThrust(rigidbody);
        HandlePropeller();
    }

    private void CalculateThrust(float throttle)
    {
        _currentPower = throttle * _maxPower;

        // Рассчитываем площадь сечения пропеллера
        float propellerArea = Mathf.PI * Mathf.Pow(_propellerRadius, 2);

        float force = Mathf.Pow(_currentPower * _propellerRadius * 2, 2f/3f) * Mathf.Pow(_airDensity*Mathf.PI/2, 1f/3f);
        // Рассчитываем тягу
        _thrust = 0.7f * force;
        // Вычисляем угловую скорость пропеллера и RPM
        _angularVelocity = _currentPower / (_thrust * _propellerRadius);
        _currentRPM = _angularVelocity / (2 * Mathf.PI) * 60f;
    }




    private void ApplyThrust(Rigidbody rigidbody)
    {
        // Рассчитываем силу тяги, применяемую к дрону
        Vector3 force = transform.up * _thrust;

        // Применяем силу тяги к центру массы дрона
        rigidbody.AddForceAtPosition(force, transform.position, ForceMode.Force);
    }

    public void SetClockwiseRotation(bool value)
    {
        _isClockwised = value;
    }

    public void HandlePropeller() 
    {
        if(!_propeller) 
        {
            return;
        }

        // Определяем направление вращения пропеллера
        float rotationDirection = _isClockwised ? 1f : -1f; // 1 - по часовой, -1 - против часовой
        _propeller.Rotate(Vector3.up, rotationDirection * _currentRPM * Time.deltaTime);
    }
}
