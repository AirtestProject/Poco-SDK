LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

$(call import-add-path, $(LOCAL_PATH)/../../../cocos2d-x/external)

LOCAL_MODULE := cocos2djs_shared

LOCAL_MODULE_FILENAME := libcocos2djs

ifeq ($(USE_ARM_MODE),1)
LOCAL_ARM_MODE := arm
endif

LOCAL_SRC_FILES := hellojavascript/main.cpp \
                   ../../Classes/AppDelegate.cpp \
                   ../../Classes/WebSocketServer.cpp \
                   ../../Classes/jsb_websocketserver.cpp

LOCAL_C_INCLUDES := $(LOCAL_PATH)/../../Classes \
					$(LOCAL_PATH)/../../../cocos2d-x/external/websockets/include/android

LOCAL_STATIC_LIBRARIES := cocos2d_js_static websockets_static

LOCAL_EXPORT_CFLAGS := -DCOCOS2D_DEBUG=2 -DCOCOS2D_JAVASCRIPT

include $(BUILD_SHARED_LIBRARY)

$(call import-module, websockets/prebuilt/android)
$(call import-module, scripting/js-bindings/proj.android)
