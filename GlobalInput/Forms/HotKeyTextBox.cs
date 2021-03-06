﻿using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using GlobalInput.Keyboard;
using GlobalInput.Keyboard.KeyNaming;

namespace GlobalInput.Forms
{
    /// <summary>
    /// Represents a <see cref="TextBox"/> control for hotkey picking.
    /// </summary>
    public class HotkeyTextBox : TextBox
    {
        #region Properties
        /// <summary>
        /// Gets or sets the key code value of the hotkey (the non-modifier).
        /// </summary>
        [Browsable(false)]
        public Keys HotkeyKeyCode
        {
            get { return hotkey.GetKeyCode(); }
            set { Hotkey = HotkeyModifiers | value; }
        }

        /// <summary>
        /// Gets or sets the hotkey modifier flags.
        /// </summary>
        [Browsable(false)]
        public Keys HotkeyModifiers
        {
            get { return hotkey.GetModifiers(); }
            set { Hotkey =  HotkeyKeyCode | value; }
        }

        private Keys hotkey;
        /// <summary>
        /// Gets or sets the user inputted hotkey.
        /// </summary>
        [DefaultValue(Keys.None), Category("Input")]
        [Description("The user inputted hotkey.")]
        public Keys Hotkey
        {
            get { return hotkey; }
            set
            {
                if (!AllowSoloModifiers && value.IsOnlyModifiers())
                {
                    hotkey = Keys.None;
                }
                else if (!AllowToggleKeys && value.HasToggleKey())
                {
                    hotkey = value.GetModifiers();
                }
                else
                {
                    hotkey = value;
                }

                Text = Hotkey.KeyDataToString();
            }
        }

        private string noKeyString = "None";
        /// <summary>
        /// Gets or sets the string to display when there are no valid keys inputted.
        /// </summary>
        [Category("Appearance"), DefaultValue("None")]
        [Description("The string to display when there are no valid keys inputted.")]
        public string NoKeyString
        {
            get { return noKeyString; }
            set
            {
                noKeyString = value;

                if (hotkey == Keys.None)
                    Text = noKeyString;
            }
        }

        private bool allowToggleKeys;
        /// <summary>
        /// Gets or sets whether to allow input of toggle keys (e.x. Tab, CapsLock).
        /// </summary>
        [Category("Behavior"), DefaultValue(false)]
        [Description("Whether to allow input of toggle keys.")]
        public bool AllowToggleKeys
        {
            get { return allowToggleKeys; }
            set
            {
                allowToggleKeys = value;
                Hotkey = Hotkey; // Update
            }
        }

        private bool allowSoloModifiers;
        /// <summary>
        /// Gets or sets whether to allow submission of only modifiers.
        /// If true, only key data having a key-code can be set as a hotkey.
        /// </summary>
        [Category("Behavior"), DefaultValue(false)]
        [Description("Whether to allow input of just modifiers.")]
        public bool AllowSoloModifiers
        {
            get { return allowSoloModifiers; }
            set
            {
                allowSoloModifiers = value;

                if (!allowSoloModifiers)
                {
                    Hotkey = hotkey.GetKeyCode();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current text in the <see cref="T:System.Windows.Forms.TextBox"/>.
        /// </summary>
        /// <returns>
        /// The text displayed in the control.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="HotkeyTextBox"/> class.
        /// </summary>
        public HotkeyTextBox()
        {
            base.Text = NoKeyString;
            GotFocus += delegate { NativeMethods.HideCaret(Handle); };
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyPress"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyPressEventArgs"/> that contains the event data. </param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.PreviewKeyDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PreviewKeyDownEventArgs"/> that contains the event data.</param>
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            // Keep tab from shifting focus.
            switch (e.KeyCode)
            {
                case Keys.Tab: e.IsInputKey = true; break;
            }
        }

        /// <summary>
        /// Processes a key message and generates the appropriate control events.
        /// </summary>
        /// <returns>
        /// true if the message was processed by the control; otherwise, false.
        /// </returns>
        /// <param name="m">A <see cref="T:System.Windows.Forms.Message"/>, passed by reference, that represents the window message to process. </param>
        protected override bool ProcessKeyEventArgs(ref Message m)
        {
            Keys key = (Keys)m.WParam;

            switch (key)
            {
                case Keys.PrintScreen: HotkeyKeyCode = Keys.PrintScreen; break;
            }

            return base.ProcessKeyEventArgs(ref m);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"/> that contains the event data. </param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Keep alt key from dropping focus.
            if (e.Modifiers == Keys.Alt)
            {
                e.Handled = true;
            }

            switch (e.KeyCode)
            {
                // Use backspace for clearing.
                case Keys.Back: Hotkey = Keys.None; return;
                // Keep delete key from deleting first character.
                case Keys.Delete: e.SuppressKeyPress = true; break;
                // Keep tab key from dropping focus.
                case Keys.Tab:
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    break;
            }

            Hotkey = e.KeyData;
        }
    }
}