using System;
using System.IO;

namespace TRock.Music.Grooveshark.IO
{
    public class ReadFullyStream : Stream
    {
        #region Fields

        private long _pos; // psuedo-position
        private readonly byte[] _readAheadBuffer;
        private int _readAheadLength;
        private int _readAheadOffset;
        private readonly Stream _sourceStream;

        #endregion Fields

        #region Constructors

        public ReadFullyStream(Stream sourceStream)
        {
            _sourceStream = sourceStream;
            _readAheadBuffer = new byte[4096];
        }

        #endregion Constructors

        #region Properties

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                return _pos;
            }
        }

        public override long Position
        {
            get
            {
                return _pos;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        #endregion Properties

        #region Methods

        public override void Flush()
        {
            throw new InvalidOperationException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            while (bytesRead < count)
            {
                int readAheadAvailableBytes = _readAheadLength - _readAheadOffset;
                int bytesRequired = count - bytesRead;
                if (readAheadAvailableBytes > 0)
                {
                    int toCopy = Math.Min(readAheadAvailableBytes, bytesRequired);
                    Array.Copy(_readAheadBuffer, _readAheadOffset, buffer, offset + bytesRead, toCopy);
                    bytesRead += toCopy;
                    _readAheadOffset += toCopy;
                }
                else
                {
                    _readAheadOffset = 0;
                    _readAheadLength = _sourceStream.Read(_readAheadBuffer, 0, _readAheadBuffer.Length);
                    //Debug.WriteLine(String.Format("Read {0} bytes (requested {1})", readAheadLength, readAheadBuffer.Length));
                    if (_readAheadLength == 0)
                    {
                        break;
                    }
                }
            }

            _pos += bytesRead;
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException();
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }

        #endregion Methods
    }
}