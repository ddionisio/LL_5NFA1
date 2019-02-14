using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSlotsWidget : CardDropWidgetBase {
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

        public void SetCard(CardWidget aCard, bool releasePrevious) {
            if(card != aCard) {
                if(releasePrevious && card) card.poolData.Release();

                card = aCard;
                
                if(card) {
                    card.isFractionVisual = true;
                    card.canDragInside = true;

                    card.transform.SetParent(anchor, false);
                    card.transform.localPosition = Vector3.zero;
                }

                if(highlightGO) highlightGO.SetActive(false);
            }

            if(emptyGO) emptyGO.SetActive(card == null);
        }

        public void Init() {
            SetCard(null, true);

            if(emptyGO)
                emptyGO.SetActive(false);

            if(highlightGO)
                highlightGO.SetActive(false);
        }
    }

    public OperandData[] slots;

    public event System.Action updateCallback;

    private int mHighlightIndex;

    public void Init() {
        for(int i = 0; i < slots.Length; i++)
            slots[i].Init();

        mHighlightIndex = -1;
    }

    public void SetCard(int index, CardWidget card) {
        if(index < 0 || index >= slots.Length) {
            Debug.LogWarning("Invalid index: " + index);
            return;
        }

        slots[index].SetCard(card, true);
        slots[index].SetActive(true);
    }

    public void SetActive(int index, bool aActive) {
        if(index < 0 || index >= slots.Length) {
            Debug.LogWarning("Invalid index: " + index);
            return;
        }

        slots[index].SetActive(aActive);
    }

    public override int CardDropGetSlotIndex(CardWidget card) {
        int retInd = -1;

        //assume no rotation/scale
        var cardRect = card.dragRoot.rect;
        cardRect.position = card.dragRoot.position;

        for(int i = 0; i < slots.Length; i++) {
            var slot = slots[i];
            var anchor = slot.anchor;

            var cardLocalRect = cardRect;
            cardLocalRect.position = anchor.InverseTransformPoint(cardRect.position);

            if(anchor.rect.Overlaps(cardLocalRect)) {
                retInd = i;
                break;
            }
        }

        return retInd;
    }
    
    public override CardWidget CardDropSet(int index, CardWidget card) {
        ClearHighlight();

        var prevCard = slots[index].card;
        slots[index].SetCard(card, false);

        if(updateCallback != null)
            updateCallback();

        return prevCard;
    }

    public override void CardDropPositionUpdate(CardWidget card) {
        //check which slot to highlight
        int highlightInd = CardDropGetSlotIndex(card);

        if(mHighlightIndex != highlightInd) {
            ClearHighlight();

            mHighlightIndex = highlightInd;

            if(mHighlightIndex != -1) {
                if(slots[mHighlightIndex].highlightGO)
                    slots[mHighlightIndex].highlightGO.SetActive(true);
            }
        }
    }
    
    private void ClearHighlight() {
        if(mHighlightIndex != -1) {
            if(slots[mHighlightIndex].highlightGO)
                slots[mHighlightIndex].highlightGO.SetActive(false);

            mHighlightIndex = -1;
        }
    }
}
