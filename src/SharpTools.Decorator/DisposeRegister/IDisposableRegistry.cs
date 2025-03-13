﻿namespace SharpTools.Decorator.DisposeRegister;

public interface IDisposableRegistry
{
    void Register(IDisposable disposable);
    void Dispose();
}
