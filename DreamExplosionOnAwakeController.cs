using System.Collections;
using UnityEngine;
using OWML.Common;
using UnityEngine.Analytics;

namespace ModJamWarmup;
public class DreamExplosionOnAwakeController : MonoBehaviour
{
    [SerializeField]
    private Transform _scaleRoot;

    [SerializeField]
    private OWRenderer _flameRenderer;

    [SerializeField]
    private OWLight2 _flameLight;

    [SerializeField]
    private OWAudioSource _audioSource;

    private float _explodeTime;

    private bool _exploding;
    private Vector3 maxScale = new Vector3(256, 256, 256); // set scale amount in vector3

    public void Awake()
    {
        base.enabled = true;
        _explodeTime = Time.time + 3f;
        //StartCoroutine(AutoReset());
    }

    public void OnExitDreamWorld()
    {
        if (_exploding)
        {
            ResetExplosion();
        }
    }

    private void ResetExplosion()
    {
        _scaleRoot.localScale = Vector3.one;
        _flameRenderer.SetActivation(active: false);
        _flameLight.SetActivation(active: false);
        _exploding = false;
        base.enabled = false;
    }

    public IEnumerator AutoReset()
    {
        yield return new WaitForSeconds(3f); // wait 3 seconds before setting it disabled
        _exploding = false;
        ResetExplosion();
    }

    private void FixedUpdate()
    {
        ModJamWarmup.WriteLine("Explode? " + _exploding, MessageType.Success);
        if (!_exploding)
        {
            _exploding = true;
            _audioSource.PlayOneShot(AudioType.DreamFire_Explosion);
            _scaleRoot.localScale = Vector3.one;
            _flameRenderer.SetActivation(active: true);
            _flameLight.SetActivation(active: true);
            _flameLight.SetIntensityScale(0f);
        }
        if (_exploding)
        {
            float num = Time.time - _explodeTime;
            _scaleRoot.localScale = Vector3.one + Vector3.one * 10f * num * num;
            _flameLight.SetIntensityScale(Mathf.InverseLerp(0f, 0.5f, num));
        }
        if (_scaleRoot.localScale == maxScale)
        {
            _exploding = false;
        }
    }
}
