using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDeckWidget : CardDropWidgetBase {
    public GameObject rootGO;

    [Header("Card")]
    public string cardPoolGroup = "cardPool";
    public int cardPoolCapacity = 4;
    public GameObject cardTemplate; //operand display
    public bool cardWholeEnabled = true;

    [Header("Highlight")]
    public Graphic highlightTarget;
    public float highlightAmplify = 0.3f;
    public float highlightDelay = 0.5f;

    [Header("Slots")]
    public RectTransform[] slotAnchors;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeShow;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeHide;

    [Header("Audio")]
    [M8.SoundPlaylist]
    public string audioShow;
    [M8.SoundPlaylist]
    public string audioHide;

    public CardWidget[] cards { get; private set; }

    public int count { get; private set; }

    public bool isBusy { get { return mRout != null; } }

    private M8.PoolController mPool;

    private M8.GenericParams mCardParms = new M8.GenericParams();

    private Coroutine mRout;

    private Color mHighlightDefaultColor;
    private Color mHighlightColor;
    private bool mIsHighlighted;

    public void Show() {
        Stop();

        if(rootGO) rootGO.SetActive(true);

        M8.SoundPlaylist.instance.Play(audioShow, false);

        mRout = StartCoroutine(DoShow());
    }

    public void Hide() {
        Stop();

        M8.SoundPlaylist.instance.Play(audioHide, false);

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
            mCardParms[CardWidget.parmWholeEnabled] = cardWholeEnabled;
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
        Stop();
    }

    void Awake() {
        if(rootGO) rootGO.SetActive(false);

        if(highlightTarget) {
            mHighlightDefaultColor = highlightTarget.color;

            mHighlightColor = new Color(
                Mathf.Clamp01(mHighlightDefaultColor.r + mHighlightDefaultColor.r * highlightAmplify),
                Mathf.Clamp01(mHighlightDefaultColor.g + mHighlightDefaultColor.g * highlightAmplify),
                Mathf.Clamp01(mHighlightDefaultColor.b + mHighlightDefaultColor.b * highlightAmplify),
                mHighlightDefaultColor.a);
        }
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

    IEnumerator DoHighlight() {
        mIsHighlighted = true;

        float curTime;
        var delay = highlightDelay * 0.5f;

        var easeIn = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.InSine);
        var easeOut = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutSine);

        while(true) {
            curTime = 0f;
            while(curTime < delay) {
                yield return null;

                curTime += Time.deltaTime;

                var t = easeIn(curTime, delay, 0f, 0f);

                highlightTarget.color = Color.Lerp(mHighlightDefaultColor, mHighlightColor, t);
            }

            curTime = 0f;
            while(curTime < delay) {
                yield return null;

                curTime += Time.deltaTime;

                var t = easeOut(curTime, delay, 0f, 0f);

                highlightTarget.color = Color.Lerp(mHighlightColor, mHighlightDefaultColor, t);
            }
        }
    }

    private void Stop() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        if(mIsHighlighted) {
            if(highlightTarget)
                highlightTarget.color = mHighlightDefaultColor;

            mIsHighlighted = false;
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

    public override void CardDropHighlight(int index) {
        if(index != -1) {
            if(!mIsHighlighted) {
                Stop();
                mRout = StartCoroutine(DoHighlight());
            }
        }
        else
            Stop();
    }

    public override void CardDropHighlightClear() {
        Stop();
    }
}
