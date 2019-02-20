using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAttackController : MonoBehaviour {
    [Header("Data")]
    public int attackCount = 1;
    public int opCount = 2;
    public float timerDelay = 10f;
    public MixedNumber[][] numbers;

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

    private CombatCharacterController mAttacker;
    private CombatCharacterController mDefender;

    private M8.CacheList<MixedNumber> mAttackNumbers;
    private MixedNumberOps mOperations;

    private int mCurNumbersIndex = 0;

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

        //this will reset the operation
        opsWidget.operation = mOperations;
                
        timerWidget.SetActive(false);
        timerWidget.delay = timerDelay;
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

        timerWidget.Show();
        counterWidget.Show();
        //

        //timerWidget.ResetValue();

        var waitBrief = new WaitForSeconds(0.3f);

        //loop
        for(int attackIndex = 0; attackIndex < attackCount; attackIndex++) {
            //fill deck
            deckWidget.Show();
            while(deckWidget.isBusy)
                yield return null;

            FillSlots();
            yield return waitBrief;

            timerWidget.SetActive(true);
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
                if(attackIndex > 0 && timerWidget.value <= 0f)
                    break;

                yield return null;
            }

            //ready for next
            timerWidget.SetActive(false);

            signalAnswer.callback -= OnAnswerSubmit;

            opsWidget.ClearOperands();

            deckWidget.Hide();
            while(deckWidget.isBusy)
                yield return null;

            deckWidget.Clear();
            //

            //add answer if submitted and correct
            if(mIsAnswerSubmitted && mIsAnswerCorrect) {
                mAttackNumbers.Add(mAnswerNumber);

                counterWidget.FillIncrement();
            }

            //check if time expired, exception for attackIndex = 0
            if(attackIndex > 0 && timerWidget.value <= 0f) {

                break;
            }
        }

        //hide interfaces
        timerWidget.Hide();
        counterWidget.Hide();

        opsWidget.Hide();
        while(opsWidget.isBusy)
            yield return null;
        //

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

        mRout = null;
    }

    void OnAnswerSubmit(bool correct) {
        if(!mIsAnswerSubmitted) {
            mAnswerNumber = opsWidget.answerInput.number;
            mIsAnswerCorrect = correct;
            mIsAnswerSubmitted = true;
        }
    }
}
