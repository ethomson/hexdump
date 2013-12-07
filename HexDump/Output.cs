using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HexDump
{
    public abstract class Output
    {
        public int BytesPerLine
        {
            get { return 16; }
        }

        public abstract String FormatLine(byte[] buf, int length);

        protected bool IsPrintable(byte b)
        {
            return (b >= 32 && b < 127);
        }

        protected string GetEscapeString(byte b)
        {
            string[] escaped = { "\\a", "\\b", "\\t", "\\n", "\\v", "\\f", "\\r" };

            if (b == 0)
            {
                return "\\0";
            }
            else if (b >= 7 && b <= 13)
            {
                return escaped[b - 7];
            }

            return null;
        }
    }

    public class DefaultHexOutput : HexOutput
    {
        protected override string ChunkSeparator
        {
            get { return " "; }
        }
    }

    public class HexOutput : ShortOutput
    {
        protected override string ChunkSeparator
        {
            get { return "    "; }
        }

        protected override string FormatShort(ushort value)
        {
            return String.Format("{0,4:x4}", value);
        }
    }

    public class DecimalOutput : ShortOutput
    {
        protected override string ChunkSeparator
        {
            get { return "   "; }
        }

        protected override string FormatShort(ushort value)
        {
            return String.Format("{0,5:d5}", value);
        }
    }

    public abstract class ShortOutput : Output
    {
        public ShortOutput()
        {
            SwapByteOrder = true;
        }

        protected virtual string ChunkSeparator
        {
            get { return " "; }
        }

        protected abstract string FormatShort(ushort value);

        protected bool SwapByteOrder
        {
            get;
            set;
        }

        public override String FormatLine(byte[] buf, int count)
        {
            StringBuilder line = new StringBuilder();
            int linelen = Math.Min(BytesPerLine, count);

            for (int i = 0; i < linelen; i += 2)
            {
                line.Append(ChunkSeparator);

                int value;

                if (SwapByteOrder)
                {
                    value = (buf[i] | ((i + 1 < count) ? (buf[i + 1] << 8) : 0));
                }
                else
                {
                    value = ((buf[i] << 8) | ((i + 1 < count) ? (int)buf[i + 1] : 0));
                }

                line.Append(FormatShort((ushort)value));
            }

            return line.ToString();
        }
    }

    public class OctalOutput : Output
    {
        public override String FormatLine(byte[] buf, int count)
        {
            StringBuilder line = new StringBuilder();
            int linelen = Math.Min(BytesPerLine, count);

            for (int i = 0; i < linelen; i++)
            {
                string octal = System.Convert.ToString(buf[i], 8);

                line.Append(" ");

                for (int j = octal.Length; j < 3; j++)
                {
                    line.Append("0");
                }

                line.Append(octal);
            }

            return line.ToString();
        }
    }

    public class CharOutput : Output
    {
        public override String FormatLine(byte[] buf, int count)
        {
            StringBuilder line = new StringBuilder();
            string escapeString;

            for (int i = 0; i < BytesPerLine; i++)
            {
                line.Append(" ");

                if (IsPrintable(buf[i]))
                {
                    line.Append("  ");
                    line.Append(Encoding.ASCII.GetString(new byte[] { buf[i] }));
                }
                else if ((escapeString = GetEscapeString(buf[i])) != null)
                {
                    line.Append(" ");
                    line.Append(escapeString);
                }
                else
                {
                    line.Append(String.Format("{0,3:d}", buf[i]));
                }
            }

            return line.ToString();
        }
    }

    public class CanonicalOutput : Output
    {
        private const int chunksize = 8;

        private string ChunkSeparator
        {
            get { return "  "; }
        }

        public override String FormatLine(byte[] buf, int count)
        {
            StringBuilder line = new StringBuilder();

            for (int i = 0; i < BytesPerLine; i++)
            {
                if ((i % chunksize) == 0)
                {
                    line.Append(ChunkSeparator);
                }
                else if (i > 0)
                {
                    line.Append(" ");
                }

                String b = (i < count) ? FormatHex(buf[i]) : "  ";

                line.Append(b);
            }

            line.Append("  |");

            for (int i = 0; i < Math.Min(count, BytesPerLine); i++)
            {
                String c = FormatChar(buf[i]);
                line.Append(c);
            }

            line.Append("|");

            return line.ToString();
        }

        private String FormatHex(byte b)
        {
            return String.Format("{0,2:x2}", b);
        }

        private String FormatChar(byte b)
        {
            if (IsPrintable(b))
            {
                return Encoding.ASCII.GetString(new byte[] { b });
            }

            return ".";
        }
    }
}
