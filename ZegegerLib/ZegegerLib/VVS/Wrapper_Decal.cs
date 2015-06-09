///////////////////////////////////////////////////////////////////////////////
//File: Wrapper_Decal.cs
//
//Description: Contains MetaViewWrapper classes implementing Decal views.
//
//References required:
//  System.Drawing
//  Decal.Adapter
//
//This file is Copyright (c) 2010 VirindiPlugins
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using Decal.Adapter;
using Decal.Adapter.Wrappers;

#if METAVIEW_PUBLIC_NS
namespace Zegeger.Decal.VVS.DecalControls
#else
namespace Zegeger.Decal.VVS.DecalControls
#endif

{
#if VVS_WRAPPERS_PUBLIC
    public
#else
    internal
#endif
    class View : IView
    {
        ViewWrapper myView;
        public ViewWrapper Underlying { get { return myView; } }

        #region IView Members

        public void Initialize(PluginHost p, string pXML)
        {
            myView = p.LoadViewResource(pXML);
        }

        public void InitializeRawXML(PluginHost p, string pXML)
        {
            myView = p.LoadView(pXML);
        }

        public void SetIcon(int icon, int iconlibrary)
        {
            myView.SetIcon(icon, iconlibrary);
        }

        public void SetIcon(int portalicon)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        public string Title
        {
            get
            {
                return myView.Title;
            }
            set
            {
                myView.Title = value;
            }
        }

        public bool Visible
        {
            get
            {
                return myView.Activated;
            }
            set
            {
                myView.Activated = value;
            }
        }

        public bool Activated
        {
            get
            {
                return Visible;
            }
            set
            {
                Visible = value;
            }
        }

        public void Activate()
        {
            Visible = true;
        }

        public void Deactivate()
        {
            Visible = false;
        }

        public System.Drawing.Point Location
        {
            get
            {
                return new System.Drawing.Point(myView.Position.X, myView.Position.Y);
            }
            set
            {
                int w = myView.Position.Width;
                int h = myView.Position.Height;
                myView.Position = new System.Drawing.Rectangle(value.X, value.Y, w, h);
            }
        }

        public System.Drawing.Rectangle Position
        {
            get
            {
                return myView.Position;
            }
            set
            {
                myView.Position = value;
            }
        }

        public System.Drawing.Size Size
        {
            get
            {
                return new System.Drawing.Size(myView.Position.Width, myView.Position.Height);
            }
        }

#if VVS_WRAPPERS_PUBLIC
        internal
#else
        public
#endif
        ViewSystemSelector.eViewSystem ViewType { get { return ViewSystemSelector.eViewSystem.DecalInject; } }

        public IControl this[string id]
        {
            get
            {
                Control ret = null;
                IControlWrapper iret = myView.Controls[id];
                if (iret.GetType() == typeof(PushButtonWrapper))
                    ret = new Button();
                if (iret.GetType() == typeof(CheckBoxWrapper))
                    ret = new CheckBox();
                if (iret.GetType() == typeof(TextBoxWrapper))
                    ret = new TextBox();
                if (iret.GetType() == typeof(ChoiceWrapper))
                    ret = new Combo();
                if (iret.GetType() == typeof(SliderWrapper))
                    ret = new Slider();
                if (iret.GetType() == typeof(ListWrapper))
                    ret = new List();
                if (iret.GetType() == typeof(StaticWrapper))
                    ret = new StaticText();
                if (iret.GetType() == typeof(NotebookWrapper))
                    ret = new Notebook();
                if (iret.GetType() == typeof(ProgressWrapper))
                    ret = new ProgressBar();

                if (ret == null) return null;

                ret.myControl = iret;
                ret.myName = id;
                ret.Initialize();
                allocatedcontrols.Add(ret);
                return ret;
            }
        }

        List<Control> allocatedcontrols = new List<Control>();

        #endregion

        #region IDisposable Members

        bool disposed = false;
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            GC.SuppressFinalize(this);

            foreach (Control c in allocatedcontrols)
                c.Dispose();

            myView.Dispose();
        }

        #endregion
    }

#if VVS_WRAPPERS_PUBLIC
    public
#else
    internal
#endif
    class Control : IControl
    {
        internal IControlWrapper myControl;
        public IControlWrapper Underlying { get { return myControl; } }
        internal string myName;

        public virtual void Initialize()
        {

        }

        #region IControl Members

        public string Name
        {
            get { return myName; }
        }

        public bool Visible
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public string TooltipText
        {
            get
            {
                return "";
            }
            set
            {

            }
        }

        #endregion

        #region IDisposable Members

        bool disposed = false;
        public virtual void Dispose()
        {
            if (disposed) return;
            disposed = true;

            //myControl.Dispose();
        }

        #endregion
    }

#if VVS_WRAPPERS_PUBLIC
    public
#else
    internal
#endif
    class Button : Control, IButton
    {
        public override void Initialize()
        {
            base.Initialize();
            ((PushButtonWrapper)myControl).Hit += new EventHandler<ControlEventArgs>(Button_Hit);
            ((PushButtonWrapper)myControl).Click += new EventHandler<ControlEventArgs>(Button_Click);
        }

        public override void Dispose()
        {
            base.Dispose();
            ((PushButtonWrapper)myControl).Hit -= new EventHandler<ControlEventArgs>(Button_Hit);
            ((PushButtonWrapper)myControl).Click -= new EventHandler<ControlEventArgs>(Button_Click);
        }

        void Button_Hit(object sender, ControlEventArgs e)
        {
            if (Hit != null)
                Hit(this, null);
        }

        void Button_Click(object sender, ControlEventArgs e)
        {
            if (Click != null)
                Click(this, new MVControlEventArgs(0));
        }

        #region IButton Members

        public string Text
        {
            get
            {
                return ((PushButtonWrapper)myControl).Text;
                //throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                ((PushButtonWrapper)myControl).Text = value;
                //throw new Exception("The method or operation is not implemented.");
            }
        }

        public System.Drawing.Color TextColor
        {
            get
            {
                return ((PushButtonWrapper)myControl).TextColor;
            }
            set
            {
                ((PushButtonWrapper)myControl).TextColor = value;
            }
        }

        public event EventHandler Hit;
        public event EventHandler<MVControlEventArgs> Click;

        #endregion
    }

#if VVS_WRAPPERS_PUBLIC
    public
#else
    internal
#endif
    class CheckBox : Control, ICheckBox
    {
        public override void Initialize()
        {
            base.Initialize();
            ((CheckBoxWrapper)myControl).Change += new EventHandler<CheckBoxChangeEventArgs>(CheckBox_Change);
        }

        public override void Dispose()
        {
            base.Dispose();
            ((CheckBoxWrapper)myControl).Change -= new EventHandler<CheckBoxChangeEventArgs>(CheckBox_Change);
        }

        void CheckBox_Change(object sender, CheckBoxChangeEventArgs e)
        {
            if (Change != null)
                Change(this, new MVCheckBoxChangeEventArgs(0, Checked));
            if (Change_Old != null)
                Change_Old(this, null);
        }

        #region ICheckBox Members

        public string Text
        {
            get
            {
                return ((CheckBoxWrapper)myControl).Text;
            }
            set
            {
                ((CheckBoxWrapper)myControl).Text = value;
            }
        }

        public bool Checked
        {
            get
            {
                return ((CheckBoxWrapper)myControl).Checked;
            }
            set
            {
                ((CheckBoxWrapper)myControl).Checked = value;
            }
        }

        public event EventHandler<MVCheckBoxChangeEventArgs> Change;
        public event EventHandler Change_Old;

        #endregion
    }

#if VVS_WRAPPERS_PUBLIC
    public
#else
    internal
#endif
    class TextBox : Control, ITextBox
    {
        public override void Initialize()
        {
            base.Initialize();
            ((TextBoxWrapper)myControl).Change += new EventHandler<TextBoxChangeEventArgs>(TextBox_Change);
            ((TextBoxWrapper)myControl).End += new EventHandler<TextBoxEndEventArgs>(TextBox_End);
        }

        public override void Dispose()
        {
            base.Dispose();
            ((TextBoxWrapper)myControl).Change -= new EventHandler<TextBoxChangeEventArgs>(TextBox_Change);
            ((TextBoxWrapper)myControl).End -= new EventHandler<TextBoxEndEventArgs>(TextBox_End);
        }

        void TextBox_Change(object sender, TextBoxChangeEventArgs e)
        {
            if (Change != null)
                Change(this, new MVTextBoxChangeEventArgs(0, e.Text));
            if (Change_Old != null)
                Change_Old(this, null);
        }

        void TextBox_End(object sender, TextBoxEndEventArgs e)
        {
            if (End != null)
                End(this, new MVTextBoxEndEventArgs(0, e.Success));
        }

        #region ITextBox Members

        public string Text
        {
            get
            {
                return ((TextBoxWrapper)myControl).Text;
            }
            set
            {
                ((TextBoxWrapper)myControl).Text = value;
            }
        }

        public int Caret
        {
            get
            {
                return ((TextBoxWrapper)myControl).Caret;
            }
            set
            {
                ((TextBoxWrapper)myControl).Caret = value;
            }
        }

        public event EventHandler<MVTextBoxChangeEventArgs> Change;
        public event EventHandler Change_Old;
        public event EventHandler<MVTextBoxEndEventArgs> End;

        #endregion
    }

#if VVS_WRAPPERS_PUBLIC
    public
#else
    internal
#endif
    class Combo : Control, ICombo
    {
        public override void Initialize()
        {
            base.Initialize();
            ((ChoiceWrapper)myControl).Change += new EventHandler<IndexChangeEventArgs>(Combo_Change);
        }

        public override void Dispose()
        {
            base.Dispose();
            ((ChoiceWrapper)myControl).Change -= new EventHandler<IndexChangeEventArgs>(Combo_Change);
        }

        void Combo_Change(object sender, IndexChangeEventArgs e)
        {
            if (Change != null)
                Change(this, new MVIndexChangeEventArgs(0, e.Index));
            if (Change_Old != null)
                Change_Old(this, null);
        }

        #region ICombo Members

        public IComboIndexer Text
        {
            get
            {
                return new ComboIndexer(this);
            }
        }

        public IComboDataIndexer Data
        {
            get
            {
                return new ComboDataIndexer(this);
            }
        }

        public int Count
        {
            get
            {
                return ((ChoiceWrapper)myControl).Count;
            }
        }

        public int Selected
        {
            get
            {
                return ((ChoiceWrapper)myControl).Selected;
            }
            set
            {
                ((ChoiceWrapper)myControl).Selected = value;
            }
        }

        public event EventHandler<MVIndexChangeEventArgs> Change;
        public event EventHandler Change_Old;

        public void Add(string text)
        {
            ((ChoiceWrapper)myControl).Add(text, null);
        }

        public void Add(string text, object obj)
        {
            ((ChoiceWrapper)myControl).Add(text, obj);
        }

        public void Insert(int index, string text)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RemoveAt(int index)
        {
            ((ChoiceWrapper)myControl).Remove(index);
        }

        public void Remove(int index)
        {
            RemoveAt(index);
        }

        public void Clear()
        {
            ((ChoiceWrapper)myControl).Clear();
        }

        #endregion

        internal class ComboIndexer: IComboIndexer
        {
            Combo myCombo;
            internal ComboIndexer(Combo c)
            {
                myCombo = c;
            }

            #region IComboIndexer Members

            public string this[int index]
            {
                get
                {
                    return ((ChoiceWrapper)myCombo.myControl).Text[index];
                }
                set
                {
                    ((ChoiceWrapper)myCombo.myControl).Text[index] = value;
                }
            }

            #endregion
        }

        internal class ComboDataIndexer : IComboDataIndexer
        {
            Combo myCombo;
            internal ComboDataIndexer(Combo c)
            {
                myCombo = c;
            }

            #region IComboIndexer Members

            public object this[int index]
            {
                get
                {
                    return ((ChoiceWrapper)myCombo.myControl).Data[index];
                }
                set
                {
                    ((ChoiceWrapper)myCombo.myControl).Data[index] = value;
                }
            }

            #endregion
        }
    }

#if VVS_WRAPPERS_PUBLIC
    public
#else
    internal
#endif
    class Slider : Control, ISlider
    {
        public override void Initialize()
        {
            base.Initialize();
            ((SliderWrapper)myControl).Change += new EventHandler<IndexChangeEventArgs>(Slider_Change);
        }

        public override void Dispose()
        {
            base.Dispose();
            ((SliderWrapper)myControl).Change -= new EventHandler<IndexChangeEventArgs>(Slider_Change);
        }

        void Slider_Change(object sender, IndexChangeEventArgs e)
        {
            if (Change != null)
                Change(this, new MVIndexChangeEventArgs(0, e.Index));
            if (Change_Old != null)
                Change_Old(this, null);
        }

        #region ISlider Members

        public int Position
        {
            get
            {
                return ((SliderWrapper)myControl).Position;
            }
            set
            {
                ((SliderWrapper)myControl).Position = value;
            }
        }

        public event EventHandler<MVIndexChangeEventArgs> Change;
        public event EventHandler Change_Old;

        #endregion
    }

#if VVS_WRAPPERS_PUBLIC
    public
#else
    internal
#endif
    class List : Control, IList
    {
        public override void Initialize()
        {
            base.Initialize();
            ((ListWrapper)myControl).Selected += new EventHandler<ListSelectEventArgs>(List_Selected);
        }

        public override void Dispose()
        {
            base.Dispose();
            ((ListWrapper)myControl).Selected -= new EventHandler<ListSelectEventArgs>(List_Selected);
        }

        void List_Selected(object sender, ListSelectEventArgs e)
        {
            if (Click != null)
                Click(this, e.Row, e.Column);
            if (Selected != null)
                Selected(this, new MVListSelectEventArgs(0, e.Row, e.Column));
        }

        #region IList Members

        public event dClickedList Click;
        public event EventHandler<MVListSelectEventArgs> Selected;

        public void Clear()
        {
            ((ListWrapper)myControl).Clear();
        }

        public IListRow this[int row]
        {
            get
            {
                return new ListRow(this, row);
            }
        }

        public IListRow AddRow()
        {
            ((ListWrapper)myControl).Add();
            return new ListRow(this, ((ListWrapper)myControl).RowCount - 1);
        }

        public IListRow Add()
        {
            return AddRow();
        }

        public IListRow InsertRow(int pos)
        {
            ((ListWrapper)myControl).Insert(pos);
            return new ListRow(this, pos);
        }

        public IListRow Insert(int pos)
        {
            return InsertRow(pos);
        }

        public int RowCount
        {
            get { return ((ListWrapper)myControl).RowCount; }
        }

        public void RemoveRow(int index)
        {
            ((ListWrapper)myControl).Delete(index);
        }

        public void Delete(int index)
        {
            RemoveRow(index);
        }

        public int ColCount
        {
            get
            {
                return ((ListWrapper)myControl).ColCount;
            }
        }

        public int ScrollPosition
        {
            get
            {
                return ((ListWrapper)myControl).ScrollPosition;
            }
            set
            {
                ((ListWrapper)myControl).ScrollPosition = value;
            }
        }

        #endregion

        public class ListRow : IListRow
        {
            internal List myList;
            internal int myRow;
            internal ListRow(List l, int r)
            {
                myList = l;
                myRow = r;
            }


            #region IListRow Members

            public IListCell this[int col]
            {
                get { return new ListCell(myList, myRow, col); }
            }

            #endregion
        }

        public class ListCell : IListCell
        {
            internal List myList;
            internal int myRow;
            internal int myCol;
            public ListCell(List l, int r, int c)
            {
                myList = l;
                myRow = r;
                myCol = c;
            }

            #region IListCell Members

            public void ResetColor()
            {
                Color = System.Drawing.Color.White;
            }

            public System.Drawing.Color Color
            {
                get
                {
                    return ((ListWrapper)myList.myControl)[myRow][myCol].Color;
                }
                set
                {
                    ((ListWrapper)myList.myControl)[myRow][myCol].Color = value;
                }
            }

            public int Width
            {
                get
                {
                    return ((ListWrapper)myList.myControl)[myRow][myCol].Width;
                }
                set
                {
                    ((ListWrapper)myList.myControl)[myRow][myCol].Width = value;
                }
            }

            public object this[int subval]
            {
                get
                {
                    return ((ListWrapper)myList.myControl)[myRow][myCol][subval];
                }
                set
                {
                    ((ListWrapper)myList.myControl)[myRow][myCol][subval] = value;
                }
            }

            #endregion
        }

    }

#if VVS_WRAPPERS_PUBLIC
    public
#else
    internal
#endif
    class StaticText : Control, IStaticText
    {

        #region IStaticText Members

        public string Text
        {
            get
            {
                return ((StaticWrapper)myControl).Text;
            }
            set
            {
                ((StaticWrapper)myControl).Text = value;
            }
        }

#pragma warning disable 0067
        public event EventHandler<MVControlEventArgs> Click;
#pragma warning restore 0067

        #endregion
    }

#if VVS_WRAPPERS_PUBLIC
    public
#else
    internal
#endif
    class Notebook : Control, INotebook
    {
        public override void Initialize()
        {
            base.Initialize();
            ((NotebookWrapper)myControl).Change += new EventHandler<IndexChangeEventArgs>(Notebook_Change);
        }

        public override void Dispose()
        {
            base.Dispose();
            ((NotebookWrapper)myControl).Change -= new EventHandler<IndexChangeEventArgs>(Notebook_Change);
        }

        void Notebook_Change(object sender, IndexChangeEventArgs e)
        {
            if (Change != null)
                Change(this, new MVIndexChangeEventArgs(0, e.Index));
        }

        #region INotebook Members

        public event EventHandler<MVIndexChangeEventArgs> Change;

        public int ActiveTab
        {
            get
            {
                return ((NotebookWrapper)myControl).ActiveTab;
            }
            set
            {
                ((NotebookWrapper)myControl).ActiveTab = value;
            }
        }

        #endregion
    }

#if VVS_WRAPPERS_PUBLIC
    public
#else
    internal
#endif
    class ProgressBar : Control, IProgressBar
    {

        #region IProgressBar Members

        public int Position
        {
            get
            {
                return ((ProgressWrapper)myControl).Value;
            }
            set
            {
                ((ProgressWrapper)myControl).Value = value;
            }
        }

        public int Value
        {
            get
            {
                return Position;
            }
            set
            {
                Position = value;
            }
        }

        public string PreText
        {
            get
            {
                return ((ProgressWrapper)myControl).PreText;
            }
            set
            {
                ((ProgressWrapper)myControl).PreText = value;
            }
        }

        #endregion
    }
}

