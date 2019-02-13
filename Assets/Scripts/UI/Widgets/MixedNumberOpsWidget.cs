using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MixedNumberOpsWidget : MonoBehaviour {
    
        
    [Header("Main")]
    public GameObject activeGO; //when operation is active

    [Header("Card")]
    public string cardPoolGroup = "cardPool";
    public GameObject cardTemplate; //operand display

    [Header("Operation")]
    public CardSlotsWidget operandSlots;
    public OperatorWidget[] operatorSlots;

    [Header("Answer")]
    public MixedNumberInputWidget answerInput;
        
    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeExit;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeWrong;

    [Header("Signal Invokes")]
    public SignalBoolean signalAnswer; //true if correct

    public MixedNumberOps operation {
        get { return mOperation; }

        set {
            mOperation = value.Clone(operandSlots.slots.Length);

            ApplyCurrentOperation();
        }
    }

    private M8.PoolController mPool;

    private MixedNumberOps mOperation;

    private M8.GenericParams mCardParms = new M8.GenericParams();
    
    public void Show() {
        StopAllCoroutines();

        StartCoroutine(DoShow());
    }

    public void Hide() {
        StopAllCoroutines();

        StartCoroutine(DoHide());
    }

    void OnDisable() {
        Clear();
    }

    void OnDestroy() {
        if(answerInput)
            answerInput.submitCallback -= OnInputSubmit;
    }

    void Awake() {
        mPool = M8.PoolController.CreatePool(cardPoolGroup);
        mPool.AddType(cardTemplate, operandSlots.slots.Length, operandSlots.slots.Length);
                
        answerInput.submitCallback += OnInputSubmit;

        if(animator && !string.IsNullOrEmpty(takeEnter))
            animator.ResetTake(takeEnter);
    }

    void OnInputSubmit() {
        //fail-safe, shouldn't be able to submit
        if(mOperation == null || mOperation.isAnyOperandEmpty)
            return;
                
        var opAnswer = mOperation.Evaluate();

        //NOTE: assume we can only input positives
        opAnswer.isNegative = false;

        bool isCorrect = answerInput.number.isValid && answerInput.number == opAnswer;

        if(!isCorrect) {
            if(animator && !string.IsNullOrEmpty(takeWrong))
                animator.Play(takeWrong);
        }

        signalAnswer.Invoke(isCorrect);
    }

    IEnumerator DoShow() {
        if(activeGO) activeGO.SetActive(true);

        if(animator && !string.IsNullOrEmpty(takeEnter))
            yield return animator.PlayWait(takeEnter);
    }

    IEnumerator DoHide() {
        answerInput.isLocked = true;

        if(animator && !string.IsNullOrEmpty(takeExit))
            yield return animator.PlayWait(takeExit);

        if(activeGO) activeGO.SetActive(false);
    }

    private void Clear() {
        operandSlots.Init();

        answerInput.isLocked = false;

        if(activeGO) activeGO.SetActive(false);
    }

    private void ApplyCurrentOperation() {
        Clear();

        //setup operands
        var operandCount = mOperation.operands.Length;

        for(int i = 0; i < operandCount; i++) {
            var operand = mOperation.operands[i];

            CardWidget newCard;

            if(operand.isEmpty)
                newCard = null;
            else {
                mCardParms[CardWidget.parmNumber] = operand.number;
                mCardParms[CardWidget.parmCanDragInside] = true;
                mCardParms[CardWidget.parmCanDragOutside] = false;

                newCard = mPool.Spawn<CardWidget>(cardTemplate.name, "", null, mCardParms);
            }

            operandSlots.SetCard(i, newCard);
        }

        for(int i = operandCount; i < operandSlots.slots.Length; i++) //hide other operands
            operandSlots.SetActive(i, false);

        operandSlots.ClearHighlights();
        //

        //setup operators
        int operatorCount = mOperation.operators.Length;

        for(int i = 0; i < operatorCount; i++) {
            var op = mOperation.operators[i];
            var opSlot = operatorSlots[i];

            opSlot.SetOperator(op);
            opSlot.gameObject.SetActive(true);
        }

        for(int i = operatorCount; i < operatorSlots.Length; i++) //hide other operands
            operatorSlots[i].gameObject.SetActive(false);
        //

        //setup answer input
        RefreshAnswerInput();
    }

    private void RefreshAnswerInput() {
        bool isValid = !mOperation.isAnyOperandEmpty;

        answerInput.CloseNumpad();

        if(isValid) {
            var opAnswer = mOperation.Evaluate();

            bool isWholeEnabled = Mathf.Abs(opAnswer.fValue) >= 1.0f;

            answerInput.Init(isWholeEnabled, opAnswer.isNegative);
        }
        else
            answerInput.Init(false, false);

        answerInput.isLocked = !isValid;
    }
}
