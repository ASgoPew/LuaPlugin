using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLua
{
    public interface ILuaHookHandler
    {
        string Name { get; }
        bool Active { get; }

        void Update();
        void Enable();
        void Disable();
    }
}
