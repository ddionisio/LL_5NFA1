using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MixedNumberOpsWidget : MonoBehaviour {
    [System.Serializable]
    public class OperandData {
        public RectTransform anchor;
        public GameObject highlightGO;

        public CardWidget card {
            get { return mCard; }
            set {
                if(mCard != value) {
                    if(mCard) mCard.Release();

                    mCard = value;
                }
            }
        }

        private CardWidget mCard;

        public void Init() {
            card = null;

            if(highlightGO)
                highlightGO.SetActive(false);
        }
    }

    [Header("Templates")]
    public string poolGroup;
    public GameObject cardTemplate; //operand display

    [Header("Main")]
    public GameObject activeGO; //when operation is active

    [Header("Operation")]
    public OperandData[] operands;
    public OperatorWidget[] operators;

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
            Clear();

            mOperation = value.Clone();

            ApplyCurrentOperation();
        }
    }

    private M8.PoolController mPool;

    private MixedNumberOps mOperation;

    public void Show() {

    }

    public void Hide() {

    }

    void OnDisable() {
        Clear();
    }

    void OnDestroy() {
        if(answerInput)
            answerInput.submitCallback -= OnInputSubmit;
    }

    void Awake() {
        mPool = M8.PoolController.CreatePool(poolGroup);
        mPool.AddType(cardTemplate, operands.Length, operands.Length);
                
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

    private void Clear() {
        for(int i = 0; i < operands.Length; i++)
            operands[i].Init();

        answerInput.gameObject.SetActive(false);

        if(activeGO) activeGO.SetActive(false);
    }

    private void ApplyCurrentOperation() {
        //setup operands and operators

        //setup answer input
        RefreshAnswerInput();
    }

    private void RefreshAnswerInput() {
        bool isValid = !mOperation.isAnyOperandEmpty;

        if(answerInput.gameObject.activeSelf != isValid) {
            if(isValid) {
                var opAnswer = mOperation.Evaluate();

                bool isWholeEnabled = opAnswer.whole != 0 || opAnswer.numerator > opAnswer.denominator;

                answerInput.Init(isWholeEnabled);
            }
            else
                answerInput.CloseNumpad();

            answerInput.gameObject.SetActive(isValid);
        }
    }
}
