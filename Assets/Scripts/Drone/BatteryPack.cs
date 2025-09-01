using UnityEngine;

[System.Serializable]
public sealed class BatteryPack
{
    [Header("Pack")]
    public int cells = 3;               // S
    public float capacity_Ah = 1.5f;    // Ah
    public float R_int_ohm = 0.025f;    // Ом (всего)
    [Range(0, 1)] public float SOC = 1.0f;

    [Header("OCV per cell vs SOC")]
    public AnimationCurve ocvPerCell = new AnimationCurve(
        new Keyframe(0.0f, 3.3f),
        new Keyframe(0.2f, 3.6f),
        new Keyframe(0.5f, 3.75f),
        new Keyframe(0.8f, 3.95f),
        new Keyframe(1.0f, 4.2f)
    );

    public float Vbat { get; private set; } // В, клеммы

    public float Step(float I_pack_A, float dt)
    {
        float C_As = Mathf.Max(1e-3f, capacity_Ah * 3600f);
        SOC = Mathf.Clamp01(SOC - I_pack_A * dt / C_As); // расход
        float Voc = cells * ocvPerCell.Evaluate(SOC);
        Vbat = Mathf.Max(0f, Voc - I_pack_A * R_int_ohm);
        return Vbat;
    }
}
