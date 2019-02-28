using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLuaTest
{
    class Program
    {
        public static Lua lua = new Lua();
        public static LuaFunction f;

        static void Main(string[] args)
        {
            int n = 100;
            lua.State.Encoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            //lua.DoString("f = function() end");
            //f = lua.GetFunction("f");
            f = lua.GetFunction("print");
            Task[] tasks = new Task[n];
            for (int i = 0; i < n; i++)
                tasks[i] = Task.Run(() => f1());
            Task.WaitAll(tasks);
        }

        static void f1()
        {
            try
            {
                //lua.DoFile("test.lua");
                lua.DoString("print('кек')");
                //f.Call("lol");
                //lua.LoadString("print('lol')", "testChunk");
                //lua.LoadFile("test.lua");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
