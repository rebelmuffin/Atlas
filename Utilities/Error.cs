using System.Diagnostics;

namespace Atlas.Utilities
{
    // #TODO: Could probably use an exception type other than base Exception
    public class Error
    {
        [DebuggerHidden]
        public static void IfTrue(bool condition, string? message = null)
        {
            if (condition)
            {
                if (Debugger.IsAttached)
                {
                    Debug.Fail(message);
                }
                else
                {
                    throw new Exception(message);
                }
            }
        }

        [DebuggerHidden]
        public static void IfFalse(bool condition, string? message = null)
        {
            if (!condition)
            {
                if (Debugger.IsAttached)
                {
                    Debug.Fail(message);
                }
                else
                {
                    throw new Exception(message);
                }
            }
        }

        [DebuggerHidden]
        public static void IfNull(object? targetObj, string? message = null)
        {
            if (targetObj is null)
            {
                if (Debugger.IsAttached)
                {
                    Debug.Fail(message);
                }
                else
                {
                    throw new Exception(message);
                }
            }
        }
    }
}