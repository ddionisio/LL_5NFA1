using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OperationsIntroDefenseController : MonoBehaviour {
    [System.Serializable]
    public struct DeckItem {
        public MixedNumber[] numbers;
    }

    [System.Serializable]
    public struct Item {
        public MixedNumber number;
        public DeckItem[] deckNumbers;
    }

    [Header("Data")]
    public Item[] items;

    [Header("Widgets")]
    public MixedNumberOpsWidget opsWidget;
    public CardDeckWidget deckWidget;

    [Header("Hint")]
    public GameObject hintNegativeGO;
    public Text hintNegativeText;
    public int hintNegativeShowWrongCount = 3; //show hint after this amount of error
    public float hintNegativeShowDelay = 60f;

    [Header("Signal Listens")]
    public M8.Signal signalOpen;
    public M8.Signal signalClose;
    public SignalBoolean signalAnswerSubmit;

    [Header("Signal Invokes")]
    public M8.Signal signalComplete;

    private int mItemInd;
    private int mDeckInd;

    private int mWrongNegativeCount;

    private MixedNumber[] mDeckNumberCache;

    void OnDestroy() {
        if(signalOpen) signalOpen.callback -= OnSignalOpen;
        if(signalClose) signalClose.callback -= OnSignalClose;
        if(signalAnswerSubmit) signalAnswerSubmit.callback -= OnSignalAnswerSubmit;
    }

    void Awake() {
        opsWidget.gameObject.SetActive(true);
        deckWidget.gameObject.SetActive(true);

        if(hintNegativeGO) hintNegativeGO.SetActive(false);

        mItemInd = Random.Range(0, items.Length);
        mDeckInd = 0;

        int cacheSize = 0;
        for(int i = 0; i < items[mItemInd].deckNumbers.Length; i++) {
            if(items[mItemInd].deckNumbers[i].numbers.Length > cacheSize)
                cacheSize = items[mItemInd].deckNumbers[i].numbers.Length;
        }
        mDeckNumberCache = new MixedNumber[cacheSize];

        mWrongNegativeCount = 0;

        signalOpen.callback += OnSignalOpen;
        signalClose.callback += OnSignalClose;
        signalAnswerSubmit.callback += OnSignalAnswerSubmit;
    }

    void OnSignalOpen() {
        StartCoroutine(DoOpen());
    }

    void OnSignalClose() {
        StopAllCoroutines();

        opsWidget.Hide();
        deckWidget.Hide();
    }

    void OnSignalAnswerSubmit(bool correct) {
        var answerNum = opsWidget.answerInput.number;
        answerNum.isNegative = opsWidget.answerInput.numberIsNegative;

        if(correct) {
            if(hintNegativeGO) hintNegativeGO.SetActive(false);

            if(answerNum.fValue <= 0f) {
                //finish
                if(signalComplete)
                    signalComplete.Invoke();
            }
            else {
                //refresh with new operand and deck
                opsWidget.MoveAnswerToOperand(0);
                PopulateDeck();
            }
        }
        else {
            if(answerNum.isNegative && hintNegativeGO && !hintNegativeGO.activeSelf)
                mWrongNegativeCount++;
        }
    }

    IEnumerator DoOpen() {
        //setup ops
        var ops = new MixedNumberOps();

        ops.operands = new MixedNumberOps.Operand[] { new MixedNumberOps.Operand(), new MixedNumberOps.Operand() };

        ops.operands[0].ApplyNumber(items[mItemInd].number);

        ops.operators = new OperatorType[] { OperatorType.Subtract };

        opsWidget.operation = ops;

        deckWidget.Clear();
        
        //open widgets
        opsWidget.Show();
        deckWidget.Show();

        while(deckWidget.isBusy)
            yield return null;

        //setup deck
        PopulateDeck();

        StartCoroutine(DoHintNegativeWatch());
    }

    IEnumerator DoHintNegativeWatch() {
        float curTimeWait = 0f;

        int curOp1Num = 0;
        int curOp2Num = 0;

        while(true) {
            bool isHide = false;

            if(!opsWidget.answerInput.isLocked) {
                //make sure answer will be negative
                if(opsWidget.answerInput.numberIsNegative) {
                    //make sure denominators are the same
                    var op1Denom = opsWidget.operandSlots.slots[0].card ? opsWidget.operandSlots.slots[0].card.number.denominator : 0;
                    var op2Denom = opsWidget.operandSlots.slots[1].card ? opsWidget.operandSlots.slots[1].card.number.denominator : 0;

                    bool isDenomsEqual = op1Denom == op2Denom;

                    if(isDenomsEqual) {
                        if(hintNegativeGO) {
                            if(hintNegativeGO.activeSelf) {
                                //update number
                                var op1Num = opsWidget.operandSlots.slots[0].card ? opsWidget.operandSlots.slots[0].card.number.numerator : 0;
                                var op2Num = opsWidget.operandSlots.slots[1].card ? opsWidget.operandSlots.slots[1].card.number.numerator : 0;
                                if(curOp1Num != op1Num || curOp2Num != op2Num) {
                                    curOp1Num = op1Num;
                                    curOp2Num = op2Num;
                                    ApplyHintNegativeText();
                                }
                            }
                            else {
                                curTimeWait += Time.deltaTime;

                                if(curTimeWait >= hintNegativeShowDelay || mWrongNegativeCount >= hintNegativeShowWrongCount) {
                                    if(hintNegativeGO) hintNegativeGO.SetActive(true);
                                    ApplyHintNegativeText();
                                }
                            }
                        }
                    }
                    else
                        isHide = true;
                }
                else
                    isHide = true;
            }
            else
                isHide = true;

            if(isHide) {
                if(hintNegativeGO) hintNegativeGO.SetActive(false);
                curTimeWait = 0f;
                mWrongNegativeCount = 0;
            }

            yield return null;
        }
    }

    private void PopulateDeck() {
        var nums = items[mItemInd].deckNumbers[mDeckInd].numbers;
        M8.ArrayUtil.Shuffle(nums);

        int count = 0;

        //make sure numbers are less than current first operand        
        var card = opsWidget.operandSlots.GetCard(0);
        if(card) {
            var opNum = card.number;

            for(int i = 0; i < nums.Length; i++) {
                var num = nums[i];
                if(num <= opNum) {
                    mDeckNumberCache[count] = num;
                    count++;
                }
            }

            //if none of these numbers or there's only one, then just add card with same number as operand
            if(count <= 1) {
                var newNum = opNum.simplified;
                if(count == 0 || newNum != mDeckNumberCache[0]) {
                    mDeckNumberCache[count] = newNum;
                    count++;
                }
            }
        }

        deckWidget.Fill(mDeckNumberCache, count);

        mDeckInd++;
        if(mDeckInd == items[mItemInd].deckNumbers.Length)
            mDeckInd = 0;
    }

    private void ApplyHintNegativeText() {
        var op1Num = opsWidget.operandSlots.slots[0].card ? opsWidget.operandSlots.slots[0].card.number.numerator : 0;
        var op2Num = opsWidget.operandSlots.slots[1].card ? opsWidget.operandSlots.slots[1].card.number.numerator : 0;

        if(hintNegativeText)
            hintNegativeText.text = string.Format("{0} - {1} = {2}", op1Num, op2Num, op1Num - op2Num);
    }
}
