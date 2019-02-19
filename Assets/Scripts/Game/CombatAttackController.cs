using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAttackController : MonoBehaviour {
    [Header("Data")]
    public int attackCount = 1;
    public int opCount = 2;
    public MixedNumber[][] numbers;

    [Header("UI")]
    public MixedNumberOpsWidget opsWidget;
    public CardDeckWidget deckWidget;
    public TimerWidget timerWidget;
    public CounterWidget counterWidget;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeReady;

    public bool isPlaying { get { return mRout != null; } }

    private CombatCharacterController mAttacker;
    private CombatCharacterController mDefender;

    private M8.CacheList<MixedNumber> mAttackNumbers;
    private MixedNumberOps mOperations;

    private int mCurNumbersIndex = 0;

    private Coroutine mRout;

    public void Init(CombatCharacterController attacker, CombatCharacterController defender) {
        if(mAttackNumbers == null || mAttackNumbers.Capacity != attackCount)
            mAttackNumbers = new M8.CacheList<MixedNumber>(attackCount);
        else
            mAttackNumbers.Clear();

        //create empty sum operation
        if(mOperations == null || mOperations.operands.Length != opCount) {
            mOperations = new MixedNumberOps();

            mOperations.operands = new MixedNumberOps.Operand[opCount];
            for(int i = 0; i < mOperations.operands.Length; i++)
                mOperations.operands[i] = new MixedNumberOps.Operand();

            mOperations.operators = new OperatorType[opCount - 1];
            for(int i = 0; i < mOperations.operators.Length; i++)
                mOperations.operators[i] = OperatorType.Add;
        }

        //this will reset the operation
        opsWidget.operation = mOperations;
                
        timerWidget.SetActive(false);
        timerWidget.ResetValue();

        counterWidget.Init(attackCount);

        deckWidget.Clear();

        mAttacker = attacker;
        mDefender = defender;
    }

    public void FillSlots() {
        deckWidget.Fill(numbers[mCurNumbersIndex]);

        mCurNumbersIndex++;
        if(mCurNumbersIndex == numbers.Length)
            mCurNumbersIndex = 0;
    }

    public void Play() {
        mRout = StartCoroutine(DoPlay());
    }

    public void Stop() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }

    void OnDisable() {
        Stop();
    }

    void Awake() {
        if(animator && !string.IsNullOrEmpty(takeReady))
            animator.ResetTake(takeReady);
    }

    IEnumerator DoPlay() {
        if(animator && !string.IsNullOrEmpty(takeReady))
            yield return animator.PlayWait(takeReady);



        mRout = null;
    }
}
