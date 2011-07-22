using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TorDNSd.Utils
{
    public class InteractiveShell
    {
        /// <summary>
        /// Default input prefix '> ' 
        /// </summary>
        public const string DEFAULT_INPUT_PREFIX = "> ";

        /// <summary>
        /// Prefix to put before any input. Do not forget an empty space at the end. Cannot contain newlines.
        /// </summary>
        public string InputPrefix { get; set; }

        /// <summary>
        /// Is the interactive shell running?
        /// </summary>
        public bool IsRunning { get; protected set; }
        
        /// <summary>
        /// Current user input.
        /// </summary>
        public string Input { get; protected set; }

        /// <summary>
        /// Current text cursor position.
        /// </summary>
        public int CursorPosition { get; protected set; }

        /// <summary>
        /// Event triggered when the user presses the 'tab' key.
        /// </summary>
        public event Action<string> OnTab;

        /// <summary>
        /// Event triggered when the user presses the 'enter' key.
        /// </summary>
        public event Action<string> OnEnter;

        /// <summary>
        /// Construct a new InteractiveShell instance.
        /// </summary>
        public InteractiveShell()
            : this(DEFAULT_INPUT_PREFIX)
        {

        }

        /// <summary>
        /// Construct a new InteractiveShell instance.
        /// </summary>
        public InteractiveShell(string inputPrefix)
        {
            InputPrefix = string.IsNullOrEmpty(inputPrefix) ? DEFAULT_INPUT_PREFIX : inputPrefix;
        }

        /// <summary>
        /// Force the input. The cursor position will be automaticly set to the max.
        /// </summary>
        /// <param name="input">New input.</param>
        public void ForceInput(string input)
        {
            input = input ?? string.Empty;

            Input = input;
            CursorPosition = input.Length;
        }

        /// <summary>
        /// Will not return until Halt() is called.
        /// </summary>
        public void Run()
        {
            IsRunning = true;

            // Reset 'Input'
            Input = string.Empty;

            RenderInitial();

            while (IsRunning)
            {
                ConsoleKeyInfo key = Console.ReadKey(false);
                string oldInput = Input;
                string input;
                switch (key.Key)
                {
                    // Keys that should be ignored for now
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.Escape:
                    case ConsoleKey.F1:
                    case ConsoleKey.F2:
                    case ConsoleKey.F3:
                    case ConsoleKey.F4:
                    case ConsoleKey.F5:
                    case ConsoleKey.F6:
                    case ConsoleKey.F7:
                    case ConsoleKey.F8:
                    case ConsoleKey.F9:
                    case ConsoleKey.F10:
                    case ConsoleKey.F11:
                    case ConsoleKey.F12:
                        break;

                    case ConsoleKey.Tab:
                        if (OnTab != null)
                        {
                            OnTab(Input);
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        // See if we can move to the left
                        if (CursorPosition > 0)
                        {
                            CursorPosition--;
                        }

                        break;
                    case ConsoleKey.RightArrow:
                        // See if we can move to the right
                        if (CursorPosition + 1 < Input.Length)
                        {
                            CursorPosition++;
                        }

                        break;
                    case ConsoleKey.Backspace:
                        // See if we can move to the right
                        if (CursorPosition > 0)
                        {
                            CursorPosition--;
                            input = Input;

                            Input = string.Empty;
                            if (CursorPosition > 0)
                            {
                                Input += input.Substring(0, CursorPosition);
                            }

                            Input += input.Substring(CursorPosition + 1);
                        }

                        break;
                    case ConsoleKey.Enter:
                        CursorPosition = 0;

                        input = Input;

                        Input = string.Empty;
                        CursorPosition = 0;

                        if (Environment.OSVersion.Platform != PlatformID.Unix && Environment.OSVersion.Platform != PlatformID.MacOSX)
                        {
                            Console.CursorTop++;
                        }

                        Console.CursorLeft = 0;
                        oldInput = string.Empty;

                        // If the event is registered, raise it
                        if (OnEnter != null)
                        {
                            OnEnter(input);
                        }

                        break;
                    default:
                        Input += key.KeyChar;
                        CursorPosition += key.KeyChar.ToString().Length;
                        break;
                }

                if (!IsRunning)
                {
                    // No longer running
                    break;
                } 
                
                Render(oldInput);
            }
        }

        /// <summary>
        /// Render the initial shell.
        /// </summary>
        private void RenderInitial()
        {
            Console.Write(InputPrefix);
        }

        /// <summary>
        /// Render the shell.
        /// </summary>
        private void Render(string previousInput)
        {
            int oldLength = previousInput.Length + InputPrefix.Length;
            int newLength = Input.Length + InputPrefix.Length;
            int diffLength = newLength - oldLength;

            // Reset the cursor position
            Console.CursorLeft = 0;

            // Calculate how many rows to move up to get to the 'current' row
            int rows = (int)Math.Ceiling((double)(oldLength / Console.BufferWidth));
            rows = rows > 0 ? rows + 1 : 1;
            rows--;

            if (oldLength > 0 && ((oldLength+1)  % Console.BufferWidth == 0))
            {
                rows++; 
            }

            if (diffLength >= 0)
            {
                // Render more or equal amount of text compared to previous

                Console.CursorTop -= rows;
                Console.Write("{0}{1}", InputPrefix, Input);
                Console.CursorLeft = ((CursorPosition % Console.BufferWidth) + InputPrefix.Length) % Console.BufferWidth;
            }
            else
            {
                // Less text to render

                Console.CursorTop -= rows;
                Console.Write("{0}{1}", InputPrefix, Input);
                Console.Write("{0}",  " ".PadRight(oldLength - newLength, ' '));

                Console.CursorLeft = ((CursorPosition % Console.BufferWidth) + InputPrefix.Length) % Console.BufferWidth;
            }
        }

        public void WriteLine(string line, params string[] args)
        {
            line = string.Format(line, args);

            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                line = line.Replace("\t", "        ");
            }

            var currentInput = Input;
            Input = string.Empty;
            Render(currentInput);

            Console.CursorLeft = 0;
            Console.WriteLine(line.PadRight(InputPrefix.Length));

            Input = currentInput;
            Render(string.Empty);
        }

        public void WriteLine()
        {
            WriteLine(string.Empty);
        }

        public void Halt()
        {
            IsRunning = false;
        }
    }
}
