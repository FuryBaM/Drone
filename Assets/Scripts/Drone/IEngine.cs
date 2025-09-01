public interface IEngine
{
    bool Clockwise { get; }
    void SetClockwiseRotation(bool cw);
    // Возвращает ток, потребленный от батареи (A) за текущий шаг
    float StepEngine(UnityEngine.Rigidbody rb, float duty01, float vbat, float rho);
}