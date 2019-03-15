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
local mime = nil
xpcall(function() 
    mime = _G.mime or require('mime')
end, function()
    -- cocos2dx-lua 里的兼容写法
    mime = cc.exports.mime
end)
local IScreen = import('.sdk.IScreen')

local b64encode = nil
if mime ~= nil then
    b64encode = mime.b64
else
    b64encode = import('.support.base64').enc
end

local screen = {}
screen.__index = screen
setmetatable(screen, IScreen)

local director = cc.Director:getInstance()
local winSize = director:getWinSize()  -- default win size is the design resolution
local frameSize = director:getOpenGLView():getFrameSize()

function screen:getPortSize()
    return {frameSize.width, frameSize.height}
end

function screen:getScreen(width)
    local designRes = winSize
    local filename = "screenshot.png"

    local screenshotScaleFactor = width / designRes.width
    local scene = director:getRunningScene()

    -- return a future object
    return function(cb)
        cc.utils:captureScreen(function(succeed, outputFile)
            if succeed then
                print('截图成功：' .. outputFile)
                local f = io.open(outputFile, "rb")
                if not f then
                    print('截图文件不存在 2333')
                end
                local screendata = f:read("*all")
                screendata = b64encode(screendata)
                cb({screendata, 'png'})
                f:close()
                print('done!')
            else
                print('截图失败')
            end
        end, filename)
    end
end

return screen