#include "jsb_websocketserver.h"

#include "base/ccUTF8.h"
#include "base/CCDirector.h"
#include "WebSocketServer.h"
#include "platform/CCPlatformMacros.h"
#include "scripting/js-bindings/manual/ScriptingCore.h"
#include "scripting/js-bindings/manual/cocos2d_specifics.hpp"
#include "scripting/js-bindings/manual/spidermonkey_specifics.h"

using namespace cocos2d::network;

class JSB_WebSocketServerDelegate : public WebSocketServer::Delegate
{
public:

    JSB_WebSocketServerDelegate()
    {
        JSContext* cx = ScriptingCore::getInstance()->getGlobalContext();
        _JSDelegate.construct(cx);
    }

    ~JSB_WebSocketServerDelegate()
    {
        _JSDelegate.destroyIfConstructed();
    }

    virtual void onConnection(WebSocketServer* ws,  const int socketID)
    {
        js_proxy_t * p = jsb_get_native_proxy(ws);
        if (!p) return;

        if (cocos2d::Director::getInstance() == nullptr || cocos2d::ScriptEngineManager::getInstance() == nullptr)
            return;

        JSB_AUTOCOMPARTMENT_WITH_GLOBAL_OBJCET

        JSContext* cx = ScriptingCore::getInstance()->getGlobalContext();
        JS::RootedObject jsobj(cx, JS_NewObject(cx, NULL, JS::NullPtr(), JS::NullPtr()));
        JS::RootedValue vp(cx);
        vp = c_string_to_jsval(cx, "open");
        JS_SetProperty(cx, jsobj, "type", vp);

        JS::RootedValue datasocketID(cx);
        datasocketID = int32_to_jsval(cx, socketID);
        JS_SetProperty(cx, jsobj, "socketId", datasocketID);

        jsval args = OBJECT_TO_JSVAL(jsobj);

        ScriptingCore::getInstance()->executeFunctionWithOwner(OBJECT_TO_JSVAL(_JSDelegate.ref()), "onConnection", 1, &args);
    }

    virtual void onMessage(WebSocketServer* ws,  const int socketID, const WebSocketServer::Data& data)
    {
        js_proxy_t * p = jsb_get_native_proxy(ws);
        if (p == nullptr) return;

        if (cocos2d::Director::getInstance() == nullptr || cocos2d::ScriptEngineManager::getInstance() == nullptr)
            return;

        JSB_AUTOCOMPARTMENT_WITH_GLOBAL_OBJCET

        JSContext* cx = ScriptingCore::getInstance()->getGlobalContext();
        JS::RootedObject jsobj(cx, JS_NewObject(cx, NULL, JS::NullPtr(), JS::NullPtr()));
        JS::RootedValue vp(cx);
        vp = c_string_to_jsval(cx, "message");
        JS_SetProperty(cx, jsobj, "type", vp);

        JS::RootedValue datasocketID(cx);
        datasocketID = int32_to_jsval(cx, socketID);
        JS_SetProperty(cx, jsobj, "socketId", datasocketID);

        JS::RootedValue args(cx, OBJECT_TO_JSVAL(jsobj));
        if (data.isBinary)
        {// data is binary
            JS::RootedObject buffer(cx, JS_NewArrayBuffer(cx, static_cast<uint32_t>(data.len)));
            if (data.len > 0)
            {
                uint8_t* bufdata = JS_GetArrayBufferData(buffer);
                memcpy((void*)bufdata, (void*)data.bytes, data.len);
            }
            JS::RootedValue dataVal(cx);
            dataVal = OBJECT_TO_JSVAL(buffer);
            JS_SetProperty(cx, jsobj, "data", dataVal);
        }
        else
        {// data is string
            JS::RootedValue dataVal(cx);
            if (strlen(data.bytes) == 0 && data.len > 0)
            {// String with 0x00 prefix
                dataVal = STRING_TO_JSVAL(JS_NewStringCopyN(cx, data.bytes, data.len));
            }
            else
            {// Normal string
                dataVal = c_string_to_jsval(cx, data.bytes);
            }
            if (dataVal.isNullOrUndefined())
            {
                ws->stopAsync();
                return;
            }
            JS_SetProperty(cx, jsobj, "data", dataVal);
        }

        ScriptingCore::getInstance()->executeFunctionWithOwner(OBJECT_TO_JSVAL(_JSDelegate.ref()), "onMessage", 1, args.address());
    }

    virtual void onDisconnection(WebSocketServer* ws,  const int socketID)
    {
        js_proxy_t * p = jsb_get_native_proxy(ws);
        if (!p) return;

        if (cocos2d::Director::getInstance() != nullptr && cocos2d::Director::getInstance()->getRunningScene() && cocos2d::ScriptEngineManager::getInstance() != nullptr)
        {
            JSB_AUTOCOMPARTMENT_WITH_GLOBAL_OBJCET
            
            JSContext* cx = ScriptingCore::getInstance()->getGlobalContext();
            JS::RootedObject jsobj(cx, JS_NewObject(cx, NULL, JS::NullPtr(), JS::NullPtr()));
            JS::RootedValue vp(cx);
            vp = c_string_to_jsval(cx, "disconnection");
            JS_SetProperty(cx, jsobj, "type", vp);

            JS::RootedValue datasocketID(cx);
            datasocketID = int32_to_jsval(cx, socketID);
            JS_SetProperty(cx, jsobj, "socketId", datasocketID);
            
            JS::RootedValue args(cx, OBJECT_TO_JSVAL(jsobj));
            ScriptingCore::getInstance()->executeFunctionWithOwner(OBJECT_TO_JSVAL(_JSDelegate.ref()), "onDisconnection", 1, args.address());
        }
    }

    virtual void onError(WebSocketServer* ws, const WebSocketServer::ErrorCode& error)
    {
        js_proxy_t * p = jsb_get_native_proxy(ws);
        if (!p) return;

        if (cocos2d::Director::getInstance() == nullptr || cocos2d::ScriptEngineManager::getInstance() == nullptr)
            return;

        JSB_AUTOCOMPARTMENT_WITH_GLOBAL_OBJCET

        JSContext* cx = ScriptingCore::getInstance()->getGlobalContext();
        JS::RootedObject jsobj(cx, JS_NewObject(cx, NULL, JS::NullPtr(), JS::NullPtr()));
        JS::RootedValue vp(cx);
        vp = c_string_to_jsval(cx, "error");
        JS_SetProperty(cx, jsobj, "type", vp);

        JS::RootedValue args(cx, OBJECT_TO_JSVAL(jsobj));

        ScriptingCore::getInstance()->executeFunctionWithOwner(OBJECT_TO_JSVAL(_JSDelegate.ref()), "onError", 1, args.address());
    }

    virtual void onServerUp(WebSocketServer* ws)
    {
         js_proxy_t * p = jsb_get_native_proxy(ws);
        if (!p) return;

        if (cocos2d::Director::getInstance() == nullptr || cocos2d::ScriptEngineManager::getInstance() == nullptr)
            return;

        JSB_AUTOCOMPARTMENT_WITH_GLOBAL_OBJCET

        JSContext* cx = ScriptingCore::getInstance()->getGlobalContext();
        JS::RootedObject jsobj(cx, JS_NewObject(cx, NULL, JS::NullPtr(), JS::NullPtr()));
        JS::RootedValue vp(cx);
        vp = c_string_to_jsval(cx, "serverup");
        JS_SetProperty(cx, jsobj, "type", vp);

        JS::RootedValue args(cx, OBJECT_TO_JSVAL(jsobj));

        ScriptingCore::getInstance()->executeFunctionWithOwner(OBJECT_TO_JSVAL(_JSDelegate.ref()), "onServerUp", 1, args.address());
    }

    virtual void onServerDown(WebSocketServer* ws)
    {
        js_proxy_t * p = jsb_get_native_proxy(ws);
        if (!p) return;

        if (cocos2d::Director::getInstance() == nullptr || cocos2d::ScriptEngineManager::getInstance() == nullptr)
            return;

        JSB_AUTOCOMPARTMENT_WITH_GLOBAL_OBJCET

        JSContext* cx = ScriptingCore::getInstance()->getGlobalContext();
        JS::RootedObject jsobj(cx, JS_NewObject(cx, NULL, JS::NullPtr(), JS::NullPtr()));
        JS::RootedValue vp(cx);
        vp = c_string_to_jsval(cx, "serverdown");
        JS_SetProperty(cx, jsobj, "type", vp);

        JS::RootedValue args(cx, OBJECT_TO_JSVAL(jsobj));

        ScriptingCore::getInstance()->executeFunctionWithOwner(OBJECT_TO_JSVAL(_JSDelegate.ref()), "onServerDown", 1, args.address());
    }

    void setJSDelegate(JS::HandleObject pJSDelegate)
    {
        _JSDelegate.ref() = pJSDelegate;
    }
private:
    mozilla::Maybe<JS::PersistentRootedObject> _JSDelegate;
};

JSClass  *js_cocos2dx_websocketserver_class;
JSObject *js_cocos2dx_websocketserver_prototype;

void js_cocos2dx_WebSocketServer_finalize(JSFreeOp *fop, JSObject *obj) {
    CCLOG("jsbindings: finalizing JS object %p (WebSocketServer)", obj);
}

bool js_cocos2dx_extension_WebSocketServer_send(JSContext *cx, uint32_t argc, jsval *vp)
{
    JS::CallArgs argv = JS::CallArgsFromVp(argc, vp);
    JS::RootedObject obj(cx, argv.thisv().toObjectOrNull());
    js_proxy_t *proxy = jsb_get_js_proxy(obj);
    WebSocketServer* cobj = (WebSocketServer *)(proxy ? proxy->ptr : NULL);
    JSB_PRECONDITION2( cobj, cx, false, "Invalid Native Object");

    if(argc == 2)
    {
        bool ok = true;
        int socketID;
        ok &= jsval_to_int32(cx, argv[0], (int32_t *)&socketID);

        if (argv[1].isString())
        {
            ssize_t len = JS_GetStringLength(argv[1].toString());
            std::string data;
            jsval_to_std_string(cx, argv[1], &data);

            if (data.empty() && len > 0)
            {
                CCLOGWARN("Text message to send is empty, but its length is greater than 0!");
                //FIXME: Note that this text message contains '0x00' prefix, so its length calcuted by strlen is 0.
                // we need to fix that if there is '0x00' in text message,
                // since javascript language could support '0x00' inserted at the beginning or the middle of text message
            }

            cobj->send(socketID, data);
        }
        else if (argv[1].isObject())
        {
            uint8_t *bufdata = NULL;
            uint32_t len = 0;

            JS::RootedObject jsobj(cx, argv[1].toObjectOrNull());
            if (JS_IsArrayBufferObject(jsobj))
            {
                bufdata = JS_GetArrayBufferData(jsobj);
                len = JS_GetArrayBufferByteLength(jsobj);
            }
            else if (JS_IsArrayBufferViewObject(jsobj))
            {
                bufdata = (uint8_t*)JS_GetArrayBufferViewData(jsobj);
                len = JS_GetArrayBufferViewByteLength(jsobj);
            }

            cobj->send(socketID, bufdata, len);
        }
        else
        {
            JS_ReportError(cx, "data type to be sent is unsupported.");
            return false;
        }

        argv.rval().setUndefined();

        return true;
    }
    JS_ReportError(cx, "wrong number of arguments: %d, was expecting %d", argc, 0);
    return true;
}

bool js_cocos2dx_extension_WebSocketServer_broadcast(JSContext *cx, uint32_t argc, jsval *vp)
{
    JS::CallArgs argv = JS::CallArgsFromVp(argc, vp);
    JS::RootedObject obj(cx, argv.thisv().toObjectOrNull());
    js_proxy_t *proxy = jsb_get_js_proxy(obj);
    WebSocketServer* cobj = (WebSocketServer *)(proxy ? proxy->ptr : NULL);
    JSB_PRECONDITION2( cobj, cx, false, "Invalid Native Object");

    if(argc == 1)
    {
        bool ok = true;
        
        if (argv[0].isString())
        {
            ssize_t len = JS_GetStringLength(argv[0].toString());
            std::string data;
            jsval_to_std_string(cx, argv[0], &data);

            if (data.empty() && len > 0)
            {
                CCLOGWARN("Text message to send is empty, but its length is greater than 0!");
                //FIXME: Note that this text message contains '0x00' prefix, so its length calcuted by strlen is 0.
                // we need to fix that if there is '0x00' in text message,
                // since javascript language could support '0x00' inserted at the beginning or the middle of text message
            }

            cobj->broadcast(data);
        }
        else if (argv[0].isObject())
        {
            uint8_t *bufdata = NULL;
            uint32_t len = 0;

            JS::RootedObject jsobj(cx, argv[0].toObjectOrNull());
            if (JS_IsArrayBufferObject(jsobj))
            {
                bufdata = JS_GetArrayBufferData(jsobj);
                len = JS_GetArrayBufferByteLength(jsobj);
            }
            else if (JS_IsArrayBufferViewObject(jsobj))
            {
                bufdata = (uint8_t*)JS_GetArrayBufferViewData(jsobj);
                len = JS_GetArrayBufferViewByteLength(jsobj);
            }

            cobj->broadcast(bufdata, len);
        }
        else
        {
            JS_ReportError(cx, "data type to be sent is unsupported.");
            return false;
        }

        argv.rval().setUndefined();

        return true;
    }
    JS_ReportError(cx, "wrong number of arguments: %d, was expecting %d", argc, 0);
    return true;
}

bool js_cocos2dx_extension_WebSocketServer_stop(JSContext *cx, uint32_t argc, jsval *vp){
    JS::CallArgs args = JS::CallArgsFromVp(argc, vp);
    JS::RootedObject obj(cx, args.thisv().toObjectOrNull());
    js_proxy_t *proxy = jsb_get_js_proxy(obj);
    WebSocketServer* cobj = (WebSocketServer *)(proxy ? proxy->ptr : NULL);
    JSB_PRECONDITION2( cobj, cx, false, "Invalid Native Object");

    if(argc == 0){
        cobj->stopAsync();
        args.rval().setUndefined();
        return true;
    }
    JS_ReportError(cx, "wrong number of arguments: %d, was expecting %d", argc, 0);
    return false;
}

bool js_cocos2dx_extension_WebSocketServer_constructor(JSContext *cx, uint32_t argc, jsval *vp)
{
    JS::CallArgs args = JS::CallArgsFromVp(argc, vp);

    if (argc == 1 || argc == 2)
    {
        int port;

        do {
            bool ok = jsval_to_int32(cx, args.get(0), (int32_t *)&port);
            JSB_PRECONDITION2( ok, cx, false, "Error processing arguments");
        } while (0);

        JS::RootedObject proto(cx, js_cocos2dx_websocketserver_prototype);
        JS::RootedObject obj(cx, JS_NewObject(cx, js_cocos2dx_websocketserver_class, proto, JS::NullPtr()));

        WebSocketServer* cobj = nullptr;
        if (argc == 2)
        {
            std::vector<std::string> protocols;

            if (args.get(1).isString())
            {
                std::string protocol;
                do {
                    bool ok = jsval_to_std_string(cx, args.get(1), &protocol);
                    JSB_PRECONDITION2( ok, cx, false, "Error processing arguments");
                } while (0);
                protocols.push_back(protocol);
            }
            else if (args.get(1).isObject())
            {
                bool ok = true;
                JS::RootedObject arg2(cx, args.get(1).toObjectOrNull());
                JSB_PRECONDITION(JS_IsArrayObject( cx, arg2 ),  "Object must be an array");

                uint32_t len = 0;
                JS_GetArrayLength(cx, arg2, &len);

                for( uint32_t i=0; i< len;i++ )
                {
                    JS::RootedValue valarg(cx);
                    JS_GetElement(cx, arg2, i, &valarg);
                    std::string protocol;
                    do {
                        ok = jsval_to_std_string(cx, valarg, &protocol);
                        JSB_PRECONDITION2( ok, cx, false, "Error processing arguments");
                    } while (0);

                    protocols.push_back(protocol);
                }
            }
            
            cobj = new (std::nothrow) WebSocketServer();
            JSB_WebSocketServerDelegate* delegate = new (std::nothrow) JSB_WebSocketServerDelegate();
            delegate->setJSDelegate(obj);
            cobj->init(*delegate, port, &protocols);
        }
        else
        {
            cobj = new (std::nothrow) WebSocketServer();
            JSB_WebSocketServerDelegate* delegate = new (std::nothrow) JSB_WebSocketServerDelegate();
            delegate->setJSDelegate(obj);
            cobj->init(*delegate, port);
        }

        JS_DefineProperty(cx, obj, "PORT", args.get(0), JSPROP_ENUMERATE | JSPROP_PERMANENT | JSPROP_READONLY);

        //protocol not support yet (always return "")
        JS::RootedValue jsprotocol(cx, c_string_to_jsval(cx, ""));
        JS_DefineProperty(cx, obj, "protocol", jsprotocol, JSPROP_ENUMERATE | JSPROP_PERMANENT | JSPROP_READONLY);

        // link the native object with the javascript object
        js_proxy_t *p = jsb_new_proxy(cobj, obj);
        JS::AddNamedObjectRoot(cx, &p->obj, "WebSocketServer");

        args.rval().set(OBJECT_TO_JSVAL(obj));
        return true;
    }

    JS_ReportError(cx, "wrong number of arguments: %d, was expecting %d", argc, 0);
    return false;
}

static bool js_cocos2dx_extension_WebSocketServer_get_readyState(JSContext *cx, uint32_t argc, jsval *vp)
{
    JS::CallArgs args = JS::CallArgsFromVp(argc, vp);
    JS::RootedObject jsobj(cx, args.thisv().toObjectOrNull());
    js_proxy_t *proxy = jsb_get_js_proxy(jsobj);
    WebSocketServer* cobj = (WebSocketServer *)(proxy ? proxy->ptr : NULL);
    JSB_PRECONDITION2( cobj, cx, false, "Invalid Native Object");

    if (cobj) {
        args.rval().set(INT_TO_JSVAL((int)cobj->getReadyState()));
        return true;
    } else {
        JS_ReportError(cx, "Error: WebSocketServer instance is invalid.");
        return false;
    }
}

void register_jsb_websocketserver(JSContext *cx, JS::HandleObject global)
{
    js_cocos2dx_websocketserver_class = (JSClass *)calloc(1, sizeof(JSClass));
    js_cocos2dx_websocketserver_class->name = "WebSocketServer";
    js_cocos2dx_websocketserver_class->addProperty = JS_PropertyStub;
    js_cocos2dx_websocketserver_class->delProperty = JS_DeletePropertyStub;
    js_cocos2dx_websocketserver_class->getProperty = JS_PropertyStub;
    js_cocos2dx_websocketserver_class->setProperty = JS_StrictPropertyStub;
    js_cocos2dx_websocketserver_class->enumerate = JS_EnumerateStub;
    js_cocos2dx_websocketserver_class->resolve = JS_ResolveStub;
    js_cocos2dx_websocketserver_class->convert = JS_ConvertStub;
    js_cocos2dx_websocketserver_class->finalize = js_cocos2dx_WebSocketServer_finalize;
    js_cocos2dx_websocketserver_class->flags = JSCLASS_HAS_RESERVED_SLOTS(2);

    static JSPropertySpec properties[] = {
        JS_PSG("readyState", js_cocos2dx_extension_WebSocketServer_get_readyState, JSPROP_ENUMERATE | JSPROP_PERMANENT),
        JS_PS_END
    };



    static JSFunctionSpec funcs[] = {
        JS_FN("send",js_cocos2dx_extension_WebSocketServer_send, 2, JSPROP_PERMANENT | JSPROP_ENUMERATE),
        JS_FN("broadcast",js_cocos2dx_extension_WebSocketServer_broadcast, 1, JSPROP_PERMANENT | JSPROP_ENUMERATE),
        JS_FN("stop",js_cocos2dx_extension_WebSocketServer_stop, 0, JSPROP_PERMANENT | JSPROP_ENUMERATE),
        JS_FS_END
    };

    static JSFunctionSpec st_funcs[] = {
        JS_FS_END
    };

    js_cocos2dx_websocketserver_prototype = JS_InitClass(
                                                cx, global,
                                                JS::NullPtr(),
                                                js_cocos2dx_websocketserver_class,
                                                js_cocos2dx_extension_WebSocketServer_constructor, 0, // constructor
                                                properties,
                                                funcs,
                                                NULL, // no static properties
                                                st_funcs);

    JS::RootedObject jsclassObj(cx, anonEvaluate(cx, global, "(function () { return WebSocketServer; })()").toObjectOrNull());

    JS_DefineProperty(cx, jsclassObj, "STARTING", (int)WebSocketServer::State::STARTING, JSPROP_ENUMERATE | JSPROP_PERMANENT | JSPROP_READONLY);
    JS_DefineProperty(cx, jsclassObj, "UP", (int)WebSocketServer::State::UP, JSPROP_ENUMERATE | JSPROP_PERMANENT | JSPROP_READONLY);
    JS_DefineProperty(cx, jsclassObj, "STOPPING", (int)WebSocketServer::State::STOPPING, JSPROP_ENUMERATE | JSPROP_PERMANENT | JSPROP_READONLY);
    JS_DefineProperty(cx, jsclassObj, "DOWN", (int)WebSocketServer::State::DOWN, JSPROP_ENUMERATE | JSPROP_PERMANENT | JSPROP_READONLY);
}
