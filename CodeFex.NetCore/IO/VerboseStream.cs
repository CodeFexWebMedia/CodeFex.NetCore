using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace CodeFex.NetCore.IO
{
    public class VerboseStream : Stream
    {
        protected List<Stream> Streams;
        protected List<Stream> OwnedStreams;

        protected bool DebugEnabled;

        public VerboseStream(bool autoAttachDebug = true)
        {
#if DEBUG
            if (autoAttachDebug)
            {
                AttachDebug();
            }
#endif
        }

        public new void Dispose()
        {
            base.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (OwnedStreams != null)
                {
                    foreach (var strem in OwnedStreams)
                    {
                        strem.Dispose();
                    }
                }
            }
        }

        public VerboseStream AttachFile(string filename)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
            }

            return AttachStream(new FileStream(filename, FileMode.Create), true);
        }

        public VerboseStream AttachStream(Stream stream, bool autoDispose = false)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            if (autoDispose)
            {
                OwnedStreams = OwnedStreams ?? new List<Stream>();
                OwnedStreams.Add(stream);
            }

            Streams = Streams ?? new List<Stream>();
            Streams.Add(stream);

            return this;
        }

        public VerboseStream AttachConsole()
        {
            AttachStream(Console.OpenStandardOutput(), true);

            return this;
        }

        public VerboseStream AttachDebug()
        {
            DebugEnabled = true;

            return this;
        }

        public StreamWriter StreamWriter
        {
            get
            {
                return new StreamWriter(this)
                {
                    AutoFlush = true
                };
            }
        }

        public override bool CanRead
        {
            get
            {
                return false;
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
                return true;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        protected void Write(byte[] bytes)
        {
            Write(bytes, 0, bytes.Length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (this)
            {
                if (DebugEnabled)
                {
                    Debug.Write(Encoding.UTF8.GetString(buffer, offset, count));
                }

                if (Streams != null)
                {
                    foreach (var stream in Streams)
                    {
                        stream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        public void WriteLine(string line = null)
        {
            Write(Encoding.UTF8.GetBytes(string.Concat(line ?? string.Empty, Environment.NewLine)));
        }

        public new void Close()
        {
            if (OwnedStreams != null)
            {
                foreach (var strem in OwnedStreams)
                {
                    strem.Close();
                }
            }
        }
    }
}
