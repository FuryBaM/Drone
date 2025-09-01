using UnityEngine;

[System.Serializable]
public sealed class BLDCMotorPhys
{
    [Header("Electrical")]
    public float Kv_rpm_per_V = 2300f;
    public float R = 0.12f;        // Ом
    public float L = 0.0002f;      // Гн
    public float I0 = 0.8f;        // А, ток холостого хода (потери)

    [Header("Mechanical")]
    public float J = 1.2e-5f;      // кг·м^2 (ротор+проп)
    public float B = 1.0e-5f;      // Н·м·с/рад

    // state
    public float i;                // А
    public float omega;            // рад/с

    float Ke => 60f / (2f * Mathf.PI * Mathf.Max(1e-6f, Kv_rpm_per_V)); // В·с/рад
    float Kt => Ke; // Н·м/А

    // ESC потери как коэффициент <1
    public float Step(
        float dt, float Vin_eff, float loadTorque,
        out float tau_em, out float E, out float Pin_W, out float Pout_W, out float eff)
    {
        // substeps для устойчивости механики
        int N = Mathf.Clamp(Mathf.CeilToInt(dt * 4f), 1, 16);
        float h = dt / N;

        for (int k = 0; k < N; k++)
        {
            // back-EMF
            E = (60f / (2f * Mathf.PI * Mathf.Max(1e-6f, Kv_rpm_per_V))) * omega; // Ke*omega

            // аналитическое решение RL: i(t+h) = i_inf + (i - i_inf) * exp(-R/L h)
            float R_ = Mathf.Max(R, 1e-5f);
            float L_ = Mathf.Max(L, 1e-7f);
            float i_inf = (Vin_eff - E) / R_;
            float a = Mathf.Exp(-R_ / L_ * h);
            i = i_inf + (i - i_inf) * a;

            // электромомент
            float Kt = 60f / (2f * Mathf.PI * Mathf.Max(1e-6f, Kv_rpm_per_V));
            tau_em = Kt * Mathf.Max(0f, i - I0);

            // механика
            float J_ = Mathf.Max(J, 1e-8f);
            float B_ = Mathf.Max(B, 0f);
            float domega = (tau_em - B_ * omega - Mathf.Max(0f, loadTorque)) / J_;
            omega = Mathf.Max(0f, omega + domega * h);
        }

        // мощности и КПД
        E = (60f / (2f * Mathf.PI * Mathf.Max(1e-6f, Kv_rpm_per_V))) * omega;
        tau_em = (60f / (2f * Mathf.PI * Mathf.Max(1e-6f, Kv_rpm_per_V))) * Mathf.Max(0f, i - I0);
        Pin_W = Mathf.Max(0f, Vin_eff * Mathf.Max(i, 0f));
        Pout_W = Mathf.Max(0f, tau_em * omega);
        eff = Mathf.Clamp01(Pout_W / Mathf.Max(Pin_W, 1e-6f));
        return omega;
    }
}
