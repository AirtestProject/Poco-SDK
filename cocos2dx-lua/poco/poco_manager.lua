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

local socket = _G.socket or require('socket')
local json = import('.support.dkjson')  -- 一定要用这个模块不然字符串发送有问题
local struct = _G.struct or import('.support.struct')
local VERSION = import('.POCO_SDK_VERSION')
local Dumper = import('.Cocos2dxDumper')
local Screen = import('.Cocos2dxScreen')

local DEBUG = false

-- client handler
local ClientConnection = {}
ClientConnection.__index = ClientConnection

ClientConnection.sock = nil
ClientConnection.buf = ''

function ClientConnection:new(sock)
    local c = {}
    setmetatable(c, ClientConnection)
    c.sock = sock
    c.buf = ''
    c.sock:setoption("tcp-nodelay", true)
    c.sock:setoption('keepalive', true)
    c.sock:settimeout(0.0)
    return c
end

function ClientConnection:input(data)
    self.buf = self.buf .. data
    if #self.buf > 4 then 
       local len = struct.unpack('i', string.sub(self.buf, 1, 4))
        if #self.buf >= len + 4 then
            local content = string.sub(self.buf, 5, 4 + len)
            self.buf = string.sub(self.buf, 5 + len)
            if DEBUG then
                print(content)
            end
            return json.decode(content)
        end
    end
    return nil
end

function ClientConnection:receive()
    local chunk, status, partial = self.sock:receive(65535)
    if DEBUG then
        print('client recv', partial or chunk)
    end
    if (not chunk or chunk == '') and (not partial or partial == '') then
        self:close()
        return ''
    else
        local req = self:input(partial or chunk)
        if req ~= nil then
            req.client = self
            return req
        end
    end
end

function ClientConnection:send(data)
    local data = json.encode(data)
    local sdata = struct.pack('i', #data) .. data
    -- FIXME
    if DEBUG then
        print(sdata)
    end
    self.sock:send(sdata)
end

function ClientConnection:close()
    self.sock:shutdown('both')
    self.sock:close()
    self.buf = ''
    print('[poco] client disconnect')
end

function ClientConnection:getAddress()
    return self.sock:getpeername()
end

-- poco
local PocoManager = {}
PocoManager.__index = PocoManager

PocoManager.VERSION = VERSION
PocoManager.server_sock = nil
PocoManager.all_socks = {}
PocoManager.clients = {}

-- rpc methods registration
-- rpc 方法统一才是Pascal命名方式，其实是为了跟unity3d里使用的poco-sdk命名相同
local dispatcher = {
    GetSDKVersion = function() return VERSION end,
    Dump = function() return Dumper:dumpHierarchy() end,
    Screenshot = function(width)
        width = width or 720
        return Screen:getScreen(width) 
    end,
    GetScreenSize = function() return Screen:getPortSize() end,
    SetText = function(_instanceId, val)
        -- TODO
    end,
    test = function(arg1, arg2) return string.format('test arg1:%s arg2:%s', arg1, arg2) end,
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
    print('[poco]', server_sock:listen(5))

    -- 放在定时器里循环
    cc.Director:getInstance():getScheduler():scheduleScriptFunc(function() self:server_loop() end, 0.025, false)
end

function PocoManager:server_loop()
    local r, w, e = socket.select(self.all_socks, nil, 0)
    local removed_socks = {}
    if #r > 0 then
        for i, v in ipairs(r) do
            if v == self.server_sock then
                local client_sock, err = self.server_sock:accept()
                print('[poco] new client accepted', client_sock:getpeername(), err)
                table.insert(self.all_socks, client_sock)
                self.clients[client_sock] = ClientConnection:new(client_sock)
            else
                local client = self.clients[v]
                local req = client:receive()
                if req == '' then
                    self.clients[v] = nil
                    table.insert(removed_socks, v)
                elseif req ~= nil then
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
    else
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
        end
    end
    client:send(ret)
end

return PocoManager
