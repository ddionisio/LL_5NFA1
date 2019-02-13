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

    [Header("Data")]
    public float mixedNumberSplit = 0.5f;

    [Header("Display")]
    public MixedNumberWidget numberWidget;

    [Header("Drag")]
    public Transform dragRoot;
    public GameObject dragInsideGO; //when dragging inside
    public GameObject dragWholeToFractionGO;
    public GameObject dragFractionToWholeGO;

    public MixedNumber number { get { return numberWidget.number; } set { numberWidget.number = value; } }

    public bool canDragOutside { get; private set; }
    public bool canDragInside { get; private set; }

    public RectTransform rectTransform {
        get {
            if(!mRectTransform)
                mRectTransform = transform as RectTransform;

            return mRectTransform;
        }
    }
    
    private M8.PoolDataController mPoolData;

    private DragAreaType mDragAreaBeginType;
    private DragAreaType mDragAreaCurType;

    private RectTransform mRectTransform;

    private Vector3 mDragRootDefaultLocalPos;

    public void Release() {
        if(mPoolData)
            mPoolData.Release();
    }

    void Awake() {
        if(dragRoot)
            mDragRootDefaultLocalPos = dragRoot.localPosition;
    }

    void M8.IPoolDespawn.OnDespawned() {
        ResetDrag();
    }

    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
        if(!mPoolData)
            mPoolData = GetComponent<M8.PoolDataController>();

        canDragInside = false;
        canDragOutside = false;

        if(parms != null) {
            if(parms.ContainsKey(parmNumber))
                number = parms.GetValue<MixedNumber>(parmNumber);

            if(parms.ContainsKey(parmCanDragInside))
                canDragInside = parms.GetValue<bool>(parmCanDragInside);

            if(parms.ContainsKey(parmCanDragOutside))
                canDragOutside = parms.GetValue<bool>(parmCanDragOutside);
        }

        numberWidget.number = number;

        //no drag inside if number is < 1.0
        if(canDragInside)
            canDragInside = Mathf.Abs(number.fValue) >= 1.0f;

        ResetDrag();
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        mDragAreaBeginType = GetDragType(eventData.position);
        mDragAreaCurType = mDragAreaBeginType;

        RefreshDragDisplay(eventData.position);
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        mDragAreaCurType = GetDragType(eventData.position);

        RefreshDragDisplay(eventData.position);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
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
                break;
            case DragAreaType.Fraction:
                if(canDragInside) {
                    if(mDragAreaBeginType == DragAreaType.Whole) {
                        var num = numberWidget.number;
                        num.WholeToFraction();
                        numberWidget.number = num;
                    }
                }
                break;
            case DragAreaType.Outside:
                if(canDragOutside) {
                    //determine if we want to swap or placing to an empty slot in operation
                }
                break;
        }
                
        ResetDrag();
    }

    void RefreshDragDisplay(Vector2 pos) {
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

    private void ResetDrag() {
        mDragAreaBeginType = DragAreaType.None;
        mDragAreaCurType = DragAreaType.None;

        if(dragRoot) {
            dragRoot.SetParent(transform, false);
            dragRoot.localPosition = mDragRootDefaultLocalPos;
        }

        if(dragInsideGO) dragInsideGO.SetActive(false);
        if(dragWholeToFractionGO) dragWholeToFractionGO.SetActive(false);
        if(dragFractionToWholeGO) dragFractionToWholeGO.SetActive(false);
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
