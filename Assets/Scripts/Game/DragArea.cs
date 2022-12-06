using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragArea {
    public const string tagDrag = "DragArea";

    public static Transform transform {
        get {
            if(!mTransform) {
                var go = GameObject.FindGameObjectWithTag(tagDrag);
                if(go)
                    mTransform = go.transform;
            }

            return mTransform;
        }
    }

    private static Transform mTransform;
}
