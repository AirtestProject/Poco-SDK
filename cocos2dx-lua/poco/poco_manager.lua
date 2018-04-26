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
local socket = nil
xpcall(function() 
    socket = _G.socket or require('socket.core')
end, function()
    -- cocos2dx-lua 里的兼容写法
    socket = cc.exports.socket
end)
local VERSION = import('.POCO_SDK_VERSION')
local Dumper = import('.Cocos2dxFrozenDumper')
local Screen = import('.Cocos2dxScreen')
local ClientConnection = import('.ClientConnection')

-- poco
local PocoManager = {}
PocoManager.__index = PocoManager

PocoManager.DEBUG = false
PocoManager.VERSION = VERSION

PocoManager.server_sock = nil
PocoManager.all_socks = {}
PocoManager.clients = {}

-- rpc methods registration
-- rpc 方法统一才是Pascal命名方式，其实是为了跟unity3d里使用的poco-sdk命名相同
local dispatcher = {
    GetSDKVersion = function() return VERSION end,
    Dump = function(onlyVisibleNode) 
        if onlyVisibleNode == nil then
            onlyVisibleNode = true
        end
        return Dumper:dumpHierarchy(onlyVisibleNode)
    end,
    Screenshot = function(width)
        width = width or 720
        return Screen:getScreen(width) 
    end,
    GetScreenSize = function() return Screen:getPortSize() end,
    SetText = function(_instanceId, val)
        local node = Dumper:getCachedNode(_instanceId)
        if node ~= nil then
            return node:setAttr('text', val)
        end
        return false
    end,
    test = function(arg1, arg2) 
        return string.format('test arg1:%s arg2:%s', arg1, arg2) 
    end,
}


function PocoManager:init_server(port)
    port = port or 15004
    local server_sock, err = socket.tcp()
    assert(server_sock)
    table.insert(self.all_socks, server_sock)
    self.server_sock = server_sock
    server_sock:setoption('reuseaddr', true)
    server_sock:setoption('keepalive', true)
    server_sock:settimeout(0.0)
    server_sock:bind('*', port)
    server_sock:listen(5)
    print(string.format('[poco] server listens on tcp://*:%s', port))

    -- 放在定时器里循环
    cc.Director:getInstance():getScheduler():scheduleScriptFunc(function() self:server_loop() end, 0.025, false)
end

function PocoManager:server_loop()
    for _, c in pairs(self.clients) do
        c:drainOutputBuffer()
    end

    local r, w, e = socket.select(self.all_socks, nil, 0)
    if #r > 0 then
        local removed_socks = {}

        for i, v in ipairs(r) do
            if v == self.server_sock then
                local client_sock, err = self.server_sock:accept()
                print('[poco] new client accepted', client_sock:getpeername(), err)
                table.insert(self.all_socks, client_sock)
                self.clients[client_sock] = ClientConnection:new(client_sock, self.DEBUG)
            else
                local client = self.clients[v]
                local reqs = client:receive()
                if reqs == '' then
                    -- client is gone
                    self.clients[v] = nil
                    table.insert(removed_socks, v)
                elseif reqs ~= nil then
                    for _, req in ipairs(reqs) do
                        self:onRequest(req)
                    end
                end
            end
        end

        -- 移除已断开的client socket
        for _, s in pairs(removed_socks) do
            for i, v in ipairs(self.all_socks) do
                if v == s then
                    table.remove(self.all_socks, i)
                    break  -- break inner loop only
                end
            end
        end
    end

    for _, c in pairs(self.clients) do
        c:drainOutputBuffer()
    end
end

function PocoManager:onRequest(req)
    local client = req.client
    local method = req.method
    local params = req.params
    local func = dispatcher[method]
    local ret = {
        id = req.id,
        jsonrpc = req.jsonrpc,
        result = nil,
        error = nil,
    }
    if func == nil then
        ret.error = {message = string.format('No such rpc method "%s", reqid: %s, client:%s', method, req.id, req.client:getAddress())}
        client:send(ret)
    else
        xpcall(function()
            local result = func(unpack(params))
            if type(result) == 'function' then
                -- 如果返回的是一个function，则表示这个调用是异步的，目前就通过这种callback的形式的约定
                result(function(cbresult)
                    ret.result = cbresult
                    client:send(ret)
                end)
                return
            else
                ret.result = result
                client:send(ret)
            end
        end, function(msg)
            ret.error = {message = debug.traceback(msg)}
            client:send(ret)
        end)
    end
end

return PocoManager
