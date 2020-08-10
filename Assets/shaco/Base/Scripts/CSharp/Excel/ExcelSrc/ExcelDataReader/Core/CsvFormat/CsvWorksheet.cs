using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using shaco.ExcelDataReader.Core.NumberFormat;

namespace shaco.ExcelDataReader.Core.CsvFormat
{
    internal class CsvWorksheet : IWorksheet
    {
        public CsvWorksheet(Stream stream, Encoding fallbackEncoding, char[] autodetectSeparators, int analyzeInitialCsvRows)
        {
            Stream = stream;
            Stream.Seek(0, SeekOrigin.Begin);
            try
            {
                // Try as UTF-8 first, or use BOM if present
                int fieldCount, bomLength, rowCount;
                char separator;
                Encoding encoding;
                CsvAnalyzer.Analyze(Stream, autodetectSeparators, Encoding.UTF8, analyzeInitialCsvRows, out fieldCount, out separator, out encoding, out bomLength, out rowCount);
                FieldCount = fieldCount;
                AnalyzedRowCount = rowCount;
                AnalyzedPartial = analyzeInitialCsvRows > 0;
                Encoding = encoding;
                Separator = separator;
                BomLength = bomLength;
            }
            catch (DecoderFallbackException)
            {
                // If cannot parse as UTF-8, try fallback encoding
                Stream.Seek(0, SeekOrigin.Begin);

                int fieldCount, bomLength, rowCount;
                char separator;
                Encoding encoding;
                CsvAnalyzer.Analyze(Stream, autodetectSeparators, fallbackEncoding, analyzeInitialCsvRows, out fieldCount, out separator, out encoding, out bomLength, out rowCount);
                FieldCount = fieldCount;
                AnalyzedRowCount = rowCount;
                AnalyzedPartial = analyzeInitialCsvRows > 0;
                Encoding = encoding;
                Separator = separator;
                BomLength = bomLength;
            }
        }

        public string Name {get{return string.Empty; }}

        public string CodeName {get{return null; }}

        public string VisibleState { get { return null; } }

        public HeaderFooter HeaderFooter { get { return null; } }

        public CellRange[] MergeCells { get { return null; } }

        public int FieldCount { get; }

        public int RowCount
        {
            get
            {
                if (AnalyzedPartial)
                {
                    throw new InvalidOperationException("Cannot use RowCount with AnalyzeInitialCsvRows > 0");
                }

                return AnalyzedRowCount;
            }
        }

        public Stream Stream { get; }

        public Encoding Encoding { get; }

        public char Separator { get; }

        public Col[] ColumnWidths { get { return null; } }

        private int BomLength { get; set; }

        private bool AnalyzedPartial { get; }

        private int AnalyzedRowCount { get; }

        public NumberFormatString GetNumberFormatString(int index)
        {
            return null;
        }

        public IEnumerable<Row> ReadRows()
        {
            var bufferSize = 1024;
            var buffer = new byte[bufferSize];
            var rowIndex = 0;
            var csv = new CsvParser(Separator, Encoding);
            var skipBomBytes = BomLength;

            Stream.Seek(0, SeekOrigin.Begin);
            while (Stream.Position < Stream.Length)
            {
                var bytesRead = Stream.Read(buffer, 0, bufferSize);
                System.Collections.Generic.List<System.Collections.Generic.List<string>> bufferRows;
                csv.ParseBuffer(buffer, skipBomBytes, bytesRead - skipBomBytes, out bufferRows);

                skipBomBytes = 0; // Only skip bom on first iteration

                foreach (var row in GetReaderRows(rowIndex, bufferRows))
                {
                    yield return row;
                }

                rowIndex += bufferRows.Count;
            }

            System.Collections.Generic.List<System.Collections.Generic.List<string>> flushRows;
            csv.Flush(out flushRows);
            foreach (var row in GetReaderRows(rowIndex, flushRows))
            {
                yield return row;
            }
        }

        private IEnumerable<Row> GetReaderRows(int rowIndex, List<List<string>> rows)
        {
            foreach (var row in rows)
            {
                var cells = new List<Cell>(row.Count);
                for (var index = 0; index < row.Count; index++)
                {
                    cells.Add(new Cell()
                    {
                        ColumnIndex = index,
                        Value = row[index]
                    });
                }

                yield return new Row()
                {
                    Height = 12.75, // 255 twips
                    Cells = cells,
                    RowIndex = rowIndex
                };

                rowIndex++;
            }
        }
    }
}
