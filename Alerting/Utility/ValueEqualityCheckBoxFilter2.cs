//
// Copyright Dapfor 2007 - 2013. All rights reserved.
//
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// sales@dapfor.com or on our web site http://www.dapfor.com/en/net-suite/net-grid/license.
// Any infringement will be prosecuted under applicable laws.
//

using Dapfor.Net.Editors;
using Dapfor.Net.Ui;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
#if TRADER
using Trader.Properties;
#endif
#if CHARTS
using Charts.Properties;
#endif

namespace Dapfor.Test.Editors
{
    /// <summary>
    /// Column filter that enables multiple _values selection.
    /// </summary>
    [Serializable, XmlRoot("ValueEqualityCheckBoxFilter2")]
    public class ValueEqualityCheckBoxFilter2 : UITypeEditor, IFilter, IXmlSerializable
    {
#region Fields

        private Column _column;
        private Size? _size;

        private IList _filterValues;
        private Dictionary<object, ValueInfo> _dico;
        private bool _deserialized;

#endregion Fields

        //This constructor is called while deserialization
        public ValueEqualityCheckBoxFilter2()
        {
        }

        public ValueEqualityCheckBoxFilter2(IList filterValues)
        {
            _filterValues = filterValues;
            _dico = new Dictionary<object, ValueInfo>();
            if (_filterValues != null)
            {
                foreach (object filterValue in _filterValues)
                {
                    if (filterValue != null && !_dico.ContainsKey(filterValue))
                    {
                        _dico.Add(filterValue, new ValueInfo(true, filterValue));
                    }
                }
            }
        }

#region IFilter Members

        /// <summary>
        /// Determines whether the specified <see cref="Row"/> is filtered.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>
        /// 	<c>true</c> if the specified row is filtered; otherwise, <c>false</c>.
        /// </returns>
        public bool IsFiltered(Row row)
        {
            if (_column != null && _dico != null && _dico.Count > 0)
            {
                Cell cell = row[_column.Id];
                if (cell != null)
                {
                    object value = cell.Value;
                    //string stringValue;
                    ValueInfo valueInfo;
                    if (value != null && _dico.TryGetValue(value, out valueInfo))
                    {
                        return !valueInfo.IsChecked;
                    }
                    else if (value == null)
                    {
                        return false;
                    }
                    else if (_dico.ContainsKey(value) == false)
                    {
                        return false;
                    }
                    return true;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Occurs when the filter's conditions are changed.
        /// </summary>
        /// <remarks>This event is raised to force the <see cref="Grid"/> to verify the state of all inserted <see cref="Row"/>s.</remarks>
        /// <threadsafety>This event may be raised from any thread. If the <see cref="Grid"/> receives a notification from the non-GUI thread, then it will synchronize
        /// the calling thread with the GUI thread in the asynchronous way without blocking the calling thread. </threadsafety>
        public event EventHandler<EventArgs> FilterUpdated;

#endregion IFilter Members

#region UITypeEditor implementation

        /// <summary>
        /// Gets <see cref="UITypeEditorEditStyle.DropDown"/> style.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that can be used to gain additional context information.</param>
        /// <returns>
        /// <see cref="UITypeEditorEditStyle.DropDown"/> style.
        /// </returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        /// <summary>
        /// Edits the specified object's value using the editor style indicated by the <see cref="M:System.Drawing.Design.UITypeEditor.GetEditStyle"/> method.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that can be used to gain additional context information.</param>
        /// <param name="provider">An <see cref="T:System.IServiceProvider"/> that this editor can use to obtain services.</param>
        /// <param name="value">The object to edit.</param>
        /// <returns>
        /// The new value of the object. If the value of the object has not changed, this should return the same object it was passed.
        /// </returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IFilter result = null;
            if (value is Column)
            {
                _column = (Column)value;
            }
            IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            if (service != null)
            {
                Header header = _column != null ? _column.Header : null;
                Grid grid = header != null ? header.Grid : null;
                if (grid != null)
                {
                    EditorControl control;
                    using (control = new EditorControl(this, service, _dico))
                    {
                        if (_size != null)
                        {
                            control.Size = _size.Value;
                        }
                        else
                        {
                            control.Width = Math.Max(165, _column.Width);
                        }

                        //style the control
                        if (grid.Theme != null)
                        {
                            control.Grid.Theme = grid.Theme;
                        }

                        service.DropDownControl(control);
                        _size = control.Size;
                    }
                    result = control.Filter;

                    if (_deserialized)
                    {
                        _deserialized = false;
                        result = this;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true.
        /// </summary>
        /// <value></value>
        /// <returns>Always <c>true</c>.
        /// </returns>
        public override bool IsDropDownResizable
        {
            get { return true; }
        }

#endregion UITypeEditor implementation

#region IXmlSerializable Members

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            _filterValues = new ArrayList();
            _dico = new Dictionary<object, ValueInfo>();
            reader.MoveToContent();

            //1. Read value type
            string valueType = !reader.IsEmptyElement ? reader["valueType"] : null;
            bool isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();
            TypeConverter tc = null;
            if (!isEmptyElement) // (1)
            {
                //2. Deserialize value type and try to get a TypeConverter
                if (!string.IsNullOrEmpty(valueType))
                {
                    Type vt = Type.GetType(valueType);
                    if (vt != null)
                    {
                        tc = TypeDescriptor.GetConverter(vt);
                    }
                }

                //3. Read serialized values
                isEmptyElement = reader.IsEmptyElement;
                reader.ReadStartElement();
                if (!isEmptyElement)
                {
                    while (reader.NodeType != XmlNodeType.EndElement)
                    {
                        //4. Read 'active' attribute
                        string active = reader["active"];
                        bool isActive = !string.IsNullOrEmpty(active) && String.Compare(active, "true", StringComparison.OrdinalIgnoreCase) == 0;
                        string value = reader.ReadElementString("value");

                        //5. Try to deserialize values
                        if (tc != null)
                        {
                            object v = tc.ConvertFromInvariantString(value);
                            if (v != null)
                            {
                                _filterValues.Add(v);
                                if (!_dico.ContainsKey(v))
                                {
                                    _dico.Add(v, new ValueInfo(isActive, v));
                                }
                            }
                        }
                    }

                    reader.ReadEndElement();
                }
                reader.ReadEndElement();
            }
            _deserialized = true;
        }

        public void WriteXml(XmlWriter writer)
        {
            if (_filterValues != null && _filterValues.Count > 0 && _filterValues[0] != null)
            {
                Type t = _filterValues[0].GetType();

                string[] s = t.Assembly.FullName.Split(',');
                string typeName = s.Length > 0 ? string.Format("{0}, {1}", t.FullName, s[0].Trim()) : t.FullName;
                writer.WriteAttributeString("valueType", typeName);

                writer.WriteStartElement("values");
                foreach (object value in _filterValues)
                {
                    if (value != null)
                    {
                        writer.WriteStartElement("value");

                        string active = "true";
                        ValueInfo info;
                        if (_dico != null && _dico.TryGetValue(value, out info))
                        {
                            active = info != null && info.IsChecked ? "true" : "false";
                        }

                        writer.WriteAttributeString("active", active);
                        //writer.WriteEndAttribute();
                        writer.WriteValue(string.Format(CultureInfo.InvariantCulture, "{0}", value));

                        writer.WriteEndElement();
                    }
                }

                writer.WriteEndElement();
            }
        }

#endregion IXmlSerializable Members

#region Private methods

        private void PopulateRecursively(int level, Row row, IDictionary<object, ValueInfo> values)
        {
            if (row.IsGroup)
            {
                foreach (Row child in row.Children)
                {
                    PopulateRecursively(level, child, values);
                }
            }
            else
            {
                if (row.Level == level && values != null)
                {
                    Cell cell = row[_column.Id];
                    if (cell != null)
                    {
                        object value = cell.Value;
                        if (value != null && !values.ContainsKey(value))
                        {
                            string text = cell.Text;
                            text = text != null ? text.Trim() : null;

                            if (!string.IsNullOrEmpty(text))
                            {
                                bool isChecked = true;
                                //string stringValue;
                                ValueInfo vi;
                                if (values.TryGetValue(value, out vi))
                                {
                                    isChecked = vi.IsChecked;
                                }

                                vi = new ValueInfo(isChecked, value);
                                values.Add(text, vi);
                            }
                        }
                    }
                }
                else
                {
                    if (row.Children != null)
                    {
                        foreach (Row child in row.Children)
                        {
                            PopulateRecursively(level, child, values);
                        }
                    }
                }
            }
        }

#endregion Private methods

        public void AddNewFilterItems(IList newItemList)
        {
            foreach (var item in newItemList)
            {
                if (item != null)
                {
                    if (_filterValues.Contains(item) == false)
                        _filterValues.Add(item);
                    if (_dico.ContainsKey(item) == false)
                    {
                        _dico.Add(item,new ValueInfo(true,item));
                    }
                }

            }
        }

        public void AddFilterItem(string item)
        {
            if (item != null)
            {
                if (_filterValues.Contains(item) == false)
                    _filterValues.Add(item);
                if (_dico.ContainsKey(item) == false)
                    _dico.Add(item, new ValueInfo(true, item));
            }
        }

        public void RemoveFilterItems(string item)
        {
            if (item != null)
            {
                if (_filterValues.Contains(item))
                    _filterValues.Remove(item);
                if (_dico.ContainsKey(item))
                    _dico.Remove(item);
            }
        }


#region Nested type: EditorControl

        private class EditorControl : UserControl
        {
            private const string ColumnCheckBox = "cb";
            private const string ColumnName = "name";
            private readonly IDictionary<object, ValueInfo> _values;
            private readonly IDictionary<object, ValueInfo> _valuesCopy = new Dictionary<object, ValueInfo>();

            private readonly ValueEqualityCheckBoxFilter2 _owner;
            private readonly IWindowsFormsEditorService _service;

            private Button _btnAdd;
            private Button _btnApply;
            private Button _btnClose;
            private Button _btnRemove;
            private Button _btnToggle;
            private bool _cancelled;
            private IFilter _filter;
            private Grid _grid;
            private Panel _panel1;
            private bool _updating;

            public EditorControl(ValueEqualityCheckBoxFilter2 owner, IWindowsFormsEditorService service, IDictionary<object, ValueInfo> values)
            {
                _owner = owner;
                _values = values;
                foreach (KeyValuePair<object, ValueInfo> pair in values)
                {
                    _valuesCopy.Add(pair.Key, new ValueInfo(pair.Value.IsChecked, pair.Value.RawValue));
                }

                _service = service;
                InitializeComponent();

                //Populate grid;
                object[] keys = new object[values.Keys.Count];
                _values.Keys.CopyTo(keys, 0);
                List<object> l = new List<object>(keys);
                l.Sort(delegate(object x, object y)
                {
                    ValueInfo xv;
                    ValueInfo yv;

                    values.TryGetValue(x, out xv);
                    values.TryGetValue(y, out yv);

                    if (xv != null && xv.RawValue is IComparable &&
                        yv != null && yv.RawValue is IComparable)
                    {
                        return ((IComparable)xv.RawValue).CompareTo(yv.RawValue);
                    }

                    string xs = xv != null ? string.Format(CultureInfo.InvariantCulture, "{0}", xv.RawValue) : string.Empty;
                    string ys = yv != null ? string.Format(CultureInfo.InvariantCulture, "{0}", yv.RawValue) : string.Empty;
                    //string ys;

                    return String.Compare(xs, ys, true);
                });

                _grid.Headers.Add(new Header());
                _grid.Headers[0].Add(new Column(ColumnCheckBox, "checked", 16));
                _grid.Headers[0].Add(new Column(ColumnName));
                _grid.Headers[0].Visible = false;
                _grid.Headers[0][0].Editable = true;
                _grid.Headers[0][0].Editor = new CheckBoxEditor();
                _grid.Headers[0][1].Resizable = false;
                _grid.Headers[0][1].Appearance.CellTextAlignment = ContentAlignment.MiddleLeft;
                _grid.Headers[0].StretchMode = ColumnStretchMode.All;
                _grid.Hierarchy.ButtonBehaviour = ExpansionButtonBehaviour.HideAlways;

                foreach (object s in l)
                {
                    _grid.Rows.Add(new object[] { _values[s].IsChecked, s });
                }

                _grid.RowUpdated += delegate
                {
                    if (!_updating)
                    {
                        UpdateDicoValues();
                        UpdateFilter();
                    }
                };
            }

            public IFilter Filter
            {
                get { return _filter; }
            }

            public Grid Grid
            {
                get { return _grid; }
            }

            protected override void OnHandleDestroyed(EventArgs e)
            {
                if (!_cancelled)
                {
                    _updating = true;
                    UpdateDicoValues();
                    _updating = false;
                    UpdateFilter();
                }

                base.OnHandleDestroyed(e);
            }

            private void InitializeComponent()
            {
                _panel1 = new Panel();
                _btnApply = new Button();
                _btnClose = new Button();
                _btnRemove = new Button();
                _btnToggle = new Button();
                _btnAdd = new Button();
                _grid = new Grid();
                _panel1.SuspendLayout();
                ((ISupportInitialize)(_grid)).BeginInit();
                SuspendLayout();
                //
                // panel1
                //
                _panel1.Controls.Add(_btnClose);
                _panel1.Controls.Add(_btnApply);
                _panel1.Controls.Add(_btnRemove);
                _panel1.Controls.Add(_btnToggle);
                _panel1.Controls.Add(_btnAdd);
                _panel1.Dock = DockStyle.Top;
                _panel1.Location = new Point(0, 0);
                _panel1.Name = "_panel1";
                _panel1.Size = new Size(217, 31);
                _panel1.TabIndex = 0;
                //
                // btnApply
                //
                _btnApply.Anchor = (((AnchorStyles.Top | AnchorStyles.Right)));
                //_btnApply.BackgroundImage = IconExtractor.Extract("document_ok");
                _btnApply.BackgroundImage = Resources.document_ok;
                _btnApply.BackgroundImageLayout = ImageLayout.Center;
                _btnApply.Location = new Point(165, 3);
                _btnApply.Name = "_btnApply";
                _btnApply.Size = new Size(23, 23);
                _btnApply.TabIndex = 3;
                _btnApply.UseVisualStyleBackColor = true;
                _btnApply.Click += btnApply_Click;
                _btnApply.KeyDown += On_KeyDown;
                System.Windows.Forms.ToolTip tooltip1 = new System.Windows.Forms.ToolTip();
                tooltip1.SetToolTip(_btnApply, "Apply");

                //
                // btnClose
                //
                _btnClose.Anchor = (((AnchorStyles.Top | AnchorStyles.Right)));
                //_btnClose.BackgroundImage = IconExtractor.Extract("icon_delete");
                _btnClose.BackgroundImage = Resources.icon_delete;
                _btnClose.BackgroundImageLayout = ImageLayout.Center;
                _btnClose.Location = new Point(191, 3);
                _btnClose.Name = "_btnClose";
                _btnClose.Size = new Size(23, 23);
                _btnClose.TabIndex = 4;
                _btnClose.UseVisualStyleBackColor = true;
                _btnClose.Click += btnClose_Click;
                _btnClose.KeyDown += On_KeyDown;
                System.Windows.Forms.ToolTip tooltip2 = new System.Windows.Forms.ToolTip();
                tooltip2.SetToolTip(_btnClose, "Close");
                //
                // btnRemove
                //
                //_btnRemove.BackgroundImage = IconExtractor.Extract("icon_filter_delete");
                _btnRemove.BackgroundImage = Resources.icon_filter_delete;
                _btnRemove.BackgroundImageLayout = ImageLayout.Center;
                _btnRemove.Location = new Point(57, 3);
                _btnRemove.Name = "_btnRemove";
                _btnRemove.Size = new Size(23, 23);
                _btnRemove.TabIndex = 2;
                _btnRemove.UseVisualStyleBackColor = true;
                _btnRemove.Click += btnRemove_Click;
                _btnRemove.KeyDown += On_KeyDown;
                System.Windows.Forms.ToolTip tooltip3 = new System.Windows.Forms.ToolTip();
                tooltip3.SetToolTip(_btnRemove, "Remove");
                //
                // btnToggle
                //
                //_btnToggle.BackgroundImage = IconExtractor.Extract("recycle_preferences");
                _btnToggle.BackgroundImage = Resources.recycle_preferences;
                _btnToggle.BackgroundImageLayout = ImageLayout.Center;
                _btnToggle.Location = new Point(30, 3);
                _btnToggle.Name = "_btnToggle";
                _btnToggle.Size = new Size(23, 23);
                _btnToggle.TabIndex = 1;
                _btnToggle.UseVisualStyleBackColor = true;
                _btnToggle.Click += btnToggle_Click;
                _btnToggle.KeyDown += On_KeyDown;
                System.Windows.Forms.ToolTip tooltip4 = new System.Windows.Forms.ToolTip();
                tooltip4.SetToolTip(_btnToggle, "Toggle");
                //
                // btnAdd
                //
                //_btnAdd.BackgroundImage = IconExtractor.Extract("icon_filter_add");
                _btnAdd.BackgroundImage = Resources.icon_filter_add;
                _btnAdd.BackgroundImageLayout = ImageLayout.Center;
                _btnAdd.Location = new Point(3, 3);
                _btnAdd.Name = "_btnAdd";
                _btnAdd.Size = new Size(23, 23);
                _btnAdd.TabIndex = 0;
                _btnAdd.UseVisualStyleBackColor = true;
                _btnAdd.Click += btnAdd_Click;
                _btnAdd.KeyDown += On_KeyDown;
                System.Windows.Forms.ToolTip tooltip5 = new System.Windows.Forms.ToolTip();
                tooltip5.SetToolTip(_btnAdd, "Add");
                //
                // grid
                //
                _grid.BackColor = SystemColors.Window;
                _grid.Dock = DockStyle.Fill;
                _grid.Location = new Point(0, 0);
                _grid.Name = "_grid";
                _grid.TabIndex = 0;
                _grid.Selection.Enabled = false;
                _grid.FocusSettings.Color = Color.Transparent;
                _grid.Highlighting.Enabled = false;
                _grid.EditInPlace.HotFrameEnabled = false;
                _grid.KeyDown += On_KeyDown;
                //
                // EditorControl
                //
                AutoScaleDimensions = new SizeF(6F, 13F);
                AutoScaleMode = AutoScaleMode.Font;
                Controls.Add(_grid);
                Controls.Add(_panel1);
                Name = "EditorControl";
                Size = new Size(217, 180);
                _panel1.ResumeLayout(false);
                ((ISupportInitialize)(_grid)).EndInit();
                ResumeLayout(false);
            }

            private void On_KeyDown(object sender, KeyEventArgs e)
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        _cancelled = true;
                        _values.Clear();
                        foreach (KeyValuePair<object, ValueInfo> pair in _valuesCopy)
                        {
                            _values.Add(pair.Key, new ValueInfo(pair.Value.IsChecked, pair.Value.RawValue));
                        }

                        _filter = _owner._column != null && _owner._column.HasFilter ? _owner : null;
                        _service.CloseDropDown();
                        break;

                    case Keys.Enter:
                        _service.CloseDropDown();
                        break;
                }
            }

            private void btnClose_Click(object sender, EventArgs e)
            {
                _cancelled = true;
                _values.Clear();
                foreach (KeyValuePair<object, ValueInfo> pair in _valuesCopy)
                {
                    _values.Add(pair.Key, new ValueInfo(pair.Value.IsChecked, pair.Value.RawValue));
                }
                _filter = null;
                _service.CloseDropDown();
            }

            private void btnApply_Click(object sender, EventArgs e)
            {
                _service.CloseDropDown();
            }

            private void btnToggle_Click(object sender, EventArgs e)
            {
                IEnumerable<Row> collection = _grid.Selection.Count > 0 ? (IEnumerable<Row>)_grid.Selection : _grid.Rows;
                foreach (Row row in collection)
                {
                    bool b = Equals(false, row[ColumnCheckBox].Value);
                    row[ColumnCheckBox].Value = b;
                    object value = row[ColumnName].Value;
                    ValueInfo vi;
                    if (value != null && _values.TryGetValue(value, out vi))
                    {
                        vi.IsChecked = b;
                    }
                }
                _updating = true;
                UpdateDicoValues();
                _updating = false;
                UpdateFilter();
            }

            private void btnRemove_Click(object sender, EventArgs e)
            {
                IEnumerable<Row> collection = _grid.Selection.Count > 0 ? (IEnumerable<Row>)_grid.Selection : _grid.Rows;
                foreach (Row row in collection)
                {
                    row[ColumnCheckBox].Value = false;

                    object value = row[ColumnName].Value;
                    ValueInfo vi;
                    if (value != null && _values.TryGetValue(value, out vi))
                    {
                        vi.IsChecked = false;
                    }
                }
                _updating = true;
                UpdateDicoValues();
                _updating = false;
                UpdateFilter();
            }

            private void btnAdd_Click(object sender, EventArgs e)
            {
                IEnumerable<Row> collection = _grid.Selection.Count > 0 ? (IEnumerable<Row>)_grid.Selection : _grid.Rows;
                foreach (Row row in collection)
                {
                    row[ColumnCheckBox].Value = true;
                    object value = row[ColumnName].Value;
                    ValueInfo vi;
                    if (value != null && _values.TryGetValue(value, out vi))
                    {
                        vi.IsChecked = true;
                    }
                }
                _updating = true;
                UpdateDicoValues();
                _updating = false;
                UpdateFilter();
            }

            private void UpdateDicoValues()
            {
                foreach (Row row in _grid.Rows)
                {
                    object key = row[ColumnName].Value;
                    bool isChecked = (bool)row[ColumnCheckBox].Value;
                    if (key != null && _values.ContainsKey(key))
                    {
                        _values[key].IsChecked = isChecked;
                    }
                }
                _filter = _values != null && _values.Count > 0 ? _owner : null;
            }

            private void UpdateFilter()
            {
                if (_owner.FilterUpdated != null)
                {
                    _owner.FilterUpdated(_owner, EventArgs.Empty);
                }
            }
        }

#endregion Nested type: EditorControl

#region Nested type: ValueInfo

        private class ValueInfo
        {
            private bool _isChecked;
            private readonly object _rawValue;

            public ValueInfo(bool isChecked, object rawValue)
            {
                _isChecked = isChecked;
                _rawValue = rawValue;
            }

            public bool IsChecked
            {
                get { return _isChecked; }
                set { _isChecked = value; }
            }

            public object RawValue
            {
                get { return _rawValue; }
            }

            public override string ToString()
            {
                return string.Format("{0}:{1}", _rawValue != null ? _rawValue.ToString() : string.Empty, IsChecked);
            }
        }

#endregion Nested type: ValueInfo

        private static class IconExtractor
        {
            private static readonly Type ResourceType = Type.GetType("Dapfor.Net.Resources, Dapfor.Net");

            public static Image Extract(string iconName)
            {
                try
                {
                    PropertyInfo pi = ResourceType != null ? ResourceType.GetProperty(iconName, BindingFlags.NonPublic | BindingFlags.Static) : null;
                    return (Image)(pi != null ? pi.GetValue(null, null) : null);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}