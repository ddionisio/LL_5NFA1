using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardDropWidgetBase : MonoBehaviour {
    public abstract int CardDropGetSlotIndex(CardWidget card);
    public abstract CardWidget CardDropSet(int index, CardWidget card); //return previous card from that index
    public abstract void CardDropHighlight(int index);
    public abstract void CardDropHighlightClear();
}
