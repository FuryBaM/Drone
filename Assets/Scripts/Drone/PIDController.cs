using UnityEngine;

[System.Serializable]
public sealed class PID
{
    public float kp = 1, ki = 0, kd = 0;
    [SerializeField] float integ, prev;
    public void Reset() { integ = 0; prev = 0; }
    public float Update(float err, float dt, float iClamp = 0.5f)
    {
        integ = Mathf.Clamp(integ + err * dt, -iClamp, iClamp);
        float deriv = (err - prev) / Mathf.Max(dt, 1e-4f);
        prev = err;
        return kp * err + ki * integ + kd * deriv;
    }
}
