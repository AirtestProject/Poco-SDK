# Unity3D Integration Guide

PocoSDK supports Unity3D version 4 & 5 and above, ngui & ugui & fairygui, C# only for now. 

1. Clone source code from [poco-sdk](https://github.com/AirtestProject/Poco-SDK) repo.

2. Copy the `Unity3D` folder to your unity project script folder.

3. 
    - If you are using ngui, just delete the sub folder `Unity3D/ugui` and `Unity3D/fairygui`. 
    - If you are using ugui, just delete the sub folder `Unity3D/ngui` and  `Unity3D/fairygui`.
    - If you are using fairygui, please refer [fairygui guide](https://github.com/AirtestProject/Poco-SDK/tree/master/Unity3D/fairygui)

4. Add `Unity3D/PocoManager.cs` as script component on any GameObject, generally on main camera.

## SDK接入指南
PocoSDK支持Unity3D版本4和5及更高版本，支持ngui、ugui与fairygui的C#版本，暂不支持lua调用。
1. 从[poco-sdk](https://github.comAirtestProjectPoco-SDK) 仓库克隆源代码。
2. 将`Unity3D`文件夹复制到您的unity项目的Scripts文件夹下。 
3.
    - 如果使用的是ngui，需要删除子文件夹`Unity3D/ugui`和`Unity3D/fairygui`
    - 如果您使用的是ugui，只需删除子文件夹`Unity3D/ngui`和`Unity3D/fairygui`
    - 如果您使用的是fairygui，请参阅[fairygui指南](https://github.comAirtestProjectPoco-SDKtreemasterUnity3Dfairygui)

4. 在任何GameObject上（通常在主摄像机上，或创建一个新的空GameObject）添加`Unity3DPocoManager.cs`作为脚本组件。

