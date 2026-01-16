using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PolytopeSolutions.Toolset.Files {
    public class CSVHandler {
        public abstract class CSVEntry : IEnumerable<object> {
            protected abstract List<object> values {
                get;
                set;
            }
            public object this[int index] {
                get { return this.values[index]; }
                set { this.values.Insert(index, value); }
            }
            public int Count => this.values.Count;

            public CSVEntry() { }
            public abstract void Init(string[] data);

            public IEnumerator<object> GetEnumerator() {
                return this.values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }
            public void SetValues(IEnumerable<object> _values) {
                this.values = _values.ToList();
            }
        }

        public class CSVData<TEntry> : IEnumerable where TEntry : CSVEntry, new() {
            private string[] headers;
            private TEntry[] data;

            public CSVData(string[] _headers, TEntry[] _data) {
                this.headers = _headers;
                this.data = _data;
            }

            public int Count {
                get {
                    return this.data.Length;
                }
            }
            public bool HasHeaders {
                get {
                    return headers != null;
                }
            }
            public int ColumnCount {
                get {
                    return this.HasHeaders ? this.headers.Length : this.data[0].Count;
                }
            }
            public string[] Headers => this.headers;
            public TEntry[] Data => this.data;
            public bool IsValidRow(int rowIndex) => (rowIndex >= 0) && (rowIndex < this.Count);
            public bool IsValidColumn(int columnIndex) => (columnIndex >= 0) && (columnIndex < this.ColumnCount);

            public TEntry this[int rowIndex] {
                get {
                    rowIndex = (rowIndex + this.Count) % this.Count;
                    if (!this.IsValidRow(rowIndex))
                        return null;
                    return this.data[rowIndex];
                }
            }
            public object this[int columnIndex, int rowIndex] {
                get {
                    if (!this.IsValidColumn(columnIndex) || !this.IsValidRow(rowIndex)) return null;
                    return this.data[rowIndex][columnIndex];
                }
                set {
                    if (!this.IsValidColumn(columnIndex) || !this.IsValidRow(rowIndex)) return;
                    this.data[rowIndex][columnIndex] = value;
                }
            }
            public object this[string header, int rowIndex] {
                get {
                    return this[ColumnIndex(header), rowIndex];
                }
                set {
                    this[ColumnIndex(header), rowIndex] = value;
                }
            }
            public int ColumnIndex(string header) {
                if (!this.HasHeaders) return -1;
                int columnIndex = -1;
                if (this.headers.Contains(header))
                    columnIndex = Array.IndexOf(this.headers, header);
                return columnIndex;
            }
            public object[] GetColumn(int columnIndex) {
                if (!this.IsValidColumn(columnIndex)) return null;
                object[] values = new object[this.Count];
                for (int i = 0; i < this.Count; i++)
                    values[i] = this[columnIndex, i];
                return values;
            }
            public object[] GetColumn(string header) {
                return GetColumn(this.ColumnIndex(header));
            }

            public void SetRow(int rowIndex, IEnumerable<object> values) {
                if (!this.IsValidRow(rowIndex)) return;
                if (values.Count() != this.ColumnCount) return;
                this[rowIndex].SetValues(values.ToList());
            }
            public void SetColumn(int columnIndex, IEnumerable<object> values) {
                if (!this.IsValidColumn(columnIndex)) return;
                if (values.Count() != this.Count) return;
                int i = 0;
                foreach (object value in values)
                    this[columnIndex, i++] = value;
            }
            public void SetColumn(string header, IEnumerable<object> values) {
                SetColumn(this.ColumnIndex(header), values);
            }

            public IEnumerator GetEnumerator() {
                return this.data.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }
        }
        public static CSVData<TEntry> Parse<TEntry>(string rawData, char separator = ',', bool withHeader = true)
            where TEntry : CSVEntry, new() {
            string[] rawEntries = rawData.Split('\n'), rawValues;
            if (rawEntries.Length == 0) return null;
            int i = 0, entryCount = -1;
            string[] headers = null;
            Regex patternWhiteSpaces = new Regex("s/[\r\n\t]");
            if (withHeader) {
                headers = rawEntries[i++].Split(new char[] { separator })
                    .Select(item => patternWhiteSpaces.Replace(item, string.Empty)
                        .Trim()
                    ).ToArray();
                entryCount = headers.Length;
            }

            List<TEntry> data = new List<TEntry>();
            TEntry entry;
            Regex patternCommaInQuotes = new Regex(@"([""\'])(.*),(.*)(\1)");
            string line;
            for (int j = 0; i < rawEntries.Length; i++, j++) {
                line = rawEntries[i];
                if (string.IsNullOrEmpty(line)) continue;
                entry = new TEntry();
                line = patternCommaInQuotes.Replace(line, @"$2.$3");
                rawValues = line.Split(new char[] { separator })
                    .Select(item => patternWhiteSpaces.Replace(item, string.Empty)
                        .Trim()
                    ).ToArray();
                if (entryCount == -1)
                    entryCount = rawValues.Length;
                if (rawValues.Length != entryCount)
                    return null;
                entry.Init(rawValues);
                data.Add(entry);
            }

            return new CSVData<TEntry>(headers, data.ToArray());
        }
    }
}