function delay(time, func)
	env:LuaDelay(time, func)
end

function sleep(time)
	env:LuaSleep(time)
end

function generate_error()
	kek.lol = 0
end

function SharpShow(o)
	return env:SharpShow(o)
end

function emptyfunction()
end

function shallowcopy(orig)
	local orig_type = type(orig)
	local copy
	if orig_type == 'table' then
		copy = {}
		for orig_key, orig_value in pairs(orig) do
			copy[orig_key] = orig_value
		end
	else -- number, string, boolean, etc
		copy = orig
	end
	return copy
end

function deepcopy(orig)
	local orig_type = type(orig)
	local copy
	if orig_type == 'table' then
		copy = {}
		for orig_key, orig_value in next, orig, nil do
			copy[deepcopy(orig_key)] = deepcopy(orig_value)
		end
		setmetatable(copy, deepcopy(getmetatable(orig)))
	else -- number, string, boolean, etc
		copy = orig
	end
	return copy
end

function math.round(num, multiplier)
	multiplier = multiplier or 1
	return math.floor((num / multiplier) + 0.5) * multiplier
end

function table.shuffle(t)
	local size = #t
	for i = size, 1, -1 do
		local rand = math.random(size)
		t[i], t[rand] = t[rand], t[i]
	end
	return tbl
end

function table.fill_holes(t1, t2)
	for k, o in pairs(t2) do
		t1[k] = t1[k] or o
	end
	return t1
end

function table.concat(t1, t2)
	local result = {}
	for k, v in pairs(t1) do
		result[k] = v
	end
	for k, v in pairs(t2) do
		result[k] = v
	end
	return result
end

function string.starts(str, beginning)
	return string.sub(str,1,string.len(beginning))==beginning
end
function string.ends(str, ending)
	return ending=='' or string.sub(str,-string.len(ending))==ending
end
function string.trim(s)
	return s:match'^()%s*$' and '' or s:match'^%s*(.*%S)'
end
function string.lines(str)
   local t = {}
   local function helper(line)
	  table.insert(t, line)
	  return ""
   end
   helper((str:gsub("(.-)\r?\n", helper)))
   return t
end
function string.words(str)
   local t = {}
   local function helper(line)
	  table.insert(t, line)
	  return ""
   end
   helper((str:gsub("(.-) +", helper)))
   return t
end
function string.count(base, pattern)
	return select(2, string.gsub(base, pattern, ""))
end

function testspeedf(f)
	local t = os.clock();
	local count = 0
	while os.clock() - t < 1 do
		f()
		count = count + 1
	end
	print('function called ' .. tostring(count) .. ' times per 1 second')
end