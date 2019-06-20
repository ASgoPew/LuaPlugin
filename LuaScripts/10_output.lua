output_colors = {
	["white"]={255, 255, 255}
}
chat_prefix = "[c/00cc00:<Lua>] "
console_chat_prefix = "<Lua> "
default_color = {150, 150, 255}

function process_output(...)
	local args = {...}
	local output = ""
	for i = 1, #args do
		if args[i] ~= nil then
			output = output .. tostring(args[i])
		else
			output = output .. 'nil'
		end
		if i ~= #args then output = output .. ', ' end
	end
	if #args == 0 then output = '<empty>' end
	return output
end

function puts(...)
	SendMessage(process_output(...), default_color[1], default_color[2], default_color[3])
end

function putsc(color, ...)
	SendMessage(process_output(...), color[1], color[2], color[3])
end

function perror(...)
	SendErrorMessage(process_output(...))
end

function SendMessage(text, r, g, b)
	console:SendInfoMessage(console_chat_prefix .. text)
	text = chat_prefix .. text
	for i = 0, 254 do
		local player = TShock.Players[i]
		if player ~= nil and player.Active and player:HasPermission(LuaConfig.ControlPermission) then
			player:SendMessage(text, r, g, b)
		end
	end
end

function SendErrorMessage(text)
	console:SendErrorMessage(console_chat_prefix .. text)
	text = chat_prefix .. text
	for i = 0, 254 do
		local player = TShock.Players[i]
		if player ~= nil and player.Active and player:HasPermission(LuaConfig.ControlPermission) then
			player:SendErrorMessage(text)
		end
	end
end

function show(t)
	if type(t) == 'userdata' then
		t = getmetatable(t)
	end
	if type(t) == 'table' then
		print('table:')
		for k,v in pairs(t) do
			print(k, v)
		end
	else
		print(t)
	end
end