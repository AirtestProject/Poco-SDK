var POCO_SDK_VERSION = '1.0.3'

try {
    module.exports = POCO_SDK_VERSION;
} catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = POCO_SDK_VERSION;
    }
}
