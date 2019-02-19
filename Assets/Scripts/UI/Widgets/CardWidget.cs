using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CardWidget : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public enum DragAreaType {
        None,        
        Whole,
        Fraction,
        Outside
    }

    public const string parmNumber = "mixedNumber";
    public const string parmCanDragOutside = "dragOutside";
    public const string parmCanDragInside = "dragInside";
    public const string parmFractionVisual = "fractionVisual";

    public const string parmCardDrop = "cardDrop";
    public const string parmCardDropIndex = "cardDropIndex";

    [Header("Data")]
    public float mixedNumberSplit = 0.5f;

    [Header("Display")]
    public MixedNumberWidget numberWidget;
    public GameObject fractionVisualGO;

    [Header("Drag")]
    public RectTransform dragRoot;
    public float dragReturnDelay = 0.3f;
    public DG.Tweening.Ease dragReturnEase = DG.Tweening.Ease.OutSine;
    public GameObject dragInsideGO; //when dragging inside
    public GameObject dragWholeToFractionGO;
    public GameObject dragFractionToWholeGO;

    [Header("Signals")]
    public M8.Signal signalFractionVisibleUpdate;

    public MixedNumber number { get { return numberWidget.number; } set { numberWidget.number = value; } }

    public bool isFractionVisual {
        get { return mIsFractionVisual; }
        set {
            mIsFractionVisual = value;
            UpdateFractionVisualShow();
        }
    }

    public bool canDragOutside { get; set; }

    public bool canDragInside {
        get { return mCanDragInside && Mathf.Abs(number.fValue) >= 1.0f; }
        set { mCanDragInside = value; }
    }

    public RectTransform rectTransform {
        get {
            if(!mRectTransform)
                mRectTransform = transform as RectTransform;

            return mRectTransform;
        }
    }

    public CardDropWidgetBase currentCardDrop { get; private set; }
    public int currentCardDropIndex { get; private set; }

    public M8.PoolDataController poolData {
        get {
            if(!mPoolData)
                mPoolData = GetComponent<M8.PoolDataController>();
            return mPoolData;
        }
    }

    private M8.PoolDataController mPoolData;

    private DragAreaType mDragAreaBeginType;
    private DragAreaType mDragAreaCurType;

    private RectTransform mRectTransform;

    private Vector3 mDragRootDefaultLocalPos;

    private bool mCanDragInside;

    private bool mIsFractionVisual;

    private bool mIsDragging;
    private Coroutine mRout;

    public void MoveDragAnchorToOrigin() {
        StopRoutine();
        mRout = StartCoroutine(DoMoveDragAnchorToOrigin());
    }

    void OnApplicationFocus(bool focus) {
        if(!focus) {
            if(mIsDragging) {
                ResetDrag(true);
                UpdateFractionVisualShow();
            }
        }
    }

    void Awake() {
        if(dragRoot)
            mDragRootDefaultLocalPos = dragRoot.localPosition;
    }

    void M8.IPoolDespawn.OnDespawned() {
        if(signalFractionVisibleUpdate)
            signalFractionVisibleUpdate.callback -= UpdateFractionVisualShow;

        currentCardDrop = null;

        ResetDrag(true);

        if(fractionVisualGO)
            fractionVisualGO.SetActive(false);
    }

    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
        canDragInside = false;
        canDragOutside = false;

        currentCardDrop = null;
        currentCardDropIndex = -1;

        mIsFractionVisual = false;

        if(parms != null) {
            if(parms.ContainsKey(parmNumber))
                number = parms.GetValue<MixedNumber>(parmNumber);

            if(parms.ContainsKey(parmCanDragInside))
                canDragInside = parms.GetValue<bool>(parmCanDragInside);

            if(parms.ContainsKey(parmCanDragOutside))
                canDragOutside = parms.GetValue<bool>(parmCanDragOutside);

            if(parms.ContainsKey(parmFractionVisual))
                mIsFractionVisual = parms.GetValue<bool>(parmFractionVisual);

            if(parms.ContainsKey(parmCardDrop))
                currentCardDrop = parms[parmCardDrop] as CardDropWidgetBase;

            if(parms.ContainsKey(parmCardDropIndex))
                currentCardDropIndex = parms.GetValue<int>(parmCardDropIndex);
        }

        numberWidget.number = number;
        
        ResetDrag(true);

        UpdateFractionVisualShow();

        if(signalFractionVisibleUpdate)
            signalFractionVisibleUpdate.callback += UpdateFractionVisualShow;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        mDragAreaBeginType = GetDragType(eventData.position);
        mDragAreaCurType = mDragAreaBeginType;

        RefreshDragDisplay(eventData.position);

        if(mDragAreaBeginType != DragAreaType.None) {
            mIsDragging = true;
            UpdateFractionVisualShow();
        }
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(!mIsDragging)
            return;

        mDragAreaCurType = GetDragType(eventData.position);

        RefreshDragDisplay(eventData.position);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if(!mIsDragging)
            return;

        mDragAreaCurType = GetDragType(eventData.position);
                
        //determine action
        switch(mDragAreaCurType) {
            case DragAreaType.Whole:
                if(canDragInside) {
                    if(mDragAreaBeginType == DragAreaType.Fraction) {
                        var num = numberWidget.number;
                        num.FractionToWhole();
                        numberWidget.number = num;
                    }
                }

                ResetDrag(true);
                UpdateFractionVisualShow();
                break;
            case DragAreaType.Fraction:
                if(canDragInside) {
                    if(mDragAreaBeginType == DragAreaType.Whole) {
                        var num = numberWidget.number;
                        num.WholeToFraction();
                        numberWidget.number = num;
                    }
                }

                ResetDrag(true);
                UpdateFractionVisualShow();
                break;
            case DragAreaType.Outside:
                if(canDragOutside) {
                    //determine if we want to swap or placing to an empty slot
                    if(eventData.pointerCurrentRaycast.isValid && eventData.pointerCurrentRaycast.gameObject) {
                        var dropWidget = eventData.pointerCurrentRaycast.gameObject.GetComponent<CardDropWidgetBase>();
                        if(dropWidget) {
                            int index = dropWidget.CardDropGetSlotIndex(this);
                            if(index != -1)
                                SetCurrentCardDrop(dropWidget, index);
                        }
                        else {
                            var cardWidget = eventData.pointerCurrentRaycast.gameObject.GetComponent<CardWidget>();
                            if(cardWidget && cardWidget.canDragOutside) {
                                if(cardWidget.currentCardDrop && cardWidget.currentCardDropIndex != -1)
                                    SetCurrentCardDrop(cardWidget.currentCardDrop, cardWidget.currentCardDropIndex); //swap
                            }
                        }
                    }

                    ResetDrag(false);
                    MoveDragAnchorToOrigin();
                }
                break;
        }
    }

    IEnumerator DoMoveDragAnchorToOrigin() {
        if(dragRoot) {
            Vector2 startPos = dragRoot.position;

            dragRoot.SetParent(DragArea.transform, true);

            var easeFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(dragReturnEase);

            dragRoot.position = startPos;

            var curTime = 0f;
            while(curTime < dragReturnDelay) {
                yield return null;

                curTime += Time.deltaTime;

                var t = easeFunc(curTime, dragReturnDelay, 0f, 0f);

                Vector2 endPos = transform.TransformPoint(mDragRootDefaultLocalPos);

                dragRoot.position = Vector2.Lerp(startPos, endPos, t);
            }

            dragRoot.SetParent(transform, true);
        }

        mRout = null;

        UpdateFractionVisualShow();
    }
        
    private void SetCurrentCardDrop(CardDropWidgetBase cardDrop, int index) {
        if(currentCardDrop != cardDrop || currentCardDropIndex != index) {
            var prevCardDrop = currentCardDrop;
            var prevIndex = currentCardDropIndex;

            currentCardDrop = cardDrop;
            currentCardDropIndex = index;

            CardWidget prevCard;
            if(currentCardDrop)
                prevCard = cardDrop.CardDropSet(index, this);
            else
                prevCard = null;

            if(prevCardDrop) {
                if(prevCard) {
                    prevCard.MoveDragAnchorToOrigin();

                    prevCard.currentCardDrop = prevCardDrop;
                    prevCard.currentCardDropIndex = prevIndex;
                }

                prevCardDrop.CardDropSet(prevIndex, prevCard);
            }
            else if(prevCard)
                prevCard.poolData.Release();
        }
    }

    private void RefreshDragDisplay(Vector2 pos) {
        var _dragInside = false;
        var _dragWholeToFraction = false;
        var _dragFractionToWhole = false;

        switch(mDragAreaCurType) {
            case DragAreaType.Whole:
                _dragInside = true;
                _dragFractionToWhole = mDragAreaBeginType == DragAreaType.Fraction;
                break;
            case DragAreaType.Fraction:
                _dragInside = true;
                _dragWholeToFraction = mDragAreaBeginType == DragAreaType.Whole;
                break;
            case DragAreaType.Outside:
                //set drag display position
                var dragAreaRoot = DragArea.transform;
                if(dragRoot.parent != dragAreaRoot)
                    dragRoot.SetParent(dragAreaRoot, false);

                dragRoot.position = pos;
                break;
        }

        //revert drag root if no longer "outside"
        if(mDragAreaCurType != DragAreaType.Outside) {
            if(dragRoot.parent != transform) {
                dragRoot.SetParent(transform, false);
                dragRoot.localPosition = mDragRootDefaultLocalPos;
            }
        }

        if(dragInsideGO) dragInsideGO.SetActive(_dragInside);
        if(dragWholeToFractionGO) dragWholeToFractionGO.SetActive(_dragWholeToFraction);
        if(dragFractionToWholeGO) dragFractionToWholeGO.SetActive(_dragFractionToWhole);
    }

    private void ResetDrag(bool resetDragRoot) {
        StopRoutine();

        mIsDragging = false;
        mDragAreaBeginType = DragAreaType.None;
        mDragAreaCurType = DragAreaType.None;

        if(resetDragRoot && dragRoot) {
            dragRoot.SetParent(transform, false);
            dragRoot.localPosition = mDragRootDefaultLocalPos;
        }

        if(dragInsideGO) dragInsideGO.SetActive(false);
        if(dragWholeToFractionGO) dragWholeToFractionGO.SetActive(false);
        if(dragFractionToWholeGO) dragFractionToWholeGO.SetActive(false);
    }

    private void StopRoutine() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }

    private void UpdateFractionVisualShow() {
        if(fractionVisualGO)
            fractionVisualGO.SetActive(mIsFractionVisual && !mIsDragging && FractionVisualToggle.isVisible && mRout == null);
    }

    private DragAreaType GetDragType(Vector2 pos) {
        Vector2 lPos = transform.InverseTransformPoint(pos);

        var rect = rectTransform.rect;

        if(canDragInside) {
            var wholeRect = new Rect(rect.position, new Vector2(rect.width * mixedNumberSplit, rect.height));

            if(wholeRect.Contains(lPos))
                return DragAreaType.Whole;

            var fractionRect = new Rect(new Vector2(rect.x + rect.width * mixedNumberSplit, rect.y), new Vector2(rect.width * (1.0f - mixedNumberSplit), rect.height));

            if(fractionRect.Contains(lPos))
                return DragAreaType.Fraction;

            return canDragOutside ? DragAreaType.Outside : DragAreaType.None;
        }
        else if(canDragOutside) {
            return DragAreaType.Outside;
        }

        return DragAreaType.None;
    }

    void OnDrawGizmos() {
        Rect rect;

        var rectT = transform as RectTransform;
        if(rectT)
            rect = rectT.rect;
        else
            rect = new Rect();

        //grab corners and draw wire
        var t = transform;

        var p0 = t.TransformPoint(new Vector2(rect.xMin, rect.yMin));
        var p1 = t.TransformPoint(new Vector2(rect.xMax, rect.yMin));
        var p2 = t.TransformPoint(new Vector2(rect.xMax, rect.yMax));
        var p3 = t.TransformPoint(new Vector2(rect.xMin, rect.yMax));

        Gizmos.color = Color.green;

        Gizmos.DrawLine(p0, p1);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p0);

        Gizmos.DrawLine(Vector3.Lerp(p0, p1, mixedNumberSplit), Vector3.Lerp(p3, p2, mixedNumberSplit));
    }
}
