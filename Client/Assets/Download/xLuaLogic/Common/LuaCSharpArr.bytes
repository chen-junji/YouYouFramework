local apiInit = nil
local apiCreateAccess = nil

if CS ~= nil then -- xlua
    apiInit = CS.LuaArrAccessAPI.Init
    apiCreateAccess = CS.LuaArrAccessAPI.CreateLuaShareAccess
else
    apiInit = LuaArrAccessAPI.Init
    apiCreateAccess = LuaArrAccessAPI.CreateLuaShareAccess
end

apiInit(jit ~= nil)

local LuaCSharpArr = 
{
    class = "LuaCSharpArr",
}
local fields = {}
local pin_func = lua_safe_pin_bind
local IsJit = 0
if jit then
    IsJit = 1
end
setmetatable(LuaCSharpArr, LuaCSharpArr)
LuaCSharpArr.__index = function(t,k)
    local var = rawget(LuaCSharpArr, k)
    return var
end

local needDetect = true
local function GlobalAutoDetectArch()
    if needDetect == false then
        return
    end

    needDetect = false

    if jit then
        local data = LuaCSharpArr.New(3)
        data:AutoDetectArch()
    end
end


function LuaCSharpArr.New(len)
    GlobalAutoDetectArch()
    local v = {}
    for i = 1, len do
        v[i] = 0
    end

    --SetGCTbl(v)

    setmetatable(v, LuaCSharpArr)
    return v
end

local oldGCFunc = nil

local function newGCFunc(self)
	self:OnGC()
	oldGCFunc(self)
end

local function SetCSharpAccessGCFunc(pin)
	local mt = getmetatable(pin)
	if oldGCFunc == nil then
		oldGCFunc = mt.__gc
	end
	
    mt.__gc = newGCFunc
end

function LuaCSharpArr:GetCSharpAccess()
    if self.__pin == nil then
        self.__pin = apiCreateAccess()
        pin_func(self, self.__pin)
        SetCSharpAccessGCFunc(self.__pin)
    end

    -- body
    return self.__pin
end

function LuaCSharpArr:DestroyCSharpAccess()
    if self.__pin ~= nil then
        self.__pin:OnGC()
        self.__pin = nil
    end
end

function LuaCSharpArr:AutoDetectArch()
    if jit then
        self[1] = 32167
        self[2] = 9527.5
        self[3] = -2000000
        local acc = self:GetCSharpAccess()
        acc:AutoDetectArch()
    end
end


return LuaCSharpArr