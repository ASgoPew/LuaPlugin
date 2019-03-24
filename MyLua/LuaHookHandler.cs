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
                Disable();
                LuaEnv.Set(Name, null);
                LuaEnv.RaiseLuaException($"Hook: {Name}", e);
            }
        }

        public void InvokeGeneric() => Invoke();
        public void InvokeGeneric<U>(U args) => Invoke(args);
        public void InvokeGeneric<U, V>(U arg0, V arg1) => Invoke(arg0, arg1);

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
