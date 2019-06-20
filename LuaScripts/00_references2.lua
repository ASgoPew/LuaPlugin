-- You should understand that hooks can invoke while your lua code is executing (but only these hooks that gets activated by your straight actions)

--import('System', 'System.Diagnostics');
--import('TShockAPI', 'TShockAPI.Hooks')
--import('OTAPI', 'Microsoft.Xna.Framework');

import ('mscorlib', 'System')
import('TShockAPI', 'TShockAPI')
--import('OTAPI', 'OTAPI')
import('OTAPI', 'Terraria')
import('OTAPI', 'Terraria.ID')
import('TerrariaServer', 'TerrariaApi.Server')
import('MyLua', 'MyLua')
import('LuaPlugin', 'LuaPlugin')
import('Mono.Data.Sqlite', 'Mono.Data.Sqlite')
import('TShockAPI', 'TShockAPI.DB')
import('MySql.Data', 'MySql.Data.MySqlClient')