
.. raw:: html
    
    <div>
        <img src='doc/img/logo-simple-poco-sdk-unity3d.png' width='150px' height='150px' alt='poco-sdk for Unity3D' />
        <img src='doc/img/logo-simple-poco-sdk-cocos2dx.png' width='150px' height='150px' alt='poco-sdk for cocos2dx' />
    </div>

poco-sdk
========

This repo contains all implememented poco-sdk, for most popular game engines.

Each directory is an engine specific poco-sdk implementation. You can simply copy the corresponding directory to your project and initialize the module/class from your game scripts.

``sdk`` directory contains the unimplemented language specific base sdk, which can be used to implement an poco-sdk for other game engines

For more detailed integration steps, please refer to `Integration Guide`_.

Issue title format
------------------

Feel free to `open an issue`_ if you are stuck at integrating the SDK module in your game/app.

The title format will be ``[<EngineName>-sdk] xxx``. e.g. ``[cocos2dx-lua sdk] XXX Error occurs when...``


.. _Integration Guide: http://poco.readthedocs.io/en/latest/source/doc/integration.html
.. _open an issue: https://github.com/AirtestProject/Poco-SDK/issues/new

Contribution
------------

We welcome everyone to implement other 3rd party platforms/engines for poco-sdk.

Acknowledgement
---------------

`Egret`_: By `github.com/szzg <https://github.com/szzg>`_

.. _Egret: https://www.egret.com/en/
