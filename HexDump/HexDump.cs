using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;

namespace HexDump
{
    public class HexDump
    {
        private class Options
        {
            public Options()
            {
                Length = -1;

                Output = new DefaultHexOutput();
                Files = new List<string>();
            }

            public long Skip
            {
                get;
                set;
            }

            public long Length
            {
                get;
                set;
            }

            public Output Output
            {
                get;
                set;
            }

            public bool Verbose
            {
                get;
                set;
            }

            public List<string> Files
            {
                get;
                private set;
            }
        }

        public static void Main(string[] args)
        {
            long read = 0, loc = 0;

            Options opts = ParseOptions(args);

            byte[] previous = new byte[opts.Output.BytesPerLine];
            byte[] buf = new byte[opts.Output.BytesPerLine];
            bool previousIdentical = false;
            Stream file;

            if (opts.Files.Count == 0)
            {
                file = new ForwardSeekingStream(Console.OpenStandardInput());
            }
            else
            {
                file = new ConcatenatedStream(opts.Files.ToArray(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }

            using (file)
            {
                loc += file.Seek(opts.Skip, SeekOrigin.Begin);

                while (opts.Length == -1 || read < opts.Length)
                {
                    int len = file.Read(buf, 0, buf.Length);

                    if (!opts.Verbose && len == opts.Output.BytesPerLine && loc > opts.Skip)
                    {
                        bool identical = true;

                        for (int i = 0; i < opts.Output.BytesPerLine; i++)
                        {
                            if (previous[i] != buf[i])
                            {
                                identical = false;
                                break;
                            }
                        }

                        if (identical)
                        {
                            if (!previousIdentical)
                            {
                                Console.Out.WriteLine("*");
                            }

                            previousIdentical = true;
                            read += len;
                            loc += opts.Output.BytesPerLine;

                            continue;
                        }
                    }

                    previousIdentical = false;

                    Console.Out.Write(String.Format("{0,8:x8}", loc));

                    if (len == 0)
                    {
                        break;
                    }

                    int writelen = (opts.Length >= 0) ? (int)Math.Min(opts.Length - read, len) : len;

                    Console.Out.WriteLine(opts.Output.FormatLine(buf, writelen));

                    read += len;
                    loc += len;

                    Array.Copy(buf, previous, len);
                }

                file.Close();
            }
        }

        private static String ProgramName
        {
            get
            {
                int lastSlash;

                String programName = Assembly.GetExecutingAssembly().Location;

                if ((lastSlash = programName.LastIndexOf('\\')) >= 0)
                    programName = programName.Substring(lastSlash + 1);

                if (programName.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
                    programName = programName.Substring(0, programName.Length - 4);

                return programName;
            }
        }

        private static Options ParseOptions(string[] args)
        {
            Options options = new Options();
            bool literal = false;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (!literal && arg.Equals("--"))
                {
                    literal = true;
                    continue;
                }
                else if (!literal && (arg.StartsWith("-") || arg.StartsWith("/")))
                {
                    String opts = arg;

                    while ((opts = opts.Substring(1)).Length > 0)
                    {
                        char opt = opts[0];

                        if (opt == 'b')
                        {
                            options.Output = new OctalOutput();
                        }
                        else if (opt == 'c')
                        {
                            options.Output = new CharOutput();
                        }
                        else if (opt == 'C')
                        {
                            options.Output = new CanonicalOutput();
                        }
                        else if (opt == 'd')
                        {
                            options.Output = new DecimalOutput();
                        }
                        else if (opt == 'l')
                        {
                            options.Length = ParseLong(opt, GetRemainderOrNext(opt, opts, args, ref i));
                            break;
                        }
                        else if (opt == 's')
                        {
                            options.Skip = ParseLong(opt, GetRemainderOrNext(opt, opts, args, ref i));
                            break;
                        }
                        else if (opt == 'v')
                        {
                            options.Verbose = true;
                            break;
                        }
                        else if (opt == 'x')
                        {
                            options.Output = new HexOutput();
                        }
                        else
                        {
                            Console.Error.WriteLine("{0}: invalid option -- '{1}'", ProgramName, opt);
                            Usage();
                            Environment.Exit(1);
                        }
                    }
                }
                else
                {
                    options.Files.Add(arg);
                }
            }

            return options;
        }

        private static String GetRemainderOrNext(char opt, string remainder, string[] args, ref int argsIdx)
        {
            if (remainder.Length > 1)
            {
                return remainder.Substring(1);
            }

            if (++argsIdx >= args.Length)
            {
                Console.Error.WriteLine("{0}: option requires an argument -- '{1}'", ProgramName, opt);
                Usage();
                Environment.Exit(1);
            }

            return args[argsIdx];
        }

        private static long ParseLong(char opt, string value)
        {
            try
            {
                return long.Parse(value);
            }
            catch (FormatException)
            {
                Console.Error.WriteLine("{0}: option '{1}' requires a numeric argument -- '{2}'", ProgramName, opt, value);
                Environment.Exit(1);
            }

            return -1;
        }

        private static void Usage()
        {
            Console.Error.WriteLine("usage: {0} [-bcCdvx] [-n length] [-s skip] [file]", ProgramName);
        }
    }
}
