using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatDefenseController : MonoBehaviour {
    [Header("Data")]
    public int opCount = 2;
    public float timerDelay = 90f;
    public float postDefenseDelay = 2f; //delay after defense is finished
    public MixedNumberGroup[] attackNumberGroups; //pick one per round
    public MixedNumberGroup[] numberGroups;

    [Header("UI")]
    public TimerWidget timerWidget;
    public MixedNumberOpsWidget opsWidget;
    public CardDeckWidget deckWidget;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeReady;

    [Header("Signal")]
    public SignalBoolean signalAnswer;

    public bool isPlaying { get { return mRout != null; } }

    public MixedNumber defenseTotalNumber { get; private set; }

    private CombatCharacterController mAttacker;
    private CombatCharacterController mDefender;

    private MixedNumber mDefenseNumber;
    private MixedNumberOps mOperations;

    private int mCurAttackNumbersIndex = 0;
    private int mCurNumbersIndex = 0;

    private Coroutine mRout;

    private bool mIsAnswerSubmitted;
    private bool mIsAnswerCorrect;
    private MixedNumber mAnswerNumber;

    public void Init(CombatCharacterController attacker, CombatCharacterController defender) {
        //create empty subtract operation
        if(mOperations == null || mOperations.operands.Length != opCount) {
            mOperations = new MixedNumberOps();

            mOperations.operands = new MixedNumberOps.Operand[opCount];
            for(int i = 0; i < mOperations.operands.Length; i++)
                mOperations.operands[i] = new MixedNumberOps.Operand();

            mOperations.operators = new OperatorType[opCount - 1];
            for(int i = 0; i < mOperations.operators.Length; i++)
                mOperations.operators[i] = OperatorType.Subtract;
        }

        //fill first operand
        if(mOperations.operands.Length > 0) {
            var num = GetNumber();
            mOperations.operands[0].ApplyNumber(num);
        }

        //this will reset the operation
        opsWidget.operation = mOperations;

        if(timerWidget) {
            timerWidget.SetActive(false);
            timerWidget.delay = timerDelay;
            timerWidget.ResetValue();
        }
        
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
        //attack animation towards defender
        mAttacker.action = CombatCharacterController.Action.Attack;
        mDefender.action = CombatCharacterController.Action.Defend;

        while(mAttacker.isBusy)
            yield return null;
        //

        //ready animation countdown thing
        if(animator && !string.IsNullOrEmpty(takeReady))
            yield return animator.PlayWait(takeReady);

        //show interfaces
        opsWidget.Show();
        while(opsWidget.isBusy)
            yield return null;

        if(timerWidget) timerWidget.Show();
        //

        //timerWidget.ResetValue();

        defenseTotalNumber = new MixedNumber();

        var waitBrief = new WaitForSeconds(0.3f);

        while(!IsTimerExpired()) {
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
            while(!IsTimerExpired()) {
                if(mIsAnswerSubmitted) {
                    if(mIsAnswerCorrect)
                        break;

                    mIsAnswerSubmitted = false;
                }

                yield return null;
            }

            //ready for next
            if(timerWidget) timerWidget.SetActive(false);

            signalAnswer.callback -= OnAnswerSubmit;

            if(deckWidget) {
                deckWidget.Hide();
                while(deckWidget.isBusy)
                    yield return null;

                deckWidget.Clear();
            }
            //

            //add answer if submitted and correct, move to first operand
            if(mIsAnswerSubmitted && mIsAnswerCorrect) {
                for(int i = 1; i < opsWidget.operation.operands.Length; i++)
                    defenseTotalNumber += opsWidget.operation.operands[i].number;

                if(mAnswerNumber.fValue < 0f) //no longer need to accumulate
                    break;

                opsWidget.MoveAnswerToOperand(0);
            }

            yield return waitBrief;
        }

        //hide interfaces
        if(timerWidget) timerWidget.Hide();

        opsWidget.Hide();
        while(opsWidget.isBusy)
            yield return null;
        //

        //show defender's hp
        mDefender.hpWidget.Show();

        //hurt defender based on final answer
        var fval = mAnswerNumber.fValue;
        if(fval < 0f)
            fval = 0f;

        if(fval > 0f) {
            mDefender.hpCurrent -= fval;
            mDefender.action = CombatCharacterController.Action.Hurt;
        }

        //fancy floaty number
        yield return waitBrief;

        mAttacker.action = CombatCharacterController.Action.Idle;
        while(mAttacker.isBusy)
            yield return null;

        if(mDefender.hpCurrent > 0f)
            mDefender.action = CombatCharacterController.Action.Idle;
        else
            mDefender.action = CombatCharacterController.Action.Death;

        if(postDefenseDelay > 0f)
            yield return new WaitForSeconds(postDefenseDelay);

        //hide defender's hp
        mDefender.hpWidget.Hide();

        mRout = null;
    }

    void OnAnswerSubmit(bool correct) {
        if(!mIsAnswerSubmitted) {
            mAnswerNumber = opsWidget.answerInput.number;
            mAnswerNumber.isNegative = opsWidget.answerInput.numberIsNegative;
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

    private MixedNumber GetNumber() {
        if(attackNumberGroups.Length == 0)
            return new MixedNumber();

        var nums = attackNumberGroups[mCurAttackNumbersIndex].numbers;

        var num = nums[Random.Range(0, nums.Length)];

        mCurAttackNumbersIndex++;
        if(mCurAttackNumbersIndex == attackNumberGroups.Length)
            mCurAttackNumbersIndex = 0;

        return num;
    }

    private bool IsTimerExpired() {
        return timerWidget && timerWidget.value <= 0f;
    }
}