using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : MonoBehaviour
{
    public DropType _dropType;
    public float _value;

    public float _rotationalSpeed = 100f;
    public float _floatFrequency = 1f;
    public float _floatAmplitude = 0.5f;
    private Vector3 _initLocalPosition;
    public Transform _Model;


    private void Start()
    {
        _Model = transform.GetChild(0);
        _initLocalPosition = _Model.localPosition;
    }
    private void Update()
    {
        _Model.eulerAngles += new Vector3(0, _rotationalSpeed * Time.deltaTime, 0);
        _Model.localPosition = new Vector3(_Model.localPosition.x, _initLocalPosition.y + _floatAmplitude * Mathf.Sin(Time.time * _floatFrequency), _Model.localPosition.z);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.transform.GetComponent<PlayerCollisionControl>().PickDropItem(_dropType, _value);
            Destroy(gameObject);
        }
    }

}
