using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WateringHitDetection : MonoBehaviour
{
    [SerializeField] private ParticleSystem _waterParticle;
    public List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    private Texture2D _wetnessMap;
    private Terrain _terrain;
    private int _wetnessMapSize = 512;

    void Start()
    {
        _terrain = GetComponent<Terrain>();
        InitializeWetnessMap();
    }

    void InitializeWetnessMap()
    {
        _wetnessMap = new Texture2D(_wetnessMapSize, _wetnessMapSize, TextureFormat.RGBA32, false);
        for (int y = 0; y < _wetnessMapSize; y++)
        {
            for (int x = 0; x < _wetnessMapSize; x++)
            {
                _wetnessMap.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }
        _wetnessMap.Apply();
        _terrain.materialTemplate.SetTexture("_WetnessMap", _wetnessMap);
    }

    private void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = _waterParticle.GetCollisionEvents(this.gameObject, collisionEvents);

        foreach (ParticleCollisionEvent collisionEvent in collisionEvents)
        {
            Vector3 pos = collisionEvent.intersection;
            UpdateWetnessMap(pos);
        }
    }

    void UpdateWetnessMap(Vector3 position)
    {
        Vector3 terrainPos = position - _terrain.transform.position;
        Vector3 mapCoord = new Vector3(
            terrainPos.x / _terrain.terrainData.size.x,
            0,
            terrainPos.z / _terrain.terrainData.size.z
        );

        int x = (int)(mapCoord.x * _wetnessMapSize);
        int y = (int)(mapCoord.z * _wetnessMapSize);

        Color currentColor = _wetnessMap.GetPixel(x, y);
        _wetnessMap.SetPixel(x, y, new Color(0, 0, 0, Mathf.Clamp01(currentColor.a + 0.1f)));
        _wetnessMap.Apply();
    }
}
