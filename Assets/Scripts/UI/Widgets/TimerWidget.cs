using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerWidget : MonoBehaviour {
    public enum Mode {
        Increase,
        Decrease
    }

    [Header("Data")]
    [SerializeField]
    Mode _mode = Mode.Decrease;
    [SerializeField]
    float _delay = 1f;
    [Range(0, 1f)]
    [SerializeField]
    float _defaultValue = 1f;
    [SerializeField]
    public bool fillInvert = true;

    [Header("Display")]
    public GameObject rootGO;
    public Image fillImage;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeExit;

    [Header("Signal")]
    public M8.Signal signalFinished;

    public float delay { get { return _delay; } set { _delay = value; } }
    public float value {
        get { return mCurTime / _delay; }
        set {
            var t = Mathf.Clamp01(value);
            mCurTime = t * _delay;
        }
    }
    public bool isBusy { get { return mRout != null; } }

    private bool mIsActive = false;
    private float mCurTime;

    private Coroutine mRout;

    public void ResetValue() {
        mCurTime = _defaultValue * _delay;
        RefreshDisplay();
    }

    public void SetActive(bool aActive) {
        mIsActive = aActive;
    }

    public void Show() {
        Stop();
        mRout = StartCoroutine(DoShow());
    }

    public void Hide() {
        Stop();
        mRout = StartCoroutine(DoHide());
    }
        
    void OnDisable() {
        mRout = null;
    }

    void Awake() {
        if(animator && !string.IsNullOrEmpty(takeEnter))
            animator.ResetTake(takeEnter);

        if(rootGO)
            rootGO.SetActive(false);
    }

    void Update() {
        if(!mIsActive)
            return;

        bool isDone = false;

        switch(_mode) {
            case Mode.Increase:
                mCurTime += Time.deltaTime;
                if(mCurTime > _delay) {
                    mCurTime = _delay;
                    isDone = true;
                }

                RefreshDisplay();
                break;

            case Mode.Decrease:
                mCurTime -= Time.deltaTime;
                if(mCurTime < 0f) {
                    mCurTime = 0f;
                    isDone = true;
                }

                RefreshDisplay();
                break;
        }

        if(isDone) {
            mIsActive = false;

            if(signalFinished)
                signalFinished.Invoke();
        }
    }

    IEnumerator DoShow() {
        if(rootGO)
            rootGO.SetActive(true);

        if(animator && !string.IsNullOrEmpty(takeEnter))
            yield return animator.PlayWait(takeEnter);

        mRout = null;
    }

    IEnumerator DoHide() {
        if(animator && !string.IsNullOrEmpty(takeExit))
            yield return animator.PlayWait(takeExit);

        if(rootGO)
            rootGO.SetActive(false);

        mRout = null;
    }

    private void RefreshDisplay() {
        fillImage.fillAmount = fillInvert ? 1f - value : value;
    }

    private void Stop() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }
}
