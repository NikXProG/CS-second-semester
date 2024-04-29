using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharp2sem
{
    public class Program
    {
        
        class TokenTextFile : IDisposable, IEnumerable<string>
        {
            private HashSet<char> _lexemes;
            private readonly StreamReader _sreaderFile;
            private readonly string _filePath;  
            public TokenTextFile(HashSet<char> collection, string filePath)
            {
                this._lexemes = collection;
                this._filePath = filePath;
                this._sreaderFile = new StreamReader(filePath);
            }
            
            
            public IEnumerator<string> GetEnumerator()
            {
                return new StreamReaderEnumerator(_lexemes, _sreaderFile);
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
            public void Dispose()
            {
                _sreaderFile.Dispose();
                System.GC.SuppressFinalize(this);

            }
            ~TokenTextFile()
            {
                _sreaderFile.Close();
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
                this._collection = collection ?? throw new ArgumentNullException();;
                this._streamReaderFile = srFile ?? throw new ArgumentNullException();;;
                this._line = this._streamReaderFile.ReadLine();
            }
            public bool MoveNext()
            {
                
                if (_line == null) return false;
                
                foreach (var i in _collection)
                {
                    _charPos = _line.IndexOf(i, 0); 
                    if (_charPos != -1) break;
                }
                    
                _current = ((_charPos == -1) ? (_line.Substring(0, _line.Length)) : (_line.Substring(0, _charPos)));
                _line = _line.Remove(0, _charPos + 1);
                    
                if ((_charPos == -1)) _line = _streamReaderFile.ReadLine();

                return true;

            }

            public string Current
            {
                get
                {
                    if (_streamReaderFile == null || _current == null)
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
                _current = null;
            }

            public void Dispose()
            {
                _streamReaderFile.Dispose();
                GC.SuppressFinalize(this);

            }
            ~StreamReaderEnumerator()
            {
                _streamReaderFile.Close();
            }
        }
        public static int Main()
        {



            try
            {
                HashSet<char> col = ['.',';','*','~'];
                string filePath = @"C:\Users\tiger\OneDrive\Рабочий стол\AAA.txt";
                TokenTextFile tokenizer = new TokenTextFile(col, filePath);
                foreach (var token in tokenizer)
                {
                    Console.WriteLine(token);
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex.Message);
            }


            
            return 0;

        }
            
    }
}
