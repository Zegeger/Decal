using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Globalization;
using Zegeger.Decal.VVS;

namespace Zegeger.Decal.Data
{
    public interface IControlSetting<ControlType, SettingType>
    {
        void AttachControl(ControlType control);
        SettingType Value { get; set; }
    }

    public delegate void ControlSettingChangedEvent<T>(ControlSettingChangedEventArgs<T> e);

    public class ControlSettingChangedEventArgs<T>
    {
        public T NewValue { get; private set; }

        public ControlSettingChangedEventArgs(T newValue)
        {
            NewValue = newValue;
        }
    }

    public class CheckboxSetting : IControlSetting<ICheckBox, bool>
    {
        private bool _value;
        public bool Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                if(_control != null)
                    _control.Checked = value;
                SettingsProfileHandler.Save();
                RaiseEvent();
            }
        }

        private ICheckBox _control;

        public event ControlSettingChangedEvent<bool> ControlSettingChanged;
        
        private void RaiseEvent()
        {
            if(ControlSettingChanged != null)
                    ControlSettingChanged(new ControlSettingChangedEventArgs<bool>(_value));
        }

        void _control_Change(object sender, MVCheckBoxChangeEventArgs e)
        {
            _value = e.Checked;
            SettingsProfileHandler.Save();
            RaiseEvent();
        }

        public void AttachControl(ICheckBox control)
        {
            if(_control != null)
                _control.Change -= _control_Change;
            _control = control;
            _control.Checked = _value;
            _control.Change += _control_Change;
        }

        public CheckboxSetting()
        {

        }

        public CheckboxSetting(bool InitialValue)
        {
            _value = InitialValue;
        }
    }

    public class TextBoxStringSetting : IControlSetting<ITextBox, string>
    {
        private string _value;
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                if (_control != null)
                    _control.Text = value;
                SettingsProfileHandler.Save();
                RaiseEvent();
            }
        }

        private ITextBox _control;
        public event ControlSettingChangedEvent<string> ControlSettingChanged;

        private void RaiseEvent()
        {
            if (ControlSettingChanged != null)
                ControlSettingChanged(new ControlSettingChangedEventArgs<string>(_value));
        }

        public void AttachControl(ITextBox control)
        {
            if (_control != null)
                _control.End -= _control_End;
            _control = control;
            _control.Text = _value;
            _control.End += _control_End;
        }

        void _control_End(object sender, MVTextBoxEndEventArgs e)
        {
            _value = _control.Text;
            SettingsProfileHandler.Save();
            RaiseEvent();
        }

        public TextBoxStringSetting()
        {

        }

        public TextBoxStringSetting(string InitialValue)
        {
            _value = InitialValue;
        }
    }

    public class TextBoxIntSetting : IControlSetting<ITextBox, int>
    {
        private int _value;
        public int Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                if (_control != null)
                    _control.Text = value.ToString();
                SettingsProfileHandler.Save();
                RaiseEvent();
            }
        }

        private int _default;
        private ITextBox _control;
        public event ControlSettingChangedEvent<int> ControlSettingChanged;

        private void RaiseEvent()
        {
            if (ControlSettingChanged != null)
                ControlSettingChanged(new ControlSettingChangedEventArgs<int>(_value));
        }

        public void AttachControl(ITextBox control)
        {
            if (_control != null)
                _control.End -= _control_End;
            _control = control;
            _control.Text = _value.ToString();
            _control.End += _control_End;
        }

        void _control_End(object sender, MVTextBoxEndEventArgs e)
        {
            int val;
            if (int.TryParse(_control.Text, out val))
            {
                _value = val;
                SettingsProfileHandler.Save();
                RaiseEvent();
            }
            else
            {
                _control.Text = _default.ToString();
            }
        }

        public TextBoxIntSetting()
        {

        }

        public TextBoxIntSetting(int InitialValue)
        {
            _value = InitialValue;
            _default = InitialValue;
        }

        public TextBoxIntSetting(int InitialValue, int Default) : this(InitialValue)
        {
            _default = Default;
        }
    }

    public class TextBoxDoubleSetting : IControlSetting<ITextBox, double>
    {
        private double _value;
        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                if (_control != null)
                {
                    SetControlText();
                }
                SettingsProfileHandler.Save();
                RaiseEvent();
            }
        }

        public bool IsPercentage { get; set; }

        private double _default;
        private ITextBox _control;
        public event ControlSettingChangedEvent<double> ControlSettingChanged;

        private void RaiseEvent()
        {
            if (ControlSettingChanged != null)
                ControlSettingChanged(new ControlSettingChangedEventArgs<double>(_value));
        }

        public void AttachControl(ITextBox control)
        {
            if (_control != null)
                _control.End -= _control_End;
            _control = control;
            SetControlText();
            _control.End += _control_End;
        }

        void _control_End(object sender, MVTextBoxEndEventArgs e)
        {
            double val;
            if (TryParsePercentageString(_control.Text, out val))
            {
                _value = val;
                SettingsProfileHandler.Save();
                RaiseEvent();
                SetControlText();
            }
            else
            {
                _control.Text = _default.ToString();
            }
        }

        private void SetControlText()
        {
            if (IsPercentage)
            {
                _control.Text = _value.ToString("P0");
            }
            else
            {
                _control.Text = _value.ToString("F");
            }
        }

        public TextBoxDoubleSetting()
        {

        }

        public TextBoxDoubleSetting(double InitialValue)
        {
            _value = InitialValue;
            _default = InitialValue;
        }

        public TextBoxDoubleSetting(double InitialValue, double Default)
            : this(InitialValue)
        {
            _default = Default;
        }

        public bool TryParsePercentageString(string value, out double num)
        {
            num = double.NaN;
            bool success = true;
            CultureInfo culture = CultureInfo.CurrentUICulture;
            try
            {
                string str = (string)value;
                if ((culture != null) && !string.IsNullOrEmpty(str))
                {
                    str = ((string)value).Trim();
                    int divisor = 1;
                    if ((!culture.IsNeutralCulture && (str.Length > 0)) && (culture.NumberFormat != null))
                    {
                        switch (culture.NumberFormat.PercentPositivePattern)
                        {
                            case 0:
                            case 1:
                                if ((str.Length - 1) == str.LastIndexOf(culture.NumberFormat.PercentSymbol, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    str = str.Substring(0, str.Length - 1);
                                    divisor = 100;
                                }
                                break;

                            case 2:
                                if (str.IndexOf(culture.NumberFormat.PercentSymbol, StringComparison.CurrentCultureIgnoreCase) == 0)
                                {
                                    str = str.Substring(1);
                                    divisor = 100;
                                }
                                break;
                        }
                    }
                    num =  Convert.ToDouble(str, culture) / divisor;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                success = false;
            }
            catch (ArgumentNullException)
            {
                success = false;
            }
            catch (FormatException)
            {
                success = false;
            }
            catch (OverflowException)
            {
                success = false;
            }
            return success;
        }
    }

    public class ComboSetting : IControlSetting<ICombo, string>
    {
        private string _value;
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                if (_control != null)
                {
                    for (int i = 0; i < _control.Count; i++)
                    {
                        if (_control.Text[i] == _value)
                        {
                            _control.Selected = i;
                            break;
                        }
                    }
                }
                SettingsProfileHandler.Save();
                RaiseEvent();
            }
        }

        private ICombo _control;
        public event ControlSettingChangedEvent<string> ControlSettingChanged;

        private void RaiseEvent()
        {
            if (ControlSettingChanged != null)
                ControlSettingChanged(new ControlSettingChangedEventArgs<string>(_value));
        }

        public void AttachControl(ICombo control)
        {
            if (_control != null)
                _control.Change -= _control_Change;
            _control = control;
            for (int i = 0; i < _control.Count; i++)
            {
                if (_control.Text[i] == _value)
                {
                    _control.Selected = i;
                    break;
                }
            }
            _control.Change += _control_Change;
        }

        void _control_Change(object sender, MVIndexChangeEventArgs e)
        {
            _value = _control.Text[e.Index];
            SettingsProfileHandler.Save();
            RaiseEvent();
        }

        public ComboSetting()
        {
            _value = "";
        }

        public ComboSetting(string InitialValue)
        {
            _value = InitialValue;
        }
    }
}
