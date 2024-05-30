using UnityEngine;

public interface IEngine 
{
    void InitEngine();

    void UpdateEngine(Rigidbody rigidbody, float forceCoefficient);
}
