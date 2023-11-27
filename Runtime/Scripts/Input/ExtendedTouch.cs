using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Runtime.InteropServices;

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PolytopeSolutions.Toolset.Input {
    [StructLayout(LayoutKind.Explicit, Size = sizeof(float)*count)]
    public struct ExtendedTouchState : IInputStateTypeInfo {
        internal const int count = 10;
        internal const string parameters = "clamp=true,clampMin=0,clampMax=10,normalize=true,normalizeMin=0,normalizeMax=10,normalizeZero=0";
        // FourCC type codes are used to identify the memory layouts of state blocks.
        public FourCC format => new FourCC('E', 'X', 'T', 'C');

        [InputControl(displayName = "Left Single Finger Touch", layout = "Analog", parameters = parameters)]
        [FieldOffset(0 * sizeof(float))]
        public float singleFingerLeft;
        [InputControl(displayName = "Right Single Finger Touch", layout = "Analog", parameters = parameters)]
        [FieldOffset(1 * sizeof(float))] 
        public float singleFingerRight;
        [InputControl(displayName = "Up Single Finger Touch", layout = "Analog", parameters = parameters)]
        [FieldOffset(2 * sizeof(float))]
        public float singleFingerUp;
        [InputControl(displayName = "Down Single Finger Touch", layout = "Analog", parameters = parameters)]
        [FieldOffset(3 * sizeof(float))]
        public float singleFingerDown;
        [InputControl(displayName = "Left Two Finger Touch", layout = "Analog", parameters = parameters)]
        [FieldOffset(4 * sizeof(float))]
        public float twoFingerLeft;
        [InputControl(displayName = "Right Two Finger Touch", layout = "Analog", parameters = parameters)]
        [FieldOffset(5 * sizeof(float))]
        public float twoFingerRight;
        [InputControl(displayName = "Up Two Finger Touch", layout = "Analog", parameters = parameters)]
        [FieldOffset(6 * sizeof(float))]
        public float twoFingerUp;
        [InputControl(displayName = "Down Two Finger Touch", layout = "Analog", parameters = parameters)]
        [FieldOffset(7 * sizeof(float))]
        public float twoFingerDown;
        [InputControl(displayName = "Pinch Two Finger Touch", layout = "Analog", parameters = parameters)]
        [FieldOffset(8 * sizeof(float))]
        public float twoFingerPinch;
        [InputControl(displayName = "Zoom Two Finger Touch", layout = "Analog", parameters = parameters)]
        [FieldOffset(9 * sizeof(float))]
        public float twoFingerZoom;
    }

    [InputControlLayout(displayName = "PolytopeSolutions/ExtendedTouch", stateType = typeof(ExtendedTouchState))]
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    public class ExtendedTouch : InputDevice {
        public AxisControl singleFingerLeft { get; private set; }
        public AxisControl singleFingerRight { get; private set; }
        public AxisControl singleFingerUp { get; private set; }
        public AxisControl singleFingerDown { get; private set; }
        public AxisControl twoFingerLeft { get; private set; }
        public AxisControl twoFingerRight { get; private set; }
        public AxisControl twoFingerUp { get; private set; }
        public AxisControl twoFingerDown { get; private set; }
        public AxisControl twoFingerPinch { get; private set; }
        public AxisControl twoFingerZoom { get; private set; }

        private Vector2 _primaryFingerDelta;
        private Vector2 _secondaryFingerDelta;
        private Vector2 _primaryFingerPosition;
        private Vector2 _secondaryFingerPosition;
        private bool _primaryFingerCurrentContact;
        private bool _secondaryFingerCurrentContact;

        public static ExtendedTouch current { get; internal set; }

        public override void MakeCurrent() {
            base.MakeCurrent();
            current = this;
        }
        protected override void OnRemoved() {
            base.OnRemoved();
            if (current == this)
                current = null;
        }
        // Register the device.
        static ExtendedTouch() {
            InputSystem.RegisterLayout<ExtendedTouch>(
                    matches: new InputDeviceMatcher()
                        .WithInterface("HID")
                        .WithDeviceClass("Touchscreen")
                );
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeInPlayer() { }

        protected override void FinishSetup() {
            this.singleFingerLeft = GetChildControl<AxisControl>("singleFingerLeft");
            this.singleFingerRight = GetChildControl<AxisControl>("singleFingerRight");
            this.singleFingerUp = GetChildControl<AxisControl>("singleFingerUp");
            this.singleFingerDown = GetChildControl<AxisControl>("singleFingerDown");
            this.twoFingerLeft = GetChildControl<AxisControl>("twoFingerLeft");
            this.twoFingerRight = GetChildControl<AxisControl>("twoFingerRight");
            this.twoFingerUp = GetChildControl<AxisControl>("twoFingerUp");
            this.twoFingerDown = GetChildControl<AxisControl>("twoFingerDown");
            this.twoFingerPinch = GetChildControl<AxisControl>("twoFingerPinch");
            this.twoFingerZoom = GetChildControl<AxisControl>("twoFingerZoom");

            BindActions();

            base.FinishSetup();
        }
        public void BindActions() {
            if (Touchscreen.current != null) { //Mouse.current != null || 
                InputSystem.EnableDevice(Touchscreen.current);
                //InputSystem.EnableDevice(Mouse.current);
                InputAction primaryContactAction = new InputAction(binding: "<Touchscreen>/touch0/press");//"<Mouse>/leftButton");//
                primaryContactAction.started += _ => {
                    this._primaryFingerCurrentContact = true;
                };
                primaryContactAction.canceled += _ => {
                    this._primaryFingerCurrentContact = false;
                    CheckState();
                };
                primaryContactAction.Enable();
                InputAction secondaryContactAction = new InputAction(binding: "<Touchscreen>/touch1/press");
                secondaryContactAction.started += _ => {
                    this._secondaryFingerCurrentContact = true;
                };
                secondaryContactAction.canceled += _ => {
                    this._secondaryFingerCurrentContact = false;
                    CheckState();
                };
                secondaryContactAction.Enable();
                InputAction primaryDeltaAction = new InputAction(binding: "<Touchscreen>/touch0/delta");//"<Mouse>/delta");//
                primaryDeltaAction.performed += ctx => {
                    if (this._primaryFingerCurrentContact) {
                        this._primaryFingerDelta = ctx.ReadValue<Vector2>();
                        CheckState();
                    }
                };
                primaryDeltaAction.canceled += _ => {
                    if (this._primaryFingerCurrentContact) {
                        this._primaryFingerDelta = Vector2.zero;
                        CheckState();
                    }
                };
                primaryDeltaAction.Enable();
                InputAction primaryPositionAction = new InputAction(binding: "<Touchscreen>/touch0/position");//"<Mouse>/position");//
                primaryPositionAction.performed += ctx => {
                    if (this._primaryFingerCurrentContact) {
                        this._primaryFingerPosition = ctx.ReadValue<Vector2>();
                    }
                };
                primaryPositionAction.Enable();
                InputAction secondaryDeltaAction = new InputAction(binding: "<Touchscreen>/touch1/delta");
                secondaryDeltaAction.performed += ctx => {
                    if (this._secondaryFingerCurrentContact) {
                        this._secondaryFingerDelta = ctx.ReadValue<Vector2>();
                        CheckState();
                    }
                }; 
                secondaryDeltaAction.canceled += _ => {
                    if (this._secondaryFingerCurrentContact) {
                        this._secondaryFingerDelta = Vector2.zero;
                        CheckState();
                    }
                };
                secondaryDeltaAction.Enable();
                InputAction secondaryPositionAction = new InputAction(binding: "<Touchscreen>/touch1/position");
                secondaryPositionAction.performed += ctx => {
                   if (this._secondaryFingerCurrentContact) {
                        this._secondaryFingerPosition = ctx.ReadValue<Vector2>();
                    }
                };
                secondaryPositionAction.Enable();
            }
        }
        private void CheckState() {
            using (StateEvent.From(this, out InputEventPtr eventPtr)) {
                this.singleFingerLeft.WriteValueIntoEvent(0f, eventPtr);
                this.singleFingerLeft.ApplyParameterChanges();
                this.singleFingerRight.WriteValueIntoEvent(0f, eventPtr);
                this.singleFingerRight.ApplyParameterChanges();
                this.singleFingerUp.WriteValueIntoEvent(0f, eventPtr);
                this.singleFingerUp.ApplyParameterChanges();
                this.singleFingerDown.WriteValueIntoEvent(0f, eventPtr);
                this.singleFingerDown.ApplyParameterChanges();
                this.twoFingerLeft.WriteValueIntoEvent(0f, eventPtr);
                this.twoFingerLeft.ApplyParameterChanges();
                this.twoFingerRight.WriteValueIntoEvent(0f, eventPtr);
                this.twoFingerRight.ApplyParameterChanges();
                this.twoFingerUp.WriteValueIntoEvent(0f, eventPtr);
                this.twoFingerUp.ApplyParameterChanges();
                this.twoFingerDown.WriteValueIntoEvent(0f, eventPtr);
                this.twoFingerDown.ApplyParameterChanges();
                this.twoFingerPinch.WriteValueIntoEvent(0f, eventPtr);
                this.twoFingerPinch.ApplyParameterChanges();
                this.twoFingerZoom.WriteValueIntoEvent(0f, eventPtr);
                this.twoFingerZoom.ApplyParameterChanges();

                if (this._primaryFingerCurrentContact && this._secondaryFingerCurrentContact) {
                    // Two finger touch
                    Vector2 primaryDelta = this._primaryFingerDelta;
                    Vector2 secondaryDelta = this._secondaryFingerDelta;
                    float allignmentFactor = Vector2.Dot(primaryDelta.normalized, secondaryDelta.normalized);
                    if (allignmentFactor > 0.35f) { 
                        // alligned movement
                        if (primaryDelta.x < 0) {
                            this.twoFingerLeft.WriteValueIntoEvent(-primaryDelta.x, eventPtr);
                            this.twoFingerLeft.ApplyParameterChanges();
                        }
                        else if (primaryDelta.x > 0) { 
                            this.twoFingerRight.WriteValueIntoEvent(primaryDelta.x, eventPtr);
                            this.twoFingerRight.ApplyParameterChanges();
                        }
                        if (primaryDelta.y < 0) { 
                            this.twoFingerDown.WriteValueIntoEvent(-primaryDelta.y, eventPtr);
                            this.twoFingerDown.ApplyParameterChanges();
                        }
                        else if (primaryDelta.y > 0) { 
                            this.twoFingerUp.WriteValueIntoEvent(primaryDelta.y, eventPtr);
                            this.twoFingerUp.ApplyParameterChanges();
                        }
                    } else if (allignmentFactor < 0f) {
                        float previousDistance = Vector2.Distance(this._primaryFingerPosition-this._primaryFingerDelta, this._secondaryFingerPosition-this._secondaryFingerDelta);
                        float currentDistance = Vector2.Distance(this._primaryFingerPosition, this._secondaryFingerPosition);
                        if (currentDistance > previousDistance) { 
                            this.twoFingerZoom.WriteValueIntoEvent(currentDistance - previousDistance, eventPtr);
                            this.twoFingerZoom.ApplyParameterChanges();
                        }
                        else if (currentDistance < previousDistance) { 
                            this.twoFingerPinch.WriteValueIntoEvent(previousDistance - currentDistance, eventPtr);
                            this.twoFingerPinch.ApplyParameterChanges();
                        }
                    }
                } else if (this._primaryFingerCurrentContact) { 
                    // Single finger touch
                    Vector2 primaryDelta = this._primaryFingerDelta;
                    //Debug.Log("SingleTouch: " + delta.ToString("F6"));
                    float allignmentFactorHorizontal = Mathf.Abs(Vector2.Dot(primaryDelta.normalized, Vector2.left));
                    float allignmentFactorVertical = Mathf.Abs(Vector2.Dot(primaryDelta.normalized, Vector2.up));
                    float threshold = 0.5f;
                    if (allignmentFactorHorizontal > threshold) { 
                        if (primaryDelta.x < 0) {
                            this.singleFingerRight.WriteValueIntoEvent(-primaryDelta.x, eventPtr);
                            this.singleFingerRight.ApplyParameterChanges();
                        }
                        else if (primaryDelta.x > 0) {
                            this.singleFingerLeft.WriteValueIntoEvent(primaryDelta.x, eventPtr);
                            this.singleFingerLeft.ApplyParameterChanges();
                        }
                    }
                    if (allignmentFactorVertical > threshold) { 
                        if (primaryDelta.y < 0) {
                            this.singleFingerUp.WriteValueIntoEvent(-primaryDelta.y, eventPtr);
                            this.singleFingerUp.ApplyParameterChanges();
                        }
                        else if (primaryDelta.y > 0) {
                            this.singleFingerDown.WriteValueIntoEvent(primaryDelta.y, eventPtr);
                            this.singleFingerDown.ApplyParameterChanges();
                        }
                    }
                }
                InputSystem.QueueEvent(eventPtr);
            }
        }
    }
}