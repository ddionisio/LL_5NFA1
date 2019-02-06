using TMPro;

namespace HutongGames.PlayMaker.Actions.TextMeshPro {
    [ActionCategory("Text Mesh Pro")]
    [Tooltip("Gets the text value of a TextMeshProUGUI component.")]
    public class TextMeshProGetText : ComponentAction<TextMeshProUGUI> {
        [RequiredField]
        [CheckForComponent(typeof(TextMeshProUGUI))]
        [Tooltip("The GameObject with the UI Text component.")]
        public FsmOwnerDefault gameObject;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("The text value of the UI Text component.")]
        public FsmString text;

        [Tooltip("Runs every frame. Useful to animate values over time.")]
        public bool everyFrame;

        private TextMeshProUGUI uiText;

        public override void Reset() {
            text = null;
            everyFrame = false;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(UpdateCache(go)) {
                uiText = cachedComponent;
            }

            DoGetTextValue();

            if(!everyFrame) {
                Finish();
            }
        }

        public override void OnUpdate() {
            DoGetTextValue();
        }

        private void DoGetTextValue() {
            if(uiText != null) {
                text.Value = uiText.text;
            }
        }

    }
}