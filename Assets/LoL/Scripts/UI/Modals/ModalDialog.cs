using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalDialog : M8.ModalController, M8.IModalActive, M8.IModalPush, M8.IModalPop {
    public const string modalNameGeneric = "dialog";

    public const string parmPortraitSprite = "p";
    public const string parmNameTextRef = "n";
    public const string parmDialogTextRef = "t";
    public const string parmNextCallback = "c";

    public Image portraitImage;
    public Text nameLabel;
    public Text textLabel;

    public bool isPortraitResize;
    public bool isCloseOnNext;
    public bool isTextSpeechAuto = true;

    [Header("Signals")]
    public M8.Signal signalNext; //when the next button is pressed.
    
    private static M8.GenericParams mParms = new M8.GenericParams();

    private string mDialogTextRef;
    private System.Action mNextCallback;

    /// <summary>
    /// Open generic dialog: modalNameGeneric
    /// </summary>
    public static void Open(string nameTextRef, string dialogTextRef, System.Action nextCallback) {
        Open(modalNameGeneric, nameTextRef, dialogTextRef, nextCallback);
    }

    public static void Open(string modalName, string nameTextRef, string dialogTextRef, System.Action nextCallback) {
        //check to see if there's one already opened
        var uiMgr = M8.ModalManager.main;
                
        if(uiMgr.IsInStack(modalName)) {
            var dlg = uiMgr.GetBehaviour<ModalDialog>(modalName);

            dlg.mNextCallback = nextCallback;

            dlg.SetupTextContent(nameTextRef, dialogTextRef);

            if(dlg.isTextSpeechAuto)
                dlg.PlayDialogSpeech();
        }
        else {
            mParms[parmNameTextRef] = nameTextRef;
            mParms[parmDialogTextRef] = dialogTextRef;
            mParms[parmNextCallback] = nextCallback;

            uiMgr.Open(modalName, mParms);
        }
    }

    /// <summary>
    /// Open generic dialog, and apply portrait: modalNameGeneric. set portrait to null to hide portrait
    /// </summary>
    public static void OpenApplyPortrait(Sprite portrait, string nameTextRef, string dialogTextRef, System.Action nextCallback) {
        OpenApplyPortrait(modalNameGeneric, portrait, nameTextRef, dialogTextRef, nextCallback);
    }

    /// <summary>
    /// Open dialog and apply portrait. set portrait to null to hide portrait
    /// </summary>
    public static void OpenApplyPortrait(string modalName, Sprite portrait, string nameTextRef, string dialogTextRef, System.Action nextCallback) {
        //check to see if there's one already opened
        var uiMgr = M8.ModalManager.main;

        if(uiMgr.IsInStack(modalName)) {
            var dlg = uiMgr.GetBehaviour<ModalDialog>(modalName);

            dlg.mNextCallback = nextCallback;

            dlg.SetupPortraitTextContent(portrait, nameTextRef, dialogTextRef);

            if(dlg.isTextSpeechAuto)
                dlg.PlayDialogSpeech();
        }
        else {
            mParms[parmPortraitSprite] = portrait;
            mParms[parmNameTextRef] = nameTextRef;
            mParms[parmDialogTextRef] = dialogTextRef;
            mParms[parmNextCallback] = nextCallback;

            uiMgr.Open(modalName, mParms);
        }
    }

    public void Next() {
        var nextCB = mNextCallback;
        mNextCallback = null;

        if(isCloseOnNext)
            Close();

        if(nextCB != null)
            nextCB();

        if(signalNext != null)
            signalNext.Invoke();
    }

    public void PlayDialogSpeech() {
        if(LoLManager.isInstantiated && !string.IsNullOrEmpty(mDialogTextRef))
            LoLManager.instance.SpeakText(mDialogTextRef);
    }
        
    void M8.IModalActive.SetActive(bool aActive) {
        //play text speech if auto
        if(aActive && isTextSpeechAuto)
            PlayDialogSpeech();
    }

    void M8.IModalPop.Pop() {
        mNextCallback = null;
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        if(parms != null) {
            if(parms.ContainsKey(parmPortraitSprite))
                SetupPortraitTextContent(parms.GetValue<Sprite>(parmPortraitSprite), parms.GetValue<string>(parmNameTextRef), parms.GetValue<string>(parmDialogTextRef));
            else
                SetupTextContent(parms.GetValue<string>(parmNameTextRef), parms.GetValue<string>(parmDialogTextRef));

            mNextCallback = parms.GetValue<System.Action>(parmNextCallback);
        }
    }

    private void SetupPortraitTextContent(Sprite portrait, string nameTextRef, string dialogTextRef) {        
        if(portraitImage) {
            if(portrait) {
                portraitImage.gameObject.SetActive(true);

                portraitImage.sprite = portrait;
                if(isPortraitResize)
                    portraitImage.SetNativeSize();
            }
            else
                portraitImage.gameObject.SetActive(false);
        }

        SetupTextContent(nameTextRef, dialogTextRef);
    }

    private void SetupTextContent(string nameTextRef, string dialogTextRef) {
        //setup other stuff?

        mDialogTextRef = dialogTextRef;

        if(nameLabel) {
            nameLabel.text = !string.IsNullOrEmpty(nameTextRef) ? M8.Localize.Get(nameTextRef) : "";
        }

        if(textLabel) {
            textLabel.text = !string.IsNullOrEmpty(dialogTextRef) ? M8.Localize.Get(dialogTextRef) : "";
        }
    }
}