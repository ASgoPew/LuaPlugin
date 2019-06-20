function Class(...)
	local cls  = {}
	local inst = { [cls] = true }

	for _, base in ipairs({...}) do
		inst[base] = true
		for k, v in pairs(base) do
			cls[k] = v
		end
	end

	cls.__index = cls
	cls.instance_of = inst -- Doesn't work for deep inheritance tree

	setmetatable(cls, {
		__call = function(c, ...)
			  local instance = setmetatable({}, c)
			  if instance.__init then
					instance:__init(...)
			  end
			  return instance
		end
	})

	if cls.__static then
		cls:__static()
	end

	return cls
end

function DebugClass(...)
	local cls  = {}
	local inst = { [cls] = true }

	local args = {...}
	print('CLASS, bases count: ' .. tostring(#args))
	for i = 1, #args do
		local base = args[i]
		print('BASE:', base)
	end
	print('BASES LOADED')

	for _, base in ipairs({...}) do
		inst[base] = true
		for k, v in pairs(base) do
			print(tostring(k) .. '=' .. tostring(v))
			cls[k] = v
		end
	end

	cls.__index = cls
	cls.instance_of = inst

	setmetatable(cls, {
		__call = function(c, ...)
			  local instance = setmetatable({}, c)
			  if instance.__init then
					instance:__init(...)
			  end
			  return instance
		end
	})

	return cls
end
