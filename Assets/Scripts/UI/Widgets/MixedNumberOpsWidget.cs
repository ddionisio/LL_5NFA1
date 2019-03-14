using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MixedNumberOpsWidget : MonoBehaviour {
    
        
    [Header("Main")]
    public GameObject activeGO; //when operation is active

    [Header("Card")]
    public string cardPoolGroup = "cardPool";
    public int cardPoolCapacity = 4;
    public GameObject cardTemplate; //operand display
    public bool cardWholeEnabled = true;
    
    [Header("Operation")]
    public CardSlotsWidget operandSlots;
    public OperatorWidget[] operatorSlots;

    [Header("Answer")]
    public bool answerWholeEnabled = true;
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

    [Header("Drag Instruction")]
    //drag instruct will be enabled until all slots are filled
    public DragToGuideWidget dragInstruct; //if valid, drag instruction is enabled
    public GameObject dragInstructDialogGO; //active druing drag instruct
    public CardDeckWidget dragInstructCardDeckWidget; //where to find cards for drag instruct

    public MixedNumberOps operation {
        get { return mOperation; }

        set {
            mOperation = value.Clone(operandSlots.slots.Length);

            ApplyCurrentOperation();
        }
    }

    public bool isShown { get { return activeGO ? activeGO.activeSelf : true; } }

    public bool isBusy { get { return mRout != null; } }

    public bool answerLocked {
        get { return mAnswerIsLocked || answerInput.isLocked; }
        set {
            if(mAnswerIsLocked != value) {
                mAnswerIsLocked = value;

                if(mAnswerIsLocked)
                    answerInput.isLocked = true;
                else {
                    bool isValid = mOperation != null ? !mOperation.isAnyOperandEmpty : false;

                    answerInput.isLocked = !isValid;
                }
            }
        }
    }

    public event System.Action slotUpdateCallback;

    private M8.PoolController mPool;

    private MixedNumberOps mOperation;

    private M8.GenericParams mCardParms = new M8.GenericParams();

    private Coroutine mRout;
    private Coroutine mDragInstructRout;
    private bool mAnswerIsLocked = false;

    private bool mDragInstructEnabled;
    private bool mDragInstructSlotUpdated;

    public void ApplyCurrentOperation() {
        operandSlots.Init();

        //setup operands
        var operandCount = mOperation.operands.Length;

        for(int i = 0; i < operandCount; i++) {
            var operand = mOperation.operands[i];

            CardWidget newCard;

            if(operand.isEmpty)
                newCard = null;
            else {
                mCardParms[CardWidget.parmWholeEnabled] = cardWholeEnabled;
                mCardParms[CardWidget.parmNumber] = operand.number;
                mCardParms[CardWidget.parmCanDragInside] = true;
                mCardParms[CardWidget.parmCanDragOutside] = false;
                mCardParms[CardWidget.parmFractionVisual] = true;
                mCardParms[CardWidget.parmCardDrop] = operandSlots;
                mCardParms[CardWidget.parmCardDropIndex] = i;

                newCard = mPool.Spawn<CardWidget>(cardTemplate.name, "", null, mCardParms);
            }

            operandSlots.SetCard(i, newCard);
        }

        for(int i = operandCount; i < operandSlots.slots.Length; i++) //hide other operands
            operandSlots.SetActive(i, false);
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

    /// <summary>
    /// This will clear out all operand slots and reset input
    /// </summary>
    public void ClearOperands() {
        operandSlots.Init();

        for(int i = 0; i < mOperation.operands.Length; i++)
            mOperation.operands[i].RemoveNumber();

        RefreshAnswerInput();
    }

    /// <summary>
    /// Note: this will clear out the slots, then create a card for given slot index
    /// </summary>
    public void MoveAnswerToOperand(int opIndex) {
        //clear out operands
        operandSlots.Init();

        for(int i = 0; i < mOperation.operands.Length; i++)
            mOperation.operands[i].RemoveNumber();
        //

        var answerNumber = answerInput.number;
        answerNumber.isNegative = answerInput.numberIsNegative;            

        mOperation.operands[opIndex].ApplyNumber(answerNumber);

        mCardParms[CardWidget.parmNumber] = answerNumber;
        mCardParms[CardWidget.parmCanDragInside] = true;
        mCardParms[CardWidget.parmCanDragOutside] = false;
        mCardParms[CardWidget.parmFractionVisual] = true;
        mCardParms[CardWidget.parmCardDrop] = operandSlots;
        mCardParms[CardWidget.parmCardDropIndex] = opIndex;

        var newCard = mPool.Spawn<CardWidget>(cardTemplate.name, "", null, mCardParms);

        //allow card to slide from answer to its proper slot
        newCard.transform.position = answerInput.transform.position;
        newCard.MoveDragAnchorToOrigin();

        operandSlots.SetCard(opIndex, newCard);

        RefreshAnswerInput();
    }
    
    public void Show() {
        ClearRoutine();

        mRout = StartCoroutine(DoShow());
    }

    public void Hide() {
        ClearRoutine();

        mRout = StartCoroutine(DoHide());
    }
    
    void OnDisable() {
        ClearRoutine();
        
        operandSlots.Init();

        if(activeGO) activeGO.SetActive(false);
    }

    void OnDestroy() {
        if(answerInput)
            answerInput.submitCallback -= OnInputSubmit;

        if(operandSlots)
            operandSlots.updateCallback -= OnSlotUpdated;
    }

    void Awake() {
        mPool = M8.PoolController.CreatePool(cardPoolGroup);
        mPool.AddType(cardTemplate, cardPoolCapacity, cardPoolCapacity);
                
        answerInput.submitCallback += OnInputSubmit;
        operandSlots.updateCallback += OnSlotUpdated;
        
        if(activeGO) activeGO.SetActive(false);

        if(dragInstruct) {
            mDragInstructEnabled = true;
        }

        if(dragInstructDialogGO) dragInstructDialogGO.SetActive(false);
    }

    void OnInputSubmit() {
        //fail-safe, shouldn't be able to submit
        if(mOperation == null || mOperation.isAnyOperandEmpty || isBusy)
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

    void OnSlotUpdated() {
        if(mOperation == null) //fail-safe
            return;
        
        var prevResult = mOperation.Evaluate();
        var prevAnyEmpty = mOperation.isAnyOperandEmpty;

        //update operands
        int opCount = Mathf.Min(operandSlots.slots.Length, mOperation.operands.Length);
        for(int i = 0; i < opCount; i++) {
            var op = mOperation.operands[i];

            if(operandSlots.slots[i].card)
                op.ApplyNumber(operandSlots.slots[i].card.number);
            else
                op.RemoveNumber();
        }

        //check if result value has changed
        var newResult = mOperation.Evaluate();
        var newAnyEmpty = mOperation.isAnyOperandEmpty;

        //reset input
        if(newResult != prevResult || newAnyEmpty != prevAnyEmpty)
            RefreshAnswerInput();

        mDragInstructSlotUpdated = true;

        if(slotUpdateCallback != null)
            slotUpdateCallback();
    }

    IEnumerator DoShow() {
        if(activeGO) activeGO.SetActive(true);

        if(animator && !string.IsNullOrEmpty(takeEnter))
            yield return animator.PlayWait(takeEnter);

        //show drag instruct
        if(mDragInstructEnabled) {
            DragInstructStart();
        }
        
        mRout = null;
    }

    IEnumerator DoHide() {
        answerInput.isLocked = true;

        if(animator && !string.IsNullOrEmpty(takeExit))
            yield return animator.PlayWait(takeExit);

        if(activeGO) activeGO.SetActive(false);
        
        DragInstructEnd();

        mRout = null;
    }

    IEnumerator DoDragInstruct() {
        while(mDragInstructEnabled) {
            if(!dragInstructCardDeckWidget)
                break;

            if(dragInstructCardDeckWidget.cards == null) {
                yield return null;
                continue;
            }

            if(!dragInstruct)
                break;

            dragInstruct.Hide(); //allow for reposition

            int opInd = -1;
            int opCount = Mathf.Min(operandSlots.slots.Length, mOperation.operands.Length);
            for(int i = 0; i < opCount; i++) {
                var op = mOperation.operands[i];
                if(op.isEmpty) {
                    opInd = i;
                    break;
                }
            }

            if(opInd != -1) {
                CardWidget card = null;
                for(int i = 0; i < dragInstructCardDeckWidget.cards.Length; i++) {
                    if(dragInstructCardDeckWidget.cards[i]) {
                        card = dragInstructCardDeckWidget.cards[i];
                        break;
                    }
                }

                if(card) {
                    Vector2 dragStart = card.transform.position;
                    Vector2 dragEnd = operandSlots.slots[opInd].anchor.position;

                    dragInstruct.Show(false, dragStart, dragEnd);
                    if(dragInstructDialogGO) dragInstructDialogGO.SetActive(true);
                }
                else { //no card available
                    if(dragInstructDialogGO) dragInstructDialogGO.SetActive(false);
                }
            }
            else { //all operands filled
                if(dragInstructDialogGO) dragInstructDialogGO.SetActive(false);
            }

            mDragInstructSlotUpdated = false;
            while(!mDragInstructSlotUpdated)
                yield return null;

            yield return null;
        }

        mDragInstructRout = null;
        DragInstructEnd();
    }

    private void ClearRoutine() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }
    
    private void RefreshAnswerInput() {
        bool isValid = !mOperation.isAnyOperandEmpty;

        answerInput.CloseNumpad();

        if(isValid) {
            var opAnswer = mOperation.Evaluate();

            bool isWholeEnabled = Mathf.Abs(opAnswer.fValue) >= 1.0f;

            answerInput.Init(answerWholeEnabled && isWholeEnabled, opAnswer.isNegative);
        }
        else
            answerInput.Init(false, false);

        answerInput.isLocked = mAnswerIsLocked || !isValid;
    }

    private void DragInstructStart() {
        if(!mDragInstructEnabled)
            return;

        if(mDragInstructRout != null)
            StopCoroutine(mDragInstructRout);

        mDragInstructRout = StartCoroutine(DoDragInstruct());
    }

    private void DragInstructEnd() {
        if(mDragInstructRout != null) {
            StopCoroutine(mDragInstructRout);
            mDragInstructRout = null;
        }

        if(dragInstruct) dragInstruct.Hide();
        if(dragInstructDialogGO) dragInstructDialogGO.SetActive(false);

        mDragInstructEnabled = false;
    }
}
