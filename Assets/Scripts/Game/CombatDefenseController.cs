using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatDefenseController : MonoBehaviour {
    [Header("Data")]
    public int opCount = 2;
    public float postDefenseDelay = 2f; //delay after defense is finished
    public float postHurtDelay = 0.5f;

    public MixedNumber[] attackNumbers; //number per round
    public MixedNumber[] defendNumbers; //shuffle and grab numbers
    public int defendCardCount = 3;
    
    [Header("UI")]
    public TimerWidget timerWidget;
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

    public MixedNumber defenseTotalNumber { get; private set; }

    private CombatCharacterController mAttacker;
    private CombatCharacterController mDefender;

    private MixedNumber mDefenseNumber;
    private MixedNumberOps mOperations;

    private int mCurAttackNumberIndex = 0;

    private Coroutine mRout;

    private bool mIsAnswerSubmitted;
    private bool mIsAnswerCorrect;
    private MixedNumber mAnswerNumber;
    
    private MixedNumber[] mDefendNumbers;
    
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
        opsWidget.gameObject.SetActive(false);
        opsWidget.operation = mOperations;

        if(timerWidget) {
            timerWidget.SetActive(false);
            timerWidget.delay = GameData.instance.defendDuration;
            timerWidget.ResetValue();
        }

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

        //attack animation towards defender
        mAttacker.action = CombatCharacterController.Action.Attack;
        mDefender.action = CombatCharacterController.Action.Defend;

        while(mAttacker.isBusy)
            yield return null;
        //

        //show interfaces
        opsWidget.gameObject.SetActive(true);
        opsWidget.Show();
        while(opsWidget.isBusy)
            yield return null;

        if(timerWidget) timerWidget.Show();
        //

        //timerWidget.ResetValue();

        mAnswerNumber = new MixedNumber();

        defenseTotalNumber = new MixedNumber();

        var waitBrief = new WaitForSeconds(0.3f);

        while(!IsTimerExpired()) {
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
                deckWidget.Clear();

                deckWidget.Hide();
                while(deckWidget.isBusy)
                    yield return null;

                deckWidget.gameObject.SetActive(false);
            }
            //

            //add answer if submitted and correct, move to first operand
            if(mIsAnswerSubmitted && mIsAnswerCorrect) {
                for(int i = 1; i < opsWidget.operation.operands.Length; i++)
                    defenseTotalNumber += opsWidget.operation.operands[i].number;

                if(mAnswerNumber.fValue <= 0f) //no longer need to accumulate
                    break;

                opsWidget.MoveAnswerToOperand(0);
            }

            yield return waitBrief;
        }

        if(!(mIsAnswerCorrect || mIsAnswerSubmitted) && mAnswerNumber.fValue == 0f)
            mAnswerNumber = mOperations.operands[0].number;

        //hide interfaces
        if(timerWidget) timerWidget.Hide();

        opsWidget.Hide();
        while(opsWidget.isBusy)
            yield return null;

        opsWidget.gameObject.SetActive(false);
        //

        //hurt defender based on final answer
        var fval = mAnswerNumber.fValue;
        if(fval < 0f)
            fval = 0f;

        if(fval > 0f) {
            //show defender's hp
            mDefender.hpWidget.Show();
            while(mDefender.hpWidget.isBusy)
                yield return null;

            mDefender.hpCurrent -= fval;
            mDefender.action = CombatCharacterController.Action.Hurt;

            M8.SoundPlaylist.instance.Play(audioHit, false);

            //do fancy hit effect
            if(damageFloater)
                damageFloater.Play(damageFloaterAnchor.position, mAnswerNumber);

            //fancy floaty number
            yield return new WaitForSeconds(postHurtDelay);

            //hide defender's hp
            mDefender.hpWidget.Hide();
        }

        mAttacker.action = CombatCharacterController.Action.Idle;
        while(mAttacker.isBusy)
            yield return null;

        if(mDefender.hpCurrent > 0f)
            mDefender.action = CombatCharacterController.Action.Idle;
        else {
            M8.SoundPlaylist.instance.Play(audioDeath, false);

            mDefender.action = CombatCharacterController.Action.Death;
        }

        if(postDefenseDelay > 0f)
            yield return new WaitForSeconds(postDefenseDelay);
                
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
            mAnswerNumber.isNegative = opsWidget.answerInput.numberIsNegative;
            mIsAnswerCorrect = correct;
            mIsAnswerSubmitted = true;
        }
    }

    private void FillSlots() {
        if(mDefendNumbers == null)
            mDefendNumbers = new MixedNumber[defendCardCount];

        M8.ArrayUtil.Shuffle(defendNumbers);

        int curCount = 0;

        var card = opsWidget.operandSlots.GetCard(0);
        if(card) {
            var attackNum = card.number;

            for(int i = 0; i < defendNumbers.Length; i++) {
                //grab number, make sure denominator doesn't match others
                var num = defendNumbers[i];

                if(num.denominator == attackNum.denominator)
                    continue;

                bool isDenomMatch = false;
                for(int j = 0; j < curCount; j++) {
                    if(num.denominator == mDefendNumbers[j].denominator) {
                        isDenomMatch = true;
                        break;
                    }
                }

                if(isDenomMatch)
                    continue;

                //don't include number that is greater than attack
                if(num > attackNum)
                    continue;

                mDefendNumbers[curCount] = num;

                curCount++;
                if(curCount == defendCardCount)
                    break;
            }

            //if none of these numbers or there's only one, then just add card with same number as operand
            if(curCount <= 1) {
                var newNum = attackNum.simplified;
                if(curCount == 0 || newNum != mDefendNumbers[0]) {
                    mDefendNumbers[curCount] = newNum;
                    curCount++;
                }
            }
        }

        if(deckWidget) deckWidget.Fill(mDefendNumbers, curCount);
    }

    private MixedNumber GetNumber() {
        if(attackNumbers.Length == 0)
            return new MixedNumber();

        var num = attackNumbers[mCurAttackNumberIndex];

        mCurAttackNumberIndex++;
        if(mCurAttackNumberIndex == attackNumbers.Length)
            mCurAttackNumberIndex = 0;

        return num;
    }

    private bool IsTimerExpired() {
        return timerWidget && timerWidget.value <= 0f;
    }
}