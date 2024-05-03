using System;
using System.Windows;

namespace Optimization
{
    static class Logger
    {
        private const ConsoleColor InfoColor = ConsoleColor.Green;
        private const ConsoleColor WarningColor = ConsoleColor.Yellow;
        private const ConsoleColor ErrorColor = ConsoleColor.Red;

        private static Window _window;
        private static string _content;

        public static void SetWindow(Window window)
        {
            _window = window;
            _window.Content = "";
        }

        public static void WriteInfo(string message)
        {
            WriteMessage(message, InfoColor);
        }

        public static void WriteWarning(string message)
        {
            WriteMessage(message, WarningColor);
        }

        public static void WriteError(string message)
        {
            WriteMessage(message, ErrorColor);
        }

        private static void WriteMessage(string message, ConsoleColor color)
        {
            string caption = "";

            switch (color)
            {
                case ConsoleColor.Green:
                    caption = "Info";
                    break;
                case ConsoleColor.Yellow:
                    caption = "Warning";
                    break;
                case ConsoleColor.Red:
                    caption = "Error";
                    break;
            }
            caption = "[" + caption + "]";
            _content += $"{caption,-11} {message}\n";
            _window.Content = _content;
        }
    }
}