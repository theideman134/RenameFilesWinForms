using System;
using System.IO;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Diagnosers;
using RenameFilesWinForms;

[MemoryDiagnoser]
public class Form1Bench
{
    private MethodInfo _newFileNameMethod;
    private MethodInfo _addedExtMethod;
    private object _form1Instance;
    private FileInfo _jpgFileInfo;

    [GlobalSetup]
    public void Setup()
    {
        var type = typeof(Form1);
        _newFileNameMethod = type.GetMethod("NewFileName", BindingFlags.Instance | BindingFlags.NonPublic);
        _addedExtMethod = type.GetMethod("AddedFileExtention", BindingFlags.Static | BindingFlags.NonPublic);
        _form1Instance = Activator.CreateInstance(type);
        _jpgFileInfo = new FileInfo("test.jpg");
    }

    [Benchmark]
    public string NewFileName_Invoke()
    {
        var result = _newFileNameMethod.Invoke(_form1Instance, new object[] { DateTime.Now });
        return (string)result;
    }

    [Benchmark]
    public string AddedExt_Invoke()
    {
        var result = _addedExtMethod.Invoke(null, new object[] { _jpgFileInfo, "prefix" });
        return (string)result;
    }
}