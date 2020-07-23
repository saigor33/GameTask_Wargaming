using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class Bullet : MonoBehaviour
{
    private Vector3 _directionShoot;
    private float _timeLive;
    private float _speedInSecond;

    private Vector3 _test_StartPosShoot;

    public virtual void Init( Vector3 directionShoot, float timeLive, float speedInSecond)
    {
        _test_StartPosShoot = transform.position;

        _directionShoot = directionShoot;
        _timeLive = timeLive;
        _speedInSecond = speedInSecond;

        Vector3 targetPositon =transform.position + _directionShoot.normalized * _speedInSecond * _timeLive;
        StartCoroutine(MoveOnTime(_timeLive, targetPositon));
    }

    // ==== Дублированный код из ShipManagerController ============
    private IEnumerator MoveOnTime(float time, Vector2 targetPosition)
    {
        Vector2 startPosition = transform.position;
        float startTime = Time.realtimeSinceStartup;
        float fraction = 0f;
        while (fraction < 1f)
        {
            fraction = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / time);
            transform.position = Vector2.Lerp(startPosition, targetPosition, fraction);
            yield return null;
        }

        //Debug.Log($"LastPositinBullet = ({transform.position.x}; {transform.position.y}) ");
        //Debug.Log($"FinishPos = {Vector3.Distance(_test_StartPosShoot, transform.position)}");
        Destroy(gameObject);
    }

}
