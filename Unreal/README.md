#### 具体接入步骤
1. 从 [UE4 Poco SDK](https://github.com/AirtestProject/Poco-SDK/tree/master/Unreal) 将 `PocoSDK` 这个文件夹克隆下来，放置到您项目的 `Plugins` 目录下。如果您项目没有名为 `Plugins` 的目录的话，需要先创建该目录。


2. 重新编译项目。确认在编辑器的 `Edit > Plugins` 当中能看到 `Poco SDK` ，且 `Enabled` 为被勾选的状态。

    如果此时 `Enabled` 未被勾选，请勾选 `Enabled` 并按提示重启编辑器/VS。
    
    ![image](https://note.youdao.com/yws/public/resource/df1f7128bebe35c6759cfc2f5193a498/xmlnote/WEBRESOURCEb44cd535cade696b1ed6c44086a4d00c/81)
    
3. 运行游戏。


#### 一些常见的问题：

1. `Poco SDK` 未能正常启动。

    确认 `Poco SDK `为 `Enabled` 的状态。
    确认 `.uproject` 文件中包含有`Poco SDK`, 且为Enabled状态。
    
2. 启动游戏时报错，`Output Log` 中信息为
    ```
    LogPluginManager: Error: Unable to load plugin 'PocoSDK'. Aborting.
    ```
    请确认`PocoSDK.uplugin`文件中的`"Installed"`这一项对应的值是`true`，且没有`"Enterprise"`这一项。