using System;

namespace Castle.IO
{
    public class ExecuteOnDispose : IDisposable
    {
        Action _action;

        public ExecuteOnDispose(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            _action();
        }
    }
}
