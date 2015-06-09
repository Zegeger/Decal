using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Zegeger.Decal.VVS;
using Zegeger.Diagnostics;

namespace Zegeger.Decal.Controls
{
    public enum MoveDirection
    {
        Up,
        Down
    }

    public interface IRowItemList
    {
        ObservableCollection<IRowItem> list { get; }
    }

    public interface IRowItem
    {
        Row RowItem { get; }
    }

    public delegate void SelectedIndexChangedEvent(SelectedIndexChangedEventArgs e);
    public delegate void SelectedRowChangedEvent(SelectedRowChangedEventArgs e);
    public delegate void ValueDataChangedEvent(ValueDataChangedEventArgs e);

    public class SelectedIndexChangedEventArgs : EventArgs
    {
        public int NewIndex { get; private set; }

        public SelectedIndexChangedEventArgs(int newIndex)
        {
            NewIndex = newIndex;
        }
    }
    
    public class SelectedRowChangedEventArgs : EventArgs
    {
        public IRowItem NewRow { get; private set; }

        public SelectedRowChangedEventArgs(IRowItem newRow)
        {
            NewRow = newRow;
        }
    }

    public class ValueDataChangedEventArgs : EventArgs
    {
        public Column ChangedColumn { get; private set; }

        public ValueDataChangedEventArgs(Column changedColumn)
        {
            ChangedColumn = changedColumn;
        }
    }

    public class Row
    {
        private Dictionary<string, Column> _columns;

        public Row()
        {
            _columns = new Dictionary<string, Column>();
        }

        public void AddColumn(string name, int columnIndex)
        {
            if (!_columns.ContainsKey(name))
            {
                _columns.Add(name, new Column(name, columnIndex, this));
            }
        }

        public Dictionary<string,Column>.ValueCollection Columns
        {
            get
            {
                return _columns.Values;
            }
        }

        public Column this[string value]
        {
            get
            {
                return _columns[value];
            }
        }
    }

    public class Column
    {
        private string _name;
        private IColumnData _data;
        private int _columnIndex;
        private Row _parentRow;

        public event ValueDataChangedEvent ValueDataChanged;

        public Column(string Name, int ColumnIndex, Row ParentRow)
        {
            _name = Name;
            _columnIndex = ColumnIndex;
            _parentRow = ParentRow;
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public int ColumnIndex
        {
            get
            {
                return _columnIndex;
            }
        }

        public Row ParentRow
        {
            get
            {
                return _parentRow;
            }
        }

        public IColumnData Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
                if (ValueDataChanged != null)
                {
                    ValueDataChanged(new ValueDataChangedEventArgs(this));
                }
            }
        }
    }

    public interface IColumnData
    {
        int SubVal { get; }
        object Content { get; }
    }

    public class BoolColumnData : IColumnData
    {
        private bool _content;
       
        public int SubVal
        {
            get
            {
                return 0;
            }
        }
  
        public BoolColumnData(bool Content)
        {
            _content = Content;
        }

        public object Content
        {
            get
            {
                return _content;
            }
        }
    }

    public class StringColumnData : IColumnData
    {
        private string _content;
       
        public int SubVal
        {
            get
            {
                return 0;
            }
        }
  
        public StringColumnData(string Content)
        {
            _content = Content;
        }

        public object Content
        {
            get
            {
                return _content;
            }
        }
    }

    public class ImageColumnData : IColumnData
    {
        private System.Drawing.Bitmap _content;

        public int SubVal
        {
            get
            {
                return 1;
            }
        }

        public ImageColumnData(System.Drawing.Bitmap Content)
        {
            _content = Content;
        }

        public object Content
        {
            get
            {
                return new VirindiViewService.ACImage(_content);
            }
        }
    }

    public class IconColumnData : IColumnData
    {
        private int _content;

        public int SubVal
        {
            get
            {
                return 1;
            }
        }

        public IconColumnData(int Content)
        {
            _content = Content;
        }

        public object Content
        {
            get
            {
                return new VirindiViewService.ACImage(_content);
            }
        }
    }

    public class RowItemList<T>:IRowItemList, ICollection<T> where T: IRowItem
    {
        public ObservableCollection<IRowItem> list { get; private set; }

        public RowItemList()
        {
            list = new ObservableCollection<IRowItem>();
        }

        public void Add(T item)
        {
            list.Add(item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void Insert(int index, T item)
        {
            list.Insert(index, item);
        }

        public void Move(int oldIndex, int newIndex)
        {
            list.Move(oldIndex, newIndex);
        }

        public bool Remove(T item)
        {
            return list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public void CopyTo(T[] array, int index)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return new RowItemListEnumerator<T>(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new RowItemListEnumerator<T>(this);
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        public T this[int index]
        {
            get
            {
                return (T)list[index];
            }
            set
            {
                list[index] = value;
            }
        }
    }

    public class RowItemListEnumerator<T> : IEnumerator<T> where T : IRowItem
    {
        protected RowItemList<T> _collection; //enumerated collection
        protected int index; //current index
        protected T _current; //current enumerated object in the collection

        // Default constructor
        public RowItemListEnumerator()
        {
            //nothing
        }

        // Paramaterized constructor which takes
        // the collection which this enumerator will enumerate
        public RowItemListEnumerator(RowItemList<T> collection)
        {
            _collection = collection;
            index = -1;
            _current = default(T);
        }

        // Current Enumerated object in the inner collection
        public virtual T Current
        {
            get
            {
                return _current;
            }
        }

        // Explicit non-generic interface implementation for IEnumerator
        // (extended and required by IEnumerator<T>)
        object System.Collections.IEnumerator.Current
        {
            get
            {
                return _current;
            }
        }

        // Dispose method
        public virtual void Dispose()
        {
            _collection = null;
            _current = default(T);
            index = -1;
        }

        // Move to next element in the inner collection
        public virtual bool MoveNext()
        {
            //make sure we are within the bounds of the collection
            if (++index >= _collection.Count)
            {
                //if not return false
                return false;
            }
            else
            {
                //if we are, then set the current element
                //to the next object in the collection
                _current = _collection[index];
            }
            //return true
            return true;
        }

        // Reset the enumerator
        public virtual void Reset()
        {
            _current = default(T); //reset current object
            index = -1;
        }
    }



    public class DecalList
    {
        private IList viewList;

        private ObservableCollection<IRowItem> list;
        
        private int selectedIndex = -1;
        private IRowItem selectedRow;
        private System.Drawing.Color textColor = System.Drawing.Color.White;
        private System.Drawing.Color highlightColor = System.Drawing.Color.Aquamarine;
        private int[] highlightColumn = {0};

        public event SelectedIndexChangedEvent SelectedIndexChanged;
        public event SelectedRowChangedEvent SelectedRowChanged;

        public IRowItemList List
        {
            set
            {
                list = value.list;
                list.CollectionChanged += list_CollectionChanged;
                Load();
            }
        }

        public int[] HighlightColumn
        {
            get
            {
                return highlightColumn;
            }
            set
            {
                ClearHighlight();
                highlightColumn = value;
                Highlight();
            }
        }

        public System.Drawing.Color HighlightColor
        {
            get
            {
                return highlightColor;
            }

            set
            {
                highlightColor = value;
                Highlight();
            }
        }

        public System.Drawing.Color TextColor
        {
            get
            {
                return textColor;
            }

            set
            {
                textColor = value;
                Highlight();
            }
        }

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set 
            {
                try
                {
                    if (value >= 0 && value < viewList.RowCount)
                    {
                        selectedIndex = value;
                        Highlight();
                        if (SelectedIndexChanged != null)
                            SelectedIndexChanged(new SelectedIndexChangedEventArgs(value));
                        if (selectedRow == null || selectedRow != list[value])
                        {
                            selectedRow = list[value];
                            if (SelectedRowChanged != null)
                                SelectedRowChanged(new SelectedRowChangedEventArgs(selectedRow));
                        }
                    }
                }
                catch (Exception ex)
                {
                    TraceLogger.Write(ex);
                }
            }
        }

        public IRowItem SelectedRow
        {
            get { return selectedRow; }
            set
            {
                try
                {
                    if (list.Contains(value))
                    {
                        selectedRow = value;
                        Highlight();
                        SelectedIndex = list.IndexOf(value);
                    }
                }
                catch (Exception ex)
                {
                    TraceLogger.Write(ex);
                }
            }
        }

        internal DecalList(IList ViewList)
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            viewList = ViewList;
            viewList.Selected += new EventHandler<MVListSelectEventArgs>(viewList_Selected);
            
            list = new ObservableCollection<IRowItem>();
            list.CollectionChanged += list_CollectionChanged;
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        internal DecalList(IList ViewList, IRowItemList RowList) : this(ViewList)
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            list = RowList.list;
            list.CollectionChanged += list_CollectionChanged;
            Load();
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        private void Load()
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            viewList.Clear();
            foreach (IRowItem item in list)
            {
                Add(item);
            }
            ResetSelected();
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void list_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            TraceLogger.Write("Enter Action: " + e.Action.ToString(), TraceLevel.Noise);
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (IRowItem item in e.NewItems)
                {
                    Add(item);
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                int i = e.OldStartingIndex;
                foreach (IRowItem item in e.OldItems)
                {
                    Remove(i);
                    if (i == selectedIndex)
                    {
                        ResetSelected();
                    }
                    else if (selectedIndex > i)
                    {
                        SelectedIndex--;
                    }
                    i++;
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                int oldindex = e.OldStartingIndex;
                int newindex = e.NewStartingIndex;
                foreach (IRowItem item in e.OldItems)
                {
                    Remove(oldindex);
                    Insert(newindex, item.RowItem);
                    if (SelectedIndex == oldindex)
                    {
                        SelectedIndex = newindex;
                    }
                    oldindex++;
                    newindex++;
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                Load();
            }
            if (selectedIndex == -1)
                ResetSelected();
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        private void Add(IRowItem item)
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            HookRowEvent(item.RowItem);
            IListRow row = viewList.Add();
            AddItemToRow(row, item.RowItem);
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        public void Insert(int index, Row item)
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            HookRowEvent(item);
            IListRow row = viewList.Insert(index);
            AddItemToRow(row, item);
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        private void Remove(int index)
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            viewList.RemoveRow(index);
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        private void HookRowEvent(Row item)
        {
            TraceLogger.Write("Enter", TraceLevel.Noise);
            foreach (Column col in item.Columns)
            {
                col.ValueDataChanged += new ValueDataChangedEvent(col_ValueDataChanged);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        void col_ValueDataChanged(ValueDataChangedEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                foreach (IRowItem item in list)
                {
                    if (item.RowItem.Equals(e.ChangedColumn.ParentRow))
                    {
                        int index = list.IndexOf(item);
                        viewList[index][e.ChangedColumn.ColumnIndex][e.ChangedColumn.Data.SubVal] = e.ChangedColumn.Data.Content;
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void viewList_Selected(object sender, MVListSelectEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                SelectedIndex = e.Row;
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void AddItemToRow(IListRow row, Row item)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                foreach (Column col in item.Columns)
                {
                    TraceLogger.Write("Column: " + col.Name + ", Value: " + col.Data.Content + ", IsNull: " + (col.Data.Content == null), TraceLevel.Verbose);
                    if (col.Data.Content.GetType() == typeof(VirindiViewService.ACImage))
                    {
                        TraceLogger.Write("ACImage Bitmap IsNull: " + (((VirindiViewService.ACImage)col.Data.Content).BitmapImage == null));
                    }
                    row[col.ColumnIndex][col.Data.SubVal] = col.Data.Content;
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void ResetSelected()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                if (viewList.RowCount > 0)
                {
                    SelectedIndex = 0;
                }
                else
                {
                    SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void ClearHighlight()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                for (int i = 0; i < viewList.RowCount; i++)
                {
                    foreach (int j in highlightColumn)
                    {
                        viewList[i][j].Color = textColor;
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void Highlight()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                ClearHighlight();
                if (selectedIndex != -1)
                {
                    TraceLogger.Write("Highlighting " + selectedIndex, TraceLevel.Verbose);
                    foreach (int i in highlightColumn)
                    {
                        viewList[selectedIndex][i].Color = highlightColor;
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }
}
