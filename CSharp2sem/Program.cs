using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace CSharp2sem
{
    public class Program
    {
        public class TokenTextFile : IDisposable, IEnumerable<string>
        {
            private HashSet<char> _lexemes;
            private readonly StreamReader _sreaderFile;

            public TokenTextFile(HashSet<char> collection, string filePath)
            {
                _lexemes = collection ?? throw new ArgumentNullException($"{nameof(collection)} не может быть null.");
                _sreaderFile = new StreamReader(filePath) ?? throw new ArgumentNullException($"{nameof(filePath)} не может быть null.");
            }
            public IEnumerable<string> Tokenize()
            {
                string line;
                while ((line = _sreaderFile.ReadLine()!) != null)
                {
                    int start = 0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (_lexemes.Contains(line[i]))
                        {
                            if (start != i)
                            {
                                yield return line.Substring(start, i - start);
                            }
                            start = i + 1;
                        }
                    }
                    if (start < line.Length)
                    {
                        yield return line.Substring(start);
                    }
                }
            }

            public IEnumerator<string> GetEnumerator()
            {
                return new StreamReaderEnumerator(_lexemes, _sreaderFile);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Dispose()
            {
                _sreaderFile.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        public class StreamReaderEnumerator : IEnumerator<string>
        {
            private readonly StreamReader _streamReaderFile;
            private readonly HashSet<char> _collection;
            private string? _current;
            private int _charPos;
            private string? _line;

            public StreamReaderEnumerator(HashSet<char> collection, StreamReader srFile)
            {
                _collection = collection ?? throw new ArgumentNullException($"{nameof(collection)} не может быть null.");
                _streamReaderFile = srFile ?? throw new ArgumentNullException($"{nameof(srFile)} не может быть null.");
                _line = _streamReaderFile.ReadLine();
            }

            public bool MoveNext()
            {
                if (_line == null) return false;

                _charPos = _line.IndexOfAny(_collection.ToArray());
                if (_charPos == -1)
                {
                    _current = _line;
                    _line = _streamReaderFile.ReadLine();
                }
                else
                {
                    _current = _line.Substring(0, _charPos);
                    _line = _line.Substring(_charPos + 1);
                }

                return _current.Length > 0 || _line != null;
            }

            public string Current
            {
                get
                {
                    if (_current == null)
                    {
                        throw new InvalidOperationException();
                    }
                    return _current;
                }
            }

            object IEnumerator.Current => Current;

            public void Reset()
            {
                _streamReaderFile.DiscardBufferedData();
                _streamReaderFile.BaseStream.Seek(0, SeekOrigin.Begin);
                _line = _streamReaderFile.ReadLine();
                _current = null;
            }

            public void Dispose()
            {
                _streamReaderFile.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        public static int Main()
        {
            try
            {
                HashSet<char> col = new HashSet<char> { '.', ';', '*', '~'};
                string filePath = "token_file.txt";
                using var tokenizer = new TokenTextFile(col, filePath);

                foreach (var token in tokenizer.Tokenize())
                {
                    Console.WriteLine(token);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return 0;
        }
    }
}