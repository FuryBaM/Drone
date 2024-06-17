using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneStatusView : MonoBehaviour
{
    [SerializeField] private DroneController _droneController;
    private Rigidbody _rigidbody;
    private DroneInputs _input;
    private GUIStyle _backgroundStyle;
    private Texture2D _backgroundTexture;

    private void Start()
    {
        _input = GetComponent<DroneInputs>();
        if (_droneController == null)
        {
            TryGetComponent<DroneController>(out _droneController);
        }
        TryGetComponent<Rigidbody>(out _rigidbody);

        // Create a texture and set its color
        _backgroundTexture = new Texture2D(1, 1);
        Color color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        _backgroundTexture.SetPixel(0, 0, color);
        _backgroundTexture.Apply();

        // Create a GUIStyle and set its background to the texture
        _backgroundStyle = new GUIStyle();
        _backgroundStyle.normal.background = _backgroundTexture;
        _backgroundStyle.padding = new RectOffset(10, 10, 10, 10);
    }

    private void OnGUI()
    {
        Vector3 currentAngles = _rigidbody.rotation.eulerAngles;

        GUILayout.BeginArea(new Rect(10, 10, 160, 160), _backgroundStyle);
        GUILayout.Label($"Throttle Input: {_input.Throttle}");
        GUILayout.Label($"Pitch Input: {_input.Cyclic.y}");
        GUILayout.Label($"Roll Input: {_input.Cyclic.x}");
        GUILayout.Label($"Yaw Input: {_input.Pedals}");
        GUILayout.EndArea();

        float currentPitch = currentAngles.x > 180f ? currentAngles.x - 360f : currentAngles.x;
        float currentRoll = currentAngles.z > 180f ? currentAngles.z - 360f : currentAngles.z;

        GUILayout.BeginArea(new Rect(180, 10, 160, 160), _backgroundStyle);
        GUILayout.Label($"Current Pitch: {Mathf.Round(currentPitch)}");
        GUILayout.Label($"Current Roll: {Mathf.Round(currentRoll)}");
        GUILayout.EndArea();
    }
}
