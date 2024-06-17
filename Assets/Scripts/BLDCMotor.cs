using UnityEngine;

public class BLDCMotor : MonoBehaviour
{
    // Параметры мотора
    public float ratedVoltage = 12f; // Номинальное напряжение мотора
    public float ratedSpeed = 3000f; // Номинальная скорость мотора (обороты в минуту)
    public float torqueConstant = 0.1f; // Коэффициент момента мотора
    public float friction = 0.01f; // Коэффициент трения
    public float loadTorque = 0.1f; // Момент нагрузки на мотор

    // Входные параметры
    public float inputVoltage = 0f; // Входное напряжение

    // Текущие параметры мотора
    [SerializeField] private float angularVelocity = 0f; // Угловая скорость мотора (радианы в секунду)
    [SerializeField] private float backEMF = 0f; // Обратная ЭДС мотора

    void Update()
    {
        // Рассчитываем обратную ЭДС
        backEMF = CalculateBackEMF(angularVelocity);

        // Рассчитываем угловую скорость
        angularVelocity = CalculateAngularVelocity(inputVoltage, backEMF, loadTorque);

        // Обновляем вращение мотора
        transform.Rotate(Vector3.forward, angularVelocity * Time.deltaTime * Mathf.Rad2Deg);
    }

    // Метод для расчета обратной ЭДС
    float CalculateBackEMF(float angularVelocity)
    {
        return Mathf.Abs(angularVelocity) * ratedVoltage / ratedSpeed;
    }

    // Метод для расчета угловой скорости
    float CalculateAngularVelocity(float inputVoltage, float backEMF, float loadTorque)
    {
        // Рассчитываем электрическую скорость мотора (обороты в минуту)
        float electricalSpeed = (inputVoltage - backEMF) / ratedVoltage * ratedSpeed;

        // Рассчитываем механическую скорость мотора (радианы в секунду)
        float mechanicalSpeed = electricalSpeed * 2 * Mathf.PI / 60;

        // Рассчитываем ускорение мотора с учетом момента нагрузки и трения
        float acceleration = (torqueConstant * (inputVoltage - backEMF) - friction * angularVelocity - loadTorque) / 0.1f;

        // Рассчитываем новую угловую скорость мотора
        float newAngularVelocity = angularVelocity + acceleration * Time.deltaTime;

        return newAngularVelocity;
    }
}
