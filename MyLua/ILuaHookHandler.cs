using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLua
{
    public interface ILuaHookHandler
    {
        string Name { get; set; }
        bool Active { get; set; }

        void Update();
        void Hook();
        void Unhook();
    }
}
