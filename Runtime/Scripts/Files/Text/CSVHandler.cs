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

            public CSVEntry() { }
            public abstract void Init(string[] data);

            public IEnumerator<object> GetEnumerator() {
                return this.values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }
        }

        public class CSVData<TEntry> where TEntry : CSVEntry, new() {
            private string[] headers;
            private TEntry[] data;

            public int Count {
                get {
                    return this.data.Length;
                }
            }
            public bool hasHeaders {
                get {
                    return headers != null;
                }
            }
            public string[] Headers => this.headers;
            public TEntry[] Data => this.data;

            public TEntry this[int rowIndex] {
                get {
                    rowIndex = (rowIndex + this.Count) % this.Count;
                    if ((rowIndex < 0) || (rowIndex >= this.Count))
                        return null;
                    return this.data[rowIndex];
                }
            }
            public object this[int columnIndex, int rowIndex] {
                get {
                    if (((columnIndex < 0) || (this.hasHeaders && columnIndex >= this.headers.Length)) 
                        || ((rowIndex < 0) || (rowIndex >= this.Count)))
                        return null;
                    return this.data[rowIndex][columnIndex];
                }
            }
            public int ColumnIndex(string header) {
                if (!this.hasHeaders) return -1;
                int columnIndex = -1;
                if (this.headers.Contains(header))
                    columnIndex = Array.IndexOf(this.headers, header);
                return columnIndex;
            }
            public object this[string header, int rowIndex] {
                get {
                    return this[this.ColumnIndex(header), rowIndex];
                }
            }

            public CSVData(string[] _headers, TEntry[] _data) {
                this.headers = _headers;
                this.data = _data;
            }
        }

        public static CSVData<TEntry> Parse<TEntry>(string rawData, char separator=',', bool withHeader=true) 
            where TEntry : CSVEntry, new() {
            string[] rawEntries = rawData.Split('\n'), rawValues;
            if (rawEntries.Length == 0) return null;
            int i = 0, entryCount = -1;
            string[] headers = null;
            Regex pattern = new Regex("s/[\r\n\t]");
            if (withHeader) {
                headers = rawEntries[i++].Split(new char[] { separator })
                    .Select(item => pattern.Replace(item,string.Empty)
                        .Trim()
                    ).ToArray();
                entryCount = headers.Length;
            }
            
            TEntry[] data = new TEntry[rawEntries.Length - i -1];

            for (int j = 0; i < rawEntries.Length; i++, j++) {
                if (string.IsNullOrEmpty(rawEntries[i])) continue;
                data[j] = new TEntry();
                rawValues = rawEntries[i].Split(new char[] { separator })
                    .Select(item => pattern.Replace(item, string.Empty)
                        .Trim()
                    ).ToArray();
                if (entryCount == -1)
                    entryCount = rawValues.Length;
                if (rawValues.Length != entryCount)
                    return null;
                data[j].Init(rawValues);
            }

            return new CSVData<TEntry>(headers, data);
        }

    }
}