# Unity3D Integration Guide

PocoSDK supports Unity3D version 4 & 5 and above, ngui & ugui & fairygui, C# only for now. 

1. Clone source code from [poco-sdk](https://github.com/AirtestProject/Poco-SDK) repo.

2. Copy the `Unity3D` folder to your unity project script folder.

3. 
    - If you are using ngui, just delete the sub folder `Unity3D/ugui` and `Unity3D/fairygui` and `Unity3D/uguiWithTMPro`. 
    - If you are using ugui and TMPro plugin , just delete the sub folder `Unity3D/ngui` and  `Unity3D/fairygui` and  `Unity3D/ugui`.
    - If you are using ugui, just delete the sub folder `Unity3D/ngui` and  `Unity3D/fairygui` and `Unity3D/uguiWithTMPro`.
    - If you are using fairygui, please refer [fairygui guide](https://github.com/AirtestProject/Poco-SDK/tree/master/Unity3D/fairygui)

4. Add `Unity3D/PocoManager.cs` as script component on any GameObject, generally on main camera.