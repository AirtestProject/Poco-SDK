-- import
function import(moduleName, currentModuleName)
    local currentModuleNameParts
    local moduleFullName = moduleName
    local offset = 1

    while true do
        if string.byte(moduleName, offset) ~= 46 then
            moduleFullName = string.sub(moduleName, offset)
            if currentModuleNameParts and #currentModuleNameParts > 0 then
                moduleFullName = table.concat(currentModuleNameParts, ".") .. "." .. moduleFullName
            end
            break
        end
        offset = offset + 1

        if not currentModuleNameParts then
            if not currentModuleName then
                local n, v = debug.getlocal(3, 1)
                currentModuleName = v
            end
            currentModuleNameParts = string.split(currentModuleName, ".")
        end
        table.remove(currentModuleNameParts, #currentModuleNameParts)
    end

    return require(moduleFullName)
end

local cc = _G.cc or require('cc')
local AbstractDumper = import('.sdk.AbstractDumper')
local Node = import('.Cocos2dxNode')

local director = cc.Director:getInstance()

local Dumper = {}
Dumper.__index = Dumper
setmetatable(Dumper, AbstractDumper)

function Dumper:getRoot()
    local winSize = director:getWinSize()
    return Node:new(director:getRunningScene(), winSize.width, winSize.height)
end

return Dumper