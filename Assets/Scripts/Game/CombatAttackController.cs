using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAttackController : MonoBehaviour {
    [Header("Data")]
    public int attackCount = 1;
    public int opCount = 2;
    public float postAttackDelay = 2f; //delay after attack is finished
    public float hitPerDelay = 1f; //delay per hit subtracting hp
    public MixedNumberGroup[] fixedGroups; //fill operands with these numbers
    public MixedNumber[] initialDeck; //fill deck the first time with these values
    public MixedNumberGroup[] numberGroups;    

    [Header("UI")]
    public TimerWidget timerWidget;
    public CounterWidget counterWidget;
    public MixedNumberOpsWidget opsWidget;
    public CardDeckWidget deckWidget;

    [Header("Damage Floater")]
    public NumberFloaterWidget damageFloater;
    public Transform damageFloaterAnchor;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnter = "enter";
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeExit = "exit";
    public float readyDelay = 1.5f;

    [Header("Audio")]
    [M8.SoundPlaylist]
    public string audioHit = "hit";
    [M8.SoundPlaylist]
    public string audioDeath = "death";

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

    private bool mIsInitialDone;

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
            var fixedNumbers = fixedGroups[mCurFixedNumbersIndex].GetNumbers();
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
        opsWidget.gameObject.SetActive(false);
        opsWidget.operation = mOperations;

        if(timerWidget) {
            timerWidget.SetActive(false);
            timerWidget.delay = GameData.instance.attackDuration;
            timerWidget.ResetValue();
        }

        if(counterWidget) counterWidget.Init(attackCount);

        if(deckWidget) {
            deckWidget.gameObject.SetActive(false);
            deckWidget.Clear();
        }

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
        if(animator)
            animator.gameObject.SetActive(false);
    }

    IEnumerator DoPlay() {
        if(animator) {
            animator.gameObject.SetActive(true);

            //ready animation countdown thing
            if(!string.IsNullOrEmpty(takeEnter))
                yield return animator.PlayWait(takeEnter);

            yield return new WaitForSeconds(readyDelay);

            if(!string.IsNullOrEmpty(takeExit))
                yield return animator.PlayWait(takeExit);

            animator.gameObject.SetActive(false);
        }

        //show interfaces
        opsWidget.gameObject.SetActive(true);

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
                deckWidget.gameObject.SetActive(true);
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
                    if(mIsAnswerCorrect) {
                        mCurNumbersIndex++;
                        if(mCurNumbersIndex == numberGroups.Length)
                            mCurNumbersIndex = 0;

                        break;
                    }

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
                deckWidget.Clear();

                deckWidget.Hide();
                while(deckWidget.isBusy)
                    yield return null;

                deckWidget.gameObject.SetActive(false);
            }
            //

            //add answer if submitted and correct
            if(mIsAnswerSubmitted && mIsAnswerCorrect) {
                mAttackNumbers.Add(mAnswerNumber);
                attackTotalNumber += mAnswerNumber;

                if(counterWidget) counterWidget.FillIncrement();
            }

            //check if time expired
            if(IsTimerExpired()) {
                break;
            }
        }

        //hide interfaces
        if(timerWidget) timerWidget.Hide();
        if(counterWidget) counterWidget.Hide();

        opsWidget.Hide();
        while(opsWidget.isBusy)
            yield return null;

        opsWidget.gameObject.SetActive(false);
        //
                
        //do attack routine
        mAttacker.action = CombatCharacterController.Action.AttackEnter;
        mDefender.action = CombatCharacterController.Action.Defend;

        while(mAttacker.isBusy)
            yield return null;

        mAttacker.action = CombatCharacterController.Action.Attack;
        mDefender.action = CombatCharacterController.Action.Hurt;

        while(mAttacker.isBusy)
            yield return null;

        //show defender's hp
        mDefender.hpWidget.Show();
        while(mDefender.hpWidget.isBusy)
            yield return null;

        //do hits
        var waitHit = new WaitForSeconds(hitPerDelay);

        for(int i = 0; i < mAttackNumbers.Count; i++) {
            var attackNum = mAttackNumbers[i];

            mDefender.hpCurrent -= attackNum.fValue;

            M8.SoundPlaylist.instance.Play(audioHit, false);

            //do fancy hit effect
            if(damageFloater)
                damageFloater.Play(damageFloaterAnchor.position, attackNum);

            yield return waitHit;
        }

        //return to idle, death for defender if hp = 0
        mAttacker.action = CombatCharacterController.Action.Idle;

        yield return waitHit;

        //hide defender's hp
        mDefender.hpWidget.Hide();

        if(mDefender.hpCurrent > 0f)
            mDefender.action = CombatCharacterController.Action.Idle;
        else {
            M8.SoundPlaylist.instance.Play(audioDeath, false);

            mDefender.action = CombatCharacterController.Action.Death;
        }

        while(mAttacker.isBusy || mDefender.isBusy)
            yield return null;
        //

        if(postAttackDelay > 0f)
            yield return new WaitForSeconds(postAttackDelay);
                
        mRout = null;
    }

    //debug
    public void ForceSubmit(MixedNumber number) {
        if(!mIsAnswerSubmitted) {
            mAnswerNumber = number;
            mIsAnswerCorrect = true;
            mIsAnswerSubmitted = true;
        }
    }

    void OnAnswerSubmit(bool correct) {
        if(!mIsAnswerSubmitted) {
            mAnswerNumber = opsWidget.answerInput.number;
            mIsAnswerCorrect = correct;
            mIsAnswerSubmitted = true;
        }
    }

    private void FillSlots() {
        MixedNumber[] nums;

        if(!mIsInitialDone && initialDeck.Length > 0) {
            mIsInitialDone = true;
            nums = initialDeck;
        }
        else {
            nums = numberGroups[mCurNumbersIndex].GetNumbers();
        }

        if(deckWidget) deckWidget.Fill(nums);
    }

    private void RefreshOperands() {
        if(fixedGroups.Length > 0) {
            var fixedNumbers = fixedGroups[mCurFixedNumbersIndex].GetNumbers();
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
