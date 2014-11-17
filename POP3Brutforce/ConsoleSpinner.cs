using System;

namespace POP3Brutforce
{
    public sealed class ConsoleSpinner
    {
        int counter;

        public ConsoleSpinner()
        {
            counter = 0;
        }

        public void Turn()
        {
            counter++;

            string[] frame = new string[] { "/", "|", "\\", "-" };

            Console.Write(frame[counter % frame.Length]);

            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
        }
    }
}
