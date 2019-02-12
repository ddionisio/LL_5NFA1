using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MixedNumberOpsWidget : MonoBehaviour {
    [System.Serializable]
    public class OperandData {
        public RectTransform anchor;
        public GameObject emptyGO;
        public GameObject highlightGO;

        public bool isFixed { get { return card != null && !card.canDragOutside; } }
        public CardWidget card { get; private set; }

        public void SetActive(bool aActive) {
            anchor.gameObject.SetActive(aActive);
        }

        public void SetCard(CardWidget aCard) {
            if(card != aCard) {
                if(card) card.Release();

                card = aCard;
            }
        }

        public void SetEmpty(bool empty) {
            if(empty) {
                if(emptyGO)
                    emptyGO.SetActive(true);

                SetCard(null);
            }
            else {
                if(emptyGO)
                    emptyGO.SetActive(false);
            }
        }

        public void Init() {
            SetCard(null);

            if(emptyGO)
                emptyGO.SetActive(false);

            if(highlightGO)
                highlightGO.SetActive(false);
        }
    }
        
    [Header("Main")]
    public GameObject activeGO; //when operation is active

    [Header("Card")]
    public string cardPoolGroup = "cardPool";
    public GameObject cardTemplate; //operand display
    public Transform cardContainer;

    [Header("Operation")]
    public OperandData[] operandSlots;
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
            mOperation = value.Clone(operandSlots.Length);

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
        mPool.AddType(cardTemplate, operandSlots.Length, operandSlots.Length);
                
        answerInput.submitCallback += OnInputSubmit;
    }

    void OnInputSubmit() {
        //fail-safe, shouldn't be able to submit
        if(mOperation == null || mOperation.isAnyOperandEmpty)
            return;
                
        var opAnswer = mOperation.Evaluate();

        bool isCorrect = answerInput.number == opAnswer;

        if(!isCorrect) {
            if(animator && !string.IsNullOrEmpty(takeWrong))
                animator.Play(takeWrong);
        }

        signalAnswer.Invoke(isCorrect);
    }

    IEnumerator DoShow() {
        answerInput.gameObject.SetActive(false);

        if(activeGO) activeGO.SetActive(true);

        if(animator && !string.IsNullOrEmpty(takeEnter))
            yield return animator.PlayWait(takeEnter);

        RefreshAnswerInput();
    }

    IEnumerator DoHide() {
        answerInput.gameObject.SetActive(false);

        if(animator && !string.IsNullOrEmpty(takeExit))
            yield return animator.PlayWait(takeExit);

        if(activeGO) activeGO.SetActive(false);
    }

    private void Clear() {
        for(int i = 0; i < operandSlots.Length; i++)
            operandSlots[i].Init();

        answerInput.gameObject.SetActive(false);

        if(activeGO) activeGO.SetActive(false);
    }

    private void ApplyCurrentOperation() {
        Clear();

        //setup operands
        var operandCount = mOperation.operands.Length;

        for(int i = 0; i < operandCount; i++) {
            var operand = mOperation.operands[i];
            var operandSlot = operandSlots[i];

            operandSlot.SetEmpty(operand.isEmpty);

            if(!operand.isEmpty) {
                mCardParms[CardWidget.parmNumber] = operand.number;
                mCardParms[CardWidget.parmCanDragInside] = true;
                mCardParms[CardWidget.parmCanDragOutside] = false;

                var newCard = mPool.Spawn<CardWidget>(cardTemplate.name, "", cardContainer, operandSlot.anchor.position, mCardParms);

                operandSlot.SetCard(newCard);
            }

            operandSlot.SetActive(true);
        }

        for(int i = operandCount; i < operandSlots.Length; i++) //hide other operands
            operandSlots[i].SetActive(false);
        //

        //setup operators
        int operatorCount = mOperation.operators.Length;

        for(int i = 0; i < operatorCount; i++) {
            var op = mOperation.operators[i];
            var opSlot = operatorSlots[i];

            opSlot.SetOperator(op);
            opSlot.gameObject.SetActive(false);
        }

        for(int i = 0; i < operatorSlots.Length; i++) //hide other operands
            operatorSlots[i].gameObject.SetActive(false);
        //

        //setup answer input
        RefreshAnswerInput();
    }

    private void RefreshAnswerInput() {
        bool isValid = !mOperation.isAnyOperandEmpty;

        if(answerInput.gameObject.activeSelf != isValid) {
            answerInput.CloseNumpad();

            if(isValid) {
                var opAnswer = mOperation.Evaluate();

                bool isWholeEnabled = opAnswer.whole != 0 || opAnswer.numerator > opAnswer.denominator;

                answerInput.Init(isWholeEnabled);
            }   

            answerInput.gameObject.SetActive(isValid);
        }
    }
}
