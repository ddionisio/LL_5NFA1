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

    [Header("Display")]
    public Image fillImage;

    [Header("Signal")]
    public M8.Signal signalFinished;

    public float delay { get { return _delay; } set { _delay = value; } }
    public float value { get { return mCurTime / _delay; } }

    private bool mIsActive = false;
    private float mCurTime;

    public void ResetValue() {
        mCurTime = _defaultValue * _delay;
        RefreshDisplay();
    }

    public void SetActive(bool aActive) {
        mIsActive = aActive;
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

    void RefreshDisplay() {
        fillImage.fillAmount = value;
    }
}
