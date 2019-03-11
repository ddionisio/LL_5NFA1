using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberFloaterWidget : MonoBehaviour {
    [Header("Template")]
    public GameObject template;
    public string poolGroup = "floater";
    public int poolCapacity = 4;

    private M8.PoolController mPool;

    [Header("Data")]
    public float delay = 1.5f;
    public Vector2 velocityMin;
    public Vector2 velocityMax;
    public Vector2 accelMin;
    public Vector2 accelMax;

    public void Play(Vector2 start, MixedNumber number) {
        //convert start, assume world space to screen space (UI)
        var cam = M8.Camera2D.main.unityCamera;
        Vector2 pos = cam.WorldToScreenPoint(start);

        var widget = mPool.Spawn<MixedNumberWidget>("", transform, pos, null);
        widget.number = number;

        StartCoroutine(DoActive(widget));
    }

    public void Clear() {
        StopAllCoroutines();

        mPool.ReleaseAllByType(template.name);
    }

    void Awake() {
        mPool = M8.PoolController.CreatePool(poolGroup);
        mPool.AddType(template, poolCapacity, poolCapacity);
    }

    IEnumerator DoActive(MixedNumberWidget widget) {

        var trans = widget.transform;

        var vel = new Vector2(Random.Range(velocityMin.x, velocityMax.x), Random.Range(velocityMin.y, velocityMax.y));
        var accel = new Vector2(Random.Range(accelMin.x, accelMax.x), Random.Range(accelMin.y, accelMax.y));

        var curTime = 0f;
        while(curTime < delay) {
            yield return null;

            var deltaTime = Time.deltaTime;

            curTime += deltaTime;

            Vector2 curPos = trans.position;
            curPos += vel * deltaTime;

            trans.position = curPos;

            vel += accel * deltaTime;
        }

        mPool.Release(widget.gameObject);
    }
}
