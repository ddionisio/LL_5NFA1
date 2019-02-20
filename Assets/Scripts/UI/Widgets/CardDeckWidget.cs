using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDeckWidget : CardDropWidgetBase {
    public GameObject rootGO;

    [Header("Card")]
    public string cardPoolGroup = "cardPool";
    public int cardPoolCapacity = 4;
    public GameObject cardTemplate; //operand display

    [Header("Slots")]
    public RectTransform[] slotAnchors;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeShow;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeHide;

    public CardWidget[] cards { get; private set; }

    public int count { get; private set; }

    public bool isBusy { get { return mRout != null; } }

    private M8.PoolController mPool;

    private M8.GenericParams mCardParms = new M8.GenericParams();

    private Coroutine mRout;

    public void Show() {
        Stop();

        if(rootGO) rootGO.SetActive(true);

        mRout = StartCoroutine(DoShow());
    }

    public void Hide() {
        Stop();
        mRout = StartCoroutine(DoHide());
    }

    public void Fill(MixedNumber[] numbers) {
        if(!mPool) {
            mPool = M8.PoolController.CreatePool(cardPoolGroup);
            mPool.AddType(cardTemplate, cardPoolCapacity, cardPoolCapacity);
        }

        if(cards != null)
            Clear();
        else
            cards = new CardWidget[slotAnchors.Length];

        M8.ArrayUtil.Shuffle(numbers);

        count = numbers.Length;
        if(count > slotAnchors.Length)
            count = slotAnchors.Length;
                
        for(int i = 0; i < count; i++) {
            mCardParms[CardWidget.parmNumber] = numbers[i];
            mCardParms[CardWidget.parmCanDragInside] = false;
            mCardParms[CardWidget.parmCanDragOutside] = true;
            mCardParms[CardWidget.parmFractionVisual] = false;
            mCardParms[CardWidget.parmCardDrop] = this;
            mCardParms[CardWidget.parmCardDropIndex] = i;

            var newCard = mPool.Spawn<CardWidget>(cardTemplate.name, "", null, mCardParms);

            newCard.transform.SetParent(slotAnchors[i], false);
            newCard.transform.localPosition = Vector3.zero;

            cards[i] = newCard;

            slotAnchors[i].gameObject.SetActive(true);
        }

        for(int i = count; i < slotAnchors.Length; i++)
            slotAnchors[i].gameObject.SetActive(false);
    }

    public void Clear() {
        if(cards == null)
            return;

        for(int i = 0; i < count; i++) {
            if(cards[i]) {
                if(cards[i].poolData)
                    cards[i].poolData.Release();

                cards[i] = null;
            }
        }

        count = 0;
    }

    void OnDisable() {
        mRout = null;
    }

    void Awake() {
        if(rootGO) rootGO.SetActive(false);
    }

    IEnumerator DoShow() {
        if(animator && !string.IsNullOrEmpty(takeShow))
            yield return animator.PlayWait(takeShow);

        mRout = null;
    }

    IEnumerator DoHide() {
        if(animator && !string.IsNullOrEmpty(takeHide))
            yield return animator.PlayWait(takeHide);

        if(rootGO) rootGO.SetActive(false);

        mRout = null;
    }

    private void Stop() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }

    public override int CardDropGetSlotIndex(CardWidget card) {
        int retInd = -1;

        //assume no rotation/scale
        var cardRect = card.dragRoot.rect;
        cardRect.position = card.dragRoot.position;

        for(int i = 0; i < count; i++) {
            var anchor = slotAnchors[i];

            var cardLocalRect = cardRect;
            cardLocalRect.position = anchor.InverseTransformPoint(cardRect.position);

            if(anchor.rect.Overlaps(cardLocalRect)) {
                retInd = i;
                break;
            }
        }

        if(retInd == -1) { //get empty slot
            for(int i = 0; i < count; i++) {
                if(!cards[i]) {
                    retInd = i;
                    break;
                }
            }
        }

        return retInd;
    }

    public override CardWidget CardDropSet(int index, CardWidget card) {
        var prevCard = cards[index];

        cards[index] = card;

        if(card) {
            card.isFractionVisual = false;
            card.canDragInside = false;
            card.transform.SetParent(slotAnchors[index], false);
            card.transform.localPosition = Vector3.zero;
        }

        return prevCard;
    }

    public override void CardDropPositionUpdate(CardWidget card) {

    }
}
