# egret安装pocoSDK说明
* 首先下载pocoSDK 
* 然后在`egretProperties.json`中修改modules属性，添加红框内相应字段，**其中name属性必须为poco**
* 路径可以是相对路径也可以是绝对路径，具体可以参考[白鹭引擎说明文档](http://developer.egret.com/cn/github/egret-docs/Engine2D/projectConfig/configFile/index.html)中有关于 modules字段的说明

![添加和修改modules属性](doc/1.png)

- 然后通过快捷键ctrl+`呼出终端 在终端中执行命令egret build -e
![](doc/2.png)

- 在入口文件`main.ts`的rungame函数中新建类型为`PocoManager`的对象，并且传入this.stage
![](doc/3.png)

- 最后再终端中输入`python -m poco.utils.net.stdbroker ws://*:5003 tcp://*:15004`打开代理服务器
- 其中websocket端口默认为5003 如果有更改的需要可以在新建pocomanager的时候传入端口参数
![](doc/4.png)

