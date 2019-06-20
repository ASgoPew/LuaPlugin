console = TSPlayer.Server
everyone = TSPlayer.All

function OnLuaInit()
  puts('INIT')
  math.randomseed(os.clock() * 1000)
  --initialize_database()
end

function OnLuaClose()
  puts("CLOSE")
  --dispose_database()
end

function execute(code, player, silent)
  if code:starts(";;") then
    if #code == 2 then
      code = "OnTick = nil"
    else
      code = "function OnTick(frame) " .. code:sub(3, #code) .. " end"
    end
  elseif code:starts("=") then
    code = "show(" .. code:sub(2, #code) .. ")"
  elseif code:starts("-") then
    code = "puts(env:SharpShow(" .. code:sub(2, #code) .. "))"
  elseif code:starts(";") then
    code = "puts(" .. code:sub(2, #code) .. ")"
  end

  me = player
  local now = os.clock()
  local result

  putsc(output_colors["white"], code)
  result = {execute_direct(code)}
  return table.unpack(result)
end

function execute_direct(code)
  local f, err = load(code)
  if f then
    return f()
  else
    error(err)
  end
end