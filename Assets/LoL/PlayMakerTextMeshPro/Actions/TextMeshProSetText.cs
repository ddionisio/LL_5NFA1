using TMPro;

namespace HutongGames.PlayMaker.Actions.TextMeshPro {
    [ActionCategory("Text Mesh Pro")]
    [Tooltip("Sets the text value of a TextMeshProUGUI component.")]
    public class TextMeshProSetText : ComponentAction<TMP_Text> {
        [RequiredField]
        [CheckForComponent(typeof(TMP_Text))]
        [Tooltip("The GameObject with the UI Text component.")]
        public FsmOwnerDefault gameObject;

        [UIHint(UIHint.TextArea)]
        [Tooltip("The text of the UI Text component.")]
        public FsmString text;

        [Tooltip("Reset when exiting this state.")]
        public FsmBool resetOnExit;

        [Tooltip("Repeats every frame")]
        public bool everyFrame;

        private TMP_Text uiText;
        private string originalString;

        public override void Reset() {
            gameObject = null;
            text = null;
            resetOnExit = null;
            everyFrame = false;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(UpdateCache(go)) {
                uiText = cachedComponent;
            }

            originalString = uiText.text;

            DoSetTextValue();

            if(!everyFrame) {
                Finish();
            }
        }

        public override void OnUpdate() {
            DoSetTextValue();
        }

        private void DoSetTextValue() {
            if(uiText == null) return;

            uiText.text = text.Value;
        }

        public override void OnExit() {
            if(uiText == null) return;

            if(resetOnExit.Value) {
                uiText.text = originalString;
            }
        }

#if UNITY_EDITOR
        public override string AutoName() {
            return "UISetText : " + ActionHelpers.GetValueLabel(text);
        }
#endif
    }
}