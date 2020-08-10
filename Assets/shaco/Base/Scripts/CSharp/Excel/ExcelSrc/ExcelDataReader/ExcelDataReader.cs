using System;
using System.Collections.Generic;
using System.Data;
using shaco.ExcelDataReader.Core;

namespace shaco.ExcelDataReader
{
    /// <summary>
    /// A generic implementation of the IExcelDataReader interface using IWorkbook/IWorksheet to enumerate data.
    /// </summary>
    /// <typeparam name="TWorkbook">A type implementing IWorkbook</typeparam>
    /// <typeparam name="TWorksheet">A type implementing IWorksheet</typeparam>
    internal abstract class ExcelDataReader<TWorkbook, TWorksheet> : IExcelDataReader
        where TWorkbook : IWorkbook<TWorksheet>
        where TWorksheet : IWorksheet
    {
        private IEnumerator<TWorksheet> _worksheetIterator;
        private IEnumerator<Row> _rowIterator;
        private IEnumerator<TWorksheet> _cachedWorksheetIterator = null;
        private List<TWorksheet> _cachedWorksheets = null;

        ~ExcelDataReader()
        {
            Dispose(false);
        }

        public string Name {get {return null != _worksheetIterator && null != _worksheetIterator.Current ? _worksheetIterator.Current.Name : string.Empty; }}

        public string CodeName {get{return null != _worksheetIterator && null != _worksheetIterator.Current ? _worksheetIterator.Current.CodeName : string.Empty; }}

        public string VisibleState {get {return null != _worksheetIterator && null != _worksheetIterator.Current ? _worksheetIterator.Current.VisibleState : string.Empty; }}

        public HeaderFooter HeaderFooter {get{return null != _worksheetIterator && null != _worksheetIterator.Current ? _worksheetIterator.Current.HeaderFooter : null; }}

        public CellRange[] MergeCells {get{return null != _worksheetIterator && null != _worksheetIterator.Current ? _worksheetIterator.Current.MergeCells : null; }}

        public int Depth { get; private set; }

        public int ResultsCount {get{return null != Workbook ? Workbook.ResultsCount : -1; }}

        public bool IsClosed { get; private set; }

        public int FieldCount {get{return null != _worksheetIterator && null != _worksheetIterator.Current ? _worksheetIterator.Current.FieldCount : 0; }}

        public int RowCount {get{return null != _worksheetIterator && null != _worksheetIterator.Current ? _worksheetIterator.Current.RowCount : 0;}}

        public int RecordsAffected {get{throw new NotSupportedException();}}

        public double RowHeight {get{return null != _rowIterator && null != _rowIterator.Current ? _rowIterator.Current.Height : 0; }}

        protected TWorkbook Workbook { get; set; }

        protected Cell[] RowCells { get; set; }

        public object this[int i] {get{return GetValue(i); }}

        public object this[string name] {get { throw new NotSupportedException(); }}

        public bool GetBoolean(int i) {return (bool)GetValue(i);}

        public byte GetByte(int i) {return (byte)GetValue(i); }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {throw new NotSupportedException();}

        public char GetChar(int i) {return (char)GetValue(i); }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
             {throw new NotSupportedException();}

        public IDataReader GetData(int i) {throw new NotSupportedException(); }

        public string GetDataTypeName(int i) {throw new NotSupportedException();}

        public DateTime GetDateTime(int i) {return (DateTime)GetValue(i); }

        public decimal GetDecimal(int i) {return (decimal)GetValue(i); }

        public double GetDouble(int i) {return (double)GetValue(i);}

        public Type GetFieldType(int i) {return null != GetValue(i) ? GetValue(i).GetType() : null;}

        public float GetFloat(int i) {return (float)GetValue(i);}

        public Guid GetGuid(int i) {return (Guid)GetValue(i);}

        public short GetInt16(int i) {return (short)GetValue(i); }

        public int GetInt32(int i) {return (int)GetValue(i);}

        public long GetInt64(int i) {return (long)GetValue(i);}

        public string GetName(int i) {throw new NotSupportedException();}

        public int GetOrdinal(string name) {throw new NotSupportedException();}

        /// <inheritdoc />
        public DataTable GetSchemaTable() {throw new NotSupportedException(); }

        public string GetString(int i) {return GetValue(i).ToString(); }

        public object GetValue(int i)
        {
            if (RowCells == null)
                throw new InvalidOperationException("No data exists for the row/column.");
            return null != RowCells[i] ? RowCells[i].Value : string.Empty;
        }

        public int GetValues(object[] values) {throw new NotSupportedException(); }
               
        public bool IsDBNull(int i) {return GetValue(i) == null; }

        public string GetNumberFormatString(int i)
        {
            if (RowCells == null)
                throw new InvalidOperationException("No data exists for the row/column.");
            if (RowCells[i] == null)
                return null;

            if (null != _worksheetIterator && null != _worksheetIterator.Current)
            {
                var tmpValue = _worksheetIterator.Current.GetNumberFormatString(RowCells[i].NumberFormatIndex);
                return null != tmpValue ? tmpValue.FormatString : string.Empty;
            }
            else
                return string.Empty;
        }

        public int GetNumberFormatIndex(int i)
        {
            if (RowCells == null)
                throw new InvalidOperationException("No data exists for the row/column.");
            if (RowCells[i] == null)
                return -1;
            return RowCells[i].NumberFormatIndex;
        }

        public double GetColumnWidth(int i)
        {
            if (i >= FieldCount)
            {
                throw new ArgumentException(string.Format("Column at index {0} does not exist.", i));
            }

            var columnWidths = null != _worksheetIterator && null != _worksheetIterator.Current ? _worksheetIterator.Current.ColumnWidths : null;
            double? retWidth = null;
            if (columnWidths != null)
            {
                var colWidthIndex = 0;
                while (colWidthIndex < columnWidths.Length && retWidth == null)
                {
                    var columnWidth = columnWidths[colWidthIndex];
                    if (i >= columnWidth.Min && i <= columnWidth.Max)
                    {
                        retWidth = columnWidth.Hidden
                            ? 0
                            : columnWidth.Width;
                    }
                    else
                    {
                        colWidthIndex++;
                    }
                }
            }

            const double DefaultColumnWidth = 8.43D;

            return retWidth ?? DefaultColumnWidth;
        }

        /// <inheritdoc />
        public void Reset()
        {
            if (null !=_worksheetIterator) _worksheetIterator.Dispose();
            if (null != _rowIterator) _rowIterator.Dispose();

            _worksheetIterator = null;
            _rowIterator = null;

            ResetSheetData();

            if (Workbook != null)
            {
                _worksheetIterator = ReadWorksheetsWithCache().GetEnumerator(); // Workbook.ReadWorksheets().GetEnumerator();
                if (!_worksheetIterator.MoveNext())
                {
                    _worksheetIterator.Dispose();
                    _worksheetIterator = null;
                    return;
                }

                _rowIterator = _worksheetIterator.Current.ReadRows().GetEnumerator();
            }
        }

        public virtual void Close()
        {
            if (IsClosed)
                return;

            if (null != _worksheetIterator) _worksheetIterator.Dispose();
            if (null != _rowIterator) _rowIterator.Dispose();

            _worksheetIterator = null;
            _rowIterator = null;
            RowCells = null;
            IsClosed = true;
        }

        public bool NextResult()
        {
            if (_worksheetIterator == null)
            {
                return false;
            }

            ResetSheetData();

            if (null != _rowIterator) _rowIterator.Dispose();
            _rowIterator = null;

            if (!_worksheetIterator.MoveNext())
            {
                _worksheetIterator.Dispose();
                _worksheetIterator = null;
                return false;
            }

            _rowIterator = _worksheetIterator.Current.ReadRows().GetEnumerator();
            return true;
        }

        public bool Read()
        {
            if (_worksheetIterator == null || _rowIterator == null)
            {
                return false;
            }

            if (!_rowIterator.MoveNext())
            {
                _rowIterator.Dispose();
                _rowIterator = null;
                return false;
            }

            ReadCurrentRow();

            Depth++;
            return true;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                Close();
        }

        private IEnumerable<TWorksheet> ReadWorksheetsWithCache()
        {
            // Iterate TWorkbook.ReadWorksheets() only once and cache the 
            // worksheet instances, which are expensive to create. 
            if (_cachedWorksheets != null)
            {
                foreach (var worksheet in _cachedWorksheets)
                {
                    yield return worksheet;
                }

                if (_cachedWorksheetIterator == null)
                {
                    yield break;
                }
            }
            else
            {
                _cachedWorksheets = new List<TWorksheet>();
            }

            if (_cachedWorksheetIterator == null)
            {
                _cachedWorksheetIterator = Workbook.ReadWorksheets().GetEnumerator();
            }

            while (_cachedWorksheetIterator.MoveNext())
            {
                _cachedWorksheets.Add(_cachedWorksheetIterator.Current);
                yield return _cachedWorksheetIterator.Current;
            }

            _cachedWorksheetIterator.Dispose();
            _cachedWorksheetIterator = null;
        }

        private void ResetSheetData()
        {
            Depth = -1;
            RowCells = null;
        }

        private void ReadCurrentRow()
        {
            if (RowCells == null)
            {
                RowCells = new Cell[FieldCount];
            }

            Array.Clear(RowCells, 0, RowCells.Length);

            foreach (var cell in _rowIterator.Current.Cells)
            {
                if (cell.ColumnIndex < RowCells.Length)
                {
                    RowCells[cell.ColumnIndex] = cell;
                }
            }
        }
    }
}
