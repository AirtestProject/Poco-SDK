--
-- 实现screen模块时请按照这个接口的规定来
--
--

local IScreen = {}
IScreen.__index = IScreen

function IScreen:getPortSize()
    -- return screen resolution
end

function IScreen:getScreen(width)
    -- return promise/future
end

return IScreen