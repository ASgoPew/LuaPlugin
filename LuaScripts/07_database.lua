function initialize_database()
	--[[import('Mono.Data.Sqlite', 'Mono.Data.Sqlite')
	import('TShockAPI', 'TShockAPI.DB')
	import('MySql.Data', 'MySql.Data.MySqlClient')
	db = SqliteConnection('uri=file://keyvalue.sqlite,Version=3')]]
	--db:Open()
	--[[sqlcreator = SqlTableCreator(db, SqliteQueryCreator())
	--idcolumn.AutoIncrement = true
	sqlcreator:EnsureTableStructure(SqlTable("data",
		SqlColumn("key", MySqlDbType.Text),
		SqlColumn("value", MySqlDbType.Text)
	))
	db:Query("CREATE UNIQUE INDEX datakey ON data(key);")]]
	--db:Open()
	db = SqliteConnection('uri=file://keyvalue.sqlite,Version=3')
	--plugin.data['db'] = db
	db:Query("CREATE TABLE IF NOT EXISTS data (key TEXT UNIQUE, value TEXT)")
	db:Query("CREATE TABLE IF NOT EXISTS user_data (user INTEGER, key TEXT, value TEXT, UNIQUE(user, key))")
	--db:Dispose()
	--db:Close()
end

function dispose_database()
	if db then
		db:Dispose()
	end
end

function dbget(key)
	assert(type(key) == 'string')
	local reader = db:QueryReader("SELECT value FROM data WHERE key=@0", key)
	local value = nil
	if reader:Read() then
		value = reader.Reader:GetString(0)
	end
	reader:Dispose()
	return value
end

function dbset(key, value)
	assert(type(key) == 'string' and type(value) == 'string')
	db:Query("REPLACE INTO data(key, value) values(@0, @1);", key, value)
end

function dbrm(key)
	assert(type(key) == 'string')
	db:Query("DELETE FROM data WHERE key=@0", key)
end

function dbgetu(user_id, key)
	assert(type(key) == 'string')
	local reader = db:QueryReader("SELECT value FROM user_data WHERE user=@0 AND key=@1", user_id, key)
	local value = nil
	if reader:Read() then
		value = reader.Reader:GetString(0)
	end
	reader:Dispose()
	return value
end

function dbsetu(user_id, key, value)
	assert(type(key) == 'string' and type(value) == 'string')
	db:Query("REPLACE INTO user_data(user, key, value) values(@0, @1, @2);", user_id, key, value)
end

function dbrmu(user_id, key)
	assert(type(key) == 'string')
	db:Query("DELETE FROM user_data WHERE user=@0 AND key=@1", user_id, key)
end

DBStream = Class({
	__init = function(self, key, user_id)
		self.key = key
		if user_id == true then
			-- random
		else
			self.user_id = user_id
		end
		self.data = {}
		self.i = 1
	end,

	load = function(self)
		local result = self.user_id and dbgetu(self.user_id, self.key)
			or dbget(self.key)
		if not result then return false end
		self.data = table.deserialize(result)
		self.i = 1
		return true
	end,

	seek = function(self, i)
		self.i = i
		return self
	end,

	read = function(self)
		local result = self.data[self.i]
		self.i = self.i + 1
		return result
	end,

	write = function(self, data)
		self.data[self.i] = data
		self.i = self.i + 1
		return self
	end,

	update = function(self)
		if self.user_id then
			dbsetu(self.user_id, self.key, table.serialize(self.data))
		else
			dbset(self.key, table.serialize(self.data))
		end
		return self
	end
})