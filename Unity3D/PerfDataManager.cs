using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class PerfDataManager : MonoBehaviour
{
    public bool isStart = false;
    
    public int frame = 0;
    public int frameRateCur = 0;
    public int uss;
    public int pss;
    public int jankSmall;
    public int jankBig;
    public long allocateMemoryForGraphicsDriver;
    public long monoHeapSize;
    public long monoUsedSize;
    public long tempAllocateSize;
    public long totalAllocateMemory;
    public long totalReservedMemory;
    public long totalUnusedReservedMemory;
    public long currentTime;
    public long jankSmallTime;
    public long jankBigTime;
    
    private long lastTicks = -1;
    private int frameCount = 0;
    private long T = 10000000;

    public long GetCurDateTime()
    {
        return DateTime.Now.Ticks;
    }

    private void CalcFrameRate()
    {
        frameCount += 1;

        if (!IsCrossT(GetCurDateTime()))
        {
            return;
        }

        frameRateCur = frameCount;
        frameCount = 0;
        
    }
    
    public void CalcCurrentTime()
    {
        currentTime = GetCurDateTime() / 10 - beginTime;
    }

    private bool IsCrossT(long curTicks)
    {
        if (lastTicks == -1)
        {
            lastTicks = curTicks;
            return false;
        }
        if (curTicks - lastTicks >= T)
        {
            lastTicks = curTicks;
            return true;
        }

        return false;
    }

    private long severalFrameTime = -1;

    private long lastSeveralFrameTime = -1;
    // private long lastCheckTicks = -1;
    private long curFrameTime = -1;
    private int fCount = 0;
    private int standardFrameNum = 3;
    private long lastUpdateTicks = -1;
    public long dumpTime = 0;
    private List<long> tmpTimes = new List<long>();

    private void UpdateFrameTime()
    {
        if (frame == 0)
        {
            return;
        }
        
        var curTicks = GetCurDateTime();

        if (lastUpdateTicks == -1)
        {
            lastUpdateTicks = curTicks;
            return;
        }
        
        curFrameTime = curTicks - lastUpdateTicks - dumpTime;
        dumpTime = 0;
        lastUpdateTicks = curTicks;
        
        if (frame > standardFrameNum + 1)
        {
            // todo 统计次数大于3之后，需要一个长度为3的左出容器 把最左侧的记录剔除
            tmpTimes.RemoveAt(0);
        }
        
        // todo 改为向容器追加
        tmpTimes.Add(curFrameTime);

        // todo 如果容器内元素个数小于3，则return
        if (tmpTimes.Count < standardFrameNum)
        {
            return;
        }
        // todo 如果容器内元素个数大于3，抛出异常
        if (tmpTimes.Count > standardFrameNum)
        {
            throw new Exception("统计帧数大于3");
        }

        lastSeveralFrameTime = severalFrameTime;
        // todo 改为对容器内元素求和
        severalFrameTime = tmpTimes.Sum();
        
    }
    
    private void CalcJank()
    {
        if (curFrameTime == -1)
        {
            return;
        }

        if (lastSeveralFrameTime == -1)
        {
            return;
        }

        long avgFrameTime = lastSeveralFrameTime / standardFrameNum;

        // 840000是840000个百纳秒，即84ms，84ms为2倍电影帧耗时
        if ((curFrameTime >= avgFrameTime * 1.5f && curFrameTime < avgFrameTime * 2.0f) && (curFrameTime >= 840000 && curFrameTime < 1250000))
        {
            jankSmall += 1;
            jankSmallTime += curFrameTime/10;
        }
        else if (curFrameTime >= avgFrameTime * 2.0f && curFrameTime >= 1250000)
        {
            jankBig += 1;
            jankBigTime += curFrameTime/10;
        }
        
        // curFrameTime = -1;
    }
    
    public AndroidJavaObject info;
    
    private void CalcUss()
    {
#if (UNITY_ANDROID && !UNITY_EDITOR) || ANDROID_CODE_VIEW
        uss = info.Call<int>("getTotalUss");
# endif
    }
    
    private void CalcPss()
    {
#if (UNITY_ANDROID && !UNITY_EDITOR) || ANDROID_CODE_VIEW
        pss = info.Call<int>("getTotalPss");
# endif
    }

    private void CalcMonoMemory()
    {
        Profiler.BeginSample("PerfDataManager.CalcMonoMemory");
        monoHeapSize = UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong() / 1024;
        monoUsedSize = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() / 1024;
        tempAllocateSize = UnityEngine.Profiling.Profiler.GetTempAllocatorSize() / 1024;
        totalAllocateMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024;
        totalReservedMemory = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / 1024;
        totalUnusedReservedMemory = UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemoryLong() / 1024;
#if (DEVELOPMENT_BUILD)
        allocateMemoryForGraphicsDriver = UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver() / 1024;
#endif
        Profiler.EndSample();
    }

    void RealUpdate()
    {
        frame += 1;
        // ScreenShot();
        UpdateFrameTime();
        CalcCurrentTime();
        if (!isStart)
        {
            return;   
        }
        
        CalcFrameRate();
        CalcJank();
        CalcUss();
        CalcPss();
        CalcMonoMemory();
    }

    public bool isRunning;
    public int androidProcessId;
    public long beginTime;
    
    public void Awake()
    {
        isRunning = true;
        beginTime = DateTime.Now.Ticks / 10;
    }

    public void Update()
    {
       // Profiler.BeginSample("PerfDataManager.Update");
       RealUpdate();
       // Profiler.EndSample();
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 280, 30), $"frame: {frameRateCur} 帧/秒");
        GUI.Label(new Rect(10, 45, 280, 30), $"littlejank: {jankSmall} 次");
        GUI.Label(new Rect(10, 80, 280, 30), $"bigjank: {jankBig} 次");
        GUI.Label(new Rect(10, 115, 280, 30), $"uss: {uss} KB");
        GUI.Label(new Rect(10, 150, 280, 30), $"monoHeapSize: {monoHeapSize} KB");
        GUI.Label(new Rect(10, 185, 280, 30), $"monoUsedSize: {monoUsedSize} KB");
        GUI.Label(new Rect(10, 220, 280, 30), $"tempAllocateSize: {tempAllocateSize} KB");
        GUI.Label(new Rect(10, 255, 280, 30), $"totalAllocateMemory: {totalAllocateMemory} KB");
        GUI.Label(new Rect(10, 290, 280, 30), $"totalReservedMemory: {totalReservedMemory} KB");
        GUI.Label(new Rect(10, 325, 280, 30), $"totalUnusedReservedMemory: {totalUnusedReservedMemory} KB");
        GUI.Label(new Rect(10, 360, 280, 30), $"allocateMemoryForGraphicsDriver: {allocateMemoryForGraphicsDriver} KB");
        GUI.Label(new Rect(10, 395, 280, 30), $"currentTime: {currentTime} us");
        GUI.Label(new Rect(10, 425, 280, 30), $"jankSmallTime: {jankSmallTime} us");
        GUI.Label(new Rect(10, 455, 280, 30), $"jankBigTime: {jankBigTime} us");
        
    }
}

