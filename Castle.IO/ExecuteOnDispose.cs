using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenFileSystem.IO
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
