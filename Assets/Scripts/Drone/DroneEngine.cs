using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DroneEngine : MonoBehaviour, IEngine 
{
    [Header("Engine Properties")]
    [SerializeField] private float _maxPower = 75f; // Максимальная мощность в ваттах
    [SerializeField] private float _voltage = 12f;
    [SerializeField] private float _current = 0;
    [SerializeField] private float _minResistance = 2f;
    [SerializeField] private float _currentResistance = 100f;
    [SerializeField] private float _maxResistance = 100f;
    [SerializeField] private float _propellerRadius = 0.1f; // Радиус пропеллера в метрах
    [SerializeField] private bool _isClockwised = true;
    [SerializeField] private Transform _propeller;

    [Header("Aerodynamics")]
    [SerializeField] private float _airDensity = 1.225f; // Плотность воздуха в кг/м^3

    [SerializeField] private float _thrust; // Тяга двигателя
    [SerializeField] private float _angularVelocity = 0f;
    [SerializeField] private float _currentRPM; // Текущие обороты двигателя
    [SerializeField] private float _currentPower = 0;
    [SerializeField] private float _torque = 0;
    [SerializeField] private float _airVelocity = 0f;
    [SerializeField] private float _pitch = 0f;

    private void Start()
    {
        float maxCurrent = _maxPower / _voltage;
        _minResistance = _voltage / maxCurrent;
    }

    public void InitEngine() 
    {
        // Здесь можно добавить логику инициализации двигателя при необходимости
    }

    public void UpdateEngine(Rigidbody rigidbody, float throttle) 
    {
        // Рассчитываем тягу (силу, поднимающую вверх сам винт)
        CalculateThrust(throttle);

        // Применяем тягу к дрону
        ApplyThrust(rigidbody);
        HandlePropeller(rigidbody);
    }

    private void CalculateThrust(float throttle)
    {
        float motorEfficiency = 0.8f;
        

        // Расчет целевого сопротивления на основе дросселя
        float targetResistance;
        if (throttle > 0)
        {
            float desiredCurrent = throttle * (_maxPower / _voltage);
            targetResistance = _voltage / desiredCurrent;
        }
        else
        {
            targetResistance = _maxResistance;
        }

        _currentResistance = Mathf.Lerp(_currentResistance, targetResistance, Time.deltaTime);
        
        // Рассчитываем текущий ток и мощность двигателя
        _current = _voltage / _currentResistance;
        _currentPower = Mathf.Min(_voltage * _current, _maxPower);

        // Рассчитываем угловую скорость двигателя (в радианах в секунду)
        float propellerArea = Mathf.PI * Mathf.Pow(_propellerRadius, 2);
        float powerFactor = motorEfficiency * _currentPower;
        _airVelocity = motorEfficiency * Mathf.Pow(2 * _currentPower / (Mathf.PI * _airDensity * Mathf.Pow(_propellerRadius*2, 2)*(1-motorEfficiency)), 1f/3f);
        _angularVelocity = Mathf.Sqrt(2 * powerFactor / (_airDensity * Mathf.PI * Mathf.Pow(_propellerRadius, 2)));

        // Рассчитываем тягу двигателя
        //_thrust = powerFactor / _voltage;
        _thrust = powerFactor / _airVelocity;

        // Рассчитываем крутящий момент
        _torque = _currentPower / _angularVelocity;

        // Переводим угловую скорость в RPM
        _currentRPM = _angularVelocity * 60f / (2 * Mathf.PI);
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

    public void HandlePropeller(Rigidbody rigidbody) 
    {
        if (!_propeller) 
        {
            return;
        }

        // Определяем направление вращения пропеллера
        float rotationDirection = _isClockwised ? 1f : -1f; // 1 - по часовой, -1 - против часовой
        _propeller.Rotate(Vector3.up, rotationDirection * _currentRPM * Time.deltaTime);
        rigidbody.AddTorque(rotationDirection * rigidbody.transform.up * _torque, ForceMode.Force);
    }
}
