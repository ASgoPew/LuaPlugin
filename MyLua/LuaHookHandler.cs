using NLua;
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

    public class LuaHookHandler<T> : ILuaHookHandler
    {
        LuaEnvironment LuaEnv { get; set; }
        public string Name { get; set; }
        Action<LuaHookHandler<T>, bool?> Control { get; set; }
        public T Handler { get; set; }
        public bool Active { get; private set; }

        public LuaHookHandler(LuaEnvironment luaEnv, string name, Action<LuaHookHandler<T>, bool?> control)
        {
            LuaEnv = luaEnv;
            Name = name;
            Control = control;
            Control(this, null);
        }

        public void Invoke(params object[] args)
        {
            try
            {
                LuaEnv.CallFunctionByName(Name, args);
            }
            catch (Exception e)
            {
                Control.Invoke(this, false);
                LuaEnv.Set(Name, null);
                LuaEnv.RaiseLuaHookException(Name, e);
            }
        }

        public void Update()
        {
            LuaFunction f = LuaEnv.Get(Name) as LuaFunction;
            if (f != null && !Active)
                Enable();
            else if (f == null && Active)
                Disable();
        }

        public void Enable()
        {
            Active = true;
            Control.Invoke(this, true);
        }

        public void Disable()
        {
            Control.Invoke(this, false);
            Active = false;
        }
    }
}
