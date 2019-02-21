using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAttackController : MonoBehaviour {
    [Header("Data")]
    public int attackCount = 1;
    public int opCount = 2;
    public float timerDelay = 10f;
    public float postAttackDelay = 2f; //delay after attack is finished
    public MixedNumberGroup[] fixedGroups; //fill operands with these numbers
    public MixedNumberGroup[] numberGroups;    

    [Header("UI")]
    public TimerWidget timerWidget;
    public CounterWidget counterWidget;
    public MixedNumberOpsWidget opsWidget;
    public CardDeckWidget deckWidget;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeReady;

    [Header("Signal")]
    public SignalBoolean signalAnswer;

    public bool isPlaying { get { return mRout != null; } }

    public MixedNumber attackTotalNumber { get; private set; }

    private CombatCharacterController mAttacker;
    private CombatCharacterController mDefender;

    private M8.CacheList<MixedNumber> mAttackNumbers;    
    private MixedNumberOps mOperations;

    private int mCurNumbersIndex = 0;
    private int mCurFixedNumbersIndex = 0;

    private Coroutine mRout;

    private bool mIsAnswerSubmitted;
    private bool mIsAnswerCorrect;
    private MixedNumber mAnswerNumber;

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

        //apply fixed numbers
        if(fixedGroups.Length > 0) {
            var fixedNumbers = fixedGroups[mCurFixedNumbersIndex].numbers;
            M8.ArrayUtil.Shuffle(fixedNumbers);
            mCurFixedNumbersIndex++;
            if(mCurFixedNumbersIndex == fixedGroups.Length)
                mCurFixedNumbersIndex = 0;

            var count = Mathf.Min(fixedNumbers.Length, mOperations.operands.Length);
            for(int i = 0; i < count; i++)
                mOperations.operands[i].ApplyNumber(fixedNumbers[i]);
            for(int i = count; i < mOperations.operands.Length; i++)
                mOperations.operands[i].RemoveNumber();
        }
        //

        //this will reset the operation
        opsWidget.operation = mOperations;

        if(timerWidget) {
            timerWidget.SetActive(false);
            timerWidget.delay = timerDelay;
            timerWidget.ResetValue();
        }

        if(counterWidget) counterWidget.Init(attackCount);

        if(deckWidget) deckWidget.Clear();

        mAttacker = attacker;
        mDefender = defender;
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

    void OnDestroy() {
        if(signalAnswer)
            signalAnswer.callback -= OnAnswerSubmit;
    }

    void Awake() {
        if(animator && !string.IsNullOrEmpty(takeReady))
            animator.ResetTake(takeReady);
    }

    IEnumerator DoPlay() {
        //ready animation countdown thing
        if(animator && !string.IsNullOrEmpty(takeReady))
            yield return animator.PlayWait(takeReady);

        //show interfaces
        opsWidget.Show();
        while(opsWidget.isBusy)
            yield return null;

        if(timerWidget) timerWidget.Show();
        if(counterWidget) counterWidget.Show();
        //

        //timerWidget.ResetValue();

        attackTotalNumber = new MixedNumber();

        var waitBrief = new WaitForSeconds(0.3f);

        //loop
        for(int attackIndex = 0; attackIndex < attackCount; attackIndex++) {
            //show and fill deck
            if(deckWidget) {
                deckWidget.Show();
                while(deckWidget.isBusy)
                    yield return null;
            }

            FillSlots();
            yield return waitBrief;

            if(timerWidget) timerWidget.SetActive(true);
            //

            //listen for answer
            mIsAnswerSubmitted = false;
            signalAnswer.callback += OnAnswerSubmit;

            //wait for correct answer, or time expired
            while(true) {
                if(mIsAnswerSubmitted) {
                    if(mIsAnswerCorrect)
                        break;

                    mIsAnswerSubmitted = false;
                }

                //ignore timer expire if we are at first attack
                if(attackIndex > 0 && IsTimerExpired())
                    break;

                yield return null;
            }

            //ready for next
            if(timerWidget) timerWidget.SetActive(false);

            signalAnswer.callback -= OnAnswerSubmit;

            RefreshOperands();

            if(deckWidget) {
                deckWidget.Hide();
                while(deckWidget.isBusy)
                    yield return null;

                deckWidget.Clear();
            }
            //

            //add answer if submitted and correct
            if(mIsAnswerSubmitted && mIsAnswerCorrect) {
                mAttackNumbers.Add(mAnswerNumber);
                attackTotalNumber += mAnswerNumber;

                if(counterWidget) counterWidget.FillIncrement();
            }

            //check if time expired, exception for attackIndex = 0
            if(attackIndex > 0 && IsTimerExpired()) {
                break;
            }
        }

        //hide interfaces
        if(timerWidget) timerWidget.Hide();
        if(counterWidget) counterWidget.Hide();

        opsWidget.Hide();
        while(opsWidget.isBusy)
            yield return null;
        //

        //show defender's hp
        mDefender.hpWidget.Show();

        //do attack routine
        mAttacker.action = CombatCharacterController.Action.Attack;
        mDefender.action = CombatCharacterController.Action.Defend;

        while(mAttacker.isBusy)
            yield return null;

        mDefender.action = CombatCharacterController.Action.Hurt;

        //do hits
        for(int i = 0; i < mAttackNumbers.Count; i++) {
            var attackNum = mAttackNumbers[i];

            mDefender.hpCurrent -= attackNum.fValue;

            //do fancy hit effect
            yield return new WaitForSeconds(0.3f);
        }

        //return to idle, death for defender if hp = 0
        mAttacker.action = CombatCharacterController.Action.Idle;

        if(mDefender.hpCurrent > 0f)
            mDefender.action = CombatCharacterController.Action.Idle;        
        else
            mDefender.action = CombatCharacterController.Action.Death;

        while(mAttacker.isBusy || mDefender.isBusy)
            yield return null;
        //

        if(postAttackDelay > 0f)
            yield return new WaitForSeconds(postAttackDelay);

        //hide defender's hp
        mDefender.hpWidget.Hide();

        mRout = null;
    }

    void OnAnswerSubmit(bool correct) {
        if(!mIsAnswerSubmitted) {
            mAnswerNumber = opsWidget.answerInput.number;
            mIsAnswerCorrect = correct;
            mIsAnswerSubmitted = true;
        }
    }

    private void FillSlots() {
        if(deckWidget) deckWidget.Fill(numberGroups[mCurNumbersIndex].numbers);

        mCurNumbersIndex++;
        if(mCurNumbersIndex == numberGroups.Length)
            mCurNumbersIndex = 0;
    }

    private void RefreshOperands() {
        if(fixedGroups.Length > 0) {
            var fixedNumbers = fixedGroups[mCurFixedNumbersIndex].numbers;
            M8.ArrayUtil.Shuffle(fixedNumbers);
            mCurFixedNumbersIndex++;
            if(mCurFixedNumbersIndex == fixedGroups.Length)
                mCurFixedNumbersIndex = 0;

            if(fixedNumbers.Length > 0) {
                var count = Mathf.Min(fixedNumbers.Length, opsWidget.operation.operands.Length);
                for(int i = 0; i < count; i++)
                    opsWidget.operation.operands[i].ApplyNumber(fixedNumbers[i]);
                for(int i = count; i < mOperations.operands.Length; i++)
                    opsWidget.operation.operands[i].RemoveNumber();

                opsWidget.ApplyCurrentOperation();
            }
            else
                opsWidget.ClearOperands();
        }
        else
            opsWidget.ClearOperands();
    }

    private bool IsTimerExpired() {
        return timerWidget && timerWidget.value <= 0f;
    }
}
